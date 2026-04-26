namespace River.Core;

/// <summary>
/// Model kinetyczny przemian zanieczyszczeń w rzece (BZT, N-NH3, O2)
/// rozwiązywany metodą kroków wzdłuż łańcucha odcinków.
/// Port logiki z <c>FramePart.cs</c> + orkiestracja z <c>FormMain.countModel</c>.
/// </summary>
public static class RiverModel
{
    private const double Alfa0 = 0.05;        // dolna granica O2 — odpowiednik ALFA0 w oryginale
    private const double NitrogenOxygenRatio = 4.3;  // gO2 / gN-NH3 zużywane w nitryfikacji

    public static SegmentResult[] Compute(ModelParams globals, IReadOnlyList<SegmentInput> segments)
    {
        ArgumentNullException.ThrowIfNull(globals);
        ArgumentNullException.ThrowIfNull(segments);

        int n = segments.Count;
        var work = new SegmentWork[n];
        var results = new SegmentResult[n];

        for (int i = 0; i < n; i++)
        {
            var w = new SegmentWork(globals, segments[i]);

            // Składanie ładunków z odcinków, których odpływ wskazuje na bieżący (vOUT + j == i).
            double upQ = 0, upLB = 0, upLN = 0, upLO = 0;
            for (int j = 0; j < i; j++)
            {
                var src = segments[j];
                if (src.OutletTo > 0 && j + src.OutletTo == i)
                {
                    var sr = results[j];
                    double outQ = sr.Q[^1];
                    upQ  += outQ;
                    upLB += sr.B[^1] * outQ;
                    upLN += sr.N[^1] * outQ;
                    upLO += sr.O[^1] * outQ;
                }
            }
            w.SetUpstreamLoads(upQ, upLB, upLN, upLO);

            results[i] = w.RunInitialAndWhole();
            work[i] = w;
        }

        return results;
    }

    /// <summary>Stan obliczeniowy pojedynczego odcinka — odpowiednik pól <c>FramePart</c>.</summary>
    private sealed class SegmentWork
    {
        private readonly SegmentInput _in;

        // Stałe szybkości po kompensacji temperaturowej (jak w setModelVar).
        private readonly double _k1, _k2, _k3, _k4, _rB, _rAP;
        private readonly double _k1At20;     // K1 zdekompensowane do 20°C — używane w convertB <-> B5
        private readonly double _osat;       // stężenie nasycenia O2

        // Ładunki początkowe odcinka (po dopływach z góry / źródeł).
        private double _iQ, _iLB, _iLN, _iLO;

        public SegmentWork(ModelParams g, SegmentInput input)
        {
            _in = input;

            // setModelVar — kompensacja temperaturowa wg współczynników z oryginału.
            double x = input.T - 20.0;
            _k1  = g.K1  * Math.Pow(1.047, x);
            _k2  = g.K2  * Math.Pow(1.037, x);
            _k3  = g.K3  * Math.Pow(1.027, x);
            _k4  = g.K4  * Math.Pow(1.05,  x);
            _rB  = g.RB  * Math.Pow(1.065, x);
            _rAP = g.RAP;                         // w oryginale: Math.Pow(1.0, x) ≡ 1

            // countHelpers
            _osat   = 481.5 / (32.6 + input.T);
            _k1At20 = _k1 / Math.Pow(1.047, x);   // == g.K1
        }

        public void SetUpstreamLoads(double q, double lb, double ln, double lo)
        {
            _iQ = q; _iLB = lb; _iLN = ln; _iLO = lo;
        }

        public SegmentResult RunInitialAndWhole()
        {
            int N = _in.N;
            var X  = new double[N + 1];
            var T  = new double[N + 1];
            var Q  = new double[N + 1];
            var B  = new double[N + 1];
            var B5 = new double[N + 1];
            var Nm = new double[N + 1];
            var O  = new double[N + 1];

            // ─── countInital ──────────────────────────────────────────────────
            // UWAGA (zachowane z oryginału): iQ jest zawsze nadpisywane na vQ,
            // czyli ładunki z góry trafiają do odcinka, ale jako stężenia liczone
            // przez vQ tego odcinka — użytkownik musi ustawić vQ jako sumaryczny przepływ.
            _iQ = _in.Q;
            double initIO = _osat - _in.InitDeficit;

            if (_in.IsInitial)
            {
                _iLB = _in.Q * ConvertB5ToB(_in.InitB);
                _iLN = _in.Q * _in.InitN;
                _iLO = _in.Q * initIO;
            }

            switch (_in.Source)
            {
                case PollutionSource.Point:
                    _iQ  += _in.SrcQ;
                    _iLB += _in.SrcQ * ConvertB5ToB(_in.SrcB);
                    _iLN += _in.SrcQ * _in.SrcN;
                    _iLO += _in.SrcQ * _in.SrcO;
                    break;

                case PollutionSource.Linear:
                    // tylko 1/N na wejściu, reszta dokładana co krok w countKinModel
                    _iQ  += _in.SrcQ / N;
                    _iLB += _in.SrcQ * ConvertB5ToB(_in.SrcB) / N;
                    _iLN += _in.SrcQ * _in.SrcN / N;
                    _iLO += _in.SrcQ * _in.SrcO / N;
                    break;
            }

            Q[0] = _iQ;
            if (_iQ > 1e-10)
            {
                B[0]  = _iLB / _iQ;
                Nm[0] = _iLN / _iQ;
                O[0]  = _iLO / _iQ;
            }
            else
            {
                Q[0]  = 1.0;
                B[0]  = _in.InitB;
                Nm[0] = _in.InitN;
                O[0]  = initIO;
            }
            B5[0] = ConvertBToB5(B[0]);
            X[0]  = 0;
            T[0]  = 0;

            // ─── countWholePart ───────────────────────────────────────────────
            double dx = _in.L / N;
            double dt = _in.L / (_in.V * 24.0) / N;   // d
            double srcQperStep = _in.Source == PollutionSource.Linear ? _in.SrcQ / N : 0;
            double srcBLoadPerStep = _in.Source == PollutionSource.Linear ? ConvertB5ToB(_in.SrcB) * _in.SrcQ / N : 0;
            double srcNLoadPerStep = _in.Source == PollutionSource.Linear ? _in.SrcN * _in.SrcQ / N : 0;
            double srcOLoadPerStep = _in.Source == PollutionSource.Linear ? _in.SrcO * _in.SrcQ / N : 0;

            for (int p = 1; p <= N; p++)
            {
                X[p]  = X[p - 1] + dx;
                T[p]  = T[p - 1] + dt;
                Q[p]  = Q[p - 1];

                double Bprev = B[p - 1];
                double Nprev = Nm[p - 1];
                double Oprev = O[p - 1];

                double dB_1 = Bprev * (1.0 - Math.Exp(-_k1 * dt));   // rozkład biochemiczny
                double dB_3 = Bprev * (1.0 - Math.Exp(-_k3 * dt));   // sedymentacja
                double dN_4 = Nprev * (1.0 - Math.Exp(-_k4 * dt));   // nitryfikacja
                double dO2_1 = dB_1;
                double dO2_2 = _osat - (_osat - Oprev) * Math.Exp(-_k2 * dt) - Oprev;  // reaeracja (przyrost)
                double dO2_4 = NitrogenOxygenRatio * dN_4;
                double dO2_5 = _rB  * dt;
                double dO2_6 = _rAP * dt;

                double oCandidate = Oprev + dO2_2 + dO2_6 - dO2_1 - dO2_4 - dO2_5;

                if (oCandidate > Alfa0)
                {
                    double Bnew = Bprev - dB_1 - dB_3;
                    if (Bnew < 0)
                    {
                        dB_1 += Bnew;
                        if (dB_1 < 0) dB_1 = 0;
                        dO2_1 = dB_1;
                        Bnew = 0;
                    }
                    double Nnew = Nprev - dN_4;
                    if (Nnew < 0)
                    {
                        dN_4 += Nnew;
                        if (dN_4 < 0) dN_4 = 0;
                        dO2_4 = dN_4 * NitrogenOxygenRatio;
                        Nnew = 0;
                    }
                    B[p]  = Bnew;
                    Nm[p] = Nnew;
                    O[p]  = Oprev + dO2_2 + dO2_6 - dO2_1 - dO2_4 - dO2_5;
                }
                else
                {
                    // Tlen wyczerpany — proporcjonalne zmniejszenie procesów tlenochłonnych.
                    double sumO = dO2_1 + dO2_4 + dO2_5;
                    double teB = sumO > 0 ? dO2_1 / sumO : 0;
                    double teN = sumO > 0 ? dO2_4 / sumO : 0;
                    O[p] = Alfa0;
                    double Bnew = Bprev - dB_3 - teB * dB_1;
                    if (Bnew < 0) Bnew = 0;
                    B[p]  = Bnew;
                    Nm[p] = Nprev - teN * dN_4;
                }

                if (_in.Source == PollutionSource.Linear)
                {
                    double denom = Q[p] + srcQperStep;
                    B[p]  = (B[p]  * Q[p] + srcBLoadPerStep) / denom;
                    Nm[p] = (Nm[p] * Q[p] + srcNLoadPerStep) / denom;
                    O[p]  = (O[p]  * Q[p] + srcOLoadPerStep) / denom;
                    Q[p]  = denom;
                }

                B5[p] = ConvertBToB5(B[p]);
            }

            return new SegmentResult { X = X, T = T, Q = Q, B = B, B5 = B5, N = Nm, O = O };
        }

        private double ConvertBToB5(double bzt)  => bzt * (1.0 - Math.Exp(-_k1At20 * 5.0));
        private double ConvertB5ToB(double bzt5) => bzt5 / (1.0 - Math.Exp(-_k1At20 * 5.0));
    }
}

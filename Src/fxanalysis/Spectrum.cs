using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using FxMath;
using FxCuda;
using System.Threading;

namespace fxanalysis
{
    class Spectrogram
    {
        public Spectrogram(Complex c, int k, float T)
        {
            coef = c;
            index = k;
            amplitude = k == 0 ? c.Magnitude : 2 * c.Magnitude;
            phi = -c.Phase * 180.0f / (float)Math.PI;
            power = k == 0 ? amplitude * amplitude : amplitude * amplitude / 2;
            energy = power * T;
            frequency = k / T;
            term = k > 0 ? (T / k) : float.NaN;
        }
        public readonly Complex coef;
        public readonly int index;
        public readonly float amplitude;
        public readonly float phi;
        public readonly float power;
        public readonly float energy;
        public readonly float frequency;
        public readonly float term;
    }

    class Spectrum : ICommand
    {
        public bool Execute(IList<string> cmd_params)
        {
            if (cmd_params.Count == 4)
            {
                WindowFunc wf = WindowFunc.Rect;
                if (Utils.StrToEnum(cmd_params[3], ref wf))
                {
                    Transform(cmd_params[2], int.Parse(cmd_params[0]), cmd_params[1].ToUpper(), wf);
                    return true;
                }
            }
            return false;
        }
        
        const int MaxApproxPower = 3;

        enum WindowFunc
        {
            Rect, Hamming, Blackman
        }

        void Transform(string binfile, int apow, string threads, WindowFunc wf)
        {
            string pair;
            DateTime first_date;
            DateTime last_date;
            short pip;
            Periods avgtype;
            Quote[] quotes = BinFile.ReadBinFile(binfile, out pair, out pip, out avgtype, out first_date, out last_date);
            string candles_type;
            candles_type = Enum.GetName(typeof(Periods), avgtype);

            float[] x, y;
            x = new float[quotes.Length];
            y = new float[quotes.Length];
            for (int i = 0; i < quotes.Length; i++)
            {
                x[i] = i;
                y[i] = quotes[i].close;
            }

            // проводим аппроксимацию и записываем результат в файл
            Console.WriteLine();
            float[][] pcoef = new float[MaxApproxPower + 1][];
            using (StreamWriter dat = new StreamWriter(Utils.CorrectFilePath(pair + "." + candles_type + ".approx.dat"), false, Encoding.ASCII))
            {
                dat.WriteLine("# {0}", pair);
                dat.WriteLine("# From {0} to {1} ({2} records)", first_date, last_date, y.Length);
                Console.WriteLine(" Writing approx datafile:");
                for (int p = 0; p < pcoef.Length; p++)
                {
                    IPolynomial aprox = new LinearApproximation(p);
                    pcoef[p] = aprox.Transform(x, y);
                    dat.WriteLine("# cond={0:0.0}dB", 10 * Math.Log10(aprox.Condition));
                    dat.WriteLine(BuildFunctionString(p, pcoef[p]));
                    Console.WriteLine(" {0} (cond={1:0.0})dB", BuildFunctionString(p, pcoef[p]), 10 * Math.Log10(aprox.Condition));
                }
            }

            // Вычесть аппроксимирующую
            Console.WriteLine();
            Console.Write(" Subtract approximating {1}: {0,6:#00.0%}", 0.0, apow);
            for (int i = 0; i < y.Length; i++)
            {
                y[i] -= Linear.Poly(pcoef[apow], x[i]);
                if (i % 3571 == 0)
                {
                    Console.Write("\b\b\b\b\b\b{0,6:#00.0%}", (double)i / (double)y.Length);
                }
            }
            Console.WriteLine("\b\b\b\b\b\b{0,6:#00.0%}", 1);

            // ДПФ
            Console.WriteLine();
            string watch_fmt = "{0} - {1} ({2,8:#00.000%})";
            string str = string.Format(watch_fmt, Utils.SpanToStr(TimeSpan.Zero), Utils.SpanToStr(TimeSpan.Zero), 0.0);
            watch_fmt = watch_fmt.PadLeft(str.Length + watch_fmt.Length, '\b');
            TimeSpan elapsed;
            TimeSpan remaining;
            IWindow winfunc = null;
            if (wf == WindowFunc.Hamming) winfunc = new FxMath.Windowed.Hamming();
            else if (wf == WindowFunc.Blackman) winfunc = new FxMath.Windowed.Blackman();
            else if (wf != WindowFunc.Rect) throw new ApplicationException("Ошибка в программе - неизвестная оконная функция " + Enum.GetName(typeof(WindowFunc), wf));
            IFourierTransform dft;
            int threads_count = 0;
            if (threads == "CUDA")
            {
                dft = new CudaDFourier(winfunc);
                Console.Write(" Fourier transform (CUDA): " + str);
            }
            else
            {
                dft = new DFourier(winfunc);
                if (!int.TryParse(threads, out threads_count))
                {
                    threads_count = 2;
                }
                Console.Write(" Fourier transform ({0} threads): " + str, threads_count);
            }
            DateTime fourier_start = DateTime.Now;
            dft.StartTransform(y, threads_count);
            while (!dft.IsDone)
            {
                Thread.Sleep(1000);
                if (dft.Count > 0)
                {
                    elapsed = DateTime.Now - fourier_start;
                    remaining = TimeSpan.FromTicks(elapsed.Ticks * y.Length / dft.Count);
                    Console.Write(watch_fmt, Utils.SpanToStr(elapsed), Utils.SpanToStr(remaining), (double)dft.Count / (double)y.Length);
                }
            }
            elapsed = DateTime.Now - fourier_start;
            remaining = TimeSpan.FromTicks(elapsed.Ticks * y.Length / dft.Count);
            Console.WriteLine(watch_fmt, Utils.SpanToStr(elapsed), Utils.SpanToStr(remaining), (double)dft.Count / (double)y.Length);

            float maxA = 0;
            float maxW = 0;
            Console.Write(" Processing fourier data: {0,6:#00.0%}", 0.0);
            Spectrogram[] spectr = new Spectrogram[y.Length]; // вообще имеет смысл делать только до N/2+1, но у нас есть еще исходный сигнал с убранным трендом
            for (int k = 0; k < spectr.Length; k++)
            {
                spectr[k] = new Spectrogram(dft.Coefs[k], k, x.Length);
                maxA = Math.Max(maxA, spectr[k].amplitude);
                maxW = Math.Max(maxW, spectr[k].power);
                if (k % 3571 == 0)
                {
                    Console.Write("\b\b\b\b\b\b{0,6:#00.0%}", (double)k / (double)spectr.Length);
                }
            }
            Console.WriteLine("\b\b\b\b\b\b{0,6:#00.0%}", 1);

            // записываем результат в файл
            using (StreamWriter dat = new StreamWriter(Utils.CorrectFilePath(pair + "." + candles_type + ".fourier.dat"), false, Encoding.ASCII))
            {
                dat.WriteLine("# Spectrogram {0} from {1} to {2} ({3} {4})", pair, first_date, last_date, x.Length, candles_type);
                dat.WriteLine("# Index-{0,-3}(1) Value(2) Frequency/{0,-3}(3)    Period(4)     Real(5)    Imaginary(6) Amplitude(7)     Power(8)    Energy(9) Phase(10)   Ampl-dB(11)  Power-dB(12)", candles_type);
                Console.Write(" Writing fourier data: {0,6:#00.0%}", 0.0);
                for (int k = 0; k < spectr.Length; k++)
                {
                    float AdB, WdB;
                    if (spectr[k].amplitude < Linear.Epsilon)
                    {
                        AdB = float.NegativeInfinity;
                        WdB = float.NegativeInfinity;
                    }
                    else
                    {
                        AdB = 20 * (float)Math.Log10(spectr[k].amplitude / maxA);
                        WdB = 10 * (float)Math.Log10(spectr[k].power / maxW);
                    }
                    string line_fmt;
                    if (pip == 4)
                    {
                        line_fmt = "{0,8}         {1,6:0.0000}     {2,12} {3,12} {4,13:0.000000e+00} {5,13:0.000000e+00}     {6,8:0.00000} {7,12} {8,12}      {9,4:##0} {10,13} {11,13}";
                    }
                    else
                    {
                        line_fmt = "{0,8}         {1,6:##0.00}     {2,12} {3,12} {4,13:0.000000e+00} {5,13:0.000000e+00}     {6,8:##0.000} {7,12} {8,12}      {9,4:##0} {10,13} {11,13}";
                    }
                    dat.WriteLine(line_fmt, x[k], y[k], spectr[k].frequency, spectr[k].term, spectr[k].coef.Re, spectr[k].coef.Im, spectr[k].amplitude, spectr[k].power, spectr[k].energy, spectr[k].phi, AdB, WdB);
                    if (k % 3571 == 0)
                    {
                        Console.Write("\b\b\b\b\b\b{0,6:#00.0%}", (double)k / (double)spectr.Length);
                    }
                }
                Console.WriteLine("\b\b\b\b\b\b{0,6:#00.0%}", 1);
            }
        }

        static string BuildFunctionString(int pow, float[] coef)
        {
            StringBuilder str = new StringBuilder();
            str.AppendFormat("f{0}(x) = ({1})", pow, coef[0]);
            for (int i = 1; i < coef.Length; i++)
            {
                if (coef[i] == 0)
                    continue;
                str.Append(" + ");
                str.AppendFormat("({0})*(x**{1})", coef[i], i);
            }
            return str.ToString();
        }
    }
}

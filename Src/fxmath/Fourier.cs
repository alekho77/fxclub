using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace FxMath
{
    public interface IFourierTransform
    {
        Complex[] Transform(float[] y);
        float[] Inverse(Complex[] c);

        void StartTransform(float[] y, int threads);
        bool IsDone { get; }
        int Count { get; }
        Complex[] Coefs { get; }
    }

    public interface IWindow
    {
        float Value(int n, int N);
        float[] Processing(float[] x, out float attenuation);
    }

    namespace Windowed
    {
        public abstract class BaseWindow : IWindow
        {
            public float[] Processing(float[] x, out float attenuation)
            {
                float[] g = new float[x.Length];
                attenuation = 0;
                for (int i = 0; i < x.Length; i++)
                {
                    float val = Value(i, x.Length);
                    g[i] = val * x[i];
                    attenuation += val;
                }
                attenuation /= x.Length;
                return g;
            }
            public abstract float Value(int n, int N);
        }
        public class Rectangle : BaseWindow // Исключельно в тестовых назначениях
        {
            public override float Value(int n, int N) { return 1; }
        }
        public class Hamming : BaseWindow // 42dB
        {
            public override float Value(int n, int N)
            {
                return 0.53836f - 0.46164f * (float)Math.Cos((2 * Math.PI * n) / (N - 1));
            }
        }
        public class Blackman : BaseWindow // 58dB
        {
            private const float a0 = 0.42f, a1 = 0.5f, a2 = 0.08f;
            public override float Value(int n, int N)
            {
                return a0 - a1 * (float)Math.Cos((2 * Math.PI * n) / (N - 1)) + a2 * (float)Math.Cos((4 * Math.PI * n) / (N - 1));
            }
        }
    }

    public class PartialDFourier
    {
        public static Complex Harmonic(float[] x, int k)
        {
            Complex coef = new Complex(0, 0);
            for (int n = 0; n < x.Length; n++)
            {
                coef += Complex.FromPolar(x[n], -(2 * (float)Math.PI * k * n) / x.Length);
            }
            return coef / x.Length;
        }
        public PartialDFourier(float[] x, int start, int end, ref Complex[] coef, float attenuation)
        {
            X = x;
            Start = start;
            End = end;
            FCoefs = coef;
            Attenuation = attenuation;
            thread = new Thread(this.Run);
            thread.Start();
        }
        public bool IsDone { get { return thread.ThreadState == ThreadState.Stopped; } }
        public int Count { get { return FCount; } }
        public Complex[] Coefs { get { return FCoefs; } }
        public readonly int Start, End;
        private void Run()
        {
            for (int k = Start; k <= End; k++, FCount++)
            {
                Coefs[k] = Harmonic(X, k) / Attenuation;
            }
        }
        private Thread thread;
        private Complex[] FCoefs;
        private int FCount = 0;
        private readonly float[] X;
        private readonly float Attenuation;
    }

    public class DFourier : IFourierTransform
    {
        public DFourier(IWindow window) { Window = window; } // null - rectangle window
        public Complex[] Transform(float[] y)
        {
            Complex[] coefs = new Complex[y.Length];
            float attenuation = 1;
            float[] g = Window == null ? y : Window.Processing(y, out attenuation);
            for (int k = 0; k < y.Length; k++)
            {
                coefs[k] = PartialDFourier.Harmonic(y, k) / attenuation;
            }
            return coefs;
        }
        public float[] Inverse(Complex[] c)
        {
            float[] Y = new float[c.Length];
            for (int k = 0; k < c.Length; k++)
            {
                Y[k] = 0;
                for (int n = 0; n < c.Length; n++)
                {
                    Y[k] += (c[n] * Complex.FromPolar(1, (2 * (float)Math.PI * k * n) / c.Length)).Re;
                }
            }
            return Y;
        }
        public void StartTransform(float[] y, int threads)
        {
            Partials = new PartialDFourier[threads];
            FCoefs = new Complex[y.Length];
            float attenuation = 1;
            float[] g = Window == null ? y : Window.Processing(y, out attenuation);
            for (int i = 0; i < threads; i++)
            {
                int start = (i * y.Length) / threads;
                int end = ((i + 1) * y.Length) / threads - 1;
                Partials[i] = new PartialDFourier(g, start, end, ref FCoefs, attenuation);
            }
        }
        public bool IsDone
        {
            get
            {
                for (int i = 0; i < Partials.Length; i++)
                {
                    if (!Partials[i].IsDone) return false;
                }
                return true;
            }
        }
        public int Count
        {
            get
            {
                int count = 0;
                for (int i = 0; i < Partials.Length; i++)
                {
                    count += Partials[i].Count;
                }
                return count;
            }
        }
        public Complex[] Coefs { get { return FCoefs; } }
        private PartialDFourier[] Partials = null;
        private Complex[] FCoefs = null;
        private IWindow Window;
    }
}

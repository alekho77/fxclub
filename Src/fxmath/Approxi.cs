using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FxMath
{
    public interface IPolynomial
    {
        float[] Transform(float[] x, float[] y);
        float Inverse(float[] coef, float x);
        float Condition { get; }
    }

    public class LinearInterpolation : IPolynomial
    {
        public float[] Transform(float[] x, float[] y)
        {
            float[,] A = new float[y.Length, y.Length];
            for (int i = 0; i < y.Length; i++)
            {
                for (int j = 0; j < y.Length; j++)
                {
                    A[i, j] = Linear.Pow(x[i], j);
                }
            }
            return Linear.LinSolve(A, y, out Cond);
        }
        public float Inverse(float[] coef, float x)
        {
            return Linear.Poly(coef, x);
        }
        public float Condition { get { return Cond; } }
        private float Cond = 0;
    }

    public class LinearApproximation : IPolynomial
    {
        public LinearApproximation(int p)
        {
            pow = p + 1;
        }
        public float[] Transform(float[] x, float[] y)
        {
            float[,] A = new float[pow, pow];
            float[] Y = new float[pow];
            for (int i = 0; i < pow; i++)
            {
                for (int j = 0; j < pow; j++)
                {
                    A[i, j] = 0;
                    for (int k = 0; k < x.Length; k++)
                    {
                        A[i, j] += Linear.Pow(x[k], i + j);
                    }
                }
                Y[i] = 0;
                for (int k = 0; k < x.Length; k++)
                {
                    Y[i] += y[k] * Linear.Pow(x[k], i);
                }
            }
            return Linear.LinSolve(A, Y, out Cond);
        }
        public float Inverse(float[] coef, float x)
        {
            return Linear.Poly(coef, x);
        }
        public float Condition { get { return Cond; } }
        private float Cond = 0;
        private readonly int pow;
    }
}

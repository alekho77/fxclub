using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FxMath
{
    public struct Complex : IEquatable<Complex>//, IFormattable
    {
        public float Re, Im;
        public Complex(float real, float imaginary)
        {
            Re = real;
            Im = imaginary;
        }
        public float Magnitude { get { return (float)Math.Sqrt(Re * Re + Im * Im); } }
        public float Phase { get { return (float)Math.Atan2(Im, Re); } }
        public Complex Conjugate { get { return new Complex(Re, -Im); } }
        public override string ToString()
        {
            return string.Format("{0}; {1}", Re, Im);
        }
        public bool Equals(Complex other)
        {
            return Re.Equals(other.Re) && Im.Equals(other.Im);
        }
        public override int GetHashCode()
        {
            return Re.GetHashCode() ^ Im.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj is Complex)
            {
                return Equals((Complex)obj);
            }
            return false;
        }

        public static Complex FromPolar(float magnitude, float phase)
        {
            return new Complex(magnitude * (float)Math.Cos(phase), magnitude * (float)Math.Sin(phase));
        }
        public static Complex operator +(Complex left, Complex right)
        {
            return new Complex(left.Re + right.Re, left.Im + right.Im);
        }
        public static Complex operator -(Complex left, Complex right)
        {
            return new Complex(left.Re - right.Re, left.Im - right.Im);
        }
        public static Complex operator *(Complex left, Complex right)
        {
            return new Complex(left.Re * right.Re - left.Im * right.Im, left.Re * right.Im + left.Im * right.Re);
        }
        public static Complex operator *(Complex left, float right)
        {
            return new Complex(left.Re * right, left.Im * right);
        }
        public static Complex operator /(Complex left, Complex right)
        {
            float Mag2 = right.Re * right.Re + right.Im * right.Im;
            return new Complex((left.Re * right.Re + left.Im * right.Im) / Mag2, (left.Im * right.Re - left.Re * right.Im) / Mag2);
        }
        public static Complex operator /(Complex left, float right)
        {
            return new Complex(left.Re / right, left.Im / right);
        }
        public static bool operator ==(Complex left, Complex right)
        {
            return left.Re == right.Re && left.Im == right.Im;
        }
        public static bool operator !=(Complex left, Complex right)
        {
            return left.Re != right.Re || left.Im != right.Im;
        }
        public static Complex operator -(Complex value)
        {
            return new Complex(-value.Re, -value.Im);
        }
    }

    public static class Linear
    {
        public const float Epsilon = 0.0000002384185791015625f; //2.2204460492503130808472633361816e-16; // отн.погрешность представления в double, равная 2^(1-53)
        public static float Pow(float x, int p)
        {
            float y = 1;
            if (p >= 0)
            {
                for (int i = 0; i < p; i++)
                {
                    y *= x;
                }
            } 
            else
            {
                for (int i = 0; i > p; i--)
                {
                    y /= x;
                }
            }
            return y;
        }
        public static float Poly(float[] coef, float x)
        {
            float result = 0;
            for (int i = 0; i < coef.Length; i++)
            {
                result += coef[i] * Pow(x, i);
            }
            return result;
        }
        public static class Swap<T>
        {
            public static void swap(ref T a, ref T b)
            {
                T temp = a;
                a = b;
                b = temp;
            }
        }
        public static float[] LinSolve(float[,] A, float[] B, out float cond)
        {
            // Метод Гаусса — Жордана
            
            // проверка размеров матрицы A
            int size = B.Length;
            if (A.GetLength(0) != A.GetLength(1))
            {
                throw new ApplicationException("The width of the matrix [A] is not equal its height.");
            }
            else if (A.Length != size * size)
            {
                throw new ApplicationException("The size of the matrix [A] is not equal size of the vector [B].");
            }
            
            float[][] Aw = new float[size][]; // рабочая исходная матрица
            float[][] Ai = new float[size][]; // обратная марица
            float[] Bw = (float[])B.Clone();
            
            // вычиление нормы матрицы A, копирование ее в рабочую, заполнение обратной единичной матрицей
            float na = 0; // норма матрицы A
            for (int i = 0; i < size; i++)
            {
                Aw[i] = new float[size];
                Ai[i] = new float[size];
                float n = 0;
                for (int j = 0; j < size; j++)
                {
                    Aw[i][j] = A[i, j];
                    Ai[i][j] = 0;
                    n += Math.Abs(A[i, j]);
                }
                Ai[i][i] = 1;
                na = Math.Max(na, n);
            }
            
            // прямой ход
            for (int j = 0; j < size; j++)
            {
                // идем по столбцу (j)
                for (int i = j + 1; i < size; i++)
                {
                    // находим в этом столбце максимальный коэффициент
                    if (Math.Abs(Aw[j][j]) < Math.Abs(Aw[i][j]))
                    {
                        // переставляем эту строку на вверх на место строки (j)
                        Swap<float[]>.swap(ref Aw[j], ref Aw[i]);
                        Swap<float[]>.swap(ref Ai[j], ref Ai[i]);
                        Swap<float>.swap(ref Bw[j], ref Bw[i]);
                    }
                }
                // проверка вырожденности
                // !ВАЖНО! сичтаем что данные отнормированные вблизи 1
                if (Math.Abs(Aw[j][j]) < Linear.Epsilon)
                {
                    cond = 0;
                    return null;
                }
                // идем по строчкам ниже (j)
                for (int i = j + 1; i < size; i++)
                {
                    // пересчитываем все коэффициенты
                    float c = -Aw[i][j] / Aw[j][j];
                    for (int k = 0; k < size; k++)
                    {
                        Aw[i][k] += c * Aw[j][k];
                        Ai[i][k] += c * Ai[j][k];
                    }
                    Bw[i] += c * Bw[j];
                }
            }
            // обратный ход
            for (int i = size - 1; i >= 0; i--)
            {
                // строку (i) делим на коэффициент главной диагонали, чтобы привести его к 1
                float m = Aw[i][i];
                Bw[i] /= m;
                for (int j = 0; j < size; j++)
                {
                    Aw[i][j] /= m;
                    Ai[i][j] /= m;
                }
                // идем по строчкам выше (i)
                for (int k = i - 1; k >= 0; k--)
                {
                    // пересчитываем все коэффициенты
                    float c = -Aw[k][i];
                    for (int j = 0; j < size; j++)
                    {
                        Aw[k][j] += c * Aw[i][j];
                        Ai[k][j] += c * Ai[i][j];
                    }
                    Bw[k] += c * Bw[i];
                }
            }

            float nt = 0; // норма обратной матрицы A
            for (int i = 0; i < size; i++)
            {
                float n = 0;
                for (int j = 0; j < size; j++)
                {
                    n += Math.Abs(Ai[i][j]);
                }
                nt = Math.Max(nt, n);
            }
            cond = na * nt;

            return Bw;
        }
    }
}

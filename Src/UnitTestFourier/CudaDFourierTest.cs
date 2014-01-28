using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FxMath;
using FxCuda;
using System.Threading;

namespace UnitTestFourier
{
    /// <summary>
    /// Summary description for CudaDFourierTest
    /// </summary>
    [TestClass]
    public class CudaDFourierTest
    {
        public CudaDFourierTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        private delegate float testfunc(float x);

        
        [TestMethod()]
        public void CudaDFTTransformTest()
        {
            testfunc[] funcs = { x => x, x => (float)Math.Cos(x) };
            float[] p = new float[] { 1, (float)Math.PI/2 };
            int[] N = { 10, 100, 300, 1000, 10000, 100000 };
            int[][] k =
            {
                new int[] { 0,1,2,4,5,6,8,9 }, // N=10
                new int[] { 0,1,2,33,49,50,51,66,98,99 }, // N=100
                new int[] { 0,1,2,33,49,50,51,66,98,99 }, // N=300
                new int[] { 0,1,2,333,499,500,501,666,998,999 }, // N=1000
                new int[] { 0,1,2,3333,4999,5000,5001,6666,9998,9999 }, // N=10000
                new int[] { 0,1,2,33333,49999,50000,50001,66666,99998,99999 } // N=100000
            };
            IFourierTransform target = new CudaDFourier(new FxMath.Windowed.Rectangle());
            for (int f = 0; f < funcs.Length; f++)
            {
                for (int n = 0; n < N.Length; n++)
                {
                    float energy_time = 0;
                    float[] y = new float[N[n]];
                    for (int i = 0; i < N[n]; i++)
                    {
                        y[i] = funcs[f](2.0f * p[f] / N[n] * i - p[f]);
                        energy_time += y[i] * y[i];
                    }

                    Dictionary<int, Complex> expected = new Dictionary<int, Complex>();
                    for (int i = 0; i < k[n].Length; i++)
                    {
                        expected[i] = PartialDFourier.Harmonic(y, k[n][i]);
                    }

                    DateTime start = DateTime.Now;
                    Complex[] actual = target.Transform(y);
                    TimeSpan dt = DateTime.Now - start;

                    float energy_freq = 0;
                    for (int i = 0; i < actual.Length; i++)
                    {
                        energy_freq += actual[i].Re * actual[i].Re + actual[i].Im * actual[i].Im;
                    }
                    energy_freq *= actual.Length;

                    float err = N[n] * Linear.Epsilon;
                    TestContext.WriteLine("Func[{0}], N={1}, time={2}, err={3:E0}", f, N[n], dt, err);
                    for (int i = 0; i < k[n].Length; i++)
                    {
                        float eps_re = (float)Math.Abs(expected[i].Re - actual[k[n][i]].Re);
                        float eps_im = (float)Math.Abs(expected[i].Im - actual[k[n][i]].Im);
                        TestContext.WriteLine("    [{0}] eps-[{1:E0}, {2:E0}]", k[n][i], eps_re, eps_im);
                        Assert.IsTrue(eps_re < err, string.Format("[{2},{3},{4}] Re eps = {0:E0} > {1:E0}", eps_re, err, f, n, i));
                        Assert.IsTrue(eps_im < err, string.Format("[{2},{3},{4}] Im eps = {0:E0} > {1:E0}", eps_im, err, f, n, i));
                    }
                    float eps_energy = (float)Math.Abs(energy_freq - energy_time);
                    Assert.IsTrue(eps_energy < N[n] * err, string.Format("[{2},{3}] Energy eps = {0:E0} > {1:E0}", eps_energy, N[n] * err, f, n));
                }
            }
        }

//         [TestMethod()]
//         public void CudaDFTInverseTest()
//         {
//             IFourierTransform target = new CudaDFourier(null);
// 
//         }

        [TestMethod()]
        public void CudaStartTransformTest()
        {
            testfunc[] funcs = { x => x, x => (float)Math.Cos(x) };
            float[] p = new float[] { 1, (float)Math.PI / 2 };
            int[] N = { 10, 100, 1000, 10000, 100000 };
            int[][] k =
            {
                new int[] { 0,1,2,4,5,6,8,9 }, // N=10
                new int[] { 0,1,2,33,49,50,51,66,98,99 }, // N=100
                new int[] { 0,1,2,333,499,500,501,666,998,999 }, // N=1000
                new int[] { 0,1,2,3333,4999,5000,5001,6666,9998,9999 }, // N=10000
                new int[] { 0,1,2,33333,49999,50000,50001,66666,99998,99999 } // N=100000
            };
            IFourierTransform target = new CudaDFourier(new FxMath.Windowed.Rectangle());
            for (int f = 0; f < funcs.Length; f++)
            {
                for (int n = 0; n < N.Length; n++)
                {
                    float[] y = new float[N[n]];
                    for (int i = 0; i < N[n]; i++)
                    {
                        y[i] = funcs[f](2.0f * p[f] / N[n] * i - p[f]);
                    }

                    Dictionary<int, Complex> expected = new Dictionary<int, Complex>();
                    for (int i = 0; i < k[n].Length; i++)
                    {
                        expected[i] = PartialDFourier.Harmonic(y, k[n][i]);
                    }

                    DateTime start = DateTime.Now;
                    target.StartTransform(y, 0);
                    while (!target.IsDone)
                        Thread.Sleep(1);
                    TimeSpan dt = DateTime.Now - start;

                    Assert.AreEqual(target.Count, target.Coefs.Length);
                    Assert.AreEqual(N[n], target.Coefs.Length);

                    float err = N[n] * Linear.Epsilon;
                    TestContext.WriteLine("Func[{0}], N={1}, time={2}, err={3:E0}", f, N[n], dt, err);
                    for (int i = 0; i < k[n].Length; i++)
                    {
                        float eps_re = (float)Math.Abs(expected[i].Re - target.Coefs[k[n][i]].Re);
                        float eps_im = (float)Math.Abs(expected[i].Im - target.Coefs[k[n][i]].Im);
                        TestContext.WriteLine("    [{0}] eps-[{1:E0}, {2:E0}]", k[n][i], eps_re, eps_im);
                        Assert.IsTrue(eps_re < err, string.Format("[{2},{3},{4}] Re eps = {0:E0} > {1:E0}", eps_re, err, f, n, i));
                        Assert.IsTrue(eps_im < err, string.Format("[{2},{3},{4}] Im eps = {0:E0} > {1:E0}", eps_im, err, f, n, i));
                    }
                }
            }
        }
    }
}

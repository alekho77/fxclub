using FxMath;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
namespace UnitTestFourier
{
    
    
    /// <summary>
    ///This is a test class for DFourierTest and is intended
    ///to contain all DFourierTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DFourierTest
    {


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
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for Transform
        ///</summary>
        [TestMethod()]
        public void DFTTransformTest()
        {
            IFourierTransform target = new DFourier(null);
            float[][] y = {
                               new float[] {1,1,1,1}, // f(x) = 1
                               new float[] {-1,-0.5f,0,0.5f}, // f(x) = x
                               new float[] {1,0.25f,0,0.25f}, // f(x) = x^2
                               new float[] {0.36787944117144f,0.60653065971263f,1,1.648721270700128f}, // f(x) = exp(x)
                               new float[] {0,0.70710678118655f,1,0.70710678118655f}  // f(x) = cos(x)               
                           };
            Complex[][] expected = {
                                       new Complex[] {new Complex(1,0),new Complex(0,0),new Complex(0,0),new Complex(0,0)}, // f(x) = 1
                                       new Complex[] {new Complex(-0.25f,0),new Complex(-0.25f,0.25f),new Complex(-0.25f,0),new Complex(-0.25f,-0.25f)}, // f(x) = x
                                       new Complex[] {new Complex(0.375f,0),new Complex(0.25f,0),new Complex(0.125f,0),new Complex(0.25f,0)}, // f(x) = x^2
                                       new Complex[] {new Complex(0.90578284289605f,0),new Complex(-0.15803013970714f,0.26054765274687f),new Complex(-0.22184312231033f,0),new Complex(-0.15803013970714f,-0.26054765274687f)}, // f(x) = exp(x)
                                       new Complex[] {new Complex(0.60355339059327f,0),new Complex(-0.25f,0),new Complex(-0.10355339059327f,0),new Complex(-0.25f,0)}  // f(x) = cos(x)
                                   };
            for (int f = 0; f < y.Length; f++)
            {
                Complex[] actual = target.Transform(y[f]);
                Assert.AreEqual(expected[f].Length, actual.Length);
                float err = 4 * expected[f].Length * expected[f].Length * Linear.Epsilon;
                float energy_time = 0;
                float energy_freq = 0;
                for (int i = 0; i < expected[f].Length; i++)
                {
                    float eps_re = (float)Math.Abs(expected[f][i].Re - actual[i].Re);
                    float eps_im = (float)Math.Abs(expected[f][i].Im - actual[i].Im);
                    energy_time += y[f][i] * y[f][i];
                    energy_freq += actual[i].Re * actual[i].Re + actual[i].Im * actual[i].Im;
                    Assert.IsTrue(eps_re < err, string.Format("[{2},{3}] Re eps = {0:E0} > {1:E0}", eps_re, err, f, i));
                    Assert.IsTrue(eps_im < err, string.Format("[{2},{3}] Im eps = {0:E0} > {1:E0}", eps_im, err, f, i));
                }
                energy_freq *= actual.Length;
                float eps_energy = (float)Math.Abs(energy_freq - energy_time);
                Assert.IsTrue(eps_energy < err, string.Format("[{2}] Energy eps = {0:E0} > {1:E0}", eps_energy, err, f));
            }
        }

        /// <summary>
        ///A test for Inverse
        ///</summary>
        [TestMethod()]
        public void DFTInverseTest()
        {
            IFourierTransform target = new DFourier(null);
            Complex[][] c = {
                               new Complex[] {new Complex(1,0),new Complex(0,0),new Complex(0,0),new Complex(0,0)}, // f(x) = 1
                               new Complex[] {new Complex(-0.25f,0),new Complex(-0.25f,0.25f),new Complex(-0.25f,0),new Complex(-0.25f,-0.25f)}, // f(x) = x
                               new Complex[] {new Complex(0.375f,0),new Complex(0.25f,0),new Complex(0.125f,0),new Complex(0.25f,0)}, // f(x) = x^2
                               new Complex[] {new Complex(0.90578284289605f,0),new Complex(-0.15803013970714f,0.26054765274687f),new Complex(-0.22184312231033f,0),new Complex(-0.15803013970714f,-0.26054765274687f)}, // f(x) = exp(x)
                               new Complex[] {new Complex(0.60355339059327f,0),new Complex(-0.25f,0),new Complex(-0.10355339059327f,0),new Complex(-0.25f,0)}  // f(x) = cos(x)
                            };
            float[][] expected = {
                                       new float[] {1,1,1,1}, // f(x) = 1
                                       new float[] {-1,-0.5f,0,0.5f}, // f(x) = x
                                       new float[] {1,0.25f,0,0.25f}, // f(x) = x^2
                                       new float[] {0.36787944117144f,0.60653065971263f,1,1.648721270700128f}, // f(x) = exp(x)
                                       new float[] {0,0.70710678118655f,1,0.70710678118655f}  // f(x) = cos(x)               
                                  };
            for (int f = 0; f < c.Length; f++)
            {
                float[] actual = target.Inverse(c[f]);
                Assert.AreEqual(expected[f].Length, actual.Length);
                float err = 6 * expected[f].Length * expected[f].Length * Linear.Epsilon;
                for (int i = 0; i < expected[f].Length; i++)
                {
                    float eps = (float)Math.Abs(expected[f][i] - actual[i]);
                    Assert.IsTrue(eps < err, string.Format("[{2},{3}] Im eps = {0:E0} > {1:E0}", eps, err, f, i));
                }
            }
        }

        /// <summary>
        ///A test for StartTransform
        ///</summary>
        [TestMethod()]
        public void StartTransformTest()
        {
            IFourierTransform target = new DFourier(null);
            float[] y = {0.36787944117144f,0.60653065971263f,1,1.648721270700128f}; // f(x) = exp(x)
            Complex[] expected = {new Complex(0.90578284289605f,0),new Complex(-0.15803013970714f,0.26054765274687f),new Complex(-0.22184312231033f,0),new Complex(-0.15803013970714f,-0.26054765274687f)}; // f(x) = exp(x)
            target.StartTransform(y, 2);
            while(!target.IsDone)
                Thread.Sleep(1);
            Assert.AreEqual(target.Count, target.Coefs.Length);
            Assert.AreEqual(expected.Length, target.Coefs.Length);
            float err = 4 * expected.Length * expected.Length * Linear.Epsilon;
            for (int i = 0; i < expected.Length; i++)
            {
                float eps_re = (float)Math.Abs(expected[i].Re - target.Coefs[i].Re);
                float eps_im = (float)Math.Abs(expected[i].Im - target.Coefs[i].Im);
                Assert.IsTrue(eps_re < err, string.Format("[{2}] Re eps = {0:E0} > {1:E0}", eps_re, err, i));
                Assert.IsTrue(eps_im < err, string.Format("[{2}] Im eps = {0:E0} > {1:E0}", eps_im, err, i));
            }
        }
    }
}

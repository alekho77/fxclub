using FxMath;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
namespace UnitTestFourier
{
    
    
    /// <summary>
    ///This is a test class for FxMathTest and is intended
    ///to contain all FxMathTest Unit Tests
    ///</summary>
    [TestClass()]
    public class FxMathTest
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
        ///A test for Poly
        ///</summary>
        [TestMethod()]
        public void PolyTest()
        {
            for (int i = 0; i < 50; i++)
            {
                float[] coef = new float[i + 1];
                for (int k = 0; k < coef.Length; k++)
                {
                    coef[k] = (float)(Linear.Pow(-1, k) * Math.Exp(1 - k));
                }
                float x = (float)Math.Exp(1);
                float expected = ((i + 1) % 2) * (float)Math.Exp(1);
                float actual = Linear.Poly(coef, x);
                TestContext.WriteLine("Test P{0} = {1} diff {2}", i, actual, Math.Abs(actual - expected));
                Assert.IsTrue(Math.Abs(expected - actual) < (i + 1) * Linear.Epsilon);
            }
        }

        /// <summary>
        ///A test for Pow
        ///</summary>
        [TestMethod()]
        public void PowTest()
        {
            float x = -(float)Math.PI;
            int[] p = {-3, 0, 1, 2};
            float[] expected = {1/(x*x*x), 1, x, x*x};
            for (int i = 0; i < expected.Length; i++)
            {
                float actual = Linear.Pow(x, p[i]);
                Assert.IsTrue(Math.Abs(expected[i] - actual) < Linear.Epsilon);
            }
        }

        /// <summary>
        ///A test for LinSolve
        ///</summary>
        [TestMethod()]
        public void LinSolveTest()
        {
            float[][,] A = {
                                new float[,] { { 2,  1, -1},
                                                {-3, -1,  2},
                                                {-2,  1,  2} },
                                new float[,] { { 2,  1, -1},
                                                { 3,  1, -2},
                                                { 1,  0,  1} },
                                new float[,] { {100, 99},
                                                { 99, 98} },
                                new float[,] { {1.00f, .99f},
                                                { .99f, .98f} }
                             };
            float[][] b = { 
                            new float[] {  8,
                                          -11,
                                           -3 },
                            new float[] {  2,
                                            3,
                                            3 },
                            new float[] { 199,
                                           197 },
                            new float[] { 1.9899f,
                                           1.9701f }
                           };
            float[][] expected = {
                                    new float[] {2, 3, -1},
                                    new float[] {2, -1, 1},
                                    new float[] {1 , 1},
                                    new float[] {2.97f , -0.99f}
                                  };
            float[] expected_cond = { 60, 27, 39601, 39601 };
            for (int k = 0; k < expected_cond.Length; k++)
            {
                float cond;
                float[] actual = Linear.LinSolve(A[k], b[k], out cond);
                Assert.IsTrue(Math.Abs(cond - expected_cond[k]) < (Linear.Epsilon * expected_cond[k] * expected_cond[k] * A[k].Length));
                for (int i = 0; i < expected[k].Length; i++)
                {
                    string str = "";
                    for (int j = 0; j < A[k].GetLength(1); j++)
                    {
                        str += A[k][i, j].ToString() + " ";
                    }
                    TestContext.WriteLine("{0} | {1} => x[{2}]={3} diff {4}", str, b[k][i], i, actual[i], Math.Abs(actual[i] - expected[k][i]));
                    Assert.IsTrue(Math.Abs(expected[k][i] - actual[i]) < (Linear.Epsilon * expected_cond[k]));
                }
                TestContext.WriteLine("cond = {0}", cond);
            }
        }
    }
}

using FxMath;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
namespace UnitTestFourier
{
    
    
    /// <summary>
    ///This is a test class for LinearInterpolationTest and is intended
    ///to contain all LinearInterpolationTest Unit Tests
    ///</summary>
    [TestClass()]
    public class LinearInterpolationTest
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
        public void InterTransformTest()
        {
            LinearInterpolation target = new LinearInterpolation();
            float[][] expected = {
                                      new float[1] { 1 },
                                      new float[2] { -1, 2 },
                                      new float[3] { -2.0f/3.0f, -1.0f/3.0f, 2.0f },
                                      new float[4] { 2.0f/9.0f, -5.0f/9.0f, -1, 2.0f }
                                  };
            foreach (float[] coef in expected)
            {
                float[] x = new float[coef.Length];
                float[] y = new float[coef.Length];
                float dx = coef.Length == 1 ? 0 : 2.0f / (coef.Length - 1);
                for (int i = 0; i < x.Length; i++)
                {
                    x[i] = -1.0f + i * dx;
                    y[i] = Linear.Poly(coef, x[i]);
                }
                float[] actual = target.Transform(x, y);
                Assert.AreEqual(actual.Length, coef.Length);
                for (int i = 0; i < coef.Length; i++)
                {
                    Assert.IsTrue(Math.Abs(coef[i] - actual[i]) < target.Condition * Linear.Epsilon);
                }
            }
        }
    }
}

using FxMath;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
namespace UnitTestFourier
{
    
    
    /// <summary>
    ///This is a test class for LinearApproximationTest and is intended
    ///to contain all LinearApproximationTest Unit Tests
    ///</summary>
    [TestClass()]
    public class LinearApproximationTest
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
        public void ApproxTransformTest()
        {
            float[] x = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            float[] y = { 0.382490273f, 0.029568921f, -0.437079361f, -0.242043006f, -0.519238821f, -0.341422037f, 0.745003113f, -1.388325135f, -0.979299096f, 0.840453459f };
            float[][] expected = {
                                      new float[1] { -0.190989169f },
                                      new float[2] { -0.0390664632f , -0.027622310145455f },
                                      new float[3] { 0.72738100755f, -0.41084604552045f, 0.034838521397727f },
                                      new float[4] { 0.16204805683333f , 0.090573972539435f , -0.073879353740095f, 0.006588962129565f },
                                      new float[5] { 3.159843673583218f, -3.752753741242501f, 1.332429196075541f, -0.18557742355953f, 0.0087348357131407f }
                                  };
            for (int p = 0; p < expected.Length; p++)
            {
                IPolynomial target = new LinearApproximation(p);
                float[] actual = target.Transform(x, y);
                Assert.AreEqual(expected[p].Length, actual.Length);
                for (int i = 0; i < expected[p].Length; i++)
                {
                    Assert.IsTrue(Math.Abs(expected[p][i] - actual[i]) < Math.Sqrt(target.Condition) * Linear.Epsilon);
                }
            }
        }
    }
}

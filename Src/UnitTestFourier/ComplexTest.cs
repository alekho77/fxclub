using FxMath;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
namespace UnitTestFourier
{
    
    
    /// <summary>
    ///This is a test class for ComplexTest and is intended
    ///to contain all ComplexTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ComplexTest
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
        ///A test for Phase
        ///</summary>
        [TestMethod()]
        public void PhaseTest()
        {
            Complex[] target = { new Complex(1, 1), new Complex(-1, 1), new Complex(-1, -1), new Complex(1, -1) };
            float[] expected = { (float)(Math.PI / 4), (float)(3 * Math.PI / 4), (float)(- 3 * Math.PI / 4), (float)(- Math.PI / 4) };
            for (int i = 0; i < target.Length; i++)
            {
                float eps = Math.Abs(target[i].Phase - expected[i]);
                TestContext.WriteLine("Test [{0}].Phase = '{1}' diff {2}", target[i].ToString(), target[i].Phase, eps);
                Assert.IsTrue(eps <= Linear.Epsilon, string.Format("Test [{0}].Phase = '{1}' diff {2}", target[i].ToString(), target[i].Phase, eps));
            }
        }

        /// <summary>
        ///A test for Magnitude
        ///</summary>
        [TestMethod()]
        public void MagnitudeTest()
        {
            Complex[] target = { new Complex(1, 1), new Complex(-1, 1), new Complex(-1, -1), new Complex(1, -1) };
            float expected = (float)Math.Sqrt(2);
            for (int i = 0; i < target.Length; i++)
            {
                float eps = (float)Math.Abs(target[i].Magnitude - expected);
                TestContext.WriteLine("Test [{0}].Magnitude = '{1}' diff {2}", target[i].ToString(), target[i].Magnitude, eps);
                Assert.IsTrue(eps <= Linear.Epsilon, string.Format("Test [{0}].Magnitude = '{1}' diff {2}", target[i].ToString(), target[i].Magnitude, eps));
            }
        }

        /// <summary>
        ///A test for Conjugate
        ///</summary>
        [TestMethod()]
        public void ConjugateTest()
        {
            Complex[] target = { new Complex(1, 1), new Complex(-1, 1) };
            Complex[] expected = { new Complex(1, -1), new Complex(-1, -1)};
            for (int i = 0; i < target.Length; i++)
            {
                float eps = (float)Math.Abs(target[i].Conjugate.Re - expected[i].Re) + Math.Abs(target[i].Conjugate.Im - expected[i].Im);
                TestContext.WriteLine("Test [{0}].Conjugate = '{1}' diff {2}", target[i].ToString(), target[i].Conjugate.ToString(), eps);
                Assert.IsTrue(eps <= Linear.Epsilon, string.Format("Test [{0}].Conjugate = '{1}' diff {2}", target[i].ToString(), target[i].Conjugate.ToString(), eps));
            }
        }

        /// <summary>
        ///A test for op_UnaryNegation
        ///</summary>
        [TestMethod()]
        public void op_UnaryNegationTest()
        {
            Complex value = new Complex(1, 1);
            Complex expected = new Complex(-1, -1);
            Complex actual;
            actual = -(value);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Subtraction
        ///</summary>
        [TestMethod()]
        public void op_SubtractionTest()
        {
            Complex left = new Complex(1, -1); // TODO: Initialize to an appropriate value
            Complex right = new Complex(-1, 1); // TODO: Initialize to an appropriate value
            Complex expected = new Complex(2, -2); // TODO: Initialize to an appropriate value
            Complex actual;
            actual = (left - right);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Multiply
        ///</summary>
        [TestMethod()]
        public void op_MultiplyTest()
        {
            Complex[] left = { new Complex(1, 1), new Complex(-1, 1), new Complex(-1, 1), new Complex(-1, -1) };
            Complex[] right = { new Complex(1, -1), new Complex(1, 1), new Complex(-1, -1), new Complex(1, -1) };
            Complex[] expected = { new Complex(2, 0), new Complex(-2, 0), new Complex(2, 0), new Complex(-2, 0) };
            for (int i = 0; i < expected.Length; i++)
            {
                Complex actual;
                actual = (left[i] * right[i]);
                TestContext.WriteLine("Test [{0}] * [{1}] = '[{2}]'", left[i].ToString(), right[i].ToString(), actual.ToString());
                Assert.AreEqual(expected[i], actual);
            }
        }

        /// <summary>
        ///A test for op_Inequality
        ///</summary>
        [TestMethod()]
        public void op_InequalityTest()
        {
            Complex left = new Complex(10.0f / 3.0f, -(float)Math.Sqrt(2));
            Complex right = new Complex(3.3333333333333333333333333333333f, -1.4142135623730950488016887242097f);
            bool expected = false;
            bool actual;
            actual = (left != right);
            TestContext.WriteLine("Test [{0}] != [{1}] is false, diff {2}", left.ToString(), right.ToString(), (left - right).ToString());
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Equality
        ///</summary>
        [TestMethod()]
        public void op_EqualityTest()
        {
            Complex left = new Complex(-10.0f / 3.0f, (float)Math.Sqrt(2));
            Complex right = new Complex(-3.3333333333333333333333333333333f, 1.4142135623730950488016887242097f);
            bool expected = true;
            bool actual;
            actual = (left == right);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for op_Division
        ///</summary>
        [TestMethod()]
        public void op_DivisionTest()
        {
            Complex[] left = { new Complex((float)Math.Sqrt(2), (float)Math.Sqrt(2)), new Complex(-(float)Math.Sqrt(2), (float)Math.Sqrt(2)), new Complex(-(float)Math.Sqrt(2), (float)Math.Sqrt(2)), new Complex(-(float)Math.Sqrt(2), -(float)Math.Sqrt(2)) };
            Complex[] right = { new Complex(1, -1), new Complex(1, 1), new Complex(-1, -1), new Complex(1, -1) };
            Complex[] expected = { new Complex(0, (float)Math.Sqrt(2)), new Complex(0, (float)Math.Sqrt(2)), new Complex(0, -(float)Math.Sqrt(2)), new Complex(0, -(float)Math.Sqrt(2)) };
            for (int i = 0; i < expected.Length; i++)
            {
                Complex actual;
                actual = (left[i] / right[i]);
                TestContext.WriteLine("Test [{0}] / [{1}] = '[{2}]'", left[i].ToString(), right[i].ToString(), actual.ToString());
                Assert.AreEqual(expected[i], actual);
            }
        }

        /// <summary>
        ///A test for op_Addition
        ///</summary>
        [TestMethod()]
        public void op_AdditionTest()
        {
            Complex left = new Complex(1, -1);
            Complex right = new Complex(-1, 1);
            Complex expected = new Complex(0, 0);
            Complex actual;
            actual = (left + right);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for FromPolar
        ///</summary>
        [TestMethod()]
        public void FromPolarTest()
        {
            float magnitude = (float)Math.Sqrt(2);
            float phase = (float)(-135.0 * Math.PI / 180.0);
            Complex expected = new Complex(-1, -1);
            Complex actual;
            actual = Complex.FromPolar(magnitude, phase);
            float eps = (float)Math.Abs(actual.Re - expected.Re) + (float)Math.Abs(actual.Im - expected.Im);
            TestContext.WriteLine("Test [{0}*exp({1}*i)] = [{2}], diff {3}", magnitude, phase, actual, eps);
            Assert.IsTrue(eps <= Linear.Epsilon, string.Format("Test [{0}*exp({1}*i)] = [{2}], diff {3}", magnitude, phase, actual, eps));
        }
    }
}

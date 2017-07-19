using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reference.Tests
{
    [TestClass]
    public class InvalidReferenceExceptionTest
    {
        [TestMethod]
        public void InvalidReferenceException_Constructor()
        {
            var ex1 = new InvalidReferenceException();
            Assert.AreEqual($"Exception of type '{ex1.GetType().FullName}' was thrown.", ex1.Message);
            Assert.AreEqual(null, ex1.InnerException);

            var ex2 = new InvalidReferenceException("test");
            Assert.AreEqual("test", ex2.Message);
            Assert.AreEqual(null, ex2.InnerException);

            var ex3 = new InvalidReferenceException("message", ex2);
            Assert.AreEqual("message", ex3.Message);
            Assert.AreEqual(ex2, ex3.InnerException);
        }
    }
}

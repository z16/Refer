using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Refer.Tests
{
    [TestClass]
    public class InvalidReferenceExceptionTest
    {
        [TestMethod]
        public void InvalidReferenceException_Constructor()
        {
            var ex = new Exception();

            var test = new InvalidReferenceException("message", ex);
            Assert.AreEqual("message", test.Message);
            Assert.AreEqual(ex, test.InnerException);
        }
    }
}

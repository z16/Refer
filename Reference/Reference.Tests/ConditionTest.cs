using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reference.Tests
{
    [TestClass]
    public class ConditionTest
    {
        private class Foo
        {
            internal Int32 Value;
        }

        [TestMethod]
        public void Condition_Default()
        {
            var foo = new Foo
            {
                Value = 0,
            };

            var condition = new Condition<Foo>(f => f.Value == 2, foo);
            for (; foo.Value < 4; ++foo.Value)
            {
                if (foo.Value == 2)
                {
                    Assert.IsTrue(condition);
                }
                else
                {
                    Assert.IsFalse(condition);
                }
            }
        }

        [TestMethod]
        public void Condition_FactoryConstructor()
        {
            var property = Condition.Create(i => i < 3, 5);
            Assert.IsFalse(property.Value);
        }

        [TestMethod]
        public void Condition_ExtensionMethodConstructor()
        {
            var property = "test".BindCondition(s => s != null);
            Assert.IsTrue(property.Value);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reference.Tests
{
    [TestClass]
    public class ReferenceTest
    {
        private class Baz
        {
            internal String Moo { get; set; }
        }
        private class Bar
        {
            internal Baz[] Bazzes { get; set; }
        }
        private class Doo
        {
            internal Int32 Daz { get; set; }
        }
        private class Ran
        {
            private Int32 Index { get; }

            private Ran(Int32 index)
            {
                Index = index;
            }

            public static implicit operator Ran(Int32 index)
            {
                return new Ran(index);
            }

            public override bool Equals(object obj)
            {
                return (obj as Ran)?.Index == Index;
            }

            public override int GetHashCode()
            {
                return Index.GetHashCode();
            }
        }
        private class Roo
        {
            internal Double Raz { get; set; }
        }
        private class Foo
        {
            internal Bar Bar { get; set; }
            internal IList<Doo> Dar { get; set; }
            internal IDictionary<Ran, Roo> Rar { get; set; }
            internal Int32 Qoo;
        }

        private static Foo MakeFoo()
            => new Foo()
            {
                Bar = new Bar()
                {
                    Bazzes = new[]
                    {
                        new Baz() {Moo = "frist"},
                        new Baz() {Moo = "sceond"},
                        new Baz() {Moo = "thrid"},
                        new Baz() {Moo = "frouth"},
                    },
                },
                Qoo = 15,
                Dar = new List<Doo>()
                {
                    new Doo() {Daz = 1},
                    new Doo() {Daz = 2},
                    new Doo() {Daz = 3},
                    new Doo() {Daz = 4},
                },
                Rar = new Dictionary<Ran, Roo>()
                {
                    [0] = new Roo() {Raz = 1.0},
                    [1] = new Roo() {Raz = 1.1},
                    [2] = new Roo() {Raz = 1.2},
                    [3] = new Roo() {Raz = 1.3},
                },
            };

        private class Indexer
        {
            // ReSharper disable once UnusedParameter.Local
            internal Object this[Object _]
            {
                get => Value;
                // ReSharper disable once UnusedMember.Local
                set => Value = value;
            }

            private Object Value;
        }

        private static void CheckValidGet<TProp>(IReference<TProp> reference, TProp control)
        {
            Assert.IsTrue(reference.Valid);
            Assert.AreEqual(control, reference.ValueOrDefault);
            Assert.AreEqual(control, reference.Value);
        }

        private static void CheckInvalidGet<TProp>(IReference<TProp> reference)
        {
            Assert.IsFalse(reference.Valid);
            Assert.AreEqual(default(TProp), reference.ValueOrDefault);
            try
            {
                var value = reference.Value;
                Assert.Fail($"Expected to throw: {reference} -> {value}");
            }
            catch(InvalidReferenceException)
            {
            }
        }

        private static void CheckValidSet<TProp>(IReference<TProp> reference, TProp value, Func<TProp> check)
        {
            reference.Value = value;
            Assert.AreEqual(value, check());
        }

        private static void CheckInvalidSet<TProp>(IReference<TProp> reference, TProp value)
        {
            try
            {
                reference.Value = value;
                Assert.Fail($"Expected to throw: {reference} -> {reference.Value}");
            }
            catch(InvalidReferenceException)
            {
            }
        }

        [TestMethod]
        public void Reference_Get_Valid()
        {
            var foo = MakeFoo();

            CheckValidGet(new Reference<Foo, Foo>(f => f, foo), foo);
            CheckValidGet(new Reference<Bar, Foo>(f => f.Bar, foo), foo.Bar);
            CheckValidGet(new Reference<Int32, Foo>(f => f.Qoo, foo), foo.Qoo);
            CheckValidGet(new Reference<Baz[], Foo>(f => f.Bar.Bazzes, foo), foo.Bar.Bazzes);
            CheckValidGet(new Reference<IList<Doo>, Foo>(f => f.Dar, foo), foo.Dar);
            CheckValidGet(new Reference<IDictionary<Ran, Roo>, Foo>(f => f.Rar, foo), foo.Rar);
            foreach (var i in Enumerable.Range(0, 4))
            {
                CheckValidGet(new Reference<Baz, Foo>(f => f.Bar.Bazzes[i], foo), foo.Bar.Bazzes[i]);
                CheckValidGet(new Reference<String, Foo>(f => f.Bar.Bazzes[i].Moo, foo), foo.Bar.Bazzes[i].Moo);
                CheckValidGet(new Reference<Doo, Foo>(f => f.Dar[i], foo), foo.Dar[i]);
                CheckValidGet(new Reference<Int32, Foo>(f => f.Dar[i].Daz, foo), foo.Dar[i].Daz);
                CheckValidGet(new Reference<Roo, Foo>(f => f.Rar[i], foo), foo.Rar[i]);
                CheckValidGet(new Reference<Double, Foo>(f => f.Rar[i].Raz, foo), foo.Rar[i].Raz);
            }
        }

        [TestMethod]
        public void Reference_Get_Invalid()
        {
            CheckValidGet(new Reference<Foo, Foo>(f => f), null);
            CheckInvalidGet(new Reference<Bar, Foo>(f => f.Bar));
            CheckInvalidGet(new Reference<Int32, Foo>(f => f.Qoo));
            CheckInvalidGet(new Reference<IList<Doo>, Foo>(f => f.Dar));
            CheckInvalidGet(new Reference<IDictionary<Ran, Roo>, Foo>(f => f.Rar));
            var foo = new Foo();
            CheckInvalidGet(new Reference<Baz[], Foo>(f => f.Bar.Bazzes, foo));
            foo.Bar = new Bar();
            CheckInvalidGet(new Reference<Baz, Foo>(f => f.Bar.Bazzes[2], foo));
            foo.Bar.Bazzes = new Baz[4];
            CheckInvalidGet(new Reference<String, Foo>(f => f.Bar.Bazzes[2].Moo, foo));
            foo.Dar = new List<Doo>() {null, null, null, null};
            CheckInvalidGet(new Reference<Int32, Foo>(f => f.Dar[2].Daz, foo));
            foo.Rar = new Dictionary<Ran, Roo>() {[0] = null, [1] = null, [2] = null, [3] = null};
            CheckInvalidGet(new Reference<Double, Foo>(f => f.Rar[2].Raz, foo));
        }

        [TestMethod]
        public void Reference_Set_Valid()
        {
            var foo = new Foo();
            var fooControl = MakeFoo();

            CheckValidSet(new Reference<Bar, Foo>(f => f.Bar, foo), fooControl.Bar, () => foo.Bar);
            foo.Bar = new Bar();
            CheckValidSet(new Reference<Int32, Foo>(f => f.Qoo, foo), fooControl.Qoo, () => foo.Qoo);
            CheckValidSet(new Reference<Baz[], Foo>(f => f.Bar.Bazzes, foo), fooControl.Bar.Bazzes, () => foo.Bar.Bazzes);
            foo.Bar.Bazzes = new Baz[4];
            CheckValidSet(new Reference<IList<Doo>, Foo>(f => f.Dar, foo), fooControl.Dar, () => foo.Dar);
            foo.Dar = new List<Doo>() {null, null, null, null};
            CheckValidSet(new Reference<IDictionary<Ran, Roo>, Foo>(f => f.Rar, foo), fooControl.Rar, () => foo.Rar);
            foo.Rar = new Dictionary<Ran, Roo>() {[0] = null, [1] = null, [2] = null, [3] = null};
            foreach (var i in Enumerable.Range(0, 4))
            {
                CheckValidSet(new Reference<Baz, Foo>(f => f.Bar.Bazzes[i], foo), fooControl.Bar.Bazzes[i], () => foo.Bar.Bazzes[i]);
                foo.Bar.Bazzes[i] = new Baz();
                CheckValidSet(new Reference<String, Foo>(f => f.Bar.Bazzes[i].Moo, foo), fooControl.Bar.Bazzes[i].Moo, () => foo.Bar.Bazzes[i].Moo);
                CheckValidSet(new Reference<Doo, Foo>(f => f.Dar[i], foo), fooControl.Dar[i], () => foo.Dar[i]);
                foo.Dar[i] = new Doo();
                CheckValidSet(new Reference<Int32, Foo>(f => f.Dar[i].Daz, foo), fooControl.Dar[i].Daz, () => foo.Dar[i].Daz);
                CheckValidSet(new Reference<Roo, Foo>(f => f.Rar[i], foo), fooControl.Rar[i], () => foo.Rar[i]);
                foo.Rar[i] = new Roo();
                CheckValidSet(new Reference<Double, Foo>(f => f.Rar[i].Raz, foo), fooControl.Rar[i].Raz, () => foo.Rar[i].Raz);
            }
        }

        [TestMethod]
        public void Reference_Set_Invalid()
        {
            CheckInvalidSet(new Reference<Bar, Foo>(f => f.Bar), new Bar());
            CheckInvalidSet(new Reference<Int32, Foo>(f => f.Qoo), 15);
            CheckInvalidSet(new Reference<IList<Doo>, Foo>(f => f.Dar), new List<Doo>());
            CheckInvalidSet(new Reference<IDictionary<Ran, Roo>, Foo>(f => f.Rar), new Dictionary<Ran, Roo>());
            var foo = new Foo();
            CheckInvalidSet(new Reference<Baz[], Foo>(f => f.Bar.Bazzes, foo), new Baz[4]);
            foo.Bar = new Bar();
            foreach (var i in Enumerable.Range(0, 4))
            {
                CheckInvalidSet(new Reference<Baz, Foo>(f => f.Bar.Bazzes[i], foo), new Baz());
                CheckInvalidSet(new Reference<Doo, Foo>(f => f.Dar[i], foo), new Doo());
                CheckInvalidSet(new Reference<Roo, Foo>(f => f.Rar[i], foo), new Roo());
            }

            foo.Bar.Bazzes = new Baz[4];
            foo.Dar = new List<Doo>() {null, null, null, null};
            foo.Rar = new Dictionary<Ran, Roo>() {[0] = null, [1] = null, [2] = null, [3] = null};
            foreach (var i in Enumerable.Range(0, 4))
            {
                CheckInvalidSet(new Reference<String, Foo>(f => f.Bar.Bazzes[i].Moo, foo), "");
                CheckInvalidSet(new Reference<Double, Foo>(f => f.Rar[i].Raz, foo), 1.0);
                CheckInvalidSet(new Reference<Int32, Foo>(f => f.Dar[i].Daz, foo), 0);
            }
        }

        [TestMethod]
        public void Reference_Indexer()
        {
            var indexer = new Indexer();

            var property = indexer.Bind(i => i[null]);
            Assert.AreEqual(null, property.Value);
            property.Value = "anything";
            Assert.AreEqual("anything", property.Value);
        }

        [TestMethod]
        public void Reference_FactoryConstructor()
        {
            var foo = MakeFoo();

            var property = Reference.Create(f => f.Bar.Bazzes[2].Moo, foo);
            Assert.AreEqual("thrid", property.Value);
            property.Value = "thriteen";
            Assert.AreEqual("thriteen", foo.Bar.Bazzes[2].Moo);
        }

        [TestMethod]
        public void Reference_ExtensionMethodConstructor()
        {
            var foo = MakeFoo();

            var property = foo.Bind(f => f.Bar.Bazzes[3].Moo);
            Assert.AreEqual("frouth", property.Value);
            property.Value = "froutheen";
            Assert.AreEqual("froutheen", foo.Bar.Bazzes[3].Moo);
        }

        [TestMethod]
        public void Reference_IReferenceValue_Valid()
        {
            var foo = MakeFoo();

            var property = (IReference)foo.Bind(f => f.Qoo);
            Assert.AreEqual(15, property.Value);
            property.Value = 13;
            Assert.AreEqual(13, foo.Qoo);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidReferenceException), "Reference could not be retrieved.")]
        public void Reference_IReferenceValue_Get_Invalid()
        {
            var foo = new Foo();

            var property = (IReference)foo.Bind(f => f.Bar.Bazzes[0].Moo);
            var _ = property.Value;
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidReferenceException), "Reference could not be set.")]
        public void Reference_IReferenceValue_Set_Invalid()
        {
            var foo = new Foo();

            var property = (IReference)foo.Bind(f => f.Bar.Bazzes[0].Moo);
            property.Value = 13;
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidReferenceException), "Setter value of incorrect type.")]
        public void Reference_IReferenceValue_Set_InvalidCast()
        {
            var foo = new Foo();

            var property = (IReference)foo.Bind(f => f.Qoo);
            property.Value = "test";
        }

        [TestMethod]
        public void Reference_IReferenceValueOrDefault_Valid()
        {
            var foo = new Foo();

            var property = (IReference)foo.Bind(f => f.Bar.Bazzes[0].Moo);
            Assert.AreEqual(null, property.ValueOrDefault);
        }

        [TestMethod]
        public void Reference_IReferenceValueOrDefault_Invalid()
        {
            var foo = new Foo();

            var property = (IReference)foo.Bind(f => f.Bar.Bazzes[0].Moo);
            Assert.AreEqual(null, property.ValueOrDefault);
        }

        [TestMethod]
        public void Reference_ImplicitConversion()
        {
            var foo = MakeFoo();

            var property = foo.Bind(f => f.Qoo);
            float value = property;
            Assert.AreEqual(15.0f, value);
        }
    }
}

﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sigil.NonGeneric;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SigilTests
{
    public partial class NewObject
    {
        [TestMethod]
        public void MultiParamNonGeneric()
        {
            var e1 = Emit.NewDynamicMethod(typeof(string), new [] { typeof(string), typeof(int), typeof(List<double>) });
            var val = typeof(ThreeClass).GetField("Value");

            e1.LoadArgument(0);
            e1.LoadArgument(1);
            e1.LoadArgument(2);
            e1.NewObject<ThreeClass, string, int, List<double>>();
            e1.LoadField(val);
            e1.Return();

            var d1 = e1.CreateDelegate<Func<string, int, List<double>, string>>();

            Assert.AreEqual("hello @10 ==> 1, 2.5, 5.1", d1("hello", 10, new List<double> { 1.0, 2.5, 5.1 }));
        }

        [TestMethod]
        public void PrivateConstructorNonGeneric()
        {
            var e1 = Emit.NewDynamicMethod(typeof(string), new [] { typeof(int) }, "E1");
            e1.LoadArgument(0);
            e1.NewObject<Foo, int>();
            e1.CallVirtual(typeof(object).GetMethod("ToString"));
            e1.Return();

            var d1 = e1.CreateDelegate<Func<int, string>>();

            Assert.AreEqual("314159", d1(314159));
        }

        [TestMethod]
        public void ReferenceTypeNonGeneric()
        {
            var e1 = Emit.NewDynamicMethod(typeof(RT), Type.EmptyTypes, "E1");
            e1.LoadConstant(314159);
            e1.NewObject<RT, int>();
            e1.Return();

            var d1 = e1.CreateDelegate<Func<RT>>();

            Assert.AreEqual(314159, d1().A);
        }

        [TestMethod]
        public void ValueTypeNonGeneric()
        {
            var e1 = Emit.NewDynamicMethod(typeof(VT), Type.EmptyTypes, "E1");
            e1.LoadConstant(3.1415926);
            e1.NewObject<VT, double>();
            e1.Return();

            var d1 = e1.CreateDelegate<Func<VT>>();

            Assert.AreEqual(3.1415926, d1().B);
        }

        [TestMethod]
        public void ConstructorBuildersNonGeneric()
        {
            var asm = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("ConstructorBuilders"), AssemblyBuilderAccess.Run);
            var mod = asm.DefineDynamicModule("Mod");
            var tb = mod.DefineType("Type");

            var cb = Emit.BuildConstructor(new Type[0], tb, MethodAttributes.Public);
            cb.Return();
            var cons = cb.CreateConstructor();

            var createProxy = Emit.BuildStaticMethod(typeof(object), new Type[0], tb, "Create", MethodAttributes.Public | MethodAttributes.Static, true, false);
            createProxy.NewObject(cons, new Type[0]);
            createProxy.Return();

            createProxy.CreateMethod();

            var t = tb.CreateType();

            var mtd = t.GetMethod("Create");
            var o = mtd.Invoke(null, new object[0]);

            Assert.IsNotNull(o);
            Assert.AreEqual("Type", o.GetType().Name);
        }
    }
}

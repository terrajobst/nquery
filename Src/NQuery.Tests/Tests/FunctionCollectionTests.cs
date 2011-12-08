using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Runtime;

namespace NQuery.Tests
{
    [TestClass]
    public class FunctionCollectionTests
    {
        private class MyContainerInstance
        {
            [FunctionBinding("MY_FUNC_1")]
            public byte MyFunc1()
            {
                return 0;
            }

            [FunctionBinding("MY_FUNC_2")]
            public sbyte MyFunc2()
            {
                return 0;
            }

            [FunctionBinding("MY_FUNC_OVERLOADING")]
            public double MyFuncOverloading()
            {
                return 0;
            }

            [FunctionBinding("MY_FUNC_OVERLOADING")]
            public float MyFuncOverloading(int a, int b)
            {
                return 0;
            }
        }

        private static class MyContainerStatic
        {
            [FunctionBinding("MY_FUNC_1")]
            public static byte MyFunc1()
            {
                return 0;
            }

            [FunctionBinding("MY_FUNC_2")]
            public static sbyte MyFunc2()
            {
                return 0;
            }

            [FunctionBinding("MY_FUNC_OVERLOADING")]
            public static double MyFuncOverloading()
            {
                return 0;
            }

            [FunctionBinding("MY_FUNC_OVERLOADING")]
            public static float MyFuncOverloading(int a, int b)
            {
                return 0;
            }
        }

        private class MyContainerInstance2
        {
            [FunctionBinding("MY_FUNC_OVERLOADING")]
            public string MyFuncOverloading(int a, int b, int c)
            {
                return String.Empty;
            }
        }

        private static class MyContainerStatic2
        {
            [FunctionBinding("MY_FUNC_OVERLOADING")]
            public static string MyFuncOverloading(int a, int b, int c)
            {
                return String.Empty;
            }
        }
        
        private static void EnsureInScope(DataContext dataContext)
        {
            Identifier myFunc1 = Identifier.CreateVerbatim("MY_FUNC_1");
            Identifier myFunc2 = Identifier.CreateVerbatim("MY_FUNC_2");
            Identifier myFuncOverloading = Identifier.CreateVerbatim("MY_FUNC_OVERLOADING");

            FunctionBinding[] myFunc1List = dataContext.Functions.Find(myFunc1);
            Assert.AreEqual(1, myFunc1List.Length);
            Assert.AreEqual("MY_FUNC_1", myFunc1List[0].Name);
            Assert.AreEqual(typeof(byte), myFunc1List[0].ReturnType);
            
            FunctionBinding[] myFunc2List = dataContext.Functions.Find(myFunc2);
            Assert.AreEqual(1, myFunc2List.Length);
            Assert.AreEqual("MY_FUNC_2", myFunc2List[0].Name);
            Assert.AreEqual(typeof(sbyte), myFunc2List[0].ReturnType);

            FunctionBinding[] myFuncOverloadingList = dataContext.Functions.Find(myFuncOverloading);
            Assert.AreEqual(2, myFuncOverloadingList.Length);

            Assert.AreEqual("MY_FUNC_OVERLOADING", myFuncOverloadingList[0].Name);
            Assert.AreEqual(typeof(double), myFuncOverloadingList[0].ReturnType);            

            Assert.AreEqual("MY_FUNC_OVERLOADING", myFuncOverloadingList[1].Name);
            Assert.AreEqual(typeof(float), myFuncOverloadingList[1].ReturnType);
        }

        private static void EnsureNotInScope(DataContext dataContext)
        {
            Identifier myFunc1 = Identifier.CreateVerbatim("MY_FUNC_1");
            Identifier myFunc2 = Identifier.CreateVerbatim("MY_FUNC_2");
            Identifier myFuncOverloading = Identifier.CreateVerbatim("MY_FUNC_OVERLOADING");

            FunctionBinding[] myFunc1List = dataContext.Functions.Find(myFunc1);
            Assert.AreEqual(0, myFunc1List.Length);

            FunctionBinding[] myFunc2List = dataContext.Functions.Find(myFunc2);
            Assert.AreEqual(0, myFunc2List.Length);

            FunctionBinding[] myFuncOverloadingList = dataContext.Functions.Find(myFuncOverloading);
            Assert.AreEqual(1, myFuncOverloadingList.Length);
        }
        
        [TestMethod]
        public void AddFromContainerType()
        {
            DataContext dataContext = new DataContext();
            dataContext.Functions.AddFromContainer(typeof(MyContainerStatic));

            EnsureInScope(dataContext);
        }

        [TestMethod]
        public void AddFromContainerInstance()
        {
            DataContext dataContext = new DataContext();
            dataContext.Functions.AddFromContainer(new MyContainerInstance());

            EnsureInScope(dataContext);
        }
        
        [TestMethod]
        public void RemoveFromContainerType()
        {
            DataContext dataContext = new DataContext();
            dataContext.Functions.AddFromContainer(typeof(MyContainerStatic));
            dataContext.Functions.AddFromContainer(typeof(MyContainerStatic2));
            dataContext.Functions.RemoveFromContainer(typeof(MyContainerStatic));
            EnsureNotInScope(dataContext);
        }
        
        [TestMethod]
        public void RemoveFromContainerInstance()
        {
            DataContext dataContext = new DataContext();
            dataContext.Functions.AddFromContainer(new MyContainerInstance());
            dataContext.Functions.AddFromContainer(new MyContainerInstance2());
            dataContext.Functions.RemoveFromContainer(new MyContainerInstance());
            EnsureNotInScope(dataContext);
        }
    }
}

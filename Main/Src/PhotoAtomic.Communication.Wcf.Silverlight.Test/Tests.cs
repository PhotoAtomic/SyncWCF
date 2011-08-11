using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoAtomic.Communication.Wcf.Silverlight.Interface.Test;
using duplicate = PhotoAtomic.Communication.Wcf.Silverlight.Interface2.Test;
using System.Threading;
using PhotoAtomic.SyncWcf;

namespace PhotoAtomic.Reflection.Silverlight.Test
{
    [TestClass]
    public class Tests : SilverlightTest
    {
        [TestMethod]
        public void PassSyncInterfaceType_Expected_AsynchInterfaceTypeReturned()
        {
            var generator = new TypeGenerator();
            var synchType = typeof(ISyncInterface);
            var asynchType = generator.GenerateAsyncInterfaceFor<ISyncInterface>();
            Assert.IsNotNull(asynchType);
            Assert.IsTrue(asynchType.IsInterface);

            var methods = asynchType.GetMethods();

            Assert.AreEqual(methods[0].Name, "BeginAMethod");
            Assert.AreEqual(methods[1].Name, "EndAMethod");
            Assert.AreEqual(2, methods.Length);
        }

        [TestMethod]
        [Asynchronous]
        public void CreateProxy_Expected_ProxyCommunicate()
        {
            var channelFactory = new AsyncChannelFactory<ITestService>();
            var channel = channelFactory.CreateChannel();
            int number = 0;
            var res =
                channel.ExecuteAsync(
                    ws => ws.Operation(1 + 4),
                    result =>
                    {
                        number = result + 1;
                        Assert.AreEqual(number, 7);
                        TestComplete();
                    });            
        }

        [TestMethod]
        public void CreateMultipleAsyncTypeOfTheSameKind_Expected_SameTypeReturned()
        {
            var generator = new TypeGenerator();
            var asynchType1 = generator.GenerateAsyncInterfaceFor<ISyncInterface>();
            var asynchType2 = generator.GenerateAsyncInterfaceFor<ISyncInterface>();
            Assert.AreEqual(asynchType1, asynchType2);
        }

        [TestMethod]
        public void CreateMultipleAsyncTypeOfDifferentKind_Expected_DifferentTypeReturned()
        {
            var generator = new TypeGenerator();
            var asynchType1 = generator.GenerateAsyncInterfaceFor<ISyncInterface>();
            var asynchType2 = generator.GenerateAsyncInterfaceFor<ISyncInterface2>();
            Assert.IsNotNull(asynchType1);
            Assert.IsNotNull(asynchType2);
            Assert.AreNotEqual(asynchType1, asynchType2);
        }

        [TestMethod]
        public void CreateMultipleFactoryForTheSameType_Expected_MultipleFactoryCreated()
        {
            var channelFactory1 = new AsyncChannelFactory<ITestService>();
            var channelFactory2 = new AsyncChannelFactory<ITestService>();
            var channel1 = channelFactory1.CreateChannel();
            var channel2 = channelFactory2.CreateChannel();
        }

        [TestMethod]
        public void CreateMultipleChannelsForTheSameType_Expected_MultipleFactoryCreated()
        {
            var channelFactory1 = new AsyncChannelFactory<ITestService>();

            var channel1 = channelFactory1.CreateChannel();
            var channel2 = channelFactory1.CreateChannel();
        }

        [TestMethod]
        [Asynchronous]
        public void InvokingFaultingOperation_Expected_ExceptionCatched()
        {
            var channelFactory = new AsyncChannelFactory<ITestService>();
            var channel = channelFactory.CreateChannel();
            var res =
                channel.ExecuteAsync(
                    ws => ws.FaultingOperation(0),
                    result =>
                    {
                        Assert.Fail();
                    },
                    exception =>
                    {
                        TestComplete();
                    });
        }

        [TestMethod]
        [Asynchronous]
        public void InvokingOperationPassingAState_Expected_StateReceived()
        {
            var channelFactory = new AsyncChannelFactory<ITestService>();
            var channel = channelFactory.CreateChannel();
            var res =
                channel.ExecuteAsync(
                    ws => ws.Operation(0),
                    "status message",
                    (result, status) =>
                    {
                        Assert.AreEqual("status message", status);
                        TestComplete();
                    });            
        }

        [TestMethod]
        [Asynchronous]
        public void InvokingVoidOperation_Expected_OperationInvoked()
        {
            var channelFactory = new AsyncChannelFactory<ITestService>();
            var channel = channelFactory.CreateChannel();
            var res =
                channel.ExecuteAsync(
                    ws => ws.VoidOperation(0),
                    () =>
                    {
                        TestComplete();
                    });
        }

        [TestMethod]
        [Asynchronous]
        public void InvokingOutOperation_Expected_OperationInvoked()
        {
            var channelFactory = new AsyncChannelFactory<ITestServiceOut>();
            var channel = channelFactory.CreateChannel();
            int a = 1;
            var res =
                channel.ExecuteAsync(
                    ws => ws.Method(ref a),
                    () =>
                    {
                        Assert.AreEqual(9, a);
                        TestComplete();
                    });
        }

        [TestMethod]
        [Asynchronous]
        public void InvokingComplexOperation_Expected_OperationInvoked()
        {
            var channelFactory = new AsyncChannelFactory<ITestServiceOut>();
            var channel = channelFactory.CreateChannel();
            ComplexDataType data = new ComplexDataType
                {
                     Description = "in",
                     Name = "in name",
                     Id = 0,                
                };

            var result =
                channel.ExecuteAsync(
                    ws => ws.ComplexMethod(ref data),
                    res =>
                    {
                        Assert.AreEqual("ref", data.Description);
                        Assert.AreEqual("ref name", data.Name);
                        Assert.AreEqual(1, data.Id);

                        Assert.AreEqual("out", res.Description);
                        Assert.AreEqual("out name", res.Name);
                        Assert.AreEqual(2, res.Id);
                        TestComplete();
                    });                    
        }

        [TestMethod]
        [Asynchronous]
        public void InvokingOutOperation2_Expected_OperationInvoked()
        {
            var channelFactory = new AsyncChannelFactory<duplicate.ITestServiceOut>();
            var channel = channelFactory.CreateChannel();
            int a = 1;            
            var res =
                channel.ExecuteAsync(
                    ws => ws.Method(ref a),
                    () =>
                    {
                        Assert.AreEqual(29, a);
                        TestComplete();
                    });            
            
        }

    }
}
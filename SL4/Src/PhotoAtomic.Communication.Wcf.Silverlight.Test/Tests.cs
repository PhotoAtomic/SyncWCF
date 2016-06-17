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
using PhotoAtomic.SyncWcf.TypeConverters;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace PhotoAtomic.Reflection.Silverlight.Test
{  
    [TestClass]
    public partial class Tests : SilverlightTest
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

        [TestMethod]
        [Asynchronous]
        public void InvokingCorrectlyWorkingOperation_Expected_OperationStatusChangeToOpened()
        {
            var channelFactory = new AsyncChannelFactory<ITestService>();
            var channel = channelFactory.CreateChannel();
            Assert.AreEqual(CommunicationState.Created, channel.State);
            var res =
                channel.ExecuteAsync(
                    ws => ws.Operation(0),
                    () =>
                    {
                        Assert.AreEqual(CommunicationState.Opened, channel.State);
                        TestComplete();
                    });
        }

        [TestMethod]
        [Asynchronous]
        public void OpenService_Expected_ServiceOpen()
        {
            var channelFactory = new AsyncChannelFactory<ITestService>();
            var channel = channelFactory.CreateChannel();
            Assert.AreEqual(CommunicationState.Created, channel.State);
            channel.Open(() =>
            {
                Assert.AreEqual(CommunicationState.Opened, channel.State);
                TestComplete();
            });
        }

        [TestMethod]
        [Asynchronous]
        public void AbortingAnOpenService_Expected_ServiceClosed()
        {
            var channelFactory = new AsyncChannelFactory<ITestService>();
            var channel = channelFactory.CreateChannel();
            Assert.AreEqual(CommunicationState.Created, channel.State);
            channel.Open(() =>
            {
                Assert.AreEqual(CommunicationState.Opened, channel.State);
                channel.Abort();
                Assert.AreEqual(CommunicationState.Closed, channel.State);
                TestComplete();
            });
        }

        [TestMethod]
        [Asynchronous]
        public void ClosingAnOpenService_Expected_ServiceClosedGracefully()
        {
            var channelFactory = new AsyncChannelFactory<ITestService>();
            var channel = channelFactory.CreateChannel();
            Assert.AreEqual(CommunicationState.Created, channel.State);
            channel.Open(() =>
            {
                Assert.AreEqual(CommunicationState.Opened, channel.State);

                channel.Close(
                    () =>
                    {
                        Assert.AreEqual(CommunicationState.Closed, channel.State);
                        TestComplete();
                    });
            });
        }

        [TestMethod]
        [Asynchronous]
        public void OpenServiceWithSatus_Expected_StatusPassed()
        {
            var channelFactory = new AsyncChannelFactory<ITestService>();
            var channel = channelFactory.CreateChannel();
            Assert.AreEqual(CommunicationState.Created, channel.State);

            channel.Open(
                "test status object",
                (status) =>
                {
                    Assert.AreEqual("test status object", status);
                    TestComplete();
                });
        }


        [TestMethod]
        [Asynchronous]
        public void InvokeAnOperationDefinedInAnInheritedServiceInterface_Expected_OperationPerformed()
        {
            var channelFactory = new AsyncChannelFactory<IDerivedTestService>();
            var channel = channelFactory.CreateChannel();

            channel.ExecuteAsync(
                ws => ws.VoidOperation(1),
                () => { 
                    TestComplete(); 
                },
                exception => { Assert.Fail(); });

        }

        [TestMethod]
        [Asynchronous]
        public void UseWithOperationContextScope_Expected_OperationPerformed()
        {
            var channelFactory = new AsyncChannelFactory<IDerivedTestService>();
            var channel = channelFactory.CreateChannel();

            var userContextHeaderName = "UserContext";
            var userContextHeaderNamespace = "http://my.context.namespace/";
            var userContextHeaderValue = "My user context";

            var testPart = new MultipartComplete(2, TestComplete);

            using (var scope = new OperationContextScope(channel.ToContextChannel()))
            {

                OperationContext.Current.OutgoingMessageHeaders.Add(
                    MessageHeader.CreateHeader(
                        userContextHeaderName,
                        userContextHeaderNamespace,
                        userContextHeaderValue));

                channel.ExecuteAsync(
                    ws => ws.ReadHeaderOperation(),
                    result =>
                    {
                        Assert.AreEqual(userContextHeaderValue, result);
                        testPart.Completed();
                    },
                    exception => { Assert.Fail(); });
            }

            channel.ExecuteAsync(
                    ws => ws.ReadHeaderOperation(),
                    result =>
                    {
                        Assert.IsNull(result);
                        testPart.Completed();
                    },
                    exception => { Assert.Fail(); });
            
        }

    }
}
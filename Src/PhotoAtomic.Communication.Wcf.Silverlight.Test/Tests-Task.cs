
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
using PhotoAtomic.Communication.Wcf.Silverlight.Interface3.Test;
using System.Threading.Tasks;

namespace PhotoAtomic.Reflection.Silverlight.Test
{
    
    public partial class Tests : SilverlightTest
    {

        [TestMethod]
        [Asynchronous]
        public void Task_CreateProxy_Expected_ProxyCommunicate()
        {

            var channelFactory = new AsyncChannelFactory<ITestService>();
            var channel = channelFactory.CreateChannel();

            var t = channel.ExecuteTask(ws => ws.Operation(1 + 4))
                .ContinueWith(x => TestComplete(), TaskScheduler.FromCurrentSynchronizationContext());
        }

        [TestMethod]
        [Asynchronous]
        public void Task_InvokingFaultingOperation_Expected_ExceptionCatched()
        {
            var channelFactory = new AsyncChannelFactory<ITestService>();
            var channel = channelFactory.CreateChannel();

            var res = channel.ExecuteTask(ws => ws.FaultingOperation(0))
                .ContinueWith(x =>
                {
                    if (x.IsFaulted) TestComplete();
                    else Assert.Fail();
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        [TestMethod]
        [Asynchronous]
        public void Task_InvokingOperationPassingAState_Expected_StateReceived()
        {
            var channelFactory = new AsyncChannelFactory<ITestService>();
            var channel = channelFactory.CreateChannel();
            var res = channel.ExecuteTask(ws => ws.Operation(0), "Status Message")
                .ContinueWith(t =>
                {

                    Assert.AreEqual("Status Message", t.AsyncState);
                    TestComplete();
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        [TestMethod]
        [Asynchronous]
        public void Task_InvokingVoidOperation_Expected_OperationInvoked()
        {
            var channelFactory = new AsyncChannelFactory<ITestService>();
            var channel = channelFactory.CreateChannel();
            var res =
                channel.ExecuteTask(ws => ws.VoidOperation(0))
                .ContinueWith(x => TestComplete(), TaskScheduler.FromCurrentSynchronizationContext());

        }

        [TestMethod]
        [Asynchronous]
        public void Task_InvokingOutOperation_Expected_OperationInvoked()
        {
            var channelFactory = new AsyncChannelFactory<ITestServiceOut>();
            var channel = channelFactory.CreateChannel();
            int a = 1;
            var res =
                channel.ExecuteTask(ws => ws.Method(ref a))
                    .ContinueWith(x =>
                    {
                        Assert.AreEqual(9, a);
                        TestComplete();
                    }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        [TestMethod]
        [Asynchronous]
        public void Task_InvokingComplexOperation_Expected_OperationInvoked()
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
                channel.ExecuteTask(ws => ws.ComplexMethod(ref data))
                .ContinueWith(
                    t =>
                    {
                        var res = t.Result;
                        Assert.AreEqual("ref", data.Description);
                        Assert.AreEqual("ref name", data.Name);
                        Assert.AreEqual(1, data.Id);

                        Assert.AreEqual("out", res.Description);
                        Assert.AreEqual("out name", res.Name);
                        Assert.AreEqual(2, res.Id);
                        TestComplete();
                    }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        [TestMethod]
        [Asynchronous]
        public void Task_InvokingOutOperation2_Expected_OperationInvoked()
        {
            var channelFactory = new AsyncChannelFactory<duplicate.ITestServiceOut>();
            var channel = channelFactory.CreateChannel();
            int a = 1;
            var res =
                channel.ExecuteTask(ws => ws.Method(ref a))
                .ContinueWith(
                    (t) =>
                    {
                        Assert.AreEqual(29, a);
                        TestComplete();
                    }, TaskScheduler.FromCurrentSynchronizationContext());

        }

        [TestMethod]
        [Asynchronous]
        public void Task_InvokingCorrectlyWorkingOperation_Expected_OperationStatusChangeToOpened()
        {
            var channelFactory = new AsyncChannelFactory<ITestService>();
            var channel = channelFactory.CreateChannel();
            Assert.AreEqual(CommunicationState.Created, channel.State);
            var res =
                channel.ExecuteTask(ws => ws.Operation(0))
                .ContinueWith(
                    (t) =>
                    {
                        Assert.AreEqual(CommunicationState.Opened, channel.State);
                        TestComplete();
                    }, TaskScheduler.FromCurrentSynchronizationContext());
        }


        [TestMethod]
        [Asynchronous]
        public void Task_InvokeAnOperationDefinedInAnInheritedServiceInterface_Expected_OperationPerformed()
        {
            var channelFactory = new AsyncChannelFactory<IDerivedTestService>();
            var channel = channelFactory.CreateChannel();

            channel.ExecuteTask(ws => ws.VoidOperation(1))
                .ContinueWith(
                (t) =>
                {
                    if (t.IsFaulted) Assert.Fail();
                    TestComplete();
                }, TaskScheduler.FromCurrentSynchronizationContext());

        }

        [TestMethod]
        [Asynchronous]
        public void Task_UseWithOperationContextScope_Expected_OperationPerformed()
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

                channel.ExecuteTask(ws => ws.ReadHeaderOperation())
                    .ContinueWith(t =>
                    {
                        if (t.IsFaulted) Assert.Fail();
                        var result = t.Result;
                        Assert.AreEqual(userContextHeaderValue, result);
                        testPart.Completed();
                    }, TaskScheduler.FromCurrentSynchronizationContext());

            }

            channel.ExecuteTask(ws => ws.ReadHeaderOperation())
                    .ContinueWith(
                    t =>
                    {
                        if (t.IsFaulted) Assert.Fail();
                        var result = t.Result;
                        Assert.IsNull(result);
                        testPart.Completed();
                    }, TaskScheduler.FromCurrentSynchronizationContext());

        }

    }
}
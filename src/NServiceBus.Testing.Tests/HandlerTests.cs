namespace NServiceBus.Testing.Tests
{
    using System;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class HandlerTests
    {
        [Test]
        public void ShouldInvokeMessageInitializerOnMessage()
        {
            Test.Handler<ReplyingHandler>()
                .ExpectReply<MyReply>(m => m.String == "hello")
                .OnMessage<MyRequest>(m =>
                {
                    m.String = "hello";
                    m.ShouldReply = true;
                });
        }

        [Test]
        public void ShouldAssertDoNotContinueDispatchingCurrentMessageToHandlersWasCalled()
        {
            Test.Handler<DoNotContinueDispatchingCurrentMessageToHandlersHandler>()
                .ExpectDoNotContinueDispatchingCurrentMessageToHandlers()
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ShouldFailAssertingDoNotContinueDispatchingCurrentMessageToHandlersWasCalled()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<EmptyHandler>()
                .ExpectDoNotContinueDispatchingCurrentMessageToHandlers()
                .OnMessage<TestMessage>());
        }

        [Test]
        public void ShouldAssertHandleCurrentMessageLaterWasCalled()
        {
            Test.Handler<HandleCurrentMessageLaterHandler>()
                .ExpectHandleCurrentMessageLater()
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ShouldAssertDeferWasCalledWithTimeSpan()
        {
            var timespan = TimeSpan.FromMinutes(10);
            Test.Handler<DeferringTimeSpanHandler>()
                .WithExternalDependencies(h => h.Defer = timespan)
                .ExpectDefer<TestMessage>((m, t) => t == timespan)
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ShouldAssertDeferWasCalledWithDateTime()
        {
            var datetime = DateTime.UtcNow;
            Test.Handler<DeferringDateTimeHandler>()
                .WithExternalDependencies(h => h.Defer = datetime)
                .ExpectDefer<TestMessage>((m, t) => t == datetime)
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ShouldFailAssertingDeferWasCalledWithTimeSpan()
        {
            var timespan = TimeSpan.FromMinutes(10);
            Assert.Throws<ExpectationException>(() => Test.Handler<EmptyHandler>()
                .ExpectDefer<TestMessage>((m, t) => t == timespan)
                .OnMessage<TestMessage>());
        }

        [Test]
        public void ShouldFailAssertingDeferWasCalledWithDateTime()
        {
            var datetime = DateTime.UtcNow;
            Assert.Throws<ExpectationException>(() => Test.Handler<EmptyHandler>()
                .ExpectDefer<TestMessage>((m, t) => t == datetime)
                .OnMessage<TestMessage>());
        }

        [Test]
        public void ShouldAssertDeferWasNotCalledWithTimeSpan()
        {
            var timespan = TimeSpan.FromMinutes(10);
            Test.Handler<EmptyHandler>()
                .ExpectNotDefer<TestMessage>((m, t) => t == timespan)
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ShouldAssertDeferWasNotCalledWithDateTime()
        {
            var datetime = DateTime.UtcNow;
            Test.Handler<EmptyHandler>()
                .ExpectNotDefer<TestMessage>((m, t) => t == datetime)
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ShouldFailAssertingDeferWasNotCalledWithTimeSpan()
        {
            var timespan = TimeSpan.FromMinutes(10);
            Assert.Throws<ExpectationException>(() => Test.Handler<DeferringTimeSpanHandler>()
                .WithExternalDependencies(h => h.Defer = timespan)
                .ExpectNotDefer<TestMessage>((m, t) => t == timespan)
                .OnMessage<TestMessage>());
        }

        [Test]
        public void ShouldFailAssertingDeferWasNotCalledWithDateTime()
        {
            var datetime = DateTime.UtcNow;
            Assert.Throws<ExpectationException>(() => Test.Handler<DeferringDateTimeHandler>()
                .WithExternalDependencies(h => h.Defer = datetime)
                .ExpectNotDefer<TestMessage>((m, t) => t == datetime)
                .OnMessage<TestMessage>());
        }

        [Test]
        public void ShouldFailAssertingHandleCurrentMessageLaterWasCalled()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<EmptyHandler>()
                .ExpectHandleCurrentMessageLater()
                .OnMessage<TestMessage>());
        }

        [Test]
        public void ShouldCallHandleOnExplicitInterfaceImplementation()
        {
            var handler = new ExplicitInterfaceImplementation();
            Assert.IsFalse(handler.IsHandled);
            Test.Handler(handler).OnMessage<TestMessage>();
            Assert.IsTrue(handler.IsHandled);
        }

        [Test]
        public void ShouldPassExpectReplyWhenReypling()
        {
            Test.Handler<ReplyingHandler>()
                .ExpectReply<MyReply>()
                .OnMessage(new MyRequest { ShouldReply = true });
        }

        [Test]
        public void ShouldPassExpectReplyProvidedOptionsToCheck()
        {
            var options = new ReplyOptions();
            ReplyOptions capturedOptions = null;

            Test.Handler<ReplyingHandler>()
                .WithExternalDependencies(handler => handler.OptionsProvider = () => options)
                .ExpectReply<MyReply>((reply, replyOptions) =>
                {
                    capturedOptions = replyOptions;
                    return true;
                })
                .OnMessage(new MyRequest
                {
                    ShouldReply = true
                });

            Assert.AreSame(options, capturedOptions);
        }

        [Test]
        public void ShouldPassExpectNotReplyWhenNotReplying()
        {
            Test.Handler<ReplyingHandler>()
                .ExpectNotReply<MyReply>()
                .OnMessage(new MyRequest { ShouldReply = false });
        }

        [Test]
        public void ShouldPassExpectNotReplyProvidedOptionsToCheck()
        {
            var options = new ReplyOptions();
            ReplyOptions capturedOptions = null;

            Test.Handler<ReplyingHandler>()
                .WithExternalDependencies(handler => handler.OptionsProvider = () => options)
                .ExpectNotReply<MyReply>((reply, replyOptions) =>
                {
                    capturedOptions = replyOptions;
                    return false;
                })
                .OnMessage(new MyRequest
                {
                    ShouldReply = true
                });

            Assert.AreSame(options, capturedOptions);
        }

        [Test]
        public void ShouldPassExpectPublishWhenPublishing()
        {
            Test.Handler<PublishingHandler<Publish1>>()
                .ExpectPublish<Publish1>()
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ShouldPassExpectPublishWhenPublishingWithCustomCheck()
        {
            Test.Handler<PublishingHandler<Publish1>>()
                .ExpectPublish<Publish1>(m => true)
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ShouldFailExpectNotPublishWhenPublishing()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<PublishingHandler<Publish1>>()
                .ExpectNotPublish<Publish1>()
                .OnMessage<TestMessage>());
        }

        [Test]
        public void ShouldFailExpectNotPublishWithCheckWhenPublishing()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<PublishingHandler<Publish1>>()
                .ExpectNotPublish<Publish1>(m => true)
                .OnMessage<TestMessage>());
        }

        [Test]
        public void ShouldPassExpectPublishWhenPublishingMultipleEvents()
        {
            Test.Handler<PublishingHandler<Publish1, Publish2>>()
                .ExpectPublish<Publish1>(m => true)
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ShouldPassExpectPublishWhenMessageIsSend()
        {
            Test.Handler<PublishingHandler<Publish1>>()
                .ExpectPublish<Publish1>(m => true)
                .OnMessage(new TestMessageImpl(), Guid.NewGuid().ToString());
        }


        [Test]
        public void ShouldPassExpectPublishWhenPublishingAndCheckingPredicate()
        {
            Test.Handler<PublishingHandler<Publish1>>()
                .WithExternalDependencies(h => h.ModifyMessage = m => m.Data = "Data")
                .ExpectPublish<Publish1>(m => m.Data == "Data")
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ShouldFailExpectNotPublishWhenPublishingAndCheckingPredicate()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<PublishingHandler<Publish1>>()
                .WithExternalDependencies(h => h.ModifyMessage = m => m.Data = "Data")
                .ExpectNotPublish<Publish1>(m => m.Data == "Data")
                .OnMessage<TestMessage>());
        }

        [Test]
        public void ShouldFailExpectPublishWhenPublishingAndCheckingPredicateThatFails()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<PublishingHandler<Publish1>>()
                .WithExternalDependencies(h => h.ModifyMessage = m => m.Data = "NotData")
                .ExpectPublish<Publish1>(m => m.Data == "Data")
                .OnMessage<TestMessage>());
        }

        [Test]
        public void ShouldPassExpectNotPublishWhenPublishingAndCheckingPredicateThatFails()
        {
            Test.Handler<PublishingHandler<Publish1>>()
                .WithExternalDependencies(h => h.ModifyMessage = m => m.Data = "NotData")
                .ExpectNotPublish<Publish1>(m => m.Data == "Data")
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ShouldFailExpectPublishIfNotPublishing()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<EmptyHandler>()
                .ExpectPublish<Publish1>(m => true)
                .OnMessage<TestMessage>());
        }

        [Test]
        public void ShouldPassExpectPublishProvidedPublishOptionsToCheck()
        {
            var options = new PublishOptions();
            PublishOptions capturedOptions = null;

            Test.Handler<PublishingHandler<Publish1>>()
                .WithExternalDependencies(h => h.OptionsProvider = () => options)
                .ExpectPublish<Publish1>((message, publishOptions) =>
                {
                    capturedOptions = publishOptions;
                    return true;
                })
                .OnMessage<TestMessage>();

            Assert.AreSame(options, capturedOptions);
        }

        [Test]
        public void ShouldPassExpectNotPublishIfNotPublishing()
        {
            Test.Handler<EmptyHandler>()
                .ExpectNotPublish<Publish1>()
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ShouldPassExpectNotPublishWithCheckIfNotPublishing()
        {
            Test.Handler<EmptyHandler>()
                .ExpectNotPublish<Publish1>(m => true)
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ShouldPassExpectNotPublishProvidedPublishOptionsToCheck()
        {
            var options = new PublishOptions();
            PublishOptions capturedOptions = null;

            Test.Handler<PublishingHandler<Publish1>>()
                .WithExternalDependencies(h => h.OptionsProvider = () => options)
                .ExpectNotPublish<Publish1>((message, publishOptions) =>
                {
                    capturedOptions = publishOptions;
                    return false;
                })
                .OnMessage<TestMessage>();

            Assert.AreSame(options, capturedOptions);
        }

        [Test]
        public void ShouldPassExpectSendIfSending()
        {
            Test.Handler<SendingHandler<Send1>>()
                .ExpectSend<Send1>()
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ShouldPassExpectSendWithCheckIfSending()
        {
            Test.Handler<SendingHandler<Send1>>()
                .ExpectSend<Send1>(m => true)
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ShouldFailExpectSendIfNotSending()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<EmptyHandler>()
                .ExpectSend<Send1>()
                .OnMessage<TestMessage>());
        }

        [Test]
        public void ShouldFailExpectSendWithCheckIfNotSending()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<EmptyHandler>()
                .ExpectSend<Send1>(m => true)
                .OnMessage<TestMessage>());
        }

        [Test]
        public void ShouldFailExpectSendIfSendingWithoutMatch()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<SendingHandler<Publish1>>()
                .ExpectSend<Send1>(m => true)
                .OnMessage<TestMessage>());
        }

        [Test]
        public void ShouldPassExpectSendProvidedSendOptionsToCheck()
        {
            var options = new SendOptions();
            SendOptions capturedOptions = null;

            Test.Handler<SendingHandler<Send1>>()
                .WithExternalDependencies(handler => handler.OptionsProvider = () => options)
                .ExpectSend<Send1>((message, sendOptions) =>
                {
                    capturedOptions = sendOptions;
                    return true;
                })
                .OnMessage<TestMessage>();

            Assert.AreSame(options, capturedOptions);
        }

        [Test]
        public void ShouldPassExpectNotSendIfNotSending()
        {
            Test.Handler<EmptyHandler>()
                .ExpectNotSend<Send1>()
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ShouldPassExpectNotSendWithCheckIfNotSending()
        {
            Test.Handler<EmptyHandler>()
                .ExpectNotSend<Send1>(m => true)
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ShouldFailExpectNotSendIfSending()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<SendingHandler<Send1>>()
                .ExpectNotSend<Send1>()
                .OnMessage<TestMessage>());
        }

        [Test]
        public void ShouldFailExpectNotSendWithCheckIfSending()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<SendingHandler<Send1>>()
                .ExpectNotSend<Send1>(m => true)
                .OnMessage<TestMessage>());
        }

        [Test]
        public void ShouldPassExpectNotSentProvidedSendOptionsToCheck()
        {
            var options = new SendOptions();
            SendOptions capturedOptions = null;

            Test.Handler<SendingHandler<Send1>>()
                .WithExternalDependencies(handler => handler.OptionsProvider = () => options)
                .ExpectNotSend<Send1>((message, sendOptions) =>
                {
                    capturedOptions = sendOptions;
                    return false;
                })
                .OnMessage<TestMessage>();

            Assert.AreSame(options, capturedOptions);
        }

        [Test]
        public void ShouldFailExpectSendLocalIfNotSendingLocal()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<SendingHandler<Send1>>()
                .ExpectSendLocal<Send1>()
                .OnMessage<TestMessage>());
        }

        [Test]
        public void ShouldFailExpectSendLocalWithCheckIfNotSendingLocal()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<SendingHandler<Send1>>()
                .ExpectSendLocal<Send1>(m => true)
                .OnMessage<TestMessage>());
        }

        [Test]
        public void ShouldPassExpectSendLocalIfSendingLocal()
        {
            Test.Handler<SendingLocalHandler<Send1>>()
                .ExpectSendLocal<Send1>()
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ShouldPassExpectSendLocalWithCheckIfSendingLocal()
        {
            Test.Handler<SendingLocalHandler<Send1>>()
                .ExpectSendLocal<Send1>(m => true)
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ShouldFailExpectSendLocalIfSendingLocalWithoutMatch()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<SendingLocalHandler<Publish1>>()
                .ExpectSendLocal<Send1>(m => true)
                .OnMessage<TestMessage>());
        }

        [Test]
        public void ShouldFailExpectSendLocalIfSendingLocalWithFailingCheck()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<SendingLocalHandler<Send1>>()
                .ExpectSendLocal<Send1>(m => false)
                .OnMessage<TestMessage>());
        }

        [Test]
        public void ShouldPassExpectNotSendLocalIfNotSendingLocal()
        {
            Test.Handler<EmptyHandler>()
                .ExpectNotSendLocal<Send1>(m => true)
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ShouldFailExpectNotSendLocalIfSendingLocal()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<SendingLocalHandler<Send1>>()
                .ExpectNotSendLocal<Send1>(m => true)
                .OnMessage<TestMessage>());
        }

        [Test]
        public void ShouldPassExpectNotSendLocalIfSendingLocalWithoutMatch()
        {
            Test.Handler<SendingLocalHandler<Publish1>>()
                .ExpectNotSendLocal<Send1>(m => true)
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ShouldPassExpectNotSendLocalIfSendingLocalWithFailingCheck()
        {
            Test.Handler<SendingLocalHandler<Send1>>()
                .ExpectNotSendLocal<Send1>(m => false)
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ShouldFailExpectPublishIfPublishWrongMessageType()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<PublishingHandler<Publish1>>()
                .ExpectPublish<Publish2>(m => true)
                .OnMessage<TestMessage>());
        }

        [Test]
        public void ShouldPassExpectNotPublishIfPublishWrongMessageType()
        {
            Test.Handler<PublishingHandler<Publish1>>()
                .ExpectNotPublish<Publish2>(m => true)
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ShouldSupportDataBusProperties()
        {
            Test.Handler<DataBusMessageHandler>()
                .ExpectNotPublish<Publish2>(m => true)
                .OnMessage<MessageWithDataBusProperty>();
        }

        [Test]
        public void ShouldSupportSendingManyMessagesAtOnce()
        {
            Test.Handler<SendingManyHandler>()
                .ExpectSend<Outgoing>(m => m.Number == 1)
                .OnMessage<Incoming>();

            Test.Handler<SendingManyHandler>()
                .ExpectSend<Outgoing>(m => m.Number == 2)
                .OnMessage<Incoming>();
        }

        [Test]
        public void ShouldSupportSendingDifferentMessagesAtOnce()
        {
            var result = 0;

            Test.Handler<SendingManyWithDifferentMessagesHandler>()
                .ExpectSend<Outgoing>(m =>
                {
                    result += m.Number;
                    return true;
                })
                .ExpectSend<Outgoing2>(m =>
                {
                    result += m.Number;
                    return true;
                })
                .OnMessage<Incoming>();

            Assert.AreEqual(3, result);
        }

        [Test]
        public void ShouldSupportPublishMoreThanOneMessageAtOnce()
        {
            Test.Handler<PublishingManyHandler>()
                .ExpectPublish<Outgoing>(m => true)
                .ExpectPublish<Outgoing>(m => true)
                .OnMessage<Incoming>();
        }

        [Test]
        public void Should_be_able_to_pass_an_already_constructed_message_into_handler_without_specifying_id()
        {
            const string expected = "dummy";

            var handler = new TestMessageWithPropertiesHandler();
            var message = new TestMessageWithProperties { Dummy = expected };
            Test.Handler(handler)
              .OnMessage(message);
            Assert.AreEqual(expected, handler.ReceivedDummyValue);
            Assert.DoesNotThrow(() => Guid.Parse(handler.AssignedMessageId), "Message ID should be a valid GUID.");
        }

        [Test]
        public void ShouldFailExpectForwardCurrentMessageToIfMessageNotForwarded()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<NotForwardingMessageHandler>()
                .ExpectForwardCurrentMessageTo()
                .OnMessage<TestMessage>());
        }

        [Test]
        public void ShouldFailExpectForwardCurrentMessageWithCheckToIfMessageNotForwarded()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<NotForwardingMessageHandler>()
                .ExpectForwardCurrentMessageTo(dest => true)
                .OnMessage<TestMessage>());
        }

        [Test]
        public void ShouldFailExpectForwardCurrentMessageToIfMessageForwardedToUnexpectedDestination()
        {
            var handler = new ForwardingMessageHandler("someOtherDestination");

            Assert.Throws<ExpectationException>(() => Test.Handler(handler)
                .ExpectForwardCurrentMessageTo(dest => dest == "expectedDestination")
                .OnMessage<TestMessage>());
        }

        [Test]
        public void ShouldPassExpectForwardCurrentMessageToIfMessageForwarded()
        {
            var handler = new ForwardingMessageHandler("somewhere");

            Test.Handler(handler)
                .ExpectForwardCurrentMessageTo()
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ShouldPassExpectForwardCurrentMessageToIfMessageForwardedToExpectedDestination()
        {
            const string forwardingDestination = "expectedDestination";
            var handler = new ForwardingMessageHandler(forwardingDestination);

            Test.Handler(handler)
                .ExpectForwardCurrentMessageTo(dest => dest == forwardingDestination)
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ShouldPassExpectNotForwardCurrentMessageToIfMessageNotForwarded()
        {
            Test.Handler<NotForwardingMessageHandler>()
                .ExpectNotForwardCurrentMessageTo()
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ShouldPassExpectNotForwardCurrentMessageToWithCheckIfMessageNotForwarded()
        {
            Test.Handler<NotForwardingMessageHandler>()
                .ExpectNotForwardCurrentMessageTo(dest => true)
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ShouldFailExpectNotForwardCurrentMessageToIfMessageForwardedToAnyDestination()
        {
            var handler = new ForwardingMessageHandler("somewhere");

            Assert.Throws<ExpectationException>(() => Test.Handler(handler)
                .ExpectNotForwardCurrentMessageTo()
                .OnMessage<TestMessage>());
        }

        [Test]
        public void ShouldFailExpectNotForwardCurrentMessageToIfMessageForwardedToExpectedDestination()
        {
            const string forwardingDestination = "expectedDestination";
            var handler = new ForwardingMessageHandler(forwardingDestination);

            Assert.Throws<ExpectationException>(() => Test.Handler(handler)
                .ExpectNotForwardCurrentMessageTo(dest => dest == forwardingDestination)
                .OnMessage<TestMessage>());
        }

        [Test]
        public void ShouldPassExpectNotForwardCurrentMessageToIfMessageForwardedToUnexpectedDestination()
        {
            var handler = new ForwardingMessageHandler("someOtherDestination");

            Test.Handler(handler)
                .ExpectNotForwardCurrentMessageTo(dest => dest == "expectedDestination")
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ExpectForwardCurrentMessageToShouldSupportMultipleForwardedMessages()
        {
            Test.Handler<MultipleForwardingsMessageHandler>()
                .ExpectForwardCurrentMessageTo(dest => dest == "dest1")
                .ExpectForwardCurrentMessageTo(dest => dest == "dest2")
                .ExpectNotForwardCurrentMessageTo(dest => dest == "dest3")
                .OnMessage<TestMessage>();
        }

        [Test]
        public void SendMessageWithMultiIncomingHeaders()
        {
            var command = new MyCommand();

            Test.Handler<MyCommandHandler>()
                .SetIncomingHeader("Key1", "Header1")
                .SetIncomingHeader("Key2", "Header2")
                .OnMessage(command);

            Assert.AreEqual("Header1", command.Header1);
            Assert.AreEqual("Header2", command.Header2);
        }

        [Test]
        public void OnMessageShouldAwaitAsyncTasks()
        {
            Test.Handler<AsyncHandler>()
                .ExpectSend<Send1>(m => true)
                .OnMessage<MyCommand>();
        }

        class MyCommand : ICommand
        {
            public string Header1 { get; set; }
            public string Header2 { get; set; }
        }

        class MyCommandHandler : IHandleMessages<MyCommand>
        {
            public Task Handle(MyCommand message, IMessageHandlerContext context)
            {
                message.Header1 = context.MessageHeaders["Key1"];
                message.Header2 = context.MessageHeaders["Key2"];

                return Task.FromResult(0);
            }
        }

        class TestMessageImpl : TestMessage
        {
        }

        public class EmptyHandler : IHandleMessages<TestMessage>
        {
            public Task Handle(TestMessage message, IMessageHandlerContext context)
            {
                return Task.FromResult(0);
            }
        }

        class AsyncHandler : IHandleMessages<MyCommand>
        {
            public async Task Handle(MyCommand message, IMessageHandlerContext context)
            {
                await Task.Yield();
                await context.Send<Send1>(m => { }, new SendOptions());
            }
        }

        public interface Publish1 : IMessage
        {
            string Data { get; set; }
        }

        public interface Send1 : IMessage
        {
            string Data { get; set; }
        }

        public interface Publish2 : IMessage
        {
            string Data { get; set; }
        }

        public class Outgoing : IMessage
        {
            public int Number { get; set; }
        }

        public class Outgoing2 : IMessage
        {
            public int Number { get; set; }
        }

        public class Incoming : IMessage
        {
        }

        public class DeferringTimeSpanHandler : IHandleMessages<TestMessage>
        {
            public TimeSpan Defer { get; set; }

            public Task Handle(TestMessage message, IMessageHandlerContext context)
            {
                var sendOptions = new SendOptions();
                sendOptions.DelayDeliveryWith(Defer);

                return context.Send(message, sendOptions);
            }
        }

        public class DeferringDateTimeHandler : IHandleMessages<TestMessage>
        {
            public DateTime Defer { get; set; }

            public Task Handle(TestMessage message, IMessageHandlerContext context)
            {
                var sendOptions = new SendOptions();
                sendOptions.DoNotDeliverBefore(Defer);

                return context.Send(message, sendOptions);
            }
        }

        public class PublishingManyHandler : IHandleMessages<Incoming>
        {
            public async Task Handle(Incoming message, IMessageHandlerContext context)
            {
                await context.Publish<Outgoing>(m => { m.Number = 1; });

                await context.Publish<Outgoing>(m => { m.Number = 2; });
            }
        }

        public class SendingManyWithDifferentMessagesHandler : IHandleMessages<Incoming>
        {
            public async Task Handle(Incoming message, IMessageHandlerContext context)
            {
                await context.Send<Outgoing>(m => { m.Number = 1; });

                await context.Send<Outgoing2>(m => { m.Number = 2; });
            }
        }

        public class SendingManyHandler : IHandleMessages<Incoming>
        {
            public async Task Handle(Incoming message, IMessageHandlerContext context)
            {
                await context.Send<Outgoing>(m => { m.Number = 1; });

                await context.Send<Outgoing>(m => { m.Number = 2; });
            }
        }

        public class SendingHandler<TSend> : IHandleMessages<TestMessage>
            where TSend : IMessage
        {
            public Action<TSend> ModifyMessage { get; set; } = m => { };

            public Func<SendOptions> OptionsProvider { get; set; } = () => new SendOptions();

            public Task Handle(TestMessage message, IMessageHandlerContext context)
            {
                return context.Send(ModifyMessage, OptionsProvider());
            }
        }

        public class SendingLocalHandler<TSend> : IHandleMessages<TestMessage>
            where TSend : IMessage
        {
            public Action<TSend> ModifyMessage { get; set; } = m => { };

            public Task Handle(TestMessage message, IMessageHandlerContext context)
            {
                return context.SendLocal(ModifyMessage);
            }
        }

        public class PublishingHandler<TPublish> : IHandleMessages<TestMessage>
            where TPublish : IMessage
        {
            public Action<TPublish> ModifyMessage { get; set; } = m => { };

            public Func<PublishOptions> OptionsProvider { get; set; } = () => new PublishOptions();

            public Task Handle(TestMessage message, IMessageHandlerContext context)
            {
                return context.Publish(ModifyMessage, OptionsProvider());
            }
        }

        public class PublishingHandler<TPublish1, TPublish2> : IHandleMessages<TestMessage>
            where TPublish1 : IMessage
            where TPublish2 : IMessage
        {
            public Action<TPublish1> ModifyPublish1 { get; set; } = m => { };
            public Action<TPublish2> ModifyPublish2 { get; set; } = m => { };

            public async Task Handle(TestMessage message, IMessageHandlerContext context)
            {
                await context.Publish(ModifyPublish1);
                await context.Publish(ModifyPublish2);
            }
        }

        public class DoNotContinueDispatchingCurrentMessageToHandlersHandler : IHandleMessages<TestMessage>
        {
            public Task Handle(TestMessage message, IMessageHandlerContext context)
            {
                context.DoNotContinueDispatchingCurrentMessageToHandlers();

                return Task.FromResult(0);
            }
        }

        public class HandleCurrentMessageLaterHandler : IHandleMessages<TestMessage>
        {
            public Task Handle(TestMessage message, IMessageHandlerContext context)
            {
                return context.HandleCurrentMessageLater();
            }
        }

        public class ExplicitInterfaceImplementation : IHandleMessages<TestMessage>
        {
            public bool IsHandled { get; set; }

            Task IHandleMessages<TestMessage>.Handle(TestMessage message, IMessageHandlerContext context)
            {
                IsHandled = true;

                return Task.FromResult(0);
            }

            // Unit test fails if this is uncommented; seems to me that this should
            // be made to pass, but it looks like a design decision based on commit
            // revision 1210.
            //public void Handle(TestMessage message) {
            //    throw new System.Exception("Shouldn't call this.");
            //}

        }

        public class NotForwardingMessageHandler : IHandleMessages<TestMessage>
        {
            public Task Handle(TestMessage message, IMessageHandlerContext context)
            {
                return Task.FromResult(0);
            }
        }

        public class ForwardingMessageHandler : IHandleMessages<TestMessage>
        {
            readonly string destination;

            public ForwardingMessageHandler(string destination)
            {
                this.destination = destination;
            }

            public Task Handle(TestMessage message, IMessageHandlerContext context)
            {
                return context.ForwardCurrentMessageTo(destination);
            }
        }

        public class MultipleForwardingsMessageHandler : IHandleMessages<TestMessage>
        {
            public async Task Handle(TestMessage message, IMessageHandlerContext context)
            {
                await context.ForwardCurrentMessageTo("dest1");
                await context.ForwardCurrentMessageTo("dest2");
            }
        }

        public class ReplyingHandler : IHandleMessages<MyRequest>
        {
            public Func<ReplyOptions> OptionsProvider { get; set; } = () => new ReplyOptions();

            public Task Handle(MyRequest message, IMessageHandlerContext context)
            {
                return message.ShouldReply ? context.Reply(new MyReply { String = message.String }, OptionsProvider()) : Task.FromResult(0);
            }
        }
    }

    public class DataBusMessageHandler : IHandleMessages<MessageWithDataBusProperty>
    {
        public Task Handle(MessageWithDataBusProperty message, IMessageHandlerContext context)
        {
            return Task.FromResult(0);
        }
    }

    public interface TestMessage : IMessage
    {
    }

    public class MessageWithDataBusProperty : IMessage
    {
    }

    public class TestMessageWithPropertiesHandler : IHandleMessages<TestMessageWithProperties>
    {
        public string ReceivedDummyValue;
        public string AssignedMessageId;

        public Task Handle(TestMessageWithProperties message, IMessageHandlerContext context)
        {
            ReceivedDummyValue = message.Dummy;
            AssignedMessageId = context.MessageId;
            return Task.FromResult(0);
        }
    }

    public class TestMessageWithProperties : IMessage
    {
        public string Dummy { get; set; }
    }
}

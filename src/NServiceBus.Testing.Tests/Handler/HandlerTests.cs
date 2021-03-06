namespace NServiceBus.Testing.Tests.Handler
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using Saga;
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
        public void ShouldCallHandleOnExplicitInterfaceImplementation()
        {
            var handler = new ExplicitInterfaceImplementation();
            Assert.IsFalse(handler.IsHandled);
            Test.Handler(handler).OnMessage<TestMessage>();
            Assert.IsTrue(handler.IsHandled);
        }

        [Test]
        public void Should_be_able_to_pass_an_already_constructed_message_into_handler_without_specifying_id()
        {
            const string expected = "dummy";

            var handler = new TestMessageWithPropertiesHandler();
            var message = new TestMessageWithProperties
            {
                Dummy = expected
            };
            Test.Handler(handler)
                .OnMessage(message);
            Assert.AreEqual(expected, handler.ReceivedDummyValue);
            Assert.DoesNotThrow(() => Guid.Parse(handler.AssignedMessageId), "Message ID should be a valid GUID.");
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
        public void ShouldBeAbleToConfigureMessageHandlerContext()
        {
            var messageId = Guid.NewGuid().ToString();
            var replyToAddress = "0118 999 881 999 119 725 3";
            var handler = new ContextAccessingHandler();
            TestableMessageHandlerContext contextInstance = null;

            Test.Handler(handler)
                .ConfigureHandlerContext(c =>
                {
                    c.MessageId = messageId;
                    c.ReplyToAddress = replyToAddress;
                    contextInstance = c;
                })
                .OnMessage<TestMessage>();

            Assert.AreEqual(messageId, handler.Context.MessageId);
            Assert.AreEqual(replyToAddress, handler.Context.ReplyToAddress);
            Assert.AreSame(contextInstance, handler.Context);
        }

        [Test]
        public void OnMessageShouldAwaitAsyncTasks()
        {
            Test.Handler<AsyncHandler>()
                .ExpectSend<Send1>(m => true)
                .OnMessage<MyCommand>();
        }
    }

    class MyCommand : ICommand
    {
        public string Header1 { get; set; }
        public string Header2 { get; set; }
    }

    public class ContextAccessingHandler : IHandleMessages<TestMessage>
    {
        public IMessageHandlerContext Context { get; private set; }

        public Task Handle(TestMessage message, IMessageHandlerContext context)
        {
            Context = context;
            return Task.FromResult(0);
        }
    }

    public class EmptyHandler : IHandleMessages<TestMessage>
    {
        public Task Handle(TestMessage message, IMessageHandlerContext context)
        {
            return Task.FromResult(0);
        }
    }

    class ConcurrentHandler : IHandleMessages<MyCommand>
    {
        public int NumberOfThreads { get; set; } = 100;

        public Task Handle(MyCommand message, IMessageHandlerContext context)
        {
            var operations = new ConcurrentBag<Task>();

            Parallel.For(0, NumberOfThreads, x => operations.Add(HandlerAction(context)));

            return Task.WhenAll(operations);
        }

        public Func<IMessageHandlerContext, Task> HandlerAction = x => Task.FromResult(0);
    }

    public interface TestMessage : IMessage
    {
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

    public class ReplyingHandler : IHandleMessages<MyRequest>
    {
        public Func<ReplyOptions> OptionsProvider { get; set; } = () => new ReplyOptions();

        public Task Handle(MyRequest message, IMessageHandlerContext context)
        {
            return message.ShouldReply ? context.Reply(new MyReply
            {
                String = message.String
            }, OptionsProvider()) : Task.FromResult(0);
        }
    }

    public class TestMessageWithPropertiesHandler : IHandleMessages<TestMessageWithProperties>
    {
        public Task Handle(TestMessageWithProperties message, IMessageHandlerContext context)
        {
            ReceivedDummyValue = message.Dummy;
            AssignedMessageId = context.MessageId;
            return Task.FromResult(0);
        }

        public string ReceivedDummyValue;
        public string AssignedMessageId;
    }

    public class TestMessageWithProperties : IMessage
    {
        public string Dummy { get; set; }
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

    class AsyncHandler : IHandleMessages<MyCommand>
    {
        public async Task Handle(MyCommand message, IMessageHandlerContext context)
        {
            await Task.Yield();
            await context.Send<Send1>(m => { }, new SendOptions());
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
    }
}
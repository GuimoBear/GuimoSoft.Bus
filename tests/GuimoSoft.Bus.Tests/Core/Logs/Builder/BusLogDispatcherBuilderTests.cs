using DeepEqual.Syntax;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Internal;
using GuimoSoft.Bus.Core.Logs;
using GuimoSoft.Bus.Core.Logs.Builder;
using GuimoSoft.Bus.Core.Logs.Builder.Stages;
using GuimoSoft.Bus.Tests.Fakes;
using Xunit;

namespace GuimoSoft.Bus.Tests.Core.Logs.Builder
{
    public class BusLogDispatcherBuilderTests
    {
        [Fact]
        public void ConstructorShouldCreateBusLogDispatcherBuilder()
        {
            var sut = new BusLogDispatcherBuilder(Mock.Of<IMediator>(), BusName.Kafka);
            Assert.IsType<BusLogDispatcherBuilder>(sut);
        }

        [Fact]
        public void ConstructorShouldThrowIfAnyParameterIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new BusLogDispatcherBuilder(null, BusName.Kafka));
        }

        [Fact]
        public void PublishAnLogWithoutMessageObjectFacts()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                var expectedLogMessage = new BusLogMessage(ServerName.Default)
                {
                    Bus = BusName.Kafka,
                    Finality = Finality.Consume,
                    Endpoint = "test",
                    Message = "test message",
                    Level = BusLogLevel.Information
                };
                expectedLogMessage.Data.Add("key-1", "value-1");

                var moqMediator = new Mock<IMediator>();

                ISwitchStage sut = new BusLogDispatcherBuilder(moqMediator.Object, BusName.Kafka);

                sut
                    .AndSwitch(ServerName.Default).AndFinality(Finality.Consume)
                    .WhileListening().TheEndpoint("test")
                    .Write()
                        .Message("test message")
                        .AndKey("key-1").FromValue("value-1")
                        .With(BusLogLevel.Information)
                    .Publish().AnLog();

                moqMediator
                    .Verify(x => x.Publish(It.Is<BusLogMessage>(actual => IsEqual(expectedLogMessage, actual)), It.IsAny<CancellationToken>()), Times.Once);
                moqMediator
                    .Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
            }
        }

        [Fact]
        public void PublishAnLogWithMessageObjectFacts()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                var expectedLogMessage = new BusLogMessage(FakeServerName.FakeHost1)
                {
                    Bus = BusName.Kafka,
                    Finality = Finality.Produce,
                    Endpoint = "test",
                    Message = "test message",
                    Level = BusLogLevel.Information
                };
                expectedLogMessage.Data.Add("key-1", "value-1");

                var fakeMessage = new FakeMessage("", "");

                var expectedTypedLogMessage = new BusTypedLogMessage<FakeMessage>(expectedLogMessage, fakeMessage);

                var moqMediator = new Mock<IMediator>();

                ISwitchStage sut = new BusLogDispatcherBuilder(moqMediator.Object, BusName.Kafka);

                sut
                    .AndSwitch(FakeServerName.FakeHost1).AndFinality(Finality.Produce)
                    .AfterReceived().TheObject(fakeMessage).FromEndpoint("test")
                    .Write()
                        .Message("test message")
                        .AndKey("key-1").FromValue("value-1")
                        .With(BusLogLevel.Information)
                    .Publish().AnLog();

                moqMediator
                    .Verify(x => x.Publish(It.Is<BusLogMessage>(actual => IsEqual(expectedLogMessage, actual)), It.IsAny<CancellationToken>()), Times.Once);
                moqMediator
                    .Verify(x => x.Publish(It.Is<object>((obj, _) => IsEqual(expectedTypedLogMessage, obj)), It.IsAny<CancellationToken>()), Times.Never);
            }
        }

        [Fact]
        public void PublishAnLogWithMessageObjectAndTypedHandlerFacts()
        {
            lock (Utils.Lock)
            {
                var sc = new ServiceCollection();
                sc.RegisterMediatorFromNewAssemblies(new List<Assembly> { typeof(FakeMessage).Assembly });

                var expectedLogMessage = new BusLogMessage(FakeServerName.FakeHost1)
                {
                    Bus = BusName.Kafka,
                    Finality = Finality.Produce,
                    Endpoint = "test",
                    Message = "test message",
                    Level = BusLogLevel.Information
                };
                expectedLogMessage.Data.Add("key-1", "value-1");

                var fakeMessage = new FakeMessage("", "");

                var expectedTypedLogMessage = new BusTypedLogMessage<FakeMessage>(expectedLogMessage, fakeMessage);

                var moqMediator = new Mock<IMediator>();

                ISwitchStage sut = new BusLogDispatcherBuilder(moqMediator.Object, BusName.Kafka);

                sut
                    .AndSwitch(FakeServerName.FakeHost1).AndFinality(Finality.Produce)
                    .AfterReceived().TheObject(fakeMessage).FromEndpoint("test")
                    .Write()
                        .Message("test message")
                        .AndKey("key-1").FromValue("value-1")
                        .With(BusLogLevel.Information)
                    .Publish().AnLog();

                moqMediator
                    .Verify(x => x.Publish(It.Is<BusLogMessage>(actual => IsEqual(expectedLogMessage, actual)), It.IsAny<CancellationToken>()), Times.Never);
                moqMediator
                    .Verify(x => x.Publish(It.Is<object>((obj, _) => IsEqual(expectedTypedLogMessage, obj)), It.IsAny<CancellationToken>()), Times.Once);

                Utils.ResetarSingletons();
            }
        }

        [Fact]
        public void PublishAnLogWithNullMessageObjectFacts()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                var expectedLogMessage = new BusLogMessage(ServerName.Default)
                {
                    Bus = BusName.Kafka,
                    Finality = Finality.Consume,
                    Endpoint = "test",
                    Message = "test message",
                    Level = BusLogLevel.Information
                };
                expectedLogMessage.Data.Add("key-1", "value-1");

                var expectedTypedLogMessage = new BusTypedLogMessage<FakeMessage>(expectedLogMessage, null);

                var moqMediator = new Mock<IMediator>();

                ISwitchStage sut = new BusLogDispatcherBuilder(moqMediator.Object, BusName.Kafka);

                sut
                    .AndSwitch(ServerName.Default).AndFinality(Finality.Consume)
                    .AfterReceived().TheObject(null).FromEndpoint("test")
                    .Write()
                        .Message("test message")
                        .AndKey("key-1").FromValue("value-1")
                        .With(BusLogLevel.Information)
                    .Publish().AnLog();

                moqMediator
                    .Verify(x => x.Publish(It.Is<BusLogMessage>(actual => IsEqual(expectedLogMessage, actual)), It.IsAny<CancellationToken>()), Times.Once);
                moqMediator
                    .Verify(x => x.Publish(It.Is<object>((obj, _) => IsEqual(expectedTypedLogMessage, obj)), It.IsAny<CancellationToken>()), Times.Never);
            }
        }

        [Fact]
        public void PublishAnExceptionWithoutMessageObjectFacts()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                var expectedExceptionMessage = new BusExceptionMessage(ServerName.Default, new Exception(""))
                {
                    Bus = BusName.Kafka,
                    Finality = Finality.Produce,
                    Endpoint = "test",
                    Message = "test message",
                    Level = BusLogLevel.Error
                };
                expectedExceptionMessage.Data.Add("key-1", "value-1");

                var moqMediator = new Mock<IMediator>();

                var sut = new BusLogDispatcherBuilder(moqMediator.Object, BusName.Kafka)
                    .AndSwitch(ServerName.Default).AndFinality(Finality.Produce)
                    .WhileListening().TheEndpoint("test")
                    .Write()
                        .Message("test message")
                        .AndKey("key-1").FromValue("value-1")
                        .With(BusLogLevel.Error)
                    .Publish();

                Assert.Throws<ArgumentNullException>(() => sut.AnException(null).ConfigureAwait(false).GetAwaiter().GetResult());

                sut.AnException(new Exception(""));

                moqMediator
                    .Verify(x => x.Publish(It.Is<BusExceptionMessage>(actual => IsEqual(expectedExceptionMessage, actual)), It.IsAny<CancellationToken>()), Times.Once);
                moqMediator
                    .Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
            }
        }

        [Fact]
        public void PublishAnExceptionWithMessageObjectFacts()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                var expectedExceptionMessage = new BusExceptionMessage(FakeServerName.FakeHost1, new Exception(""))
                {
                    Bus = BusName.Kafka,
                    Finality = Finality.Consume,
                    Endpoint = "test",
                    Message = "test message",
                    Level = BusLogLevel.Error
                };
                expectedExceptionMessage.Data.Add("key-1", "value-1");

                var fakeMessage = new FakeMessage("", "");

                var expectedTypedExceptionMessage = new BusTypedExceptionMessage<FakeMessage>(expectedExceptionMessage, fakeMessage);

                var moqMediator = new Mock<IMediator>();

                ISwitchStage sut = new BusLogDispatcherBuilder(moqMediator.Object, BusName.Kafka);

                sut
                    .AndSwitch(FakeServerName.FakeHost1).AndFinality(Finality.Consume)
                    .AfterReceived().TheObject(fakeMessage).FromEndpoint("test")
                    .Write()
                        .Message("test message")
                        .AndKey("key-1").FromValue("value-1")
                        .With(BusLogLevel.Error)
                    .Publish().AnException(new Exception(""));

                moqMediator
                    .Verify(x => x.Publish(It.Is<BusExceptionMessage>(actual => IsEqual(expectedExceptionMessage, actual)), It.IsAny<CancellationToken>()), Times.Once);
                moqMediator
                    .Verify(x => x.Publish(It.Is<object>((obj, _) => IsEqual(expectedTypedExceptionMessage, obj)), It.IsAny<CancellationToken>()), Times.Never);
            }
        }

        [Fact]
        public void PublishAnExceptionWithMessageObjectAndTypedHandlerFacts()
        {
            lock (Utils.Lock)
            {
                var sc = new ServiceCollection();
                sc.RegisterMediatorFromNewAssemblies(new List<Assembly> { typeof(FakeMessage).Assembly });

                var expectedExceptionMessage = new BusExceptionMessage(FakeServerName.FakeHost1, new Exception(""))
                {
                    Bus = BusName.Kafka,
                    Finality = Finality.Consume,
                    Endpoint = "test",
                    Message = "test message",
                    Level = BusLogLevel.Error
                };
                expectedExceptionMessage.Data.Add("key-1", "value-1");

                var fakeMessage = new FakeMessage("", "");

                var expectedTypedExceptionMessage = new BusTypedExceptionMessage<FakeMessage>(expectedExceptionMessage, fakeMessage);

                var moqMediator = new Mock<IMediator>();

                ISwitchStage sut = new BusLogDispatcherBuilder(moqMediator.Object, BusName.Kafka);

                sut
                    .AndSwitch(FakeServerName.FakeHost1).AndFinality(Finality.Consume)
                    .AfterReceived().TheObject(fakeMessage).FromEndpoint("test")
                    .Write()
                        .Message("test message")
                        .AndKey("key-1").FromValue("value-1")
                        .With(BusLogLevel.Error)
                    .Publish().AnException(new Exception(""));

                moqMediator
                    .Verify(x => x.Publish(It.Is<BusExceptionMessage>(actual => IsEqual(expectedExceptionMessage, actual)), It.IsAny<CancellationToken>()), Times.Never);
                moqMediator
                    .Verify(x => x.Publish(It.Is<object>((obj, _) => IsEqual(expectedTypedExceptionMessage, obj)), It.IsAny<CancellationToken>()), Times.Once);
                Utils.ResetarSingletons();
            }
        }

        [Fact]
        public void PublishAnExceptionWithNullMessageObjectFacts()
        {
            lock (Utils.Lock)
            {
                Utils.ResetarSingletons();
                var expectedExceptionMessage = new BusExceptionMessage(ServerName.Default, new Exception(""))
                {
                    Bus = BusName.Kafka,
                    Finality = Finality.Produce,
                    Endpoint = "test",
                    Message = "test message",
                    Level = BusLogLevel.Error
                };
                expectedExceptionMessage.Data.Add("key-1", "value-1");

                var expectedTypedExceptionMessage = new BusTypedExceptionMessage<FakeMessage>(expectedExceptionMessage, null);

                var moqMediator = new Mock<IMediator>();

                ISwitchStage sut = new BusLogDispatcherBuilder(moqMediator.Object, BusName.Kafka);

                sut
                    .AndSwitch(ServerName.Default).AndFinality(Finality.Produce)
                    .AfterReceived().TheObject(null).FromEndpoint("test")
                    .Write()
                        .Message("test message")
                        .AndKey("key-1").FromValue("value-1")
                        .With(BusLogLevel.Error)
                    .Publish().AnException(new Exception(""));

                moqMediator
                    .Verify(x => x.Publish(It.Is<BusExceptionMessage>(actual => IsEqual(expectedExceptionMessage, actual)), It.IsAny<CancellationToken>()), Times.Once);
                moqMediator
                    .Verify(x => x.Publish(It.Is<object>((obj, _) => IsEqual(expectedTypedExceptionMessage, obj)), It.IsAny<CancellationToken>()), Times.Never);
            }
        }

        private static bool IsEqual(BusLogMessage expected, object actual)
            => expected.IsDeepEqual(actual);

        private static bool IsEqual(BusTypedLogMessage<FakeMessage> expected, object actual)
            => expected.IsDeepEqual(actual);

        private static bool IsEqual(BusTypedExceptionMessage<FakeMessage> expected, object actual)
            => expected.IsDeepEqual(actual);

        private static bool IsEqual(BusExceptionMessage expected, object actual)
            => expected.IsDeepEqual(actual);
    }
}

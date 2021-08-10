using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Interfaces;
using GuimoSoft.Bus.Core.Internal;
using GuimoSoft.Bus.Tests.Fakes;
using Xunit;

namespace GuimoSoft.Bus.Tests.Consumer
{
    public class MessageMiddlewareManagerTests
    {
        [Fact]
        public void Dado_UmFakeMessageMiddleware_Se_RegistrarSemMetodoDeFabrica_Entao_TipoEhRegistradoNaCollection()
        {
            var serviceCollection = new ServiceCollection();

            var sut = new EventMiddlewareManager(serviceCollection);

            sut.Register<FakeMessage, FakeMessageMiddleware>(BusName.Kafka, ServerName.Default, ServiceLifetime.Singleton);

            using (var prov = serviceCollection.BuildServiceProvider())
            {
                prov.GetService<FakeMessageMiddleware>()
                    .Should().NotBeNull();
            }
        }

        [Fact]
        public void Dado_UmFakeMessageMiddleware_Se_RegistrarComMetodoDeFabrica_Entao_TipoEhRegistradoNaCollection()
        {
            var serviceCollection = new ServiceCollection();

            var sut = new EventMiddlewareManager(serviceCollection);

            bool factoryMethodExecuted = false;
            Func<IServiceProvider, FakeMessageMiddleware> factory =
                prov =>
                {
                    factoryMethodExecuted = true;
                    return new FakeMessageMiddleware();
                };

            sut.Register<FakeMessage, FakeMessageMiddleware>(BusName.Kafka, ServerName.Default, factory, ServiceLifetime.Singleton);

            using (var prov = serviceCollection.BuildServiceProvider())
            {
                prov.GetService<FakeMessageMiddleware>()
                    .Should().NotBeNull();

                factoryMethodExecuted
                    .Should().BeTrue();
            }
        }

        [Fact]
        public void Dado_UmFakeMessageMiddleware_Se_RegistrarComMetodoDeFabricaDefault_Entao_TipoEhRegistradoNaCollection()
        {
            var serviceCollection = new ServiceCollection();

            var sut = new EventMiddlewareManager(serviceCollection);

            sut.Register<FakeMessage, FakeMessageMiddleware>(BusName.Kafka, ServerName.Default, default(Func<IServiceProvider, FakeMessageMiddleware>), ServiceLifetime.Singleton);

            using (var prov = serviceCollection.BuildServiceProvider())
            {
                prov.GetService<FakeMessageMiddleware>()
                    .Should().NotBeNull();
            }
        }

        [Fact]
        public void Dado_UmFakeMessageMiddleware_Se_RegistrarSemMetodoDeFabricaETentarAdicionaloNovamente_Entao_EstouraExcecaoNaSegundaTentativa()
        {
            var serviceCollection = new ServiceCollection();

            var sut = new EventMiddlewareManager(serviceCollection);

            sut.Register<FakeMessage, FakeMessageMiddleware>(BusName.Kafka, ServerName.Default, ServiceLifetime.Singleton);

            Assert.Throws<InvalidOperationException>(() => sut.Register<FakeMessage, FakeMessageMiddleware>(BusName.Kafka, ServerName.Default, ServiceLifetime.Singleton))
                .Message.Should().Be($"Não foi possível registrar o middleware do tipo '{typeof(FakeMessageMiddleware).FullName}'");

            using (var prov = serviceCollection.BuildServiceProvider())
            {
                prov.GetService<FakeMessageMiddleware>()
                    .Should().NotBeNull();
            }
        }

        [Fact]
        public async Task Dado_TresMiddlewares_Se_GetPipeline_Entao_ExecutaACriacaoERetornaComSucesso()
        {
            var serviceCollection = new ServiceCollection();

            var sut = new EventMiddlewareManager(serviceCollection);

            serviceCollection.AddSingleton<IEventMiddlewareExecutorProvider>(sut);
            serviceCollection.AddSingleton(typeof(IConsumeContextAccessor<>), typeof(ConsumeContextAccessor<>));
            serviceCollection.AddSingleton(typeof(ConsumeContextAccessorInitializerMiddleware<>));

            sut.Register<FakePipelineMessage, FakePipelineMessageMiddlewareOne>(BusName.Kafka, ServerName.Default, ServiceLifetime.Singleton);
            sut.Register<FakePipelineMessage, FakePipelineMessageMiddlewareTwo>(BusName.Kafka, ServerName.Default, ServiceLifetime.Singleton);
            sut.Register<FakePipelineMessage, FakePipelineMessageMiddlewareThree>(BusName.Kafka, ServerName.Default, ServiceLifetime.Singleton);

            using (var prov = serviceCollection.BuildServiceProvider())
            {
                prov.GetService<FakePipelineMessageMiddlewareOne>()
                    .Should().NotBeNull();
                prov.GetService<FakePipelineMessageMiddlewareTwo>()
                    .Should().NotBeNull();
                prov.GetService<FakePipelineMessageMiddlewareThree>()
                    .Should().NotBeNull();

                var executorProvider = prov.GetService<IEventMiddlewareExecutorProvider>();

                executorProvider
                    .Should().NotBeNull();

                var pipeline = executorProvider.GetPipeline(BusName.Kafka, ServerName.Default, typeof(FakePipelineMessage));

                pipeline
                    .Should().NotBeNull();

                await pipeline.Execute(new FakePipelineMessage(FakePipelineMessageMiddlewareOne.Name), prov, new ConsumeInformations(BusName.Kafka, ServerName.Default, "a"), CancellationToken.None);
            }
        }

        [Fact]
        public void Dado_UmFakeMessageMiddleware_Se_RegistrarComMetodoDeFabricaEAdicionaOutroMiddlewareSemFabrica_Entao_TipoEhRegistradoNaCollectionApenasUmaVez()
        {
            var serviceCollection = new ServiceCollection();

            var sut = new EventMiddlewareManager(serviceCollection);
            bool factoryMethodExecuted = false;
            Func<IServiceProvider, FakeMessageMiddleware> factory =
                prov =>
                {
                    factoryMethodExecuted = true;
                    return new FakeMessageMiddleware();
                };

            bool factoryTwoMethodExecuted = false;
            Func<IServiceProvider, FakeMessageThrowExceptionMiddleware> factoryTwo =
                prov =>
                {
                    factoryTwoMethodExecuted = true;
                    return new FakeMessageThrowExceptionMiddleware();
                };

            sut.Register<FakeMessage, FakeMessageMiddleware>(BusName.Kafka, ServerName.Default, factory, ServiceLifetime.Singleton);

            sut.Register<FakeMessage, FakeMessageThrowExceptionMiddleware>(BusName.Kafka, ServerName.Default, factoryTwo, ServiceLifetime.Singleton);

            using (var prov = serviceCollection.BuildServiceProvider())
            {
                prov.GetService<FakeMessageMiddleware>()
                    .Should().NotBeNull();

                factoryMethodExecuted
                    .Should().BeTrue();

                prov.GetService<FakeMessageThrowExceptionMiddleware>()
                    .Should().NotBeNull();

                factoryTwoMethodExecuted
                    .Should().BeTrue();
            }
        }

        [Fact]
        public void Dado_UmFakeMessageMiddleware_Se_RegistrarComMetodoDeFabricaETentarAdicionaloNovamente_Entao_EstouraExcecaoNaSegundaTentativa()
        {
            var serviceCollection = new ServiceCollection();

            var sut = new EventMiddlewareManager(serviceCollection);
            bool factoryMethodExecuted = false;
            Func<IServiceProvider, FakeMessageMiddleware> factory =
                prov =>
                {
                    factoryMethodExecuted = true;
                    return new FakeMessageMiddleware();
                };

            bool factoryTwoMethodExecuted = false;
            Func<IServiceProvider, FakeMessageMiddleware> factoryTwo =
                prov =>
                {
                    factoryTwoMethodExecuted = true;
                    return new FakeMessageMiddleware();
                };

            sut.Register<FakeMessage, FakeMessageMiddleware>(BusName.Kafka, ServerName.Default, factory, ServiceLifetime.Singleton);

            Assert.Throws<InvalidOperationException>(() => sut.Register<FakeMessage, FakeMessageMiddleware>(BusName.Kafka, ServerName.Default, factoryTwo, ServiceLifetime.Singleton))
                .Message.Should().Be($"Não foi possível registrar o middleware do tipo '{typeof(FakeMessageMiddleware).FullName}'");

            using (var prov = serviceCollection.BuildServiceProvider())
            {
                prov.GetService<FakeMessageMiddleware>()
                    .Should().NotBeNull();

                factoryMethodExecuted
                    .Should().BeTrue();

                factoryTwoMethodExecuted
                    .Should().BeFalse();
            }
        }
    }
}

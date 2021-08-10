using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Internal;
using GuimoSoft.Bus.Core.Logs.Builder.Stages;

namespace GuimoSoft.Bus.Core.Logs.Builder
{
    internal sealed class BusLogDispatcherBuilder :
        ISwitchStage,
        IFinalityStage, 
        IListeningStage,
        IEndpointStage,
        IMessageObjectInstance,
        IEndpointAfterMessageReceivedStage,
        IWriteStage,
        IMessageStage,
        ILogLevelAndDataStage,
        IKeyValueStage,
        IBeforePublishStage,
        IPublishStage
    {
        private readonly IMediator _mediator;

        private readonly BusName _bus;
        private Enum _switch;
        private Finality _finality;

        private Type _messageType;
        private object _messageObject;

        private string _endpoint;
        private string _message;
        private BusLogLevel _level;

        private string _currentDataKey;
        private readonly Dictionary<string, object> _data;

        internal BusLogDispatcherBuilder(IMediator mediator, BusName bus)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _bus = bus;
            _data = new();
        }

        public IFinalityStage AndSwitch(Enum @switch)
        {
            _switch = @switch;
            return this;
        }

        public IListeningStage AndFinality(Finality finality)
        {
            _finality = finality;
            return this;
        }

        public IEndpointStage WhileListening()
            => this;

        public IWriteStage TheEndpoint(string endpoint)
        {
            _endpoint = endpoint;
            return this;
        }

        public IMessageObjectInstance AfterReceived()
            => this;

        public IEndpointAfterMessageReceivedStage TheObject(object @object)
            => TheObject(@object?.GetType(), @object);

        public IEndpointAfterMessageReceivedStage TheObject(Type objectType, object @object)
        {
            _messageObject = @object;
            _messageType = objectType;
            return this;
        }

        public IWriteStage FromEndpoint(string endpoint)
            => TheEndpoint(endpoint);

        public IMessageStage Write()
            => this;

        public ILogLevelAndDataStage Message(string message)
        {
            _message = message;
            return this;
        }

        public IKeyValueStage AndKey(string key)
        {
            _currentDataKey = key;
            return this;
        }

        public ILogLevelAndDataStage FromValue(object value)
        {
            _data[_currentDataKey] = value;
            return this;
        }

        public IBeforePublishStage With(BusLogLevel level)
        {
            _level = level;
            return this;
        }

        public IPublishStage Publish()
            => this;

        public async Task AnLog(CancellationToken cancellationToken = default)
        {
            var logMessage = new BusLogMessage(_switch)
            {
                Bus = _bus,
                Finality = _finality,
                Endpoint = _endpoint,
                Message = _message,
                Level = _level
            };

            foreach (var (key, value) in _data)
                logMessage.Data.Add(key, value);
            if (_messageType != default && Singletons.GetBusTypedLogMessageContainingAnHandlerCollection().Contains(_messageType))
                await PublishTypedLogMessage(logMessage, cancellationToken);
            else
                await _mediator.Publish(logMessage, cancellationToken);
        }

        public async Task AnException(Exception exception, CancellationToken cancellationToken = default)
        {
            Validate(exception);
            var exceptionMessage = new BusExceptionMessage(_switch, exception)
            {
                Bus = _bus,
                Finality = _finality,
                Endpoint = _endpoint,
                Message = _message,
                Level = _level
            };

            foreach (var (key, value) in _data)
                exceptionMessage.Data.Add(key, value);
            if (_messageType != default && Singletons.GetBusTypedExceptionMessageContainingAnHandlerCollection().Contains(_messageType))
                await PublishTypeExceptionMessage(exceptionMessage, cancellationToken);
            else
                await _mediator.Publish(exceptionMessage, cancellationToken);
        }

        private static void Validate(Exception exception)
        {
            if (exception is null)
                throw new ArgumentNullException(nameof(exception));
        }

        private async Task PublishTypedLogMessage(BusLogMessage logMessage, CancellationToken cancellationToken)
        {
            var typedLogMessageFactory = DelegateCache.GetOrAddBusLogMessageFactory(_messageType);
            var typedLogMessage = typedLogMessageFactory(logMessage, _messageObject);
            await _mediator.Publish(typedLogMessage, cancellationToken);
        }

        private async Task PublishTypeExceptionMessage(BusExceptionMessage exceptionMessage, CancellationToken cancellationToken)
        {
            var typedExceptionMessageFactory = DelegateCache.GetOrAddBusExceptionMessageFactory(_messageType);
            var typedExceptionMessage = typedExceptionMessageFactory(exceptionMessage, _messageObject);
            await _mediator.Publish(typedExceptionMessage, cancellationToken);
        }
    }
}

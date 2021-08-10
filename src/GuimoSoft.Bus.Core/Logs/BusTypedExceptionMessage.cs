using MediatR;
using System;
using System.Collections.Generic;
using GuimoSoft.Bus.Abstractions;

namespace GuimoSoft.Bus.Core.Logs
{
    public class BusTypedExceptionMessage<TEvent> : INotification
        where TEvent : IEvent
    {
        public BusName Bus { get; }
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        public Enum? Switch { get; }
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        public Finality Finality { get; }
        public string Endpoint { get; }
        public TEvent MessageObject { get; }
        public string Message { get; }
        public BusLogLevel Level { get; }
        public IDictionary<string, object> Data { get; } = new Dictionary<string, object>();
        public Exception Exception { get; }

        public BusTypedExceptionMessage(BusExceptionMessage exceptionMessage, TEvent messageObject)
        {
            Bus = exceptionMessage.Bus;
            Switch = exceptionMessage.Switch;
            Finality = exceptionMessage.Finality;
            Endpoint = exceptionMessage.Endpoint;
            MessageObject = messageObject;
            Message = exceptionMessage.Message;
            Level = exceptionMessage.Level;
            Exception = exceptionMessage.Exception;
            foreach (var (key, value) in exceptionMessage.Data)
                Data.Add(key, value);
        }
    }
}

using MediatR;
using System;
using System.Collections.Generic;
using GuimoSoft.Bus.Abstractions;

namespace GuimoSoft.Bus.Core.Logs
{
    public class BusExceptionMessage : INotification
    {
        public BusName Bus { get; internal init; }
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        public Enum? Switch { get; }
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        public Finality Finality { get; internal init; }
        public string Endpoint { get; internal init; }

        public string Message { get; internal init; }
        public BusLogLevel Level { get; internal init; }
        public IDictionary<string, object> Data { get; } = new Dictionary<string, object>();
        public Exception Exception { get; internal init; }

        public BusExceptionMessage(Enum @switch, Exception exception)
        {
            Switch = ServerName.Default.Equals(@switch) ? null : @switch;
            Exception = exception;
        }
    }
}

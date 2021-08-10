using System;
using System.Collections.Generic;
using System.Threading;

namespace GuimoSoft.Bus.Abstractions
{
    public abstract class ConsumeContextBase
    {
        public IServiceProvider Services { get; }
        public ConsumeInformations Informations { get; }
        public CancellationToken CancellationToken { get; }
        public IDictionary<string, object> Items { get; } = new Dictionary<string, object>();

        protected ConsumeContextBase(IServiceProvider services, ConsumeInformations informations, CancellationToken cancellationToken)
        {
            Services = services;
            Informations = informations;
            CancellationToken = cancellationToken;
        }

        public abstract object GetMessage();
    }

    public sealed class ConsumeContext<TEvent> : ConsumeContextBase
        where TEvent : IEvent
    {
        public TEvent Message { get; }

        public ConsumeContext(TEvent message, IServiceProvider services, ConsumeInformations informations, CancellationToken cancellationToken)
            : base(services, informations, cancellationToken)
        {
            Message = message;
        }

        public override object GetMessage()
            => Message;
    }
}

using MediatR;
using System;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Internal.Interfaces;
using GuimoSoft.Bus.Core.Logs.Builder;
using GuimoSoft.Bus.Core.Logs.Builder.Stages;

namespace GuimoSoft.Bus.Core.Logs
{
    internal sealed class BusLogDispatcher : IBusLogDispatcher
    {
        private readonly IMediator _mediator;

        public BusLogDispatcher(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public ISwitchStage FromBus(BusName bus)
            => new BusLogDispatcherBuilder(_mediator, bus);
    }
}

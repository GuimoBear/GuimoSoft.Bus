using System;
using GuimoSoft.Bus.Abstractions;

namespace GuimoSoft.Bus.Core.Interfaces
{
    internal interface IProducerManager
    {
        IBusEventDispatcher GetProducer(BusName busName, IServiceProvider services);
    }
}

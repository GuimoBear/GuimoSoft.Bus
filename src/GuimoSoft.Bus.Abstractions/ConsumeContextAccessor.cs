using System.Threading;

namespace GuimoSoft.Bus.Abstractions
{
    internal sealed class ConsumeContextAccessor<TEvent> : IConsumeContextAccessor<TEvent>
        where TEvent: IEvent
    {
        private readonly AsyncLocal<ConsumeContextHolder> _consumecontextCurrent = new AsyncLocal<ConsumeContextHolder>();

        public ConsumeContext<TEvent> Context
        {
            get
            {
                return _consumecontextCurrent.Value?.Context;
            }
            set
            {
                var holder = _consumecontextCurrent.Value;
                if (holder != null)
                    holder.Context = null;

                if (value != null)
                    _consumecontextCurrent.Value = new ConsumeContextHolder { Context = value };
            }
        }

        private class ConsumeContextHolder
        {
            public ConsumeContext<TEvent> Context;
        }
    }
}

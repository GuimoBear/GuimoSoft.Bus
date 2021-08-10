using System;

namespace GuimoSoft.Bus.Core.Logs.Builder.Stages
{
    public interface IMessageObjectInstance
    {
        IEndpointAfterMessageReceivedStage TheObject(object @object);
        IEndpointAfterMessageReceivedStage TheObject(Type objectType, object @object);
    }
}

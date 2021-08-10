using System;
using System.Threading;
using System.Threading.Tasks;

namespace GuimoSoft.Bus.Core.Logs.Builder.Stages
{
    public interface IPublishStage
    {
        Task AnLog(CancellationToken cancellationToken = default);
        Task AnException(Exception exception, CancellationToken cancellationToken = default);
    }
}

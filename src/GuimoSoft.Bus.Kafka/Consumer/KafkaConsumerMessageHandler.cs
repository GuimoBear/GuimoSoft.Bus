using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GuimoSoft.Bus.Kafka.Consumer
{
    public class KafkaConsumerMessageHandler : IHostedService
    {
        private readonly Lazy<CancellationTokenSource> _lazyCts;
        private readonly IKafkaMessageConsumerManager _manager;

        private Task _kafkaListenner = Task.CompletedTask;

        public KafkaConsumerMessageHandler(IKafkaMessageConsumerManager manager)
        {
            _manager = manager;
            _lazyCts = new Lazy<CancellationTokenSource>(() => new CancellationTokenSource());
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _kafkaListenner = Task.Factory.StartNew(() => _manager.StartConsumers(_lazyCts.Value.Token), cancellationToken);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (_lazyCts.IsValueCreated)
            {
                _lazyCts.Value.Cancel();
                _kafkaListenner.Wait(cancellationToken);
            }
            return Task.CompletedTask;
        }
    }
}

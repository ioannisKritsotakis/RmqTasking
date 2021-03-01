using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Receiver.Models;

namespace Receiver
{
    public interface IHeartbeatService
    {
    }

    public class HeartbeatService: BackgroundService, IHeartbeatService
    {
       private readonly IDistributionChannel _distributionChannel;
       private readonly ILogger<HeartbeatService> _logger;
        public HeartbeatService(
            IDistributionChannel distributionChannel, 
            ILogger<HeartbeatService> logger)
        {
            _distributionChannel = distributionChannel;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogDebug("Sending heartbeat");
                _distributionChannel.WriteHeartbeatToChannel(new HeartbeatModel(DateTime.UtcNow.Ticks));
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Starting Heartbeat service");
            return base.StartAsync(cancellationToken);
        }
    }
}

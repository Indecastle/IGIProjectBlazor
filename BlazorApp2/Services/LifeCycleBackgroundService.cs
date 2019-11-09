using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorApp2.Services
{
    public class LifeCycleBackgroundService : IHostedService, IDisposable
    {
        private int counter1, counter2;
        private int executionCount = 0;
        private readonly ILogger<LifeCycleBackgroundService> _logger;
        private Timer _timer;

        public LifeCycleBackgroundService(ILogger<LifeCycleBackgroundService> logger)
        {
            _logger = logger;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service running.");

            //_timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            executionCount++;
            counter1 += 1;
            counter2 += 2;
            Console.WriteLine($"c1: {counter1}, c2: {counter2}");
            _logger.LogInformation(
                "Timed Hosted Service is working. Count: {Count}", executionCount);
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}

using Amazon.S3;
using BlazorApp2.Data;
using BlazorApp2.Models;
using Microsoft.Extensions.DependencyInjection;
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
        private int executionCount = 0;
        private readonly ILogger _logger;
        private Timer _timer;
        ApplicationContext _db;
        public IServiceProvider Services { get; }
        IS3Service _is3;

        public LifeCycleBackgroundService(IServiceProvider services, IS3Service is3, ILogger<LifeCycleBackgroundService> logger, ILoggerFactory loggerFactory)
        {
            Services = services;
            //_logger = loggerFactory.CreateLogger("FileLogger");
            _logger = logger;
            using (var scope = Services.CreateScope())
            {
                _db = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            }
            _is3 = is3;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));

            _logger.LogInformation("Timed Hosted Service is running.");

            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            executionCount++;
            //Console.WriteLine($"c1: {counter1}, c2: {counter2}");
            //_logger.LogTrace("Trace: Timed Hosted Service...");
            //_logger.LogDebug("Debug: Timed Hosted Service...");
            //_logger.LogInformation(new EventId(12345), "Information: Timed Hosted Service...");
            //_logger.LogWarning(new Exception("Hello World"), "Warning: Timed Hosted Service is working. Count: {Count}", executionCount);
            //_logger.LogError("Error: Timed Hosted Service...");
            //_logger.LogCritical("Critical: Timed Hosted Service...");

            _logger.LogDebug("Timed Hosted Service is working. Count: {Count}", executionCount);
            using (var scope = Services.CreateScope())
            {
                var _db = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                List<FastFile> tempFiles = (    from ff in _db.FastFiles
                                                where ff.EndTime < DateTime.Now
                                                select ff ).ToList();
                //List<FastFile> tempFiles = _db.FastFiles.ToList();
                foreach (FastFile file in tempFiles)
                {
                    try
                    {
                        await _is3.DeleteFilesAsync("TempFiles/" + file.KeyName);
                        _db.FastFiles.Remove(file);
                        await _db.SaveChangesAsync();
                    }
                    catch (AmazonS3Exception e)
                    {
                        _logger.LogError("Error encountered on server. Message: '{0}' when writing an object", e.Message);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError("Unknown Exception {0}", e.Message);
                        throw;
                    }
                }
            }
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

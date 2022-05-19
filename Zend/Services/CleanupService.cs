using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Zend.Services
{
    public class CleanupService : IHostedService, IDisposable
    {
        private readonly IServiceProvider _serviceservices;
        private readonly SemaphoreSlim _cleanupLock = new(1,1);

        private System.Timers.Timer _cleanupTimer;

        public CleanupService(IServiceProvider serviceProvider)
        {
            _serviceservices = serviceProvider;
        }

        public void Dispose()
        {
            _cleanupTimer?.Dispose();
            GC.SuppressFinalize(this);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            CleanupTimer_Elapsed(this, EventArgs.Empty);
            _cleanupTimer?.Dispose();
            _cleanupTimer = new System.Timers.Timer(TimeSpan.FromDays(1).TotalMilliseconds);
            _cleanupTimer.Elapsed += CleanupTimer_Elapsed;
            _cleanupTimer.Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cleanupTimer?.Dispose();
            return Task.CompletedTask;
        }

        private void CleanupTimer_Elapsed(object sender, EventArgs e)
        {
            if (!_cleanupLock.Wait(0))
            {
                return;
            }

            try
            {
                using var scope = _serviceservices.CreateScope();
                var appDb = scope.ServiceProvider.GetRequiredService<AppDb>();
                var hostEnv = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
                var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var appData = Directory.CreateDirectory(Path.Combine(hostEnv.ContentRootPath, "App_Data")).FullName;
                var dataRetentionDays = 7;

                if (int.TryParse(config["DataRetentionDays"], out var result))
                {
                    dataRetentionDays = result;
                }

                var expirationDate = DateTimeOffset.Now - TimeSpan.FromDays(dataRetentionDays);

                var expiredFiles = appDb.SavedFiles.Where(x => x.UploadedAt < expirationDate);
                appDb.SavedFiles.RemoveRange(expiredFiles);
                appDb.SaveChanges();

                foreach (var file in Directory.EnumerateFiles(appData))
                {
                    try
                    {
                        var id = Path.GetFileNameWithoutExtension(file);
                        if (!Guid.TryParse(id, out var fileGuid))
                        {
                            continue;
                        }

                        // Delete file if it has expired.
                        if (expiredFiles.Any(x => x.Id == fileGuid))
                        {
                            File.Delete(file);
                            continue;
                        }

                        // Delete file if it doesn't exist in the DB anymore.
                        if (!appDb.SavedFiles.Any(x => x.Id == fileGuid))
                        {
                            File.Delete(file);
                            continue;
                        }
                    }

                    catch (Exception ex)
                    {
                        Console.Write($"Error while deleting file: {ex.Message}");
                    }
                }
            }
            finally
            {
                _cleanupLock.Release();
            }
        }
    }
}

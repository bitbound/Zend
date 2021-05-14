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

namespace Zend.Services
{
    public class CleanupService : IHostedService, IDisposable
    {
        public CleanupService(IServiceProvider serviceProvider)
        {
            Services = serviceProvider;
        }

        private IServiceProvider Services { get; }
        private System.Timers.Timer CleanupTimer { get; set; }

        public void Dispose()
        {
            CleanupTimer?.Dispose();
            GC.SuppressFinalize(this);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            CleanupTimer?.Dispose();
            CleanupTimer = new System.Timers.Timer(TimeSpan.FromDays(1).TotalMilliseconds);
            CleanupTimer.Elapsed += CleanupTimer_Elapsed;
            CleanupTimer.Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            CleanupTimer?.Dispose();
            return Task.CompletedTask;
        }

        private void CleanupTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            using var scope = Services.CreateScope();
            var appDb = scope.ServiceProvider.GetRequiredService<AppDb>();
            var hostEnv = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var appData = Directory.CreateDirectory(Path.Combine(hostEnv.ContentRootPath, "App_Data")).FullName;
            var dataRetentionDays = 7;

            if (int.TryParse(config["DataRetentionDays"], out var result))
            {
                dataRetentionDays = result;
            }

            var expiredFiles = appDb.SavedFiles.Where(x => x.UploadedAt.AddDays(dataRetentionDays) < DateTimeOffset.Now);
            appDb.SavedFiles.RemoveRange(expiredFiles);
            appDb.SaveChanges();

            var expiredFilePaths = Directory.EnumerateFiles(appData)
                .Where(x => expiredFiles.Any(y => x.Contains(y.Id.ToString())));

            foreach (var file in expiredFilePaths)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    Console.Write($"Error while deleting file: {ex.Message}");
                }
            }
        }
    }
}

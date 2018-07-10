using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FFMPEGStreamingTools;
using FFMPEGStreamingTools.M3u8Generators;
using FFMPEGStreamingTools.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VideoStreamer.DB;
using VideoStreamer.Utils;
using Serilog.Extensions.Logging;
using FFMPEGStreamingTools.TokenBrokers;

namespace VideoStreamer
{
    public class Startup
    {
        public IConfiguration Cfg { get; }
        private StreamingProcManager procManager;
        public static FileSystemWatcher watcher;

        public Startup(IConfiguration cfg)
        {

            watcher = new FileSystemWatcher();
            watcher.Filter = "*Channels.json";
            watcher.Changed += new FileSystemEventHandler(OnChanged);
			watcher.Path = ".";
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
           | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.EnableRaisingEvents = true;
            Console.WriteLine("File -> {0}", watcher.Path);


            Cfg = cfg;
            FFMPEGConfigLoader.Load(
                out var ffmpegConfig,
                out var streamsConfig
            );

            procManager = new StreamingProcManager();
            foreach (var streamCfg in streamsConfig)
            {
                Task.Run(
                    () => procManager.StartChunking(ffmpegConfig, streamCfg));
            }
        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
            ChannelUpdate.AddChannel();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            var connectionStr = Cfg["DBConnectionStr"];
            services.AddDbContext<StreamerContext>(
                o => o.UseSqlite(connectionStr));

            services.AddDistributedRedisCache(o =>
            {
                o.Configuration = "localhost";
                o.InstanceName = "VideoStreamings";
            });

            // Custom stuff
            services.AddTransient<IM3U8Generator, M3U8GeneratorDefault>();
			services.AddTransient<ITokenBroker, SHA256TokenBroker>();
        }

        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
			ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

			loggerFactory.AddFile("Logs/ts-{Date}.log");

            app.UseMvc();
            app.UseStaticFiles();
        }
    }
}

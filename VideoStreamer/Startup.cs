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
<<<<<<< HEAD
            watcher.Filter = "*.json";
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Path = "/Users/andrewska/Desktop/PracticeProject-FFMPEGVideoStreaming/VideoStreamer/";
=======
            watcher.Filter = "*Channels.json";
            watcher.Changed += new FileSystemEventHandler(OnChanged);
			watcher.Path = ".";
>>>>>>> e9fc30b707c0c4e76a818e6d36548d99897252d9
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
<<<<<<< HEAD
        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
            ChannelUpdate.AddChannel();
        }

=======
        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
            ChannelUpdate.AddChannel();
        }

>>>>>>> e9fc30b707c0c4e76a818e6d36548d99897252d9
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

        }

        public void Configure(
            IApplicationBuilder app,
<<<<<<< HEAD
            IHostingEnvironment env)
=======
            IHostingEnvironment env,
			ILoggerFactory loggerFactory)
>>>>>>> e9fc30b707c0c4e76a818e6d36548d99897252d9
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

			loggerFactory.AddFile("Logs/ts-{Date}.txt");

            app.UseMvc();
            app.UseStaticFiles();
        }
    }
}

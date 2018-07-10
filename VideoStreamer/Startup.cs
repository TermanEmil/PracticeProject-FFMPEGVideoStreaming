using System;
using System.Text;
using System.Threading.Tasks;
using FFMPEGStreamingTools;
using FFMPEGStreamingTools.M3u8Generators;
using FFMPEGStreamingTools.TokenBrokers;
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

namespace VideoStreamer
{
	public class Startup
    {
		public IConfiguration Cfg { get; }
		private StreamingProcManager procManager;

		public Startup(IConfiguration cfg)
        {
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
            IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
            app.UseStaticFiles();         
        }
    }
}

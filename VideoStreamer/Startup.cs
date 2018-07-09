using System.Threading.Tasks;
using FFMPEGStreamingTools;
using FFMPEGStreamingTools.M3u8Generators;
using FFMPEGStreamingTools.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VideoStreamer.DB;

namespace VideoStreamer
{
	public class Startup
    {
		public IConfiguration Cfg { get; }
		private StreamingProcManager procManager;

		public Startup(IConfiguration cfg)
        {
			Cfg = cfg;
			FFMPEGConfigLoader.Load(cfg, out var ffmpegConfig, out var streamsConfig);
			procManager = new StreamingProcManager();         
			foreach (var streamCfg in streamsConfig)
			{
				Task.Run(() => procManager.StartChunking(ffmpegConfig, streamCfg));
			}
        }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
   
			services.AddTransient<IM3U8Generator, M3U8GeneratorDefault>();

			var connectionStr = Cfg["DBConnectionStr"];
			services.AddDbContext<StreamerContext>(
				o => o.UseSqlite(connectionStr));
        }
  
        public void Configure(
			IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory,
            IApplicationLifetime applicationLifetime)
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

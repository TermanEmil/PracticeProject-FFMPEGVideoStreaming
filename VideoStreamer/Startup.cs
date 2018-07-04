using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FFMPEGStreamingTools;
using FFMPEGStreamingTools.StreamingSettings;
using FFMPEGStreamingTools.Utils;

namespace VideoStreamer
{
    public class Startup
    {
		public IConfiguration Cfg { get; }
		private StreamingProcManager procManager;

		public Startup(IConfiguration cfg)
        {
			Cfg = cfg;
			ConfigLoader.Load(cfg, out var ffmpegConfig, out var streamsConfig);
			procManager = new StreamingProcManager();         
			foreach (var streamCfg in streamsConfig)
			{
				Task.Run(() => procManager.StartChunking(ffmpegConfig, streamCfg));
			}
        }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddDistributedMemoryCache();
            services.AddSession(options =>
			{
				options.IdleTimeout = TimeSpan.FromMinutes(1.0);
			});
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
			app.UseSession();
            app.UseMvc();
            app.UseStaticFiles();
        }
    }
}

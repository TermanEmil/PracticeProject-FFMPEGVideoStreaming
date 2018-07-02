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
using ProcessStreamer;

namespace VideoStreamer
{
    public class Startup
    {
		public IConfiguration Configuration { get; }
		private StreamingProcManager procManager;

		public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            
			var ffmpegConfig = Configuration.GetSection("FFMPEGConfig")
			                                .Get<FFMPEGConfig>();
			var streamsConfig = new List<StreamConfig>();
			Configuration.GetSection("StreamsConfig")
			             .Bind(streamsConfig);
			
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
            services.AddSession();
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
            app.UseSession();
        }
    }
}

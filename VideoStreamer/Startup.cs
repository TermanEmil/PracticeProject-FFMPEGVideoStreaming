using FFMPEGStreamingTools;
using FFMPEGStreamingTools.M3u8Generators;
using FFMPEGStreamingTools.TokenBrokers;
using FFMPEGStreamingTools.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VideoStreamer.DB;

namespace VideoStreamer
{
	public class Startup
	{
		private readonly IConfiguration _cfg;

		public Startup(IConfiguration cfg)
		{
			_cfg = cfg;
		}

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc();

            // Entity Framework
			var connectionStr = _cfg["DBConnectionStr"];
			services.AddDbContext<StreamerContext>(
				o => o.UseSqlite(connectionStr));

            // Redis
			services.AddDistributedRedisCache(o =>
			{
				o.Configuration = "localhost";
				o.InstanceName = "VideoStreamings";
			});

			// Custom stuff
			services.AddSingleton<StreamingProcManager>();
			services.AddSingleton<ChannelUpdateManager>();
			services.AddTransient<StreamSourceCfgLoader>();         
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

			// Singleton intializations.
			app.ApplicationServices.GetService<StreamingProcManager>();
			app.ApplicationServices.GetService<ChannelUpdateManager>()
			   .UpdateChannels();
		}
	}
}

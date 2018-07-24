using DataLayer.Configs;
using FFMPEGStreamingTools.SessionBrokers;
using FFMPEGStreamingTools.StreamingSessionTypeIdentifiers;
using FFMPEGStreamingTools.TokenBrokers;
using FFMPEGStreamingTools.UserTypeIdentifiers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Logic;
using VideoStreamer.BusinessLogic.ChunksCollectors;
using VideoStreamer.BusinessLogic.PlaylistAssemblers;
using VideoStreamer.Db;
using VideoStreamer.Models.Configs;
using VideoStreamer.Services.TokenParsers;

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
			//services.AddSingleton<StreamingProcManager>();
			//services.AddSingleton<StreamsUpdateManager>();
            
			services.AddSingleton(ChunkerConfig.Load(_cfg));         
			services.AddSingleton(StreamerSessionCfg.Load(_cfg));
			services.AddSingleton<StreamsConfig>();
            services.AddSingleton<StreamsUpdateWatcher>();

			services.AddTransient<IChunkCollector, ChunkCollector>();
			services.AddTransient<
					IPlaylistAssembler, SimplePlaylistAssembler>();
			services.AddTransient<ITokenBroker, SHA256TokenBroker>();
			services.AddTransient<ITokenParser, TokenParser>();
			services.AddTransient<ISessionBroker, SessionBroker>();

			services.AddTransient<UserTypeIdentifier>();
			services.AddTransient<IUserTypeIdentifier,
								  PaidUserTypeIdentifier>();
			services.AddTransient<IUserTypeIdentifier,
								  GuestUserTypeIdentifier>();
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

			// Load the stream sources.
            var streamsConfig = app.ApplicationServices
                                   .GetService<StreamsConfig>();
            streamsConfig.Reload();

			// Singleton intializations.
			var updateWatcher = app.ApplicationServices
                                   .GetService<StreamsUpdateWatcher>();

            // Add the event handlers on channels change.
            updateWatcher.AddEventHandlerOnFileChange(
                (s, a) => streamsConfig.Reload());
		}
	}
}

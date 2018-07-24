using ChunksGenerator.BusinessLogic;
using DataLayer.Configs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Logic;

namespace ChunksGenerator.API
{
	public class Startup
    {
		public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
			        .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

			// Custom services
			services.AddSingleton(ChunkerConfig.Load(Configuration));         
			services.AddSingleton<StreamsConfig>();
			services.AddSingleton<StreamsUpdateWatcher>();
			services.AddSingleton<ChunkerProcManager>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            
            app.UseHttpsRedirection();
            app.UseMvc();

			// Load the stream sources.
			var streamsConfig = app.ApplicationServices
								   .GetService<StreamsConfig>();
			streamsConfig.Reload();
            
			// Initialize the singletons.
			var updateWatcher = app.ApplicationServices
			                       .GetService<StreamsUpdateWatcher>();
			var chunkerProcManager = app.ApplicationServices
										.GetService<ChunkerProcManager>();
			chunkerProcManager.StartChunkingAllSources();

            // Add the event handlers on channels change.
			updateWatcher.AddEventHandlerOnFileChange(
				(s, a) => streamsConfig.Reload());
			updateWatcher.AddEventHandlerOnFileChange(
				(s, a) => chunkerProcManager.UpdateStreams());         
        }
    }
}

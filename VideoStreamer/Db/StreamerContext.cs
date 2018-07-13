using System;
using FFMPEGStreamingTools.Models;
using Microsoft.EntityFrameworkCore;

namespace VideoStreamer.Db
{
	public class StreamerContext : DbContext
    {
		public StreamerContext(DbContextOptions<StreamerContext> options)
			: base(options)
        {
        }

		public DbSet<StreamingSession> StreamingSessions { get; set; }
    }
}

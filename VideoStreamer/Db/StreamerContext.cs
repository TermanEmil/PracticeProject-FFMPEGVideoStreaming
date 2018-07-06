using System;
using Microsoft.EntityFrameworkCore;

namespace VideoStreamer.DB
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

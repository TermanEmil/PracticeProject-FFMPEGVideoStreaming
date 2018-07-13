using System;
namespace DataLayer.Configs
{
    public class StreamSource
    {
		public string Link { get; set; }
        public string Name { get; set; }
        public double ChunkTime { get; set; }

        public override int GetHashCode()
        {
            return
                (Link.GetHashCode()) ^
                (Name.GetHashCode()) ^
                (ChunkTime.GetHashCode());
        }
    }
}

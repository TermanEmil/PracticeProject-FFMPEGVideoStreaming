using System;
using System.IO;
using DataLayer.Configs;

namespace Shared.Logic
{
    public class StreamsUpdateWatcher
    {
		private readonly FileSystemWatcher watcher;

		public StreamsUpdateWatcher(ChunkerConfig chunkerConfig)
        {
			watcher = new FileSystemWatcher
            {
				Filter = "*" + Path.GetFileName(chunkerConfig.ChannelsCfgPath),
				Path = Path.GetDirectoryName(chunkerConfig.ChannelsCfgPath),
                NotifyFilter =
                           NotifyFilters.LastAccess |
                           NotifyFilters.LastWrite |
                           NotifyFilters.FileName |
                           NotifyFilters.DirectoryName,

                EnableRaisingEvents = true,
            };
        }

		public void AddEventHandlerOnFileChange(
			FileSystemEventHandler eventHandler)
		{
			watcher.Changed += eventHandler;
		}
    }
}

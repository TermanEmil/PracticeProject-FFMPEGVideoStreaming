using System;
using System.Threading.Tasks;
using FFMPEGStreamingTools.Utils;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Linq;
using System.IO;

namespace FFMPEGStreamingTools
{
    public class ChannelUpdateManager
    {
		private readonly StreamingProcManager _procManager;
		private readonly StreamSourceCfgLoader _streamSourceCfgLoader;

		public ChannelUpdateManager(
			StreamingProcManager procManager,
			StreamSourceCfgLoader streamSourceCfgLoader)
		{
			_procManager = procManager;
			_streamSourceCfgLoader = streamSourceCfgLoader;

			var watcher = new FileSystemWatcher
            {
                Filter = "*Channels.json",
                Path = ".",
                NotifyFilter =
                           NotifyFilters.LastAccess |
                           NotifyFilters.LastWrite |
                           NotifyFilters.FileName |
                           NotifyFilters.DirectoryName,

                EnableRaisingEvents = true,
            };
            watcher.Changed += OnChanged;
		}

		private void OnChanged(object source, FileSystemEventArgs e)
        {
            Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
            UpdateChannels();
        }

		public void UpdateChannels()
        {
			var streamsConfig = _streamSourceCfgLoader.LoadStreamSources();         
			if (streamsConfig == null)
				return;
                
            foreach (var streamCfg in streamsConfig)
            {
				var procEntry = 
					_procManager.processes
					       .FirstOrDefault(x => x.Name == streamCfg.Name);
				
				if (procEntry == null)
				{
					Task.Run(() => _procManager.StartChunking(streamCfg));
				}
				else
				{
					procEntry.CloseProcess();
				}
            }
        }
    }
}
using System;
using System.Threading.Tasks;
using FFMPEGStreamingTools.Utils;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Linq;
using System.IO;
using FFMPEGStreamingTools.StreamingSettings;
using System.Collections.Generic;

namespace FFMPEGStreamingTools
{
    public class StreamsUpdateManager
    {
		private readonly StreamingProcManager _procManager;
		private readonly StreamSourceCfgLoader _streamSourceCfgLoader;

		public StreamsUpdateManager(
			IConfiguration cfg,
			StreamingProcManager procManager,
			StreamSourceCfgLoader streamSourceCfgLoader)
		{
			_procManager = procManager;
			_streamSourceCfgLoader = streamSourceCfgLoader;

			var ffmpegCfg = FFMPEGConfig.Load(cfg);
			var watcher = new FileSystemWatcher
            {
				Filter = "*" + Path.GetFileName(ffmpegCfg.ChannelsCfgPath),
				Path = Path.GetDirectoryName(ffmpegCfg.ChannelsCfgPath),
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
            UpdateChannels();
        }

		public void UpdateChannels()
        {
			var streamsConfig = _streamSourceCfgLoader.LoadStreamSources();         
			if (streamsConfig == null)
				return;

			var mentionedProcNames = new HashSet<string>();
            foreach (var streamCfg in streamsConfig)
            {
				var procEntry = 
					_procManager.processes
					       .FirstOrDefault(x => x.Name == streamCfg.Name);

				if (procEntry == null ||
					procEntry.Proc == null ||
					procEntry.Proc.HasExited)
				{
					_procManager.StartChunkingTask(streamCfg);
				}
				else if (procEntry.Hash != streamCfg.GetHashCode())
				{
					procEntry.Restart(
						() => _procManager.StartChunkingTask(streamCfg));
				}

				mentionedProcNames.Add(streamCfg.Name);
            }

			var removedProcs =
				_procManager.processes
				            .Where(x => !mentionedProcNames.Contains(x.Name));
			
			foreach (var procEntry in removedProcs)
			{
				procEntry.CloseProcess();
			}
        }
    }
}
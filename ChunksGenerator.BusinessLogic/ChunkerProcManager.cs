using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ChunksGenerator.BusinessLogic.Models;
using DataLayer.Configs;
using Microsoft.Extensions.Logging;

namespace ChunksGenerator.BusinessLogic
{
    public class ChunkerProcManager
    {
		private readonly ILogger<ChunkerProcManager> _logger;
		private readonly ChunkerConfig _chunkerConfig;
		private readonly StreamsConfig _streamsConfig;

		private int procUniqueKey = 0;

		public ConcurrentBag<ProcEntry> processes;
		
		public ChunkerProcManager(
			ILogger<ChunkerProcManager> logger,
			ChunkerConfig chunkerConfig,
			StreamsConfig streamsConfig)
        {
			_logger = logger;
			_chunkerConfig = chunkerConfig;
			_streamsConfig = streamsConfig;

			processes = new ConcurrentBag<ProcEntry>();
        }

		public void StartChunkingAllSources()
		{
			foreach (var source in _streamsConfig.StreamSources)
				StartChunkingTask(source);
		}

		private void StartChunkingTask(StreamSource streamSource)
		{
			Task.Run(() => StartChunking(streamSource));
		}

        /// <summary>
        /// Update the changed streams, remove those that are not mentioned
		/// in streams config.
        /// </summary>
		public void UpdateStreams()
		{
			var mentionedProcNames = new HashSet<string>();
			foreach (var streamCfg in _streamsConfig.StreamSources)
            {
                var procEntry =
					processes.FirstOrDefault(x => x.Name == streamCfg.Name);

                if (procEntry == null ||
                    procEntry.Proc == null ||
                    procEntry.Proc.HasExited)
                {
                    StartChunkingTask(streamCfg);
                }
                else if (procEntry.Hash != streamCfg.GetHashCode())
                    procEntry.Restart(() => StartChunkingTask(streamCfg));

                mentionedProcNames.Add(streamCfg.Name);
            }

            var removedProcs =
				processes.Where(x => !mentionedProcNames.Contains(x.Name));

            foreach (var procEntry in removedProcs)
                procEntry.CloseProcess();
		}

		private void StartChunking(StreamSource streamSource)
		{
			var procStartInfo = new ProcessStartInfo()
            {
				FileName = _chunkerConfig.BinaryPath,
                UseShellExecute = false,
				Arguments = GetProcArguments(streamSource, procUniqueKey++)
            };
            
			var proc = new Process();
			proc.StartInfo = procStartInfo;
            proc.EnableRaisingEvents = true;
            proc.Exited += OnProcExit;
                     
			if (proc.Start())
			{
				var procEntry = processes.FirstOrDefault(
                x => x.Name == streamSource.Name);

                if (procEntry == null)
                {
                    processes.Add(new ProcEntry
                    {
                        Name = streamSource.Name,
                        Hash = streamSource.GetHashCode(),
                        Proc = proc
                    });
                }
                else
                    procEntry.Proc = proc;

				proc.WaitForExit();            
			}
		}

		private string GetProcArguments(
			StreamSource streamSource,
		    int procID)
        {
			var root = $"{_chunkerConfig.ChunkStorageDir}/{streamSource.Name}/";
			var segmentFilename =
				root + $"%Y/%m/%d/%H/%M/%s-%%06d-{procID}.ts";
            var m3u8File = root + "index.m3u8";

            return string.Join(" ", new[]
            {
                "-err_detect ignore_err",
                "-reconnect 1",
				//"-reconnect_at_eof 1",
                "-reconnect_streamed 1",
				"-reconnect_delay_max 300",
                "-y -re",
                "-hide_banner",
                "-i " + streamSource.Link,
                "-map 0",
                "-codec:v copy -codec:a copy -c copy",
                "-f hls",
                "-hls_time " + streamSource.ChunkTime,
                "-use_localtime 1 -use_localtime_mkdir 1",
                "-hls_flags second_level_segment_index",
                "-hls_segment_filename " + segmentFilename,
                m3u8File
            });
        }

		private void OnProcExit(object s, EventArgs e)
		{
			if (s == null)
				return;
			var proc = s as Process;

            // FFMPEG exit code 1 means it ran into an error, like an invalid
            // link or something.
			if (proc.ExitCode == 1)
				return;

			var procEntry = processes.FirstOrDefault(x => x.Proc == proc);
			if (procEntry != null)
			{
				procEntry.Proc = null;
				if (procEntry.closeRequested)
				{
					procEntry.closeRequested = false;
					return;
				}
			}
            
			var streamCfg = _streamsConfig.StreamSources.FirstOrDefault(
                x => x.Name == procEntry.Name);

			if (streamCfg == null)
            {
                procEntry.Proc = null;
                return;
            }

			var root = Path.Combine(
				_chunkerConfig.ChunkStorageDir,
				streamCfg.Name);
			
            if (!Directory.Exists(root))
                return;

            // Delete the last file, so we make sure no stupid shit happens.
			TryDeleteLastFile(root, streamCfg);

			_logger.LogInformation("[Restarting]: {0}", streamCfg.Name);
			StartChunking(streamCfg);
		}

        // Deletes the last produced file.
		private void TryDeleteLastFile(
			string chunksRoot,
			StreamSource streamCfg)
		{
            var files = Directory.GetFiles(
                chunksRoot,
                "*.ts",
                SearchOption.AllDirectories);

            var mostRecent = files.OrderByDescending(File.GetCreationTime)
                                  .FirstOrDefault();

            if (mostRecent == null)
                return;

            File.Delete(mostRecent);
		}
    }
}

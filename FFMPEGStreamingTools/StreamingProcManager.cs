using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FFMPEGStreamingTools.StreamingSettings;
using FFMPEGStreamingTools.Utils;
using Microsoft.Extensions.Configuration;

namespace FFMPEGStreamingTools
{   
    public class StreamingProcManager
    {      
		public ConcurrentBag<ProcEntry> processes =
			new ConcurrentBag<ProcEntry>();
		public bool LogToStdout { get; set; } = true;

		// Sets of chunk indexes with file discontinuities.
		// Used in M3U8 generator.
		public
		ConcurrentDictionary<string, ConcurrentBag<int>> chunkDiscontinuities;
    	      
		private readonly FFMPEGConfig _ffmpegCfg;
		private readonly StreamSourceCfgLoader _streamSourceCfgLoader;

		public StreamingProcManager(
			IConfiguration cfg,
			StreamSourceCfgLoader streamSourceCfgLoader)
		{
			_ffmpegCfg = FFMPEGConfig.Load(cfg);
			_streamSourceCfgLoader = streamSourceCfgLoader;

			chunkDiscontinuities =
				new ConcurrentDictionary<string, ConcurrentBag<int>>();
		}

		public void StartChunkingTask(StreamSource streamCfg)
		{
			Task.Run(() => StartChunking(streamCfg));
		}

		private void StartChunking(StreamSource streamCfg)
		{
			if (!chunkDiscontinuities.ContainsKey(streamCfg.Name))
				chunkDiscontinuities.TryAdd(
					streamCfg.Name, new ConcurrentBag<int>());

			var nextID = GetLastProducedIndex(streamCfg);
            if (nextID != 0 &&
                !chunkDiscontinuities[streamCfg.Name].Contains(nextID))
            {
                chunkDiscontinuities[streamCfg.Name].Add(nextID);
            }

			var procInfo = new ProcessStartInfo()
			{
				FileName = _ffmpegCfg.BinaryPath,
				UseShellExecute = false,
				Arguments = GenerateProcArguments(streamCfg, nextID)
			};

			procInfo.RedirectStandardOutput = !LogToStdout;
			procInfo.RedirectStandardError = !LogToStdout;

			var proc = new Process
			{
				StartInfo = procInfo,
				EnableRaisingEvents = true
			};

			proc.Exited += OnProcExit;
			var procEntry = processes.FirstOrDefault(
				x => x.Name == streamCfg.Name);
			
			if (procEntry == null)
			{
				processes.Add(new ProcEntry
				{
					Name = streamCfg.Name,
					Hash = streamCfg.GetHashCode(),
					Proc = proc
				});
			}
			else
				procEntry.Proc = proc;

			proc.Start();
			proc.WaitForExit();
		}

		private string GenerateProcArguments(
			StreamSource streamCfg,
			int startID)
		{
			var root = _ffmpegCfg.ChunkStorageDir + "/" + streamCfg.Name + "/";
            var segmentFilename = root + $"%Y/%m/%d/%H/%M/%s-%%06d.ts";
            var m3u8File = root + "index.m3u8";
            
			return string.Join(" ", new[]
            {
                "-err_detect ignore_err",
                "-reconnect 1 -reconnect_at_eof 1",
                "-reconnect_streamed 1 -reconnect_delay_max 300",
                "-y -re",
                "-hide_banner",
                "-i " + streamCfg.Link,
                "-map 0",
				"-start_number " + startID,
                "-codec:v copy -codec:a copy -c copy",
                "-f hls",
                "-hls_time " + streamCfg.ChunkTime,
                "-use_localtime 1 -use_localtime_mkdir 1",
                "-hls_flags second_level_segment_index",
                "-hls_segment_filename " + segmentFilename,
                m3u8File
            });
		}

		private void OnProcExit(object s, EventArgs e)
		{
			var procEntry = processes.FirstOrDefault(
				x => x.Proc == s as Process);
            
			if (procEntry != null)
            {
                procEntry.Proc = null;
				if (procEntry.closeRequested)
				{
					procEntry.closeRequested = false;
					return;
				}
            }

			var streamsCfgs = _streamSourceCfgLoader.LoadStreamSources();
			if (streamsCfgs == null)
			{
				procEntry.Proc = null;
				return;
			}

			var streamCfg = streamsCfgs.FirstOrDefault(
				x => x.Name == procEntry.Name);

			if (streamCfg == null)
			{
				procEntry.Proc = null;
				return;
			}

			var root = Path.Combine(_ffmpegCfg.ChunkStorageDir, streamCfg.Name);
			if (!Directory.Exists(root))
				return;

			Console.WriteLine("[Restarting]: {0}", streamCfg.Name);         
            StartChunking(streamCfg);
		}

		private int GetLastProducedIndex(StreamSource streamCfg)
		{
			var chunksRoot = Path.Combine(
				_ffmpegCfg.ChunkStorageDir,
				streamCfg.Name);

			if (!Directory.Exists(chunksRoot))
				return -1;

			var files = Directory.GetFiles(
				chunksRoot,
				"*.ts",
				SearchOption.AllDirectories);
			
			var mostRecent = files.OrderByDescending(File.GetCreationTime)
			                      .FirstOrDefault();

			if (mostRecent == null)
				return -1;
                     
			File.Delete(mostRecent);
			var mostRecentChunk = new ChunkFile(mostRecent);
			return mostRecentChunk.index;
		}
    }
}

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

namespace FFMPEGStreamingTools
{
    public class StreamingProcManager
    {
		public static StreamingProcManager instance;
        
		public List<Process> processes = new List<Process>();
		public bool LogToStdout { get; set; } = true;

		// Sets of chunk indexes with file discontinuities.
		// Used in M3U8 generator.
		public ConcurrentDictionary<string, ConcurrentBag<int>>
		    chunkDiscontinuities =
    			new ConcurrentDictionary<string, ConcurrentBag<int>>();

		public StreamingProcManager()
		{
			instance = this;
		}

		public void StartChunking(
			FFMPEGConfig ffmpegCfg,
			Channel streamCfg,
		    int startNumber = 0)
		{
			if (!chunkDiscontinuities.ContainsKey(streamCfg.Name))
			{
				chunkDiscontinuities.TryAdd(
					streamCfg.Name, new ConcurrentBag<int>());
			}
            
			var procInfo = new ProcessStartInfo();
			procInfo.FileName = ffmpegCfg.BinaryPath;
			procInfo.UseShellExecute = false;
			streamCfg.Name = streamCfg.Name;
   
			var segmentFilename =
				ffmpegCfg.ChunkStorageDir + "/" +
				streamCfg.Name + "/" +
	            $"%Y/%m/%d/%H/%M/%s-%%06d.ts";

			var m3u8File =
				ffmpegCfg.ChunkStorageDir + "/" +
	            streamCfg.Name + "/" +
				"index.m3u8";
			
			procInfo.Arguments = string.Join(" ", new[]
			{
				"-err_detect ignore_err",
				"-reconnect 1 -reconnect_at_eof 1",
				"-reconnect_streamed 1 -reconnect_delay_max 300",
				"-y -re",
                "-hide_banner",
			    "-i " + streamCfg.Link,            
			    "-map 0",
				"-start_number " + startNumber,
				"-codec:v copy -codec:a copy -c copy",
			    "-f hls",
			    "-hls_time " + streamCfg.ChunkTime,
			    "-use_localtime 1 -use_localtime_mkdir 1",
				"-hls_flags second_level_segment_index",
			    "-hls_segment_filename " + segmentFilename,
			    m3u8File
			});

			procInfo.RedirectStandardOutput = !LogToStdout;
			procInfo.RedirectStandardError = !LogToStdout;

			var proc = new Process
			{
				StartInfo = procInfo,
				EnableRaisingEvents = true
			};

			proc.Exited += GenerateOnExitHandler(ffmpegCfg, streamCfg);
   
			proc.Start();         
			processes.Add(proc);
			proc.WaitForExit();
		}

		private EventHandler GenerateOnExitHandler(
			FFMPEGConfig ffmpegCfg,
			Channel streamCfg)
		{
			return (o, s) =>
            {
                processes.Remove(o as Process);
                var lastID = GetLastProducedIndex(ffmpegCfg, streamCfg);

				if (lastID == -1)
					return;

                var log = string.Format(
                    "[Restarting]: lastID = {0} | {1}",
                    lastID,
                    streamCfg.Name);

                Console.WriteLine(log);

                var nextID = lastID + 1;
                if (!chunkDiscontinuities[streamCfg.Name].Contains(nextID))
                    chunkDiscontinuities[streamCfg.Name].Add(nextID);

                StartChunking(ffmpegCfg, streamCfg, nextID);
            };
		}
        
		private int GetLastProducedIndex(
			FFMPEGConfig ffmpegCfg,
			Channel streamCfg)
		{
			var chunksRoot = Path.Combine(
				ffmpegCfg.ChunkStorageDir,
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
			return mostRecentChunk.index - 1;
		}
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessStreamer
{
    public class StreamingProcManager
    {
		public static StreamingProcManager instance;
        public static UInt64 _bytes;
		public static bool logToFile = false;
        
		public List<Process> processes = new List<Process>();
              
		public Dictionary<string, HashSet<int>> chunkDiscontinuities =
			new Dictionary<string, HashSet<int>>();

		private StreamWriter logFile;

		public StreamingProcManager()
		{
			instance = this;
			if (logToFile)
			{
				var fileStream = new FileStream("logFile.log", FileMode.Create);
				logFile = new StreamWriter(fileStream);
			}
		}

		public void StartChunking(
			FFMPEGConfig ffmpegConfig,
			StreamConfig streamConfig,
		    int startNumber = 0)
		{
			if (!chunkDiscontinuities.ContainsKey(streamConfig.Name))
			    chunkDiscontinuities.Add(streamConfig.Name, new HashSet<int>());
            
			var procInfo = new ProcessStartInfo();
			procInfo.FileName = ffmpegConfig.BinaryPath;
			streamConfig.Name = streamConfig.Name;
   
			var segmentFilename =
				ffmpegConfig.ChunkStorageDir + "/" +
				streamConfig.Name + "/" +
	            $"%Y/%m/%d/%H/%M/%s-%%06d.ts";

			var m3u8File =
				ffmpegConfig.ChunkStorageDir + "/" +
	            streamConfig.Name + "/" +
				"index.m3u8";
			
			procInfo.Arguments = string.Join(" ", new[]
			{
				"-err_detect ignore_err",
				"-reconnect 1 -reconnect_at_eof 1 -reconnect_streamed 1 -reconnect_delay_max 300",
				"-y -re",
                "-hide_banner",
			    "-i " + streamConfig.Link,            
			    "-map 0",
				"-start_number " + startNumber,
				"-codec:v copy -codec:a copy -c copy",
			    "-f hls",
			    "-hls_time " + streamConfig.ChunkTime,
			    "-use_localtime 1 -use_localtime_mkdir 1",
				"-hls_flags second_level_segment_index",
			    "-hls_segment_filename " + segmentFilename,
			    m3u8File
			});

			if (logToFile)
			{
				procInfo.RedirectStandardOutput = true;
				procInfo.RedirectStandardError = true;
			}

			var proc = new Process();
			proc.StartInfo = procInfo;
			proc.EnableRaisingEvents = true;

			proc.Exited += (o, s) =>
			{
				processes.Remove(o as Process);
				var lastID = GetLastProducedIndex(ffmpegConfig, streamConfig);

				var log = string.Format(
					"[Restarting]: lastID = {0} | {1}",
					lastID,
					streamConfig.Name);

				if (logToFile)
					logFile.WriteLine(log);
				else
					Console.WriteLine(log);

				var nextID = lastID + 1;
				if (!chunkDiscontinuities[streamConfig.Name].Contains(nextID))
				    chunkDiscontinuities[streamConfig.Name].Add(nextID);

				StartChunking(ffmpegConfig, streamConfig, nextID);
			};

			if (logToFile)
			{            
				proc.ErrorDataReceived += OutputErrDataReceived;
				proc.OutputDataReceived += OutputDataReceived;
			}

			proc.Start();

			if (logToFile)
			{
				proc.BeginOutputReadLine();
				proc.BeginErrorReadLine();
			}

			processes.Add(proc);         
			proc.WaitForExit();
		}

		private void OutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			logFile.WriteLine(e.Data);
		}

		private void OutputErrDataReceived(object sender, DataReceivedEventArgs e)
        {
			logFile.WriteLine("<[Error]>: " + e.Data);
        }

		private int GetLastProducedIndex(
			FFMPEGConfig ffmpegCfg,
			StreamConfig streamCfg)
		{
			var chunksRoot = Path.Combine(
				ffmpegCfg.ChunkStorageDir,
				streamCfg.Name);
			
			var mostRecent =
                Directory.GetFiles(chunksRoot, "*.ts", SearchOption.AllDirectories)
				         .OrderByDescending(File.GetCreationTime)
				         .FirstOrDefault();

			if (mostRecent == null)
				return 0;
                     
			var mostRecentChunk = new ChunkFile(mostRecent);
			var currentSeconds = TimeTools.CurrentSeconds();

			if (currentSeconds - mostRecentChunk.timeSeconds < streamCfg.ChunkTime * 2)
			{
				File.Delete(mostRecent);
				return mostRecentChunk.index - 1;
			}
			else
				return mostRecentChunk.index;
		}
    }
}

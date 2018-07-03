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

		public List<Process> processes = new List<Process>();

		private StreamWriter logFile;

		public StreamingProcManager()
		{
			instance = this;
			var fileStream = new FileStream("logFile.log", FileMode.Create);
			logFile = new StreamWriter(fileStream);
		}

		public void StartChunking(
			FFMPEGConfig ffmpegConfig,
			StreamConfig streamConfig,
		    int startNumber = 0)
		{
			var procInfo = new ProcessStartInfo();
			procInfo.FileName = ffmpegConfig.BinaryPath;
			streamConfig.Name = streamConfig.Name;

			var segmentFilename =
				ffmpegConfig.ChunkStorageDir + "/" +
	            streamConfig.Name + "/" +
				ffmpegConfig.SegmentFilename;

			var m3u8File =
				ffmpegConfig.ChunkStorageDir + "/" +
	            streamConfig.Name + "/" +
				"index.m3u8";
			
			procInfo.Arguments = string.Join(" ", new[]
			{
				"-y -re",
			    "-i " + streamConfig.Link,
			    "-map 0",
				"-start_number " + startNumber,
			    "-codec:v copy -codec:a copy",
			    "-f hls",
			    "-hls_time " + streamConfig.ChunkTime,
			    "-use_localtime 1 -use_localtime_mkdir 1",
				"-hls_flags second_level_segment_index",
			    "-hls_segment_filename " + segmentFilename,
			    m3u8File
			});

			procInfo.RedirectStandardOutput = true;
			procInfo.RedirectStandardError = true;

			var proc = new Process();
			proc.StartInfo = procInfo;
			proc.EnableRaisingEvents = true;

			proc.Exited += (o, s) =>
			{
				processes.Remove(o as Process);
				var lastID = GetLastProducedIndex(ffmpegConfig, streamConfig);

				logFile.WriteLine(string.Format(
					"[Restarting]: lastID = {0} | {1}",
					lastID,
					streamConfig.Name));
				StartChunking(ffmpegConfig, streamConfig, lastID + 1);            
			};

			proc.ErrorDataReceived += OutputErrDataReceived;
			proc.OutputDataReceived += OutputDataReceived;

			proc.Start();
			proc.BeginOutputReadLine();
			proc.BeginErrorReadLine();

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
                     
			var chunkFile = new ChunkFile(mostRecent);
			File.Delete(mostRecent);
			return chunkFile.index - 1;
			//var info = new FileInfo(mostRecent);
			//if (info.Length == 0)
			//{
			//	File.Delete(mostRecent);
			//	return chunkFile.index - 1;
			//}
			//else
			    //return chunkFile.index; 
		}
    }
}

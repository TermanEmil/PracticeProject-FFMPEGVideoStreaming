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

		public StreamingProcManager()
		{
			instance = this;
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
				"-hls_flags second_level_segment_duration+second_level_segment_index",
			    "-hls_segment_filename " + segmentFilename,
			    m3u8File
			});
   
			var proc = new Process();
			proc.StartInfo = procInfo;
			proc.EnableRaisingEvents = true;

			proc.Exited += (o, s) =>
			{
				processes.Remove(o as Process);
				var lastID = GetLastProducedIndex(ffmpegConfig, streamConfig);
				StartChunking(ffmpegConfig, streamConfig, lastID + 1);
			};

			proc.Start();
            
			processes.Add(proc);         
			proc.WaitForExit();
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
                         .OrderByDescending(File.GetLastWriteTime)
				         .FirstOrDefault();

			if (mostRecent == null)
				return -1;
            
			var chunkFile = new ChunkFile(mostRecent);
			File.Delete(mostRecent);
			return chunkFile.index;
		}
    }
}

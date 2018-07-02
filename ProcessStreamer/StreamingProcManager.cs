using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

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
			StreamConfig streamConfig)
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
			proc.Exited += (o, s) => StartChunking(ffmpegConfig, streamConfig);
			proc.Start();


			processes.Add(proc);
		}

		public void Cleanup()
		{
			foreach (var proc in processes)
			{
				proc.StandardInput.Close();
				if (!proc.WaitForExit(2 * 1000))
					proc.Kill();
				proc.Dispose();
			}
		}
    }
}

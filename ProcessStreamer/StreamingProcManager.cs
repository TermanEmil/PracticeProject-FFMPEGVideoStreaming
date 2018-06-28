using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace ProcessStreamer
{
    public class StreamingProcManager
    {
		private List<Process> processes = new List<Process>();

		public void StartChunking(
			FFMPEGConfig ffmpegConfig,
			StreamConfig streamConfig)
		{
			var procInfo = new ProcessStartInfo();
			procInfo.FileName = ffmpegConfig.BinaryPath;

			var segmentFilename =
				"Chunks/" +
				streamConfig.Name + "/" +
				ffmpegConfig.SegmentFilename;

			var m3u8File =
				"Chunks/" +
	            streamConfig.Name + "/" +
				"index.m3u8";

			procInfo.Arguments = string.Join(
    			" ",
    			new[]
    			{
    			    "-y -re",
				    "-i " + streamConfig.Link,
				    "-map 0",
				    "-codec:v copy -codec:a copy",
				    "-f hls",
				    ffmpegConfig.BaseUrl == "" ? "" : "-hls_base_url " + ffmpegConfig.BaseUrl,
				    "-hls_time " + streamConfig.ChunkTime,
				    "-use_localtime 1 -use_localtime_mkdir 1",
				    "-hls_segment_filename " + segmentFilename,
				    m3u8File
    			}
		    );

			var proc = new Process();
			proc.StartInfo = procInfo;
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

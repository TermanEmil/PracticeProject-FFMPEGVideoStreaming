using System;
using System.Threading.Tasks;
using FFMPEGStreamingTools.Utils;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Linq;

namespace FFMPEGStreamingTools
{
    public static class ChannelUpdate
    {
        public static void AddChannel()
        {
            string allProcessesInfo = "";
            string newChannelfile = "";

            FFMPEGConfigLoader.Load(
                out var ffmpegConfig,
                out var streamsConfig
            );

            foreach (var x in StreamingProcManager.instance.processes)
            {
                allProcessesInfo += x.StartInfo.Arguments;
            }

            foreach (var x in streamsConfig)
            {
                newChannelfile += x.Name;
            }

            foreach (var streamCfg in streamsConfig)
            {
                if (!allProcessesInfo.Contains(streamCfg.Name))
                {
                    Task.Run(() => StreamingProcManager.instance.StartChunking(ffmpegConfig, streamCfg));
                }
            }
        }
    }
}
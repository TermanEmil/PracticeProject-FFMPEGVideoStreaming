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

            FFMPEGConfigLoader.Load(
                out var ffmpegConfig,
                out var streamsConfig
            );

            foreach (var x in StreamingProcManager.instance.processes)
            {
                allProcessesInfo += x.Value.StartInfo.Arguments;
            }


            foreach (var streamCfg in streamsConfig)
            {
                if (!StreamingProcManager.instance.processes.ContainsKey(streamCfg.Name))
                {
                    Task.Run(() => StreamingProcManager.instance.StartChunking(ffmpegConfig, streamCfg));
                }
            }
        }
    }
}
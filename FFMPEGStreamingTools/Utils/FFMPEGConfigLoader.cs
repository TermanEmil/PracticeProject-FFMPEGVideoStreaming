using System;
using System.Collections.Generic;
using FFMPEGStreamingTools.StreamingSettings;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.IO;

namespace FFMPEGStreamingTools.Utils
{
    public static class FFMPEGConfigLoader
    {
        public static void Load(
            out FFMPEGConfig ffmpegCfg,
            out List<Channel> streamsConfigs)
        {
            string _jsonFile = File.ReadAllText("Channels.json");
            ffmpegCfg = new FFMPEGConfig();
            ffmpegCfg.BinaryPath = "ffmpeg";
            ffmpegCfg.ChunkStorageDir = "../Chunks";
            try
            {
                streamsConfigs = JsonConvert.DeserializeObject<List<Channel>>(_jsonFile);
            }
            catch (JsonException)
            {
                Console.WriteLine("Achtung !!!! Invalid Json");
                streamsConfigs = new List<Channel>(0);
            }
        }
    }
}

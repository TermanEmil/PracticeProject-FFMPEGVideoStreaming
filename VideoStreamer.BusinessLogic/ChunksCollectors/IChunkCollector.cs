using DataLayer;
using VideoStreamer.BusinessLogic.Models.ChunksCollectorModels;

namespace VideoStreamer.BusinessLogic.ChunksCollectors
{
    public interface IChunkCollector
    {
        ChunkFile GetClosestChunk(ChunksCollectorModelByTime model);
        ChunkFile[] GetNextBatch(ChunksCollectorModelByLast model);
    }
}
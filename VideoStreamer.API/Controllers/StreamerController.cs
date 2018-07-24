using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLayer;
using DataLayer.Configs;
using FFMPEGStreamingTools.Models;
using FFMPEGStreamingTools.SessionBrokers;
using FFMPEGStreamingTools.StreamingSettings;
using FFMPEGStreamingTools.TokenBrokers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Shared.Logic;
using VideoStreamer.BusinessLogic.ChunksCollectors;
using VideoStreamer.BusinessLogic.Models;
using VideoStreamer.BusinessLogic.Models.ChunksCollectorModels;
using VideoStreamer.BusinessLogic.PlaylistAssemblers;
using VideoStreamer.Models.Configs;
using VideoStreamer.Models.TokenParserModels;
using VideoStreamer.Services.TokenParsers;
using VideoStreamer.Utils;

namespace VideoStreamer.Controllers
{
	[Route("api")]
	public class StreamerController : Controller
	{
		private readonly IDistributedCache _cache;
		private readonly ITokenBroker _tokenBroker;
		private readonly ITokenParser _tokenParser;
		private readonly ISessionBroker _sessionBroker;
		private readonly IChunkCollector _chunkCollector;
		private readonly IPlaylistAssembler _playlistAssembler;
		private readonly ChunkerConfig _chunkerConfig;
		private readonly StreamerSessionCfg _sessionCfg;

		public StreamerController(
			ChunkerConfig chunkerConfig,
			StreamerSessionCfg streamerSessionCfg,
			IDistributedCache cache,
			ITokenBroker tokenBroker,
			ISessionBroker sessionBroker,
			ITokenParser tokenParser,
			IChunkCollector chunkCollector,
			IPlaylistAssembler playlistAssembler)
		{
			_cache = cache;
			_tokenBroker = tokenBroker;
			_tokenParser = tokenParser;
			_sessionBroker = sessionBroker;
			_chunkCollector = chunkCollector;
			_playlistAssembler = playlistAssembler;

			_chunkerConfig = chunkerConfig;
			_sessionCfg = streamerSessionCfg;
		}
        
		[Route("Stream/{channel}/index.m3u8")]
		public async Task<IActionResult> StreamAsync(
			string channel,
			int listSize = 5,
			string timeStr = null,
			int timeShiftMills = 0,
			bool displayContent = false,
			string registrationToken = null)
		{
			return await Task.Run(
				() => StreamInternalAsync(
					channel,
					listSize,
					timeStr,
					timeShiftMills,
					displayContent,
					registrationToken)
			);
		}

		private async Task<IActionResult> StreamInternalAsync(
			string channel,
			int listSize = 5,
			string timeStr = null,
			int timeShiftMills = 0,
			bool displayContent = false,
			string registrationToken = null)
		{
			DateTime requiredTime;

			if (timeStr == null)
				requiredTime = DateTime.Now;
			else
			{
				try
				{
					requiredTime =
						JsonConvert.DeserializeObject<DateTime>(timeStr);
				}
				catch (JsonReaderException)
				{ return View("InvalidTimeFormat"); }
			}

			if (timeShiftMills != 0)
				requiredTime = requiredTime.AddMilliseconds(-timeShiftMills);

			StreamSession session;
			var sessionBrokerModel = new SessionBrokerModel
			{
				Channel = channel,
				ListSize = listSize,
				RequiredTime = requiredTime,
				DisplayContent = displayContent,
				RegistrationToken = registrationToken,
				IP = HttpContext.Connection.RemoteIpAddress.ToString(),
				UserAgent = HttpContext.Request.Headers[HeaderNames.UserAgent]
			};

			try
			{
				session = _sessionBroker.InitializeSession(sessionBrokerModel);
			}
			catch (ChunkCollectorException e)
            { return Content(e.Message); }

			var token = _tokenBroker.GenerateToken(
				session,
				_sessionCfg.TokenSALT);
			
			try
			{
				await _cache.SetObjAsync(
					token,
					session,
					GetSessionExpiration());
			}
			catch (Exception)
			{ return View("RedisConnectionException"); }

			return RedirectToAction("TokenizedStreamAsync", new { token });
		}

		[Route("TokenizedStream/{channel}/index.m3u8")]
		public async Task<IActionResult> TokenizedStreamAsync(
			string channel,
			string token = null)
		{
			return await Task.Run(
				() => TokenizedStreamInternalAsync(channel, token));
		}

		private async Task<IActionResult> TokenizedStreamInternalAsync(
			string channel,
			string token = null)
		{
			var tokenParseResult = await _tokenParser.ParseAsync(
    			new TokenParserModel
    			{
    				Channel = channel, Token = token, HttpContext = HttpContext
    			});
			var session = tokenParseResult.session;

			if (session == null)
				return tokenParseResult.actionResult;

			M3U8Playlist playlist;         
			try
			{
				var collectorModel = new ChunksCollectorModelByLast
				{
					Channel = session.Channel,
					HlsListSize = session.HlsListSize,
					LastChunkPath = session.LastFilePath
				};
				playlist = _playlistAssembler.Aseemble(
					session,
					_chunkCollector.GetNextBatch(collectorModel));
				
				//playlist = _m3u8Generator.GenerateNextM3U8(
					//channel,
					//session.HlsListSize,
					//session.LastFileIndex,
					//session.LastFileTimeSpan);
			}
			catch (ChunkCollectorException e)
            { return Content(e.Message); }

			Console.WriteLine(
				"[StreamCtrl]:> m3u8: {0} TOKEN: {1}",
				string.Join(
					", ",
					playlist.Files.Select(x => x.FilePath)),
				session.LastFilePath
			);
            
			session.LastFilePath = playlist.Files.First().FilePath;

			try
			{
				await _cache.SetObjAsync(
					token,
					session,
					GetSessionExpiration());
			}
			catch (Exception)
			{ return View("RedisConnectionException"); }

			var result = playlist.Bake($"token={token}");
			if (session.DisplayContent)
				return Content(result);
			else
				return GenerateDownloadableContent(result);
		}

		[Route("{mode}/{channel}/{year}/{month}/{day}/{hour}/{minute}/{fileName}")]
		public async Task<IActionResult> GetChunkFileAsync(
			string mode,
			string channel,
			string year,
			string month,
			string day,
			string hour,
			string minute,
			string fileName,
			string token = null)
		{
			var tokenParseResult = await _tokenParser.ParseAsync(
                new TokenParserModel
                {
                    Channel = channel,
                    Token = token,
                    HttpContext = HttpContext
                });
            var session = tokenParseResult.session;

            if (session == null)
                return tokenParseResult.actionResult;
            
			var path = Path.Combine(
				_chunkerConfig.ChunkStorageDir,
				channel,
				year,
				month,
				day,
				hour,
				minute,
				fileName
			);

			if (!System.IO.File.Exists(path))
				return NotFound();

			Console.WriteLine(
				"[StreamCtrl]:> Requested TsFile {0} -> {1}",
				DateTime.Now,
				path);

			return new FileStreamResult(
				System.IO.File.OpenRead(path),
				"video/vnd.dlna.mpeg-tts");
		}

		#region Helpers
		private IActionResult GenerateDownloadableContent(string data)
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            var result = new FileContentResult(bytes, "text/utf8")
            {
                FileDownloadName = "index.m3u8"
            };

            return result;
        }
        
		private DistributedCacheEntryOptions GetSessionExpiration()
		{
			return new DistributedCacheEntryOptions()
				.SetAbsoluteExpiration(
					TimeSpan.FromSeconds(
						_sessionCfg.ExpirationTimeSeconds));
		}      
        #endregion
	}
}

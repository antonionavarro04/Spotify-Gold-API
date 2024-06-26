﻿using Microsoft.AspNetCore.Mvc;
using DAL;
using ENT;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using ENT.Dto.Metadata;
using SpotifyGoldServer.Models;
using Microsoft.AspNetCore.Authorization;
using COM;

namespace SpotifyGoldServer.Controllers.API {

    [Route("api/yt")]
    [ApiController]
    public class YoutubeController(): ControllerBase {

        /// <summary>
        /// Method that gets the audio from a YouTube video in base of the yt video id
        /// </summary>
        /// <param name="id">Id of the Video</param>
        /// <param name="quality">Quality of the audio, 0 by default, [0 - BEST, 1 - MEDIUM, 2 - BAD]</param>
        /// <param name="appendMetadata">Append the metadata of the video to the response header</param>
        [SwaggerResponse(200, "The audio was sent successfully")]
        [SwaggerResponse(401, "Unauthorized")]
        [SwaggerResponse(404, "Audio couldn't be found")]
        [HttpGet("{id}/download")]
        public async Task<IActionResult> Download(
                string id,
                [FromQuery(Name = "quality")] int quality,
                [FromQuery(Name = "appendMetadata")] bool appendMetadata = false
            ) {

            IActionResult result = NotFound("Id couldn't be found");

            ClsAudio audio = await MusicFunctions.DownloadAudio(id, quality);

            if (audio.Stream != null) {
                result = File(audio.Stream, "audio/mpeg", audio.Name);
                LogHandler.WriteToDDBB(new ClsLog(LoggingFunctions.GetIpPlusPort(HttpContext), $"Downloaded '{id}'"));
            }
            if (audio.Json != null && appendMetadata) {
                Response.Headers.Add("Metadata", audio.Json);
            }

            return result;
        }

        /// <summary>
        /// Function that gets the info of a YouTube video.
        /// It includes the title, author, description, etc.
        /// </summary>
        /// <param name="id">Id of the video</param>
        [SwaggerResponse(200, "The info was sent successfully")]
        [SwaggerResponse(401, "Unauthorized")]
        [SwaggerResponse(404, "Video doesn't exist")]
        [HttpGet("{id}/info")]
        public async Task<IActionResult> GetInfo(string id) {

            IActionResult result = NotFound("Video doesn't exist");

            string? json = await MusicFunctions.GetInfo(id);
            if (json != null) {
                result = Ok(json);
                LogHandler.WriteToDDBB(new ClsLog(LoggingFunctions.GetIpPlusPort(HttpContext), $"Get info of '{id}'"));
            }

            return result;
        }

        /// <summary>
        /// Function that searches for a list of videos in YouTube
        /// </summary>
        /// <param name="query">Query to search</param>
        /// <param name="maxResults">Max number of results</param>
        /// <returns></returns>
        [SwaggerResponse(200, "The search was sent successfully")]
        [SwaggerResponse(401, "Unauthorized")]
        [SwaggerResponse(404, "No videos found")]
        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string query,
            [FromQuery] int maxResults = 10
        ) {
            IActionResult result = NotFound("No videos found");

            string? json = await MusicFunctions.Search(query, maxResults);
            if (json != null) {
                result = Ok(json);
                LogHandler.WriteToDDBB(new ClsLog(LoggingFunctions.GetIpPlusPort(HttpContext), $"Search for '{query}' with {maxResults} results"));
            }

            return result;
        }
    }
}

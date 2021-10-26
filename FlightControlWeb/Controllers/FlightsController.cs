using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using FlightControlWeb.models;

namespace FlightControlWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightsController : ControllerBase
    {
        public readonly IMemoryCache _memoryCache;
        private FlightManager _flightManager;
        public FlightsController(IMemoryCache memoryCache,FlightManager newFlightManager)
        {
            _memoryCache = memoryCache;
            _flightManager = newFlightManager;
        }
        // GET: api/Flight
        [HttpGet, Route("/api/Flights")]
        public async Task<JsonResult> GetFlights(string relative_to, bool? sync_all=true)
        {
            JsonResult jsonResult;
            if (relative_to == null)
            {
                jsonResult = new JsonResult(null);
                return jsonResult;
            }

            DateTime relativeDate;

            //If sync_all was given
            if (sync_all!=true)
            {
                //http://localhost:50206/api/Flights?relative_to=2020-12-17T00:00:00Z&sync_all
                relativeDate = DateTime.ParseExact(relative_to, "yyyy-MM-ddTHH:mm:ssZ",
                    System.Globalization.CultureInfo.InvariantCulture);
                relativeDate=relativeDate.ToUniversalTime();
                jsonResult = new JsonResult(await _flightManager.AllReleventFlights(relativeDate));
                return jsonResult;
            }

            //Otherwise
            //http://localhost:50206/api/Flights?relative_to=2020-12-17T00:00:00Z
            relativeDate = DateTime.ParseExact(relative_to, "yyyy-MM-ddTHH:mm:ssZ",
                System.Globalization.CultureInfo.InvariantCulture);
            relativeDate=relativeDate.ToUniversalTime();
            jsonResult = new JsonResult(_flightManager.LocalReleventFlights(relativeDate));
            return jsonResult;
        }


        // DELETE: api/ApiWithActions/5
        //http://localhost:50206/api/Flights/1
        [HttpDelete ,Route("/api/Flights/{id}")]
        public ActionResult Delete(string id)
        {
            if (_flightManager.RemoveFlight(id))
            {
                return Ok();
            }
            Response.StatusCode = 400;
            return BadRequest();

        }

    }
}

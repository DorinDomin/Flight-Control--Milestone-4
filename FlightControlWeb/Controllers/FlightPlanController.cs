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
    public class FlightPlanController : ControllerBase
    {
        private readonly IMemoryCache _memoryCache;
        private FlightPlanManager _flightPlanManager;
        public FlightPlanController(IMemoryCache memoryCache,FlightPlanManager flightPlanManager)
        {
            _memoryCache = memoryCache;
            _flightPlanManager = flightPlanManager;
        }

        // GET: api/FlightPlan/5
        [HttpGet("{id}", Name = "Get")]
        public JsonResult Get(string id)
        {
            JsonResult jsonResult;

            //Gets FlightPlan that matches the giuven id
            FlightPlan returnFlightPlan = _flightPlanManager.GetFlightPlanById(id);
            
            if (returnFlightPlan == null)
            {
                Response.StatusCode = 400;
            }
            jsonResult = new JsonResult(returnFlightPlan);
            return jsonResult;
        }

        // POST: api/FlightPlan
        [HttpPost]
        public ActionResult Post([FromBody] FlightPlan value)
        {
            Dictionary<string, FlightPlan> mapKeyToFlightPlan=_flightPlanManager
                .GetMapKeyToFlightPlanFromCache();
            Flights newFlight = new Flights();

            //Checks if value is incomplete
            if (value.Company_Name == null  || value.Passengers == 0 || value.Segments == null
                ||value.Initial_Location.Date_Time==new DateTime())
            {
                return BadRequest();
            }

            //Creates new flight
            newFlight.Company_Name = value.Company_Name;
            newFlight.Longitude = value.Initial_Location.Longitude;
            newFlight.Latitude = value.Initial_Location.Latitude;
            newFlight.Passengers = value.Passengers;
            newFlight.Date_Time = value.Initial_Location.Date_Time;
            newFlight.Is_External = false;

            //Generates ID
            Random randNum = new Random();
            newFlight.Flight_Id = newFlight.Company_Name[0].ToString() + newFlight.Company_Name[1].
                ToString() + newFlight.Company_Name[2].ToString() + randNum.Next(10000,99999).
                ToString();
            while (mapKeyToFlightPlan.ContainsKey(newFlight.Flight_Id))
            {
                newFlight.Flight_Id = newFlight.Company_Name[0].ToString() + newFlight.
                    Company_Name[1].ToString() +newFlight.Company_Name[2].ToString() +
                    randNum.Next(10000, 99999).ToString();
            }
            FlightManager flightManager=new FlightManager(_memoryCache);

            //Adds new flight to data structures
            flightManager.AddFlight(newFlight);
            mapKeyToFlightPlan[newFlight.Flight_Id]= value;
            _memoryCache.Set("mapKeyToFlightPlan", mapKeyToFlightPlan);
            return Ok();
        }
    }
}

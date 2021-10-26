using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.Xml;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace FlightControlWeb.models
{
    public class FlightManager
    {
        public IMemoryCache projectMemoryCache { get; set; }

        public FlightManager(IMemoryCache memoryCache)
        {
            projectMemoryCache = memoryCache;
        }
        public List<Flights> LocalReleventFlights(DateTime relativeDate)
        {
            //Gets flightslist from cache
            List<Flights> flights = GetFlightListFromCache();

            List<Flights> flightsReturnList = new List<Flights>();

            //Gets all flights that are in the air at the time of relativeDate
            for (int flightIndex = 0; flightIndex < flights.Count; flightIndex++)
            {
                DateTime finTime = CalculateFlightEndTime(flights.ElementAt(flightIndex));
                if (flights.ElementAt(flightIndex).Is_External == false &&
                    flights.ElementAt(flightIndex).Date_Time <= relativeDate &&
                    relativeDate <= finTime)
                {
                    flightsReturnList.Add(flights.ElementAt(flightIndex));
                }
            }
            return flightsReturnList;
        }

        public async Task<List<Flights>> AllReleventFlights(DateTime relativeDate)
        {
            ServersManager serversManager = new ServersManager(projectMemoryCache);
            FlightPlanManager flightPlanManager = new FlightPlanManager(projectMemoryCache);
            List<Server> servers = serversManager.GetAllExternalServers();
            List<Flights> allFlights = LocalReleventFlights(relativeDate);
            Dictionary<string, FlightPlan> mapKeyToFlightPlan = flightPlanManager
                .GetMapKeyToFlightPlanFromCache();

            //Gets flights from external servers
            for (int i = 0; i < servers.Count; i++)
            {
                List<Flights> serverFlights = await GetRelevantFlightsFromServer(
                    servers.ElementAt(i), relativeDate);
                //Checks for error
                if (serverFlights.Count==0)
                {
                    continue;
                }

                //Loops over server's flights
                foreach (Flights currentFlight in serverFlights)
                {
                    FlightPlan currentFlightPlan = await GetFlightPlanByIDAndURL(
                        currentFlight.Flight_Id, servers.ElementAt(i).ServerURL);

                    //Checks validity
                    allFlights = AddFlightsIfValid(allFlights, currentFlightPlan, currentFlight);
                    mapKeyToFlightPlan = UpdateDictionary(currentFlightPlan, mapKeyToFlightPlan,
                        currentFlight.Flight_Id);
                    projectMemoryCache.Set("mapKeyToFlightPlan", mapKeyToFlightPlan);
                    /*if (currentFlightPlan != null)
                    {
                        allFlights.Add(currentFlight);
                        mapKeyToFlightPlan[currentFlight.Flight_Id] = currentFlightPlan;
                    }*/
                }
            }

            for( int i = 0; i < allFlights.Count; i++)
            {
                allFlights[i]= CalculateCurrentLocation(allFlights.ElementAt(i),
                    mapKeyToFlightPlan[allFlights.ElementAt(i).Flight_Id],
                    relativeDate.ToUniversalTime());
            }
            return allFlights;
        }
        public bool RemoveFlight(string flightID)
        {
            List<Flights> flights = this.GetFlightListFromCache();
            for (int i = 0; i < flights.Count; i++)
            {
                if (flights.ElementAt(i).Flight_Id == flightID)
                {
                    flights.RemoveAt(i);
                    projectMemoryCache.Set("flightsList", flights);
                    return true;
                }
            }
            return false;
        }
        public void AddFlight(Flights flight)
        {
            //Adds new flight to flightsList in memorycache
            List<Flights> flights = this.GetFlightListFromCache();
            flights.Add(flight);
            projectMemoryCache.Set("flightsList", flights);
        }

        public List<Flights> GetFlightListFromCache()
        {
            //Gets flightsList from memorycache
            List<Flights> flights;
            if (!projectMemoryCache.TryGetValue("flightsList", out flights))
            {
                projectMemoryCache.Set("flightsList", new List<Flights>());
                projectMemoryCache.TryGetValue("flightsList", out flights);
            }
            return flights;
        }

        public DateTime CalculateFlightEndTime(Flights flight)
        {
            //Initializes finTime with start time
            DateTime finTime = flight.Date_Time;
            FlightPlanManager flightPlanManager = new FlightPlanManager(projectMemoryCache);
            Dictionary<string, FlightPlan> mapKeyToFlightPlan = flightPlanManager
                .GetMapKeyToFlightPlanFromCache();

            //Calculates finish time
            for (int flightIndex = 0; flightIndex <
                mapKeyToFlightPlan[flight.Flight_Id].Segments.Count; flightIndex++)
            {
                finTime = finTime.AddSeconds(mapKeyToFlightPlan[flight.Flight_Id]
                    .Segments[flightIndex].TimeSpan_Seconds);
            }
            return finTime;
        }

        public virtual async Task<List<Flights>> GetRelevantFlightsFromServer(
            Server server, DateTime relativeDate)
        {
            //Gets local flights from external server
            List<Flights> serverFlights = new List<Flights>();
            string URL = String.Format(server.ServerURL + "/api/Flights?relative_to=" +
                relativeDate.ToString("s")+"Z");
            WebRequest request = WebRequest.Create(URL);
            request.Method = "GET";
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)await request.GetResponseAsync();
            }
            catch
            {
                return new List<Flights>();
            }
            string strResult = null;
            List<Flights> jsonObject;
            using (Stream stream = response.GetResponseStream())
            {
                StreamReader streamReader = new StreamReader(stream);
                strResult = streamReader.ReadToEnd();
                try
                {
                    jsonObject = JsonConvert.DeserializeObject<List<Flights>>(strResult);
                }
                catch
                {
                    jsonObject = new List<Flights>();
                }
                streamReader.Close();
            }

            foreach (Flights currentFlight in jsonObject)
            {
                //Checks for error
                if(currentFlight.Passengers==0||currentFlight.Flight_Id==null||
                    currentFlight.Date_Time==new DateTime() || currentFlight.Company_Name == null)
                {
                    continue;
                }
                currentFlight.Is_External = true;
                serverFlights.Add(currentFlight);
            }

            //Checks for error
            if (jsonObject == null)
            {
                return new List<Flights>();
            }
            return serverFlights;
        }

        public virtual async Task<FlightPlan> GetFlightPlanByIDAndURL(string id, string url)
        {
            //Gets FlightPlan from external server
            string URL = String.Format(url + "/api/FlightPlan/" + id);
            WebRequest request = WebRequest.Create(URL);
            request.Method = "GET";
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)(await request.GetResponseAsync());
            }
            catch
            {
                return null;
            }
            string strResult = null;
            FlightPlan jsonObject;
            using (Stream stream = response.GetResponseStream())
            {
                StreamReader streamReader = new StreamReader(stream);
                strResult = streamReader.ReadToEnd();
                jsonObject = JsonConvert.DeserializeObject<FlightPlan>(strResult);
                streamReader.Close();
            }

            //Checks for error
            if(jsonObject.Company_Name==null||jsonObject.Initial_Location.Date_Time==new DateTime()||
                jsonObject.Passengers == 0 || jsonObject.Segments == null)
            {
                return null;
            }
            return (FlightPlan)(jsonObject);
        }
        public Flights CalculateCurrentLocation(Flights currentFlight, FlightPlan currentFlightPlan
            ,DateTime relativeDate)
        {
            int sumTime = 0;
            int segmentIndex;
            for (segmentIndex = 0; (currentFlightPlan != null)&&
                segmentIndex < (currentFlightPlan.Segments.Count); segmentIndex++)
            {
                //Calculate percentage of segment completed
                double percent = ((relativeDate - currentFlightPlan.Initial_Location.Date_Time)
                    .TotalSeconds - sumTime) / currentFlightPlan.Segments[segmentIndex]
                    .TimeSpan_Seconds;

                //Checks if we are in the current segment
                if ((relativeDate - currentFlightPlan.Initial_Location.Date_Time)
                    .TotalSeconds - sumTime > currentFlightPlan.Segments[segmentIndex]
                    .TimeSpan_Seconds)
                {
                    sumTime += currentFlightPlan.Segments[segmentIndex].TimeSpan_Seconds;
                    continue;
                }

                //If segmentIndex==0
                else if (segmentIndex == 0)
                {

                    currentFlight.Latitude = currentFlightPlan.Initial_Location.Latitude +
                        percent * (currentFlightPlan.Segments[segmentIndex].Latitude -
                        currentFlightPlan.Initial_Location.Latitude);
                    currentFlight.Longitude = currentFlightPlan.Initial_Location.Longitude 
                        + percent * (currentFlightPlan.Segments[segmentIndex].Longitude -
                        currentFlightPlan.Initial_Location.Longitude);
                    break;
                }

                //Otherwise
                else
                {
                    currentFlight.Latitude = currentFlightPlan.Segments[segmentIndex - 1].Latitude +
                        percent * (currentFlightPlan.Segments[segmentIndex].Latitude -
                        currentFlightPlan.Segments[segmentIndex - 1].Latitude);
                    currentFlight.Longitude = currentFlightPlan.Segments[segmentIndex - 1].Latitude +
                        percent * (currentFlightPlan.Segments[segmentIndex].Longitude -
                        currentFlightPlan.Segments[segmentIndex - 1].Latitude);
                    break;

                }
            }
            return currentFlight;
        }

        public List<Flights> AddFlightsIfValid(List<Flights> allFlights,
            FlightPlan currentFlightPlan,Flights currentFlight)
        {
            if (currentFlightPlan != null)
            {
                allFlights.Add(currentFlight);
            }
            return allFlights;
        }
        public Dictionary<string,FlightPlan> UpdateDictionary(FlightPlan currentFlightPlan,
             Dictionary<string, FlightPlan> mapKeyToFlightPlan,string id)
        {
            if (currentFlightPlan != null)
            {
                mapKeyToFlightPlan[id] = currentFlightPlan;
            }
            return mapKeyToFlightPlan;
        }
    }
}

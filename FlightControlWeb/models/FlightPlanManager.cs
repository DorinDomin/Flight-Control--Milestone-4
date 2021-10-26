using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightControlWeb.models
{
    //FlightPlanManager Class
    public class FlightPlanManager
    {
        private readonly IMemoryCache _memoryCache;

        public FlightPlanManager(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public FlightPlan GetFlightPlanById(string id)
        {
            //Gets MapKeyToFlightPlan from memorycache
            Dictionary<string, FlightPlan> MapKeyToFlightPlan=GetMapKeyToFlightPlanFromCache();

            //Checks for id
            if(MapKeyToFlightPlan.ContainsKey(id))
                return MapKeyToFlightPlan[id];
            return null;
        }
        public Dictionary<string, FlightPlan> GetMapKeyToFlightPlanFromCache()
        {
            //Gets MapKeyToFlightPlan from memorycache
            Dictionary<string, FlightPlan> mapKeyToFlightPlan;
            if (!_memoryCache.TryGetValue("mapKeyToFlightPlan", out mapKeyToFlightPlan))
            {
                _memoryCache.Set("mapKeyToFlightPlan", new Dictionary<string, FlightPlan>());
                _memoryCache.TryGetValue("mapKeyToFlightPlan", out mapKeyToFlightPlan);
            }
            return mapKeyToFlightPlan;
        }
    }
}

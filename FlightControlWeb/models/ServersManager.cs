using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightControlWeb.models
{
    //ServersManagerClass
    public class ServersManager
    {
        private readonly IMemoryCache _memoryCache;
        public ServersManager(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }
        public List<Server> GetAllExternalServers()
        {
            //Gets serverList from memorycache
            return GetServerListFromCache();
        }
        public bool DeleteServer(string idToDelete)
        {
            //Gets serverList from memorycache
            List<Server> servers=GetServerListFromCache();

            //Tries to delete server
            Server serverToDelete = servers.Where(server =>server.ServerID.Equals(idToDelete))
                .FirstOrDefault();
            if (serverToDelete != null)
            {
                servers.Remove(serverToDelete);
                _memoryCache.Set("serversList", servers);
                return true;
            }
            return false;
        }
        public bool AddExternalServer(Server newServer)
        {
            //Gets serverList from memorycache
            List<Server> servers=GetServerListFromCache();

            //Checks if ServerID is new
            bool isIDExist = false;
            foreach(Server currentServer in servers)
            {
                if (currentServer.ServerID == newServer.ServerID)
                {
                    isIDExist = true;
                }
            }

            //If ServerID is indeed new
            if (!isIDExist)
            {
                //Adds Server
                servers.Add(newServer);
                _memoryCache.Set("serversList", servers);
                return true;
            }
            return false;  
        }

        public List<Server> GetServerListFromCache()
        {
            List<Server> servers;
            if (!_memoryCache.TryGetValue("serversList", out servers))
            {
                _memoryCache.Set("serversList", new List<Server>());
                _memoryCache.TryGetValue("serversList", out servers);
            }
            return servers;
        }
    }
}
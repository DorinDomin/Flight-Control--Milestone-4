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
    public class ServersController : ControllerBase
    {
/*        private readonly IMemoryCache _memoryCache;
        private ServersManager _serversManager;
*/
        public IMemoryCache _memoryCache { get; set; }
        public ServersManager _serversManager { get; set; }

        public ServersController(IMemoryCache memoryCache,ServersManager serversManager)
        {
            _memoryCache = memoryCache;
            _serversManager = serversManager;
        }
        // GET: api/Servers
        [HttpGet, Route("/api/servers")]
        public JsonResult Get()
        {
            //ServersManager serverManager=new ServersManager(_memoryCache);
            return new JsonResult(_serversManager.GetAllExternalServers());
        }

        // POST: api/Servers
        [HttpPost, Route("/api/servers")]
        public ActionResult Post(Server s)
        {
            //ServersManager serverManager=new ServersManager(_memoryCache);
            if (_serversManager.AddExternalServer(s))
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}"), Route("api/servers")]
        public ActionResult Delete(string id)
        {
            //ServersManager serverManager=new ServersManager(_memoryCache);
            if (_serversManager.DeleteServer(id))
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
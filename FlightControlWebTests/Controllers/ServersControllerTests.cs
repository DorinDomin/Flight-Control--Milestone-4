using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using FlightControlWeb.Controllers;
using FlightControlWeb.models;

namespace WEBFLightSimulatorTests.Controllers
{
    [TestClass()]
    public class ServersControllerTests
    {
        [TestMethod()]
        public void Get_GetsServersList_CorrectList()
        {
            //Stub
            //Arrange
            MemoryCache testMemory = new MemoryCache(new MemoryCacheOptions());
            testMemory.Set("serversList", new List<Server> {
                new Server{ServerID="1",ServerURL="abc"},
                new Server { ServerID = "2", ServerURL = "abc" }});
            ServersManager serversManager = new ServersManager(testMemory);
            ServersController serversController = new ServersController(testMemory,serversManager);
            JsonResult task = (serversController.Get());
            List<Server> jsonObject = (List<Server>)(task.Value);

            //Assert
            Assert.AreEqual(jsonObject.Count,2);
            Assert.AreEqual(jsonObject[0].ServerID,"1");
            Assert.AreEqual(jsonObject[0].ServerURL, "abc");
            Assert.AreEqual(jsonObject[1].ServerID, "2");
            Assert.AreEqual(jsonObject[1].ServerURL, "abc");
        }

        [TestMethod()]
        public void Delete_DeletingServerID_ReturnsListWithoutID()
        {
            //Stub
            //Arrange
            MemoryCache testMemory = new MemoryCache(new MemoryCacheOptions());
            testMemory.Set("serversList", new List<Server> { 
                new Server { ServerID = "1", ServerURL = "abc" },
                new Server { ServerID = "2", ServerURL = "abc" } });
            ServersManager serversManager = new ServersManager(testMemory);
            ServersController serversController =
                new ServersController(testMemory, serversManager);

            ActionResult actionResult1=serversController.Delete("1");
            JsonResult task1 = (serversController.Get());
            List<Server> jsonObject1 = (List<Server>)(task1.Value);
            ActionResult actionResult2=serversController.Delete("1");
            JsonResult task2 = (serversController.Get());
            List<Server> jsonObject2 = (List<Server>)(task2.Value);

            //Assert
            Assert.AreEqual(jsonObject1.Count, 1);
            Assert.AreEqual(actionResult1.ToString(),"Microsoft.AspNetCore.Mvc.OkResult");
            Assert.AreEqual(jsonObject2.Count, 1);
            Assert.AreEqual(actionResult2.ToString(), "Microsoft.AspNetCore.Mvc.BadRequestResult");
        }
    }
}

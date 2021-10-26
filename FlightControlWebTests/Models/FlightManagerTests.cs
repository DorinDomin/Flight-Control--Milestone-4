using Microsoft.VisualStudio.TestTools.UnitTesting;
using FlightControlWeb.Controllers;
using System;
using System.Collections.Generic;
using System.Text;
using FlightControlWeb.models;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.IO;

namespace FlightControlWeb.Controllers.Tests
{
    [TestClass()]
    public class FlightManagerTests
    {
        [TestMethod()]
        public void AllRelevantFlights_GetsAllRelevantFlights_ReturnsListOfSize4()
        {
            //Arrange

            //Creates stub memory
            MemoryCache testMemory = new MemoryCache(new MemoryCacheOptions());
            testMemory.Set
            ("flightsList", new List<Flights>
            {
                 new Flights{Flight_Id="1",Longitude=30,Latitude=30,Passengers=100,
                     Company_Name="swiss",Date_Time=new DateTime(2021,11,11,9,0,0),
                     Is_External=false},
                 new Flights{Flight_Id="2",Longitude=30,Latitude=30,Passengers=100,
                     Company_Name="swiss",Date_Time=new DateTime(2021,11,11,9,0,0),
                     Is_External=false},
                 new Flights{Flight_Id="3",Longitude=30,Latitude=30,Passengers=100,
                     Company_Name="swiss",Date_Time=new DateTime(2021,11,11,9,0,0),
                     Is_External=false}
            });
            testMemory.Set
              ("serversList", new List<Server> {
              new Server{ServerID="abc",ServerURL="abc"}
              });

            Dictionary<string, FlightPlan> mapKeyToFlightPlan =
                new Dictionary<string, FlightPlan>();
            InitialLocation dummyLocation = new InitialLocation(10, 10,
                new DateTime(2021, 11, 11, 9, 0, 0));
            List<Segment> dummySegments = new List<Segment> { new Segment(10, 10, 500) };
            mapKeyToFlightPlan.Add("1", new FlightPlan { Passengers = 100, Company_Name = "swiss",
                Initial_Location = dummyLocation, Segments =dummySegments});
            mapKeyToFlightPlan.Add("2", new FlightPlan { Passengers = 100, Company_Name = "swiss",
                Initial_Location =dummyLocation, Segments = dummySegments });
            mapKeyToFlightPlan.Add("3", new FlightPlan { Passengers = 100, Company_Name = "swiss",
                Initial_Location = dummyLocation, Segments = dummySegments });
            testMemory.Set("mapKeyToFlightPlan", mapKeyToFlightPlan);

            //Mocks FlightManager and replaces functions to test AllRelevantFlights
            var mock = new Mock<FlightManager>(testMemory);
            FlightManager flightManager = new FlightManager(testMemory);
            mock.CallBase = true;
            mock.Setup(x => x.GetRelevantFlightsFromServer(It.IsAny<Server>(),
                It.IsAny<DateTime>())).Returns(TestGetFlightsFromServer());
            mock.Setup(x => x.GetFlightPlanByIDAndURL(It.IsAny<string>(),
                It.IsAny<string>())).Returns(TestGetFlightPlanFromServer());

            //Act
            List<Flights> flights = mock.Object.AllReleventFlights(dummyLocation.Date_Time).Result;

            //Assert
            Assert.AreEqual(flights.Count, 4);
            Assert.AreEqual(flights[3].Company_Name, "swiss");
            Assert.AreEqual(flights[3].Longitude, 30);
            Assert.AreEqual(flights[3].Latitude, 30);
            Assert.AreEqual(flights[3].Passengers, 100);
            Assert.AreEqual(flights[3].Date_Time.ToString(), dummyLocation.Date_Time.ToString());
            Assert.IsTrue(flights[3].Is_External);
        }

        private async Task<List<Flights>> TestGetFlightsFromServer()
        {
            List<Flights> flights = new List<Flights> {
                new Flights { Flight_Id = "4", Longitude = 30, Latitude = 30,
                    Passengers = 100, Company_Name = "swiss",
                    Date_Time = new DateTime(2021, 11, 11, 9, 0, 0), Is_External = true }
            };
            return await Task.Run(() => flights);
        }

        private async Task<FlightPlan> TestGetFlightPlanFromServer()
        {
            FlightPlan flightPlan = new FlightPlan
            {
                Passengers = 100,
                Company_Name = "swiss",
                Initial_Location = new InitialLocation(10, 10, new DateTime(10, 10, 10)),
                Segments = new List<Segment>() { new Segment(10, 10, 500) }
            };

            return await Task.Run(() => flightPlan);
        }
        public bool TestRemoveFlight(MemoryCache memoryCache)
        {
            List<Flights> flights;
            memoryCache.TryGetValue("flightsList", out flights);
            foreach (Flights flight in flights)
            {
                if (flight.Flight_Id == "1")
                {
                    flights.Remove(flight);
                    break;
                }
            }
            memoryCache.Set("flightsList", flights);
            return true;
        }
        public Task<List<Flights>> AllReleventFlightsTest(DateTime relativeDate,
            FlightManager flightManager)
        {
            return flightManager.AllReleventFlights(relativeDate);
        }
    }
}
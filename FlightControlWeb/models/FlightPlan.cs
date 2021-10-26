using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FlightControlWeb.models
{
    //Segment struct
    public struct Segment
    {
        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("timespan_seconds")]
        public int TimeSpan_Seconds { get; set; }
        public Segment(double longitude,double latitude,int timespanSeconds)
        {
            this.Longitude = longitude;
            this.Latitude = latitude;
            this.TimeSpan_Seconds = timespanSeconds;
        }
    }

    //InitialLocation struct
    public struct InitialLocation
    {
        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("date_time")]
        public DateTime Date_Time { get; set; }
        public InitialLocation(double longitude, double latitude, DateTime date_time)
        {
            this.Longitude = longitude;
            this.Latitude = latitude;
            this.Date_Time = date_time;
        }
    }

    //FlightPlan Class
    public class FlightPlan
    {
        [JsonPropertyName("passengers")]
        public int Passengers { get; set; }

        [JsonPropertyName("company_name")]
        public string Company_Name { get; set; }

        [JsonPropertyName("initial_location")]
        public InitialLocation Initial_Location { get; set; }
        
        [JsonPropertyName("segments")]
        public List<Segment> Segments { get; set; }
    }
}

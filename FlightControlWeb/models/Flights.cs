using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FlightControlWeb.models
{
    //Flights Class
    public class Flights
    {
        [JsonPropertyName("flight_id")]
        public string Flight_Id { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("passengers")]
        public int Passengers { get; set; }

        [JsonPropertyName("company_name")]
        public string Company_Name { get; set; }

        [JsonPropertyName("date_time")]
        public DateTime Date_Time { get; set; }

        [JsonPropertyName("is_external")]
        public bool Is_External { get; set; }
    }
}

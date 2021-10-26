# Flight-Control--Milestone-4
The purpose of this extercise- we created a control system to monitor active flights, as well as adding new flights. The programm works in a way so that it shows only the active flights at the moment. The system reset every few second, in order to be as accurate as possible. In addition, our system is in sync with other systems as well. As you can see, the system follows internal flights- the ones we inserted, as well as external flights.

![image](https://user-images.githubusercontent.com/58748407/138885735-ca3835ed-ec51-43ff-8576-947a9ed31777.png)


## How it works:

If you wish to insert some flight- there are two main ways to do that:

* External Flights- you can log as a new client to our system, using the "Advanced REST client" app, following those steps:

  - enter the requsted url in the url box.
  - use the json format for external flights as shown in in the picture below.
  - choose the "Post" option.
    ![image](https://user-images.githubusercontent.com/58748407/138886860-b07df893-c2f8-4f72-be39-b3badec3e771.png)
    
 
 
 
* To add an external flight to the server, copy the external servers url to the "Resquest URL", and add the json flight details in the box below, as shown in the picture:

     ![image](https://user-images.githubusercontent.com/58748407/138886951-74607c42-aea9-481d-920f-9a696e3b669b.png)
    
    
* Internal Flights:
you can insert a new Json file with the flight detailes by drugging it into the flights box. 
Notice the time of the flight in the json should be 3 hours preior to the current time (UTC).

#### Json format for Internal flights
{ "passengers": 216, "company_name": "SwissAir", "initial_location": { "longitude": 33.244, "latitude": 31.12, "date_time": "2020-12-26T23:56:21Z" }, "segments": [ { "longitude": 33.234, "latitude": 31.18, "timespan_seconds": 650 }, /... more segments.../ ] }

#### Json format for External Servers
{ "ServerId": "[SERVER_ID]", "ServerURL": "www.server.com" }


## Version Control

We used Git for this project

https://github.com/DorinDomin/Flight-Control--Milestone-4

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.


## Authors
Dorin Domin, Netanel Tamir

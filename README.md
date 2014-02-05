orchestrator.js .NET Gadgeteer client
=====================================




How to use
----------

- create a new Gadgeteer project e.g. CoffeeMachine
- select your Gadgeteer board e.g. spider 1

- add atleast power and wifi modules to your board

- Add the OrchestratorJSClient to your solution: right-click your _SOLUTION_ -> add -> Existing Project
-- find OrchestratorJSClient project file and add it

- Add reference to your project: right-click your _PROJECT_ -> Add References and select from projects tab the OrchestratorJSClient

- Initialize the OrchestratorJSClient:

    orchestratorJSClient = new OrchestratorJSClient.OrchestratorJSClient(deviceIdentity); 

- Register your call back - the one where you handle the commands from the orchestrator.js server:
```chsharp   
    orchestratorJSClient.MethodCallReceived += new OrchestratorJSClient.OrchestratorJSClient.OnMethodCallRecivedHandler((e) => {
        if ( e.capabilityName== "MulticolorLed" && e.methodCallName == "blue")
        {
            multicolorLed.RemoveGreen();
            multicolorLed.RemoveRed();
            multicolorLed.AddBlue();
        }
        else if (e.capabilityName == "MulticolorLed" && e.methodCallName == "green")
        {
            multicolorLed.RemoveBlue();
            multicolorLed.RemoveRed();
            multicolorLed.AddGreen();
        }
        else if (e.capabilityName == "MulticolorLed" && e.methodCallName == "red")
        {
            multicolorLed.RemoveBlue();
            multicolorLed.RemoveGreen();
            multicolorLed.AddRed();
        }
        else 
        {
            Debug.Print("unknown capability and/or method");    
        }
        orchestratorJSClient.sendResponse();
    });    
```

- Initialize your wifi and in wifi's connectivityChanged handler connect to orchestrator.js server. 
This way you ensure that the Internet connection is up when you try to connect to the orchestrator. 
The initialization can be done e.g. with OrchestratorJSClient's helper like this:
```csharp
    // init wifi and register callback
    WiFiRS9110 wifi = wifi_RS21.Interface;
    wifi.WirelessConnectivityChanged += new WiFiRS9110.WirelessConnectivityChangedEventHandler((s, e) => {
        orchestratorJSClient.connect(host, port);
    });
    OrchestratorJSClient.GeneralHelpers.initWiFi(wifi, ssid, passwd);
```
- After this the client should be good to go!


Example Program.cs
------------------

```csharp
using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Touch;

using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;
using GHI.Premium.Net;
using OrchestratorJSClient;

namespace CoffeeMachine
{
    public partial class Program
    {
        string deviceIdentity = "nikkis@gadgeteer";

        // orchestrator settings
        string host = "192.168.0.12";
        string port = "9000";

        // wifi settings
        string ssid = "peltomaa";
        string passwd = "socialdevices";

        OrchestratorJSClient.OrchestratorJSClient orchestratorJSClient;

        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            // init wifi and register callback
            WiFiRS9110 wifi = wifi_RS21.Interface;
            wifi.WirelessConnectivityChanged += new WiFiRS9110.WirelessConnectivityChangedEventHandler((s, e) => {
                orchestratorJSClient.connect(host, port);
            });
            OrchestratorJSClient.GeneralHelpers.initWiFi(wifi, ssid, passwd);

            // create new instance of orchestratorJS
            orchestratorJSClient = new OrchestratorJSClient.OrchestratorJSClient(deviceIdentity); 

            // register handler for listening method calls from orchestrator
            orchestratorJSClient.MethodCallReceived += new OrchestratorJSClient.OrchestratorJSClient.OnMethodCallRecivedHandler((e) => {
                if ( e.capabilityName== "MulticolorLed" && e.methodCallName == "blue")
                {
                    multicolorLed.RemoveGreen();
                    multicolorLed.RemoveRed();
                    multicolorLed.AddBlue();
                }
                else if (e.capabilityName == "MulticolorLed" && e.methodCallName == "green")
                {
                    multicolorLed.RemoveBlue();
                    multicolorLed.RemoveRed();
                    multicolorLed.AddGreen();
                }
                else if (e.capabilityName == "MulticolorLed" && e.methodCallName == "red")
                {
                    multicolorLed.RemoveBlue();
                    multicolorLed.RemoveGreen();
                    multicolorLed.AddRed();
                }
                else 
                {
                    Debug.Print("unknown capability and/or method");    
                }
                orchestratorJSClient.sendResponse();
            });
        }
    }
}
```


orchestrator.js .NET Gadgeteer client
=====================================




How to use
----------

1. Download OrchestratorJSGadgeteer and unzip it.
2. Create a new Gadgeteer project e.g. CoffeeMachine
3. Select your Gadgeteer board e.g. spider 1

4. Add at least power and wifi modules to your board

5. Add the OrchestratorJSClient to your solution: 
	- Right-click your _SOLUTION_ -> add -> Existing Project
	- Find OrchestratorJSClient project file and add it

6. Add reference to your project: 
	- Right-click your _PROJECT_ -> Add References and select from projects tab the OrchestratorJSClient

7. Initialize the OrchestratorJSClient:

```chsharp
    orchestratorJSClient = new OrchestratorJSClient.OrchestratorJSClient(deviceIdentity); 
```

8. Register your call back - the one where you handle the commands from the orchestrator.js server:

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

9. Initialize your wifi and in wifi's connectivityChanged handler connect to orchestrator.js server. 
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
10. After this your gadget should be able to communicate orchestrator.js server!


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
            orchestratorJSClient.MethodCallReceived += new OrchestratorJSClient.OrchestratorJSClient.OnMethodCallRecievedHandler((e) => {
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


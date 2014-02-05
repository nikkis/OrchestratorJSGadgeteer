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

namespace OrchestratorJSClient
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

        OrchestratorJSClient orchestratorJSClient;

        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            // init wifi and register callback
            WiFiRS9110 wifi = wifi_RS21.Interface;
            wifi.WirelessConnectivityChanged += new WiFiRS9110.WirelessConnectivityChangedEventHandler(wifi_WirelessConnectivityChanged);
            GeneralHelpers.initWiFi(wifi, ssid, passwd);

            // create new instance of orchestratorJS
            orchestratorJSClient = new OrchestratorJSClient(deviceIdentity);

            // register handler for listening method calls from orchestrator
            orchestratorJSClient.MethodCallReceived += new OrchestratorJSClient.OnMethodCallRecievedHandler(ojs_client_MethodCallReceived);

            Debug.Print("Program Initialized");
        }



        void wifi_WirelessConnectivityChanged(object sender, WiFiRS9110.WirelessConnectivityEventArgs e)
        {
            // connect to orchestrator
            orchestratorJSClient.connect(host, port);
        }



        void ojs_client_MethodCallReceived(OrchestratorJSEventArgs e)
        {
            Debug.Print(e.capabilityName);
            Debug.Print(e.methodCallName);

            if (e.methodCallName == "blue")
            {
                multicolorLed.RemoveGreen();
                multicolorLed.RemoveRed();
                multicolorLed.AddBlue();
            }
            else if (e.methodCallName == "green")
            {
                multicolorLed.RemoveBlue();
                multicolorLed.RemoveRed();
                multicolorLed.AddGreen();
            }
            else if (e.methodCallName == "red")
            {
                multicolorLed.RemoveBlue();
                multicolorLed.RemoveGreen();
                multicolorLed.AddRed();
            }

            orchestratorJSClient.sendResponse();
        }

    }
}

using System;
using Microsoft.SPOT;
using GHI.Premium.Net;

namespace OrchestratorJSClient
{
    public class GeneralHelpers
    {

        public static void initWiFi(WiFiRS9110 wifi, String ssid, String passphrase)
        {
            if (!wifi.IsOpen)
                wifi.Open();
            wifi.NetworkInterface.EnableDhcp();
            NetworkInterfaceExtension.AssignNetworkingStackTo(wifi);

            /*
            wifi.WirelessConnectivityChanged +=
                new WiFiRS9110.WirelessConnectivityChangedEventHandler(Interface_WirelessConnectivityChanged);
            wifi.NetworkAddressChanged +=
               new NetworkInterfaceExtension.NetworkAddressChangedEventHandler(Interface_NetworkAddressChanged);
            */

            // Scan for networks and connect if found
            WiFiNetworkInfo[] ScanResp = wifi.Scan();
            int i = 0;
            foreach (WiFiNetworkInfo info in ScanResp)
            {
                Debug.Print("Found WLAN: " + i + ", " + info.SSID);
                if (info.SSID.ToString().Equals(ssid))
                {
                    wifi.Join(ScanResp[i], passphrase);
                    break;
                }
                i++;
            }
        }

    }
}

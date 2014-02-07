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
using Gadgeteer.Modules.Seeed;

using Microsoft.SPOT.Input;





namespace CoffeeMachine
{
    public partial class Program
    {

        public enum CoffeeMachineState { ON, OFF, LOADED };
        public CoffeeMachineState coffeeMachineState = CoffeeMachineState.OFF;

        string deviceIdentity = "nikkis@gadgeteer";

        // orchestrator settings
        string host = "192.168.0.12";
        string port = "9000";

        // wifi settings
        string ssid = "peltomaa";
        string passwd = "socialdevices";

        OrchestratorJSClient.OrchestratorJSClient orchestratorJSClient;

        Boolean gyroIsmesasuring;

        

        public Canvas canvas;
        public Window window;
        public Border onBtnBorder;
        public Border offBtnBorder;
        public Border loadedBtnBorder;
        public SolidColorBrush selectedOFFBackgroundBrush;
        public SolidColorBrush selectedONAndLoadedBackgroundBrush;
        public SolidColorBrush unselectedBackgroundBrush;
        private Text tempText;
        
        private GT.Timer timeUntilCoffeeReadyTimer;
        private int timeUntilCoffeeReady;
        private Text timerText;
        private int COFFEE_MAKE_TIME = 300;
        

        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            SetupWindow();

            timeUntilCoffeeReady = 10;

            gyro.Calibrate();
            gyro.StartContinuousMeasurements();

            button.ButtonPressed += new Button.ButtonEventHandler((Button sender, Button.ButtonState state) =>
            {
                /*
                if (gyroIsmesasuring)
                {
                    gyro.StopContinuousMeasurements();
                }
                else
                {
                    gyro.StartContinuousMeasurements();
                }
                gyroIsmesasuring = !gyroIsmesasuring;
                getLoadedState();
                */
                coffeeMachineOn();
            });

            coffeeRelay.TurnOn();

            // init wifi and register callback
            WiFiRS9110 wifi = wifi_RS21.Interface;
            wifi.WirelessConnectivityChanged += new WiFiRS9110.WirelessConnectivityChangedEventHandler((s, e) =>
            {
                orchestratorJSClient.connect(host, port);
            });
            OrchestratorJSClient.GeneralHelpers.initWiFi(wifi, ssid, passwd);

            // create new instance of orchestratorJS
            orchestratorJSClient = new OrchestratorJSClient.OrchestratorJSClient(deviceIdentity);

            // register handler for listening method calls from orchestrator
            orchestratorJSClient.MethodCallReceived += new OrchestratorJSClient.OrchestratorJSClient.OnMethodCallRecievedHandler((e) =>
            {

                Program.BeginInvoke(new DispatcherOperationCallback(delegate
                {
  
                

                if ( e.capabilityName == "CoffeeCapability" && e.methodCallName == "makeCoffee" )
                {
                    orchestratorJSClient.sendResponse( coffeeMachineOn() );
                    return null;
                }
                else if (e.capabilityName == "CoffeeCapability" && e.methodCallName == "turnOff")
                {
                    // turn the machine of (also called after n. minutes)
                    orchestratorJSClient.sendResponse(coffeeMachineOff());
                    return null;
                }
                else if ( e.capabilityName == "CoffeeCapability" && e.methodCallName == "isLoaded" )
                {
                    orchestratorJSClient.sendResponse( ( coffeeMachineState == CoffeeMachineState.LOADED ) );
                    return null;
                }
                else if ( e.capabilityName == "CoffeeCapability" && e.methodCallName == "isOn" )
                {
                    orchestratorJSClient.sendResponse( ( coffeeMachineState == CoffeeMachineState.ON ) );
                    return null;

                }
                else if (e.capabilityName == "CoffeeCapability" && e.methodCallName == "isCoffeeReady")
                {
                    // check if timer is 0 -> ready
                    if (timeUntilCoffeeReady <= 0)
                    {
                        Debug.Print("Coffee was READY!");
                        orchestratorJSClient.sendResponse(true);
                        return null;
                    }
                    else 
                    {
                        Debug.Print("Coffee is NOT READY yet!");
                        orchestratorJSClient.sendResponse(false);
                        return null;
                    }

                }
                else
                {
                    Debug.Print("unknown capability and/or method");
                }
                orchestratorJSClient.sendResponse();

                        return null;
                }), "");


            });
        }



        Boolean coffeeMachineOn()
        {
            Debug.Print("Setting coffeeMachine ON");
            // set state of the device
            coffeeMachineState = CoffeeMachineState.ON;

            // This is needed as the the command may come from orchestrator.js, end hence is run in different thread!
            //Program.BeginInvoke(new DispatcherOperationCallback(delegate
            //{
                onBtnBorder.Background = selectedONAndLoadedBackgroundBrush;
                offBtnBorder.Background = unselectedBackgroundBrush;
                loadedBtnBorder.Background = unselectedBackgroundBrush;

                timeUntilCoffeeReady = COFFEE_MAKE_TIME;
                timeUntilCoffeeReadyTimer = new GT.Timer(1000);
                timeUntilCoffeeReadyTimer.Tick += new GT.Timer.TickEventHandler((timer) =>
                {
                    if (timeUntilCoffeeReady == 0)
                        timerText.TextContent = "coffee ready";
                    else
                        timerText.TextContent = "" + timeUntilCoffeeReady;
                    timeUntilCoffeeReady -= 1;
                    if (timeUntilCoffeeReady < 0) { timeUntilCoffeeReadyTimer.Stop(); }
                });
                timeUntilCoffeeReadyTimer.Start();

                // the actual functionality
                coffeeRelay.TurnOff();

            //    return null;
            //}), "");

            return true;
        }


        Boolean coffeeMachineOff()
        {
            Debug.Print("coffeeMeachine OFF");

            // set state of the device
            coffeeMachineState = CoffeeMachineState.OFF;

            // This is needed as the the command may come from orchestrator.js, end hence is run in different thread!
            //Program.BeginInvoke(new DispatcherOperationCallback(delegate
            //{
                offBtnBorder.Background = selectedOFFBackgroundBrush;
                onBtnBorder.Background = unselectedBackgroundBrush;
                loadedBtnBorder.Background = unselectedBackgroundBrush;

                timeUntilCoffeeReadyTimer.Stop();
                timerText.TextContent = "off";

                // the actual functionality
                coffeeRelay.TurnOn();

            //    return null;
            //}), "");


            return false;
        }



        /*
         * This mehtod CANNOT be called from orchestrator.js
         */
        void coffeeMachineLoaded()
        {
            Debug.Print("coffeeMeachine LOADED");
            // set state of the device
            coffeeMachineState = CoffeeMachineState.LOADED;
            offBtnBorder.Background = unselectedBackgroundBrush;
            onBtnBorder.Background = unselectedBackgroundBrush;
            loadedBtnBorder.Background = selectedONAndLoadedBackgroundBrush;

            timeUntilCoffeeReadyTimer.Stop();
            timerText.TextContent = "loaded";
            

            // the actual functionality
            coffeeRelay.TurnOn();
        }




        void getLoadedState()
        {

            gyro.MeasurementComplete += gyro_MeasurementComplete;
        }

        void gyro_MeasurementComplete(Gyro sender, Gyro.SensorData sensorData)
        {

            Debug.Print("gyro state, x: " + sensorData.X + ", y: " + sensorData.Y + ", z: " + sensorData.Z);
            if (System.Math.Abs(sensorData.Y) > 20)
            {
                
                // ask user if the machine has just been loaded



            }
        }


        public void SetupWindow()
        {
            int BtnWidth = 90;
            int BtnHeight = 50;

            GT.Color selectedFronColor = GT.Color.White;

            Font font = Resources.GetFont(Resources.FontResources.NinaB);
            unselectedBackgroundBrush = new SolidColorBrush(GT.Color.FromRGB(89, 192, 255));
            selectedONAndLoadedBackgroundBrush = new SolidColorBrush(GT.Color.FromRGB(43, 255, 121));
            selectedOFFBackgroundBrush = new SolidColorBrush(GT.Color.FromRGB(240, 28, 126));
            window = display.WPFWindow;
            canvas = new Canvas();
            window.Child = canvas;
  
            StackPanel stack = new StackPanel(Orientation.Horizontal);

            // ON button
            onBtnBorder = new Border();
            onBtnBorder.SetBorderThickness(0);
            onBtnBorder.Width = BtnWidth;
            onBtnBorder.Height = BtnHeight;
            onBtnBorder.Background = unselectedBackgroundBrush;

            onBtnBorder.SetMargin(12, 10, 0, 0);
            onBtnBorder.TouchDown += new Microsoft.SPOT.Input.TouchEventHandler((s, e) =>
            {
                coffeeMachineOn();
            });

            tempText = new Text(font, "ON");
            tempText.Width = BtnWidth;
            tempText.ForeColor = selectedFronColor;
            tempText.TextAlignment = TextAlignment.Center;
            tempText.SetMargin(0, 15, 0, 0);
       
            onBtnBorder.Child = tempText;
            stack.Children.Add(onBtnBorder);


            // OFF button
            offBtnBorder = new Border();
            offBtnBorder.SetBorderThickness(0);
            offBtnBorder.Width = BtnWidth;
            offBtnBorder.Height = BtnHeight;

            offBtnBorder.Background = selectedOFFBackgroundBrush;

            offBtnBorder.SetMargin(12, 10, 0, 0);
            offBtnBorder.TouchDown += new Microsoft.SPOT.Input.TouchEventHandler((s, e) =>
            {
                coffeeMachineOff();
            });

            tempText = new Text(font, "OFF");
            tempText.Width = BtnWidth;
            tempText.ForeColor = selectedFronColor;
            tempText.TextAlignment = TextAlignment.Center;
            tempText.SetMargin(0, 15, 0, 0);
            
            offBtnBorder.Child = tempText;
            stack.Children.Add(offBtnBorder);


            // LOADED button
            loadedBtnBorder = new Border();
            loadedBtnBorder.SetBorderThickness(0);
            loadedBtnBorder.Width = BtnWidth;
            loadedBtnBorder.Height = BtnHeight;

            loadedBtnBorder.Background = unselectedBackgroundBrush;

            loadedBtnBorder.SetMargin(12, 10, 0, 0);
            loadedBtnBorder.TouchDown += new Microsoft.SPOT.Input.TouchEventHandler((s, e) =>
            {
                coffeeMachineLoaded();
            });

            tempText = new Text(font, "LOADED");
            tempText.Width = BtnWidth;
            tempText.ForeColor = selectedFronColor;
            tempText.TextAlignment = TextAlignment.Center;
            tempText.SetMargin(0, 15, 0, 0);
      
            loadedBtnBorder.Child = tempText;
            stack.Children.Add(loadedBtnBorder);

            StackPanel verticalStack = new StackPanel(Orientation.Vertical);
            verticalStack.Children.Add(stack);




            Font ff = Resources.GetFont(Resources.FontResources.moireFonts);

            timerText = new Text(ff, "off");
            timerText.Width = 320;
            timerText.ForeColor = GT.Color.FromRGB(89, 192, 255);
            timerText.TextAlignment = TextAlignment.Center;
            timerText.SetMargin(0, 35, 0, 0);
            verticalStack.Children.Add(timerText);

            canvas.Children.Add(verticalStack);

        }



    }
}

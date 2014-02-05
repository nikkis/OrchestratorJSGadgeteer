using System;
using Microsoft.SPOT;
using SocketIO.NetMF;
using System.Collections;


namespace OrchestratorJSClient
{
    public class OrchestratorJSClient : SocketIOClient
    {

        private string clientIdentity_;

        //private GTM.GHIElectronics.MulticolorLed multicolorLed_;

        public OrchestratorJSClient(string clientIdentity)
        {
            // TODO: Complete member initialization
            this.clientIdentity_ = clientIdentity;
            //this.multicolorLed_ = multicolorLed;
        }


        override public void onConnect()
        {
            Debug.Print("SocketIO connected");
            emit("login", new ArrayList() { this.clientIdentity_ });
        }

        // handle your own specified event types here
        override public void onEvent(string name, ArrayList arguments)
        {
            try
            {
                Debug.Print("got event: " + name);
                ArrayList args = (ArrayList)arguments[0];

                currentActionId = (String)args[0];
                currentMethodcallId = (String)args[1];

                String capabilityName = (String)args[2];
                String methodCallName = (String)args[3];

                ArrayList methodCallArgs = new ArrayList();
                if (args.Count > 4 && args[4].GetType() == typeof(ArrayList))
                {
                    methodCallArgs = (ArrayList)args[4];
                    foreach (Object item in methodCallArgs)
                    {
                        if (item != null)
                        {
                            Debug.Print("Arg value:");
                            Debug.Print(item.ToString());
                        }
                    }
                }

                OrchestratorJSEventArgs ea = new OrchestratorJSEventArgs();
                ea.capabilityName = capabilityName;
                ea.methodCallName = methodCallName;
                ea.methodCallArgs = methodCallArgs;

                this.MethodCallReceived(ea);
            }
            catch (Exception e)
            {
                sendException(e.ToString());
            }

        }

        public void sendException(string reason)
        {
            ArrayList args = new ArrayList();
            args.Add(currentActionId);
            args.Add(currentMethodcallId);
            args.Add(clientIdentity_);
            args.Add(reason);
            emit("ojs_exception", args);
        }

        public void sendResponse()
        {
            sendResponse(null);
        }

        public void sendResponse(Object methodReturnValue)
        {
            try
            {
                ArrayList responseArguments = new ArrayList();
                responseArguments.Add(currentActionId);
                responseArguments.Add(currentMethodcallId);

                if (methodReturnValue != null)
                {
                    if (methodReturnValue.GetType() == typeof(Hashtable))
                    {
                        responseArguments.Add(methodReturnValue);
                        //responseArguments.Add((JSONObject) methodReturnValue);
                        responseArguments.Add("JSON");
                    }
                    else if (methodReturnValue.GetType() == typeof(Boolean))
                    {
                        responseArguments.Add((Boolean)methodReturnValue);
                        responseArguments.Add("BOOL");

                    }
                    else if (methodReturnValue.GetType() == typeof(String))
                    {
                        responseArguments.Add((String)methodReturnValue);
                        responseArguments.Add("STRING");
                    }
                    else if (methodReturnValue.GetType() == typeof(int))
                    {
                        responseArguments.Add((Int32)methodReturnValue);
                        responseArguments.Add("INT");
                    }
                    else if (methodReturnValue.GetType() == typeof(float))
                    {
                        responseArguments.Add(methodReturnValue);
                        responseArguments.Add("FLOAT");
                    }
                    else if (methodReturnValue.GetType() == typeof(Double))
                    {
                        responseArguments.Add((Double)methodReturnValue);
                        responseArguments.Add("DOUBLE");
                    }
                }
                emit("methodcallresponse", responseArguments);

            }
            catch (Exception e)
            {
                //e.printStackTrace();
                Debug.Print(e.ToString());
                sendException(e.ToString());

            }
        }


        // other reserved messages and event types
        override public void onDisconnect() { Debug.Print("disconnected"); }
        override public void onHeartbeat() { Debug.Print("got heartbeat"); }
        override public void onMessage(string message) { Debug.Print("got message: " + message); }
        override public void onJsonMessage(Hashtable jsonObject) { Debug.Print("got json obj"); }

        // Handle error cases
        override public void onError(string reason) { throw new Exception(reason); }



        public delegate void OnMethodCallRecievedHandler(OrchestratorJSEventArgs e);
        public event OnMethodCallRecievedHandler MethodCallReceived;


        private string currentActionId;
        private string currentMethodcallId;
        //private Object methodReturnValue;





    }

    public class OrchestratorJSEventArgs : EventArgs
    {
        public ArrayList methodCallArgs { get; set; }
        public String methodCallName { get; set; }
        public String capabilityName { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace Octobroker
{
    /// <summary>
    /// Tracks the hardware state and provides Commands
    /// </summary>
    public class OctoprintPrinterTracker:OctoprintTracker
    {
        /// <summary>
        /// The current State.
        /// </summary>
        private OctoprintFullPrinterState currentstate;

        /// <summary>
        /// The last time it was updated.
        /// </summary>
        private DateTime lastupdated;

        /// <summary>
        /// The milisecs untill an update might be necessary.
        /// </summary>
        public int BestBeforeMilisecs;

        /// <summary>
        /// Initializes a Printertracker, this shouldn't be done directly and is part of the Connection it needs anyway
        /// </summary>
        /// <param name="con">The Octoprint connection it connects to.</param>
        public OctoprintPrinterTracker(OctoprintConnection con):base(con)
        {
        }

        /// <summary>
        /// Action for Eventhandling the Websocket Printerstate info
        /// </summary>
        public event Action<OctoprintPrinterState> PrinterstateHandlers;
        public bool StateListens()
        {
            return PrinterstateHandlers != null;
        }
        public void CallPrinterState(OctoprintPrinterState ps)
        {
            PrinterstateHandlers.Invoke(ps);
        }

        /// <summary>
        /// Action for Eventhandling the Websocket Temperature offset info
        /// </summary>
        public event Action<List<int>> OffsetHandlers;
        public bool OffsetListens()
        {
            return OffsetHandlers != null;
        }
        public void CallOffset(List<int> LI)
        {
            OffsetHandlers.Invoke(LI);
        }


        /// <summary>
        /// Action for Eventhandling the Websocket Temperature info
        /// </summary>
        public event Action<List<OctoprintHistoricTemperatureState>> TempHandlers;
        public bool TempsListens()
        {
            return TempHandlers!=null;
        }
        public void CallTemp(List<OctoprintHistoricTemperatureState> LHT)
        {
            TempHandlers.Invoke(LHT);
        }

        /// <summary>
        /// Action for Eventhandling the Websocket CurrentZ info
        /// </summary>
        public event Action<float> CurrentZHandlers;
        public bool ZListens()
        {
            return CurrentZHandlers != null;
        }
        public void CallCurrentZ( float z )
        {
            CurrentZHandlers.Invoke(z);
        }


        /// <summary>
        /// Gets the full state of the printer if the BestBeforeMilisecs haven't passed.
        /// </summary>
        /// <returns>The full printer state.</returns>
        public OctoprintFullPrinterState GetFullPrinterState()
        {
            TimeSpan passed = DateTime.Now.Subtract(lastupdated);
            if (passed.Milliseconds > BestBeforeMilisecs)
            {
                return currentstate;
            }
            string jobInfo = "";
            try {
                jobInfo = Connection.Get("api/printer");
            } catch (WebException e)
            {
                if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.Conflict) { 
                    OctoprintFullPrinterState returnval = new OctoprintFullPrinterState
                        {
                            PrinterState = new OctoprintPrinterState()
                        };
                    returnval.PrinterState.Text = "Error 409 is the Printer Connected at all?\n";
                    return returnval;
                }
            }
            JObject data=new JObject();
            data = JsonConvert.DeserializeObject<JObject>(jobInfo);
            OctoprintFullPrinterState result = new OctoprintFullPrinterState(data);
            currentstate = result;
            lastupdated = DateTime.Now;
            return result;
        }

        /// <summary>
        /// Gets the state of the printer current information only.
        /// </summary>
        /// <returns>The printer state.</returns>
        public OctoprintPrinterState GetPrinterState()
        {
            string jobInfo="";
            try
            {
                jobInfo = Connection.Get("api/printer?exclude=temperature,sd");
            }
            catch (WebException e)
            {
                if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.Conflict)
                {
                    OctoprintPrinterState returnval = new OctoprintPrinterState
                    {
                        Text = "Error 409 is the Printer Connected at all?\n"
                    };
                    return returnval;
                }
            }
            JObject data = new JObject();
            data = JsonConvert.DeserializeObject<JObject>(jobInfo);
            JToken statedata = data.Value<JToken>("state");
            OctoprintPrinterState result = new OctoprintPrinterState(statedata);
            if (currentstate != null)
                currentstate.PrinterState = result;
            return result;

        }

        /// <summary>
        /// Makes the printhead jog.
        /// </summary>
        /// <returns>The Http Result</returns>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="z">The z coordinate.</param>
        /// <param name="absolute">If set to <c>true</c> absolute.</param>
        /// <param name="speed">Speed.</param>
        public string MakePrintheadJog(float? x, float? y, float? z, bool absolute, int? speed)
        {

            string returnValue = string.Empty;
            JObject data = new JObject
            {
                { "command", "jog" },
                { "absolute", absolute}
            };
            if (x.HasValue)
            {
                data.Add("x", x);
            }
            if (y.HasValue)
            {
                data.Add("y", y);
            }
            if (y.HasValue)
            {
                data.Add("z", z);
            }
            if (speed.HasValue)
            {
                data.Add("speed", speed);
            }
            if (absolute == true)
            {
                Connection.Position.SetPos(x, y, z);
            }
            else Connection.Position.Move(x, y, z);
            try
            {
                returnValue = Connection.PostJson("api/printer/printhead", data);
            }
            catch (WebException e)
            {
                switch (((HttpWebResponse)e.Response).StatusCode)
                {
                    case HttpStatusCode.Conflict:
                        return "409 The Printer is not operational";
                    default:
                        return "unknown webexception occured";
                }

            }
            return returnValue;
        }

        /// <summary>
        /// Homes the Printhead to the given axes
        /// </summary>
        /// <returns>The Http Result</returns>
        /// <param name="axes">Axes.</param>
        public string MakePrintheadHome(string[] axes)
        {
            float? x=null, y=null, z=null;
            JArray jaxes = new JArray();
            foreach (string axis in axes)
            {
                jaxes.Add(axis);
                if (axis == "x")
                    x = 0;
                if (axis == "y")
                    y = 0;
                if (axis == "z")
                    z = 0;
            }
            string returnValue = string.Empty;
            JObject data = new JObject
            {
                { "command", "home" },
                { "axes", jaxes}
            };
            try { 
                returnValue =Connection.PostJson("api/printer/printhead", data);
            }
            catch (WebException e)
            {
                switch (((HttpWebResponse)e.Response).StatusCode)
                {
                    case HttpStatusCode.Conflict:
                        return "409 The Printer is not operational";
                    case HttpStatusCode.BadRequest:
                        return "wrong axis defined, choose only x and/or y and/or z";
                    default:
                        return "unknown webexception occured";
                }

            }
            Connection.Position.SetPos(x, y, z);
            return returnValue;
        }

        /// <summary>
        /// Sets the feedrate.
        /// </summary>
        /// <returns>The Http Result</returns>
        /// <param name="feed">Feedrate.</param>
        public string SetFeedrate(int feed)
        {
            JObject data = new JObject
            {
                {"command", "feedrate"},
                {"factor", feed}
            };
            try
            {
                return Connection.PostJson("api/printer/printhead", data);
            }
            catch (WebException e)
            {
                switch (((HttpWebResponse)e.Response).StatusCode)
                {
                    case HttpStatusCode.Conflict:
                        return "409 The Printer is not operational";
                    default:
                        return "unknown webexception occured";
                }

            }
        }

        /// <summary>
        /// Sets the feedrate.
        /// </summary>
        /// <returns>The Http Result</returns>
        /// <param name="feed">Feedrate.</param>
        public string SetFeedrate(float feed)
        {
            JObject data = new JObject
            {
                {"command", "feedrate"},
                {"factor", feed}
            };
            try
            {
                return Connection.PostJson("api/printer/printhead", data);
            }
            catch (WebException e)
            {
                switch (((HttpWebResponse)e.Response).StatusCode)
                {
                    case HttpStatusCode.Conflict:
                        return "409 The Printer is not operational";
                    default:
                        return "unknown webexception occured";
                }

            }
        }

        /// <summary>
        /// Sets the temperature target.
        /// </summary>
        /// <returns>The Http Result</returns>
        /// <param name="targets">Target temperatures of the different tool heads.</param>
        public string SetTemperatureTarget(Dictionary<string, int> targets)
        {
            string returnValue = string.Empty;
            JObject data = new JObject
            {
                { "command", "target" },
                { "targets", JObject.FromObject(targets)}
            };

            try
            {
                return Connection.PostJson("api/printer/tool", data);
            }
            catch (WebException e)
            {
                switch (((HttpWebResponse)e.Response).StatusCode)
                {
                    case HttpStatusCode.Conflict:
                        return "409 The Printer is propably not operational";
                    case HttpStatusCode.BadRequest:
                        return "400 The values given seem to not be acceptable, is the tool named like tool{n}?";
                    default:
                        return "unknown webexception occured";
                }

            }
        }

        /// <summary>
        /// Sets the temperature target of tool0.
        /// </summary>
        /// <returns>The Http Result</returns>
        /// <param name="temp">Temperature to set the target to.</param>
        public string SetTemperatureTarget(int temp)
        {
            return SetTemperatureTarget(new Dictionary<string, int>(){ {"tool0",temp} });
        }

        /// <summary>
        /// Sets the temperature offset. the offset is only used in GCode
        /// </summary>
        /// <returns>The Http Result</returns>
        /// <param name="offsets">Offsets for the different tool heads.</param>
        public string SetTemperatureOffset(Dictionary<string, int> offsets)
        {
            string returnValue = string.Empty;
            JObject data = new JObject
            {
                { "command", "offset" },
                { "offsets", JObject.FromObject(offsets)}
            };
            try
            {
                return Connection.PostJson("api/printer/tool", data);
            }
            catch (WebException e)
            {
                switch (((HttpWebResponse)e.Response).StatusCode)
                {
                    case HttpStatusCode.Conflict:
                        return "409 The Printer is propably not operational";
                    case HttpStatusCode.BadRequest:
                        return "400 The values given seem to not be acceptable, is the tool named like tool{n}?";
                    default:
                        return "unknown webexception occured";
                }

            }
        }

        /// <summary>
        /// Sets the temperature offset. the offset is only used in GCode
        /// </summary>
        /// <returns>The Http Result</returns>
        /// <param name="temp">Temperature offset of tool0.</param>
        public string SetTemperatureOffset(int temp)
        {
            return SetTemperatureOffset(new Dictionary<string, int>() { { "tool0", temp } });
        }

        /// <summary>
        /// Selects the tool for configuring.
        /// </summary>
        /// <returns>The Http Result</returns>
        /// <param name="tool">Tool.</param>
        public string SelectTool(string tool)
        {
            JObject data = new JObject
            {
                { "command", "select" },
                { "tool", tool}
            };
            try
            {
                return Connection.PostJson("api/printer/tool", data);
            }
            catch (WebException e)
            {
                switch (((HttpWebResponse)e.Response).StatusCode)
                {
                    case HttpStatusCode.Conflict:
                        return "409 The Printer is propably not operational or currently printing";
                    case HttpStatusCode.BadRequest:
                        return "400 The values given seem to not be acceptable, is the tool named like tool{n}?";
                    default:
                        return "unknown webexception occured";
                }

            }
        }

        /// <summary>
        /// Extrudes the selected tool.
        /// </summary>
        /// <returns>The Http Result</returns>
        /// <param name="mm">the amount to extrude in mm</param>
        public string ExtrudeSelectedTool(int mm)
        {
            JObject data = new JObject
            {
                { "command", "extrude" },
                { "amount", mm}
            };
            try
            {
                return Connection.PostJson("api/printer/tool", data);
            }
            catch (WebException e)
            {
                switch (((HttpWebResponse)e.Response).StatusCode)
                {
                    case HttpStatusCode.Conflict:
                        return "409 The Printer is propably not operational or currently printing";
                    case HttpStatusCode.BadRequest:
                        return "400 The values given seem to not be acceptable";
                    default:
                        return "unknown webexception occured";
                }

            }
        }


        /// <summary>
        /// Sets the flowrate of the selected tool.
        /// </summary>
        /// <returns>The Http Result</returns>
        /// <param name="flow">Flowrate percents.</param>
        public string SetFlowrateSelectedTool(int flow)
        {
            JObject data = new JObject
            {
                {"command", "flowrate"},
                {"factor", flow}
            };
            try
            {
                return Connection.PostJson("api/printer/tool", data);
            }
            catch (WebException e)
            {
                switch (((HttpWebResponse)e.Response).StatusCode)
                {
                    case HttpStatusCode.Conflict:
                        return "409 The Printer is propably not operational";
                    case HttpStatusCode.BadRequest:
                        return "400 The values given seem to not be acceptable";
                    default:
                        return "unknown webexception occured";
                }

            }
        }

        /// <summary>
        /// Sets the flowrate of the selected tool.
        /// </summary>
        /// <returns>The Http Result</returns>
        /// <param name="flow">Flow ratio</param>
        public string SetFlowrateSelectedTool(float flow)
        {
            JObject data = new JObject
            {
                {"command", "flowrate"},
                {"factor", flow}
            };
            try
            {
                return Connection.PostJson("api/printer/tool", data);
            }
            catch (WebException e)
            {
                switch (((HttpWebResponse)e.Response).StatusCode)
                {
                    case HttpStatusCode.Conflict:
                        return "409 The Printer is propably not operational";
                    case HttpStatusCode.BadRequest:
                        return "400 The values given seem to not be acceptable";
                    default:
                        return "unknown webexception occured";
                }

            }
        }

        /// <summary>
        /// Sets the temperature target of the Bed.
        /// </summary>
        /// <returns>The Http Result</returns>
        /// <param name="temperature">Temperature.</param>
        public string SetTemperatureTargetBed(int temperature)
        {
            JObject data = new JObject
            {
                { "command", "target" },
                { "target", temperature}
            };
            try
            {
                return Connection.PostJson("api/printer/bed", data);
            }
            catch (WebException e)
            {
                switch (((HttpWebResponse)e.Response).StatusCode)
                {
                    case HttpStatusCode.Conflict:
                        return "409 The Printer is propably not operational or has no heated bed";
                    case HttpStatusCode.BadRequest:
                        return "400 The values given seem to not be acceptable";
                    default:
                        return "unknown webexception occured";
                }

            }
        }

        /// <summary>
        /// Sets the temperature offset of the Bed.
        /// </summary>
        /// <returns>The Http Result</returns>
        /// <param name="offset">Offset of the Temperature for GCode.</param>
        public string SetTemperatureOffsetBed(int offset)
        {
            JObject data = new JObject
            {
                { "command", "offset" },
                { "offset", offset}
            };
            try
            {
                return Connection.PostJson("api/printer/bed", data);
            }
            catch (WebException e)
            {
                switch (((HttpWebResponse)e.Response).StatusCode)
                {
                    case HttpStatusCode.Conflict:
                        return "409 The Printer is propably not operational or has no heated bed";
                    case HttpStatusCode.BadRequest:
                        return "400 The values given seem to not be acceptable";
                    default:
                        return "unknown webexception occured";
                }

            }
        }
    }
}

using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Octobroker
{
    public class OctoprintFullPrinterState
    {
        public OctoprintFullPrinterState()
        {

        }
        public OctoprintFullPrinterState(JObject data)
        {

            JToken temperaturedata = data.Value<JToken>("temperature");
            JToken bedtemperature = temperaturedata.Value<JToken>("bed");
            JToken statedata = data.Value<JToken>("state");
            TempState = new OctoprintTemperatureState()
            {
                Bed = new OctoprintTemperature(bedtemperature)
            };
            SDState = data.Value<JToken>("sd").Value<bool?>("ready") ?? false;
            PrinterState = new OctoprintPrinterState(statedata);
            TempState.Tools = new List<OctoprintTemperature>();
            for (int i = 0; i < 256; i++)
            {
                JToken tooltemp = temperaturedata.Value<JToken>("tool"+i);
                if (tooltemp != null)
                {
                    TempState.Tools.Add(new OctoprintTemperature(tooltemp));
                }
                else
                {
                    break;
                }
            }
            if (temperaturedata != null && temperaturedata.Value<JToken>("history")!=null)
            {
                TempState.History = new List<OctoprintHistoricTemperatureState>();
                foreach (JObject historydata in temperaturedata["history"])
                {

                    OctoprintHistoricTemperatureState historicTempState = new OctoprintHistoricTemperatureState(historydata);
                    TempState.History.Add(historicTempState);
                }
            }
        }

        public OctoprintTemperatureState TempState { get; set; }
        public bool SDState { get; set; }
        public OctoprintPrinterState PrinterState { get; set; }
        public override string ToString()
        {
            string returnval ="";
            if (PrinterState != null)
                returnval += PrinterState.ToString() + "SDState: " + SDState + "\n";
            if(TempState!=null)
                returnval+= TempState.ToString();
            return returnval;
        }
    }
}
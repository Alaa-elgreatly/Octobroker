using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Octobroker
{
    public class OctoprintHistoricTemperatureState
    {
        public OctoprintHistoricTemperatureState(JToken jToken)
        {
            Time = jToken.Value<int?>("time") ?? -1;
            Bed = new OctoprintTemperature(jToken.Value<JToken>("bed"));
            Tools = new List<OctoprintTemperature>();
            for (int i = 0; i < 256; i++)
            {
                JToken tooltemp = jToken.Value<JToken>("tool" + i);
                if (tooltemp != null)
                {
                    Tools.Add(new OctoprintTemperature(tooltemp));
                }
                else
                {
                    break;
                }
            }
        }
        public int Time { get; set; }
        public List<OctoprintTemperature> Tools { get; set; }
        public OctoprintTemperature Bed { get; set; }

        public override string ToString()
        {
            string returnval = "At " + Time+"\n";
            if(Bed!=null)
                returnval += "The Bed temperature: \n" + Bed.ToString();
            if(Tools!=null)
                foreach(OctoprintTemperature Tool in Tools)
                    returnval += "The Printhead tool: \n" + Tool.ToString();
            return returnval;
        }
    }
}
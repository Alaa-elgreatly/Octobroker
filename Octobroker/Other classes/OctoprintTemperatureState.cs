using System.Collections.Generic;

namespace Octobroker
{
    public class OctoprintTemperatureState
    {
        public List<OctoprintTemperature> Tools { get; set; }
        public OctoprintTemperature Bed { get; set; }
        public List<OctoprintHistoricTemperatureState> History { get; set; }
        public override string ToString()
        {
            string returnval = "Currently:\n";
            if(Bed!=null)
                returnval += "The Bed Temperature:\n" + Bed.ToString();
            if (Tools != null)
                foreach (OctoprintTemperature Tool in Tools)
                    returnval += "The Printhead tool:\n" + Tool.ToString();

            if (History != null) {
                returnval += "Past Temperature:\n";
                foreach(OctoprintHistoricTemperatureState State in History)
                    returnval += State.ToString();
            }
            return returnval;
        }
    }
}
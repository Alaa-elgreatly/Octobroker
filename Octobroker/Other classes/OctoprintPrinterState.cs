using System;
using Newtonsoft.Json.Linq;

namespace Octobroker
{
    public class OctoprintPrinterState
    {
        public OctoprintPrinterState()
        {

        }
        public OctoprintPrinterState(JToken statedata)
        {

            JToken stateflags = statedata.Value<JToken>("flags");
            Text = statedata.Value<String>("text");
            Flags = new OctoprintPrinterFlags(stateflags);
        }

        public string Text { get; set; }
        public OctoprintPrinterFlags Flags { get; set; }
        public override string ToString()
        {
            string returnval = "State: " + Text + "\n";
            if(Flags!=null)
                returnval += Flags.ToString();
            return returnval;
        }
    }
}
using Newtonsoft.Json.Linq;

namespace Octobroker
{
    public class OctoprintPrinterFlags
    {
        public OctoprintPrinterFlags(JToken stateflags)
        {
            Operational = stateflags.Value<bool?>("operational") ?? false;
            Paused = stateflags.Value<bool?>("paused") ?? false;
            Printing = stateflags.Value<bool?>("printing") ?? false;
            Cancelling = stateflags.Value<bool?>("canceling") ?? false;
            SDReady = stateflags.Value<bool?>("sdReady") ?? false;
            Error = stateflags.Value<bool?>("error") ?? false;
            Ready = stateflags.Value<bool?>("ready") ?? false;
            ClosedOrError = stateflags.Value<bool?>("closedOrError") ?? false;
        }

        public bool Operational { get; set; }
        public bool Paused { get; set; }
        public bool Printing { get; set; }
        public bool Cancelling { get; set; }
        public bool Pausing { get; set; }
        public bool SDReady { get; set; }
        public bool Error { get; set; }
        public bool Ready { get; set; }
        public bool ClosedOrError { get; set; }

        public override string ToString()
        {
            string returnval="";
            returnval+="Is operational:\t" + Operational +"\n";
            returnval += "Is paused:  " + Paused + "\n";
            returnval += "Is printing:    " + Printing + "\n";
            returnval += "Is Cancelling:  " + Cancelling + "\n";
            returnval += "Is Pausing: " + Pausing + "\n";
            returnval += "Is SD Card ready:   " + SDReady + "\n";
            returnval += "Has Error:  " + Error + "\n";
            returnval += "Is Ready:   " + Ready + "\n";
            returnval += "Is ClosedOrReady:   "+ClosedOrError+"\n";
            return returnval;
        }
    }
}
using Newtonsoft.Json.Linq;

namespace Octobroker
{
    public class OctoprintJobInfo
    {

        public OctoprintJobInfo(JToken job)
        {
            EstimatedPrintTime = job.Value<int?>("estimatedPrintTime") ?? -1;
            JToken filament = job.Value<JToken>("filament");
            if (filament.HasValues)
                Filament = new OctoprintFilamentInfo
                {
                    Lenght = filament.Value<int?>("length") ?? -1,
                    Volume = filament.Value<int?>("volume") ?? -1
                };
            JToken file = job.Value<JToken>("file");
            File = new OctoprintFile((JObject)file);
        }

        public OctoprintFile File { get; set; }
        public int EstimatedPrintTime { get; set; }
        public OctoprintFilamentInfo Filament { get; set; }
        public override string ToString()
        {
            return "EstimatedPrinttime: " + EstimatedPrintTime + "\nAt File: " + File + "\nUsing Fillament: \n" + Filament;
        }
    }
}
using Newtonsoft.Json.Linq;

namespace Octobroker
{
    public class OctoprintTemperature
    {
        public OctoprintTemperature(JToken temperature)
        {

            Actual = temperature.Value<double?>("actual") ?? -1.0;
            Target = temperature.Value<double?>("target") ?? -1.0;
            Offset = temperature.Value<double?>("offset") ?? -1.0;
        }

        public double Actual { get; set; }
        public double Target { get; set; }
        public double Offset { get; set; }

        public override string ToString()
        {
            return "Actual Temperature: "+Actual+"°C\nTarget Temperature: "+Target+"°C\nOffset: "+Offset+"°C\n";
        }
    }
}
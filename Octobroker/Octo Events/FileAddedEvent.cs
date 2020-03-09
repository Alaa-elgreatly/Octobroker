using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Octobroker.Octo_Events
{
    public class FileAddedEvent:OctoprintEvent
    {
        public FileAddedEvent(string name, JToken payload) : base(name, payload)
        {
            ParsePayload();
        }

        protected override void ParsePayload()
        {
            FileName = Payload.Value<String>("name") ?? "";
            Path = Payload.Value<String>("path") ?? "";
            var typeArray = Payload.Value<JArray>("type");
            if (typeArray.Contains("gcode"))
                Type = "gcode";
        }

        public string FileName { get; private set; }
        public string Path { get; private set; }
        public string Type { get; private set; }

        public OctoprintFile ConvertToOctoprintFile()
        {
            return new OctoprintFile() {Name = FileName, Path = this.Path, Type = this.Type};
        }
    }
}

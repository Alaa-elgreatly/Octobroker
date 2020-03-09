using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Octobroker.Octo_Events
{
    public abstract class OctoprintEvent
    {
        public OctoprintEvent(String name, JToken payload)
        {
            this.Name = name;
            this.Payload = payload;
        }

        public string Name { get; }
        protected JToken Payload { get;  set; }

        public JToken GetGenericPayload()
        {
            return this.Payload;
        }

        protected abstract void ParsePayload();
    }
}

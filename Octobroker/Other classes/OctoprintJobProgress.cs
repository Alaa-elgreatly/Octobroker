using System;
using Newtonsoft.Json.Linq;

namespace Octobroker
{
    public class OctoprintJobProgress
    {
        public OctoprintJobProgress(JToken progress)
        {
            Completion = progress.Value<double?>("completion") ?? -1.0;
            Filepos = progress.Value<int?>("filepos") ?? -1;
            PrintTime = progress.Value<int?>("printTime") ?? -1;
            PrintTimeLeft = progress.Value<int?>("printTimeLeft")??-1;
        }

        public Double Completion { get; set; }
        public int Filepos { get; set; }
        public int PrintTime { get; set; }
        public int PrintTimeLeft { get; set; }
        public override string ToString()
        {
            if (Filepos != -1)
                return "Completion: " + Completion + "\nFilepos: " + Filepos + "\nPrintTime: " + PrintTime + "\nPrintTimeLeft: " + PrintTimeLeft + "\n";
            else
                return "No Job found running";
        }
    }
}
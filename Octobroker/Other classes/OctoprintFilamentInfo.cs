namespace Octobroker
{
    public class OctoprintFilamentInfo
    {
        public int Lenght { get; set; }
        public double Volume { get; set; }
        public override string ToString()
        {
            return "Length: " + Lenght + "\nVolume: "+Volume;
        }
    }
}
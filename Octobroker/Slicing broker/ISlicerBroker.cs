namespace Octobroker.Slicing_broker
{
    public interface ISlicerBroker
    {
        string FilePath { get; set; }
        double LayerHeightInMM { get; }
        bool SupportStructureEnabled { get; }
        int FillDensity { get; }
        string SlicerPath { get; }

        void Slice();

    }
}
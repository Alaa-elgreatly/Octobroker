using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;

namespace Octobroker.Slicing_broker
{
    /// <summary>
    /// class represents a file path to be sliced using prusa with the given specific parameters or prusa defaults
    /// </summary>
    public class PrusaSlicerBroker : ISlicerBroker
    {
        public PrusaSlicerBroker()
        {

        }
        /// <summary>
        /// temp for trial
        /// </summary>
        public PrusaSlicerBroker(string filePath, int fill = 20, double layer = 0.3, bool support = false, string outputpath = "", string outputName = "")
        {
            FilePath = filePath;
            FillDensity = fill;
            LayerHeightInMM = layer;
            SupportStructureEnabled = support;
            OutputPath = outputpath;
            OutputName = outputName;
        }
        private int fillDensity = 20;
        public string FilePath { get;  set; }
        public double LayerHeightInMM { get; private set; } = 0.3;

        public string OutputPath { get; set; }
        public string OutputName { get; set; }

        public bool SupportStructureEnabled { get; private set; } = false;

        public int FillDensity
        {
            get => fillDensity;
            private set => fillDensity = ValidateFillDensity(value);
        }

        public string SlicerPath { get; } = @"G:\Work\SiegenUniversity\Florian\PrusaSlicer-2.1.1\prusa-slicer-console.exe";


        private int ValidateFillDensity(int value)
        {
            if (value <= 0)
                return 0;
            if (value >= 100)
                return 100;
            if (value % 5 == 0)
                return value;

            return 5 * (int)Math.Round(value / 5.0);
        }

        public void Slice()
        {
            // code to use Prusa Slicer CLI to slice the file with the given parameters 

            string command = GenerateCommandString();

            var psi = new ProcessStartInfo(SlicerPath)
            {
                Arguments =command,
                UseShellExecute = true,
                CreateNoWindow = true
            };
            Process.Start(psi)?.WaitForExit(30000);
        }

        private string GenerateCommandString()
        {
            StringBuilder commandBuilder = new StringBuilder();
            string fillDensityWithPrusaFormat = GetPrusaFormatFillDensity();


            //put the path to the prusa slicer 
            //commandBuilder.Append(SlicerPath);
            //commandBuilder.Append(" ");
            //add slice command shortcut
            commandBuilder.Append("-g");
            commandBuilder.Append(" ");
            //add model file path
            commandBuilder.Append(FilePath);
            commandBuilder.Append(" ");
            //add Layer height 
            commandBuilder.Append("--layer-height=");
            commandBuilder.Append(LayerHeightInMM);
            commandBuilder.Append(" ");
            //add fill density after converting it to a value in the range of  0-1
            commandBuilder.Append("--fill-density=");
            commandBuilder.Append(fillDensityWithPrusaFormat);
            commandBuilder.Append(" ");
            //add support material if selected
            if (SupportStructureEnabled)
            {
                commandBuilder.Append("--support-material");
                commandBuilder.Append(" ");
            }

            //add a specific output path and name
            if (!String.IsNullOrEmpty(OutputPath))
            {
                commandBuilder.Append("-o");
                commandBuilder.Append(" ");
                commandBuilder.Append(OutputPath);
                commandBuilder.Append(".gcode");
            }

            //add a specific output name -in the same directory-
            else if (!String.IsNullOrEmpty(OutputName))
            {
                commandBuilder.Append("-o");
                commandBuilder.Append(" ");
                commandBuilder.Append(OutputName);
                commandBuilder.Append(".gcode");
            }




            return commandBuilder.ToString();
        }

        private string GetPrusaFormatFillDensity()
        {
            return (((double)fillDensity) / 100).ToString();
        }
    }
}

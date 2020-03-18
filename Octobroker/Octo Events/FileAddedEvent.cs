using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Octobroker.Slicing_broker;

namespace Octobroker.Octo_Events
{
    public class FileAddedEvent : OctoprintEvent
    {
        private OctoprintConnection Connection;
        public string FileName { get; private set; }
        public string Path { get; private set; }
        public string LocalFilePath { get; private set; }
        public string SlicedFilePath { get;  set; }

        public string Type { get; private set; }
        public event EventHandler<FileReadyForSlicingArgs> FileReadyForSlicing;

        public event EventHandler<FileSlicedArgs> FileSliced;

        public FileAddedEvent(string name, JToken payload) : base(name, payload)
        {
            //FileReadyForSlicing += OnFileReadyForSlicing;
            //FileSliced += OnFileSliced;
            ParsePayload();
        }

        protected override void ParsePayload()
        {
            FileName = Payload.Value<String>("name") ?? "";
            Path = Payload.Value<String>("path") ?? "";
            var typeArray = Payload.Value<JArray>("type");

            if (typeArray.Any(element => element.Value<string>() == "gcode"))
                Type = "gcode";
            if (typeArray.Any(element => element.Value<string>() == "stl"))
                Type = "stl";
        }



        public OctoprintFile ConvertToOctoprintFile()
        {
            return new OctoprintFile() { Name = FileName, Path = this.Path, Type = this.Type };
        }

        public void DownloadAssociatedFile(string location, string DownloadPath, OctoprintConnection Connection )
        {
            JObject info = Connection.Files.GetFileInfo(location, this.Path);
            JToken refs = info.Value<JToken>("refs");
            string downloadLink = refs.Value<string>("download");
            this.Connection = Connection;
            using (WebClient myWebClient = new WebClient())
            {
                try
                {
                    //myWebClient.DownloadFileAsync(new Uri(downloadLink), DownloadPath);
                    myWebClient.DownloadFile(new Uri(downloadLink), DownloadPath);
                    LocalFilePath = DownloadPath;
                    FileReadyForSlicing?.Invoke(this, new FileReadyForSlicingArgs(LocalFilePath));
                    SliceAndUpload(LocalFilePath);
                }
                catch (Exception e)
                {
                    var msg = e.Message;
                }

            }

        }



        public void SliceAndUpload(string LocalFilePath)
        {
            SliceWithPrusa(new PrusaSlicerBroker(), LocalFilePath);
            UploadToOctoprint(SlicedFilePath,Connection);
        }

        private void SliceWithPrusa(ISlicerBroker slicer ,string LocalFilePath, string OutputPath = "")
        {
            OutputPath = "G:\\Work\\vasawithlayer";

            PrusaSlicerBroker prusaSlicer = (PrusaSlicerBroker) slicer;
            if (prusaSlicer==null)
                return;

            prusaSlicer.FilePath = LocalFilePath;
            prusaSlicer.OutputPath = OutputPath;
            prusaSlicer.Slice();
            this.SlicedFilePath = OutputPath;
            FileSliced?.Invoke(this, new FileSlicedArgs(OutputPath));
        }
        /*private*/ public void UploadToOctoprint(string SlicedFilePath,OctoprintConnection Connection)
        {
            if (Connection==null)
                return;
            string uploadResponse =Connection.Files.UploadFile(SlicedFilePath,"trial");
        }



        private void OnFileReadyForSlicing(object sender, EventArgs args)
        {
            var fileInfo = (FileAddedEvent)sender;
            var fileargs = (FileReadyForSlicingArgs)args;
            if (fileargs == null || fileInfo == null)
                return;
            if (string.IsNullOrEmpty(fileargs.FilePath))
                return;
            SliceAndUpload(fileargs.FilePath);
        }
        private void OnFileSliced(object sender, EventArgs args)
        {
            var fileInfo = (FileAddedEvent)sender;
            var slicedArgs = (FileSlicedArgs)args;
            if (slicedArgs == null || fileInfo == null)
                return;
            if (string.IsNullOrEmpty(slicedArgs.SlicedFilePath))
                return;

            UploadToOctoprint(slicedArgs.SlicedFilePath,Connection);
        }

    }

    public class FileSlicedArgs : EventArgs
    {
        public FileSlicedArgs(string filePath)
        {
            SlicedFilePath = filePath;
        }

        public string SlicedFilePath { get; set; }
    }

    public class FileReadyForSlicingArgs : EventArgs
    {
        public FileReadyForSlicingArgs(string filePath)
        {
            FilePath = filePath;
        }

        public string FilePath { get; set; }
        
    }
}

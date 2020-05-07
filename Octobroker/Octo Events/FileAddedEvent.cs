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
        public string FileNameWithoutExtension => System.IO.Path.ChangeExtension(FileName, null);
        public string Path { get; private set; }
        public string LocalFilePath { get; private set; }
        public string SlicedFilePath { get;  set; }
        public string SlicedFileName => System.IO.Path.ChangeExtension(FileName, "gcode");
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

        /// <summary>
        /// Legacy funtion, to be deleted if the rest is stable
        /// </summary>
        /// <param name="location"></param>
        /// <param name="DownloadPath"></param>
        /// <param name="Connection"></param>
        public void DownloadAndSliceAndUploadAssociatedFile(string location, string DownloadPath, OctoprintConnection Connection )
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
                    myWebClient.DownloadFile(new Uri(downloadLink), DownloadPath+FileName);
                    LocalFilePath = DownloadPath + FileName;
                    SlicedFilePath = DownloadPath + FileName;
                    SlicedFilePath = System.IO.Path.ChangeExtension(LocalFilePath, null);
                    FileReadyForSlicing?.Invoke(this, new FileReadyForSlicingArgs(LocalFilePath));
                    //SliceAndUpload(LocalFilePath, SlicedFilePath);
                }
                catch (Exception e)
                {
                    var msg = e.Message;
                }

            }

        }

        public void DownloadAssociatedOnlineFile(string location, string downloadPath, OctoprintConnection connection)
        {
            JObject info = connection.Files.GetFileInfo(location, this.Path);
            JToken refs = info.Value<JToken>("refs");
            string downloadLink = refs.Value<string>("download");
            this.Connection = connection;

            using (WebClient myWebClient = new WebClient())
            {
                try
                {
                    //myWebClient.DownloadFileAsync(new Uri(downloadLink), DownloadPath);
                    myWebClient.DownloadFile(new Uri(downloadLink), downloadPath + FileName);
                    LocalFilePath = downloadPath + FileName;
                    SlicedFilePath = downloadPath + FileName;
                    SlicedFilePath = System.IO.Path.ChangeExtension(LocalFilePath, null);
                    FileReadyForSlicing?.Invoke(this, new FileReadyForSlicingArgs(LocalFilePath));
                }
                catch (Exception e)
                {
                    var msg = e.Message;
                }

            }
        }

        public void SliceAndUpload(string LocalFilePath, string OutputFilePath)
        {
            SliceWithPrusa(LocalFilePath, OutputFilePath);
            UploadToOctoprintAsync(SlicedFilePath, Connection);
        }

        private  void Slice(ISlicerBroker slicer ,string LocalFilePath, string OutputPath = "")
        {
            PrusaSlicerBroker prusaSlicer = (PrusaSlicerBroker) slicer;
            if (prusaSlicer==null)
                return;
            SliceWithPrusa(LocalFilePath,OutputPath);

        }
        public void SliceWithPrusa(string LocalFilePath, string OutputPath = "", int fill = 20, double layer = 0.3, bool support = false, string outputname = "")
        {

            PrusaSlicerBroker prusaSlicer = new PrusaSlicerBroker(this.LocalFilePath, fill, layer, support);


            //if the path of the output gcode file is specified then slice and put it in that path (must be specified without .gcode extension)
            if (!string.IsNullOrEmpty(OutputPath))
                prusaSlicer.OutputPath = OutputPath;
            if (!string.IsNullOrEmpty(outputname))
                prusaSlicer.OutputName = outputname;
            // if there is no specific slicing path, slice in the same place of the stl but remove the .stl first of the sliced path then append .gcode 
            else
                OutputPath = System.IO.Path.ChangeExtension(LocalFilePath, null);
            prusaSlicer.Slice();
            this.SlicedFilePath = OutputPath + ".gcode";
            FileSliced?.Invoke(this, new FileSlicedArgs(SlicedFilePath));
        }


        /*private*/
        public async Task UploadToOctoprintAsync(string SlicedFilePath,OctoprintConnection Connection)
        {
            if (Connection==null)
                return;
            string uploadResponse = await  Connection.Files.UploadFile(SlicedFilePath, SlicedFileName, "");
        }





        private void OnFileReadyForSlicing(object sender, EventArgs args)
        {
            var fileInfo = (FileAddedEvent)sender;
            var fileargs = (FileReadyForSlicingArgs)args;
            if (fileargs == null || fileInfo == null)
                return;
            if (string.IsNullOrEmpty(fileargs.FilePath))
                return;
            //SliceAndUpload(fileargs.FilePath,);
        }
        private void OnFileSliced(object sender, EventArgs args)
        {
            var fileInfo = (FileAddedEvent)sender;
            var slicedArgs = (FileSlicedArgs)args;
            if (slicedArgs == null || fileInfo == null)
                return;
            if (string.IsNullOrEmpty(slicedArgs.SlicedFilePath))
                return;

            UploadToOctoprintAsync(slicedArgs.SlicedFilePath,Connection);
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

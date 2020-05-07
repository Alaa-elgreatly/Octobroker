using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octobroker.Slicing_broker;

namespace Octobroker
{
    class Program
    {
        static void Main(string[] args)
        {
            string ip;
            string APIkey;

            ip = "http://192.168.1.38:5000";
            APIkey = "E3C06441F4834FD2B94E8C75FD3DF915";
            var octo = new OctoprintConnection(ip, APIkey);
            octo.WebsocketStart();
            
        }

        /// <summary>
        /// Was used as a test for slicing a file 
        /// </summary>
        //private static void SliceFile()
        //{
        //    string command= Console.ReadLine();
            
        //    // mock profile

        //    var filepath = "G:\\Work\\sotvl_Spiral-Vase.stl";
        //    int fill = 47;
        //    double layer = 0.2;
        //    bool support = true;
        //    string outputpath = "G:\\Work\\vasawithlayer";
        //    string outputname = "vasa2";

        //    PrusaSlicerBroker prusaSlicer= new PrusaSlicerBroker(filepath,fill,layer,support,outputpath,outputname);

        //    prusaSlicer.Slice();

        //}
    }
}

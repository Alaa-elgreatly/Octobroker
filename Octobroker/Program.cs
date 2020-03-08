using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Octobroker
{
    class Program
    {
        static void Main(string[] args)
        {
            var octo = new OctoprintConnection("http://192.168.1.41:5000", "E3C06441F4834FD2B94E8C75FD3DF915");
            octo.WebsocketStart();

        }
    }
}

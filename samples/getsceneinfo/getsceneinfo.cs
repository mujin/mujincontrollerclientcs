using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using mujincontrollerclient;

namespace getsceneinfo
{
    class getsceneinfo
    {
        static void Main(string[] args)
        {
            ControllerClient client = new ControllerClient("testuser", "pass", "http://192.168.11.28/");
            string[] pks = client.GetScenePrimaryKeys();
            Console.WriteLine("Have {0} scenes: ", pks.Count());
            foreach (string pk in pks)
            {
                Console.WriteLine(pk);
            }
        }
    }
}

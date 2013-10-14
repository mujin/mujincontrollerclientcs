using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mujin;

namespace APIUserAcceptanceTest
{
    partial class Program
    {
        // Get scene resource
        public static SceneResource CTUB00100()
        {
            ControllerClient controllerClient = CTUA00100();
            return controllerClient.GetScene("irex2013.mujin.dae");
        }

        // Get scene resource- Invalid primary key
        public static void CTUB00200()
        {
            ControllerClient controllerClient = CTUA00100();

            try
            {
                controllerClient.GetScene("irex2013.mujin.hogehoge.dae");
            }
            catch (ClientException ex)
            {
                if (!ex.ToString().Contains("invalid scene primary key")) throw new Exception("Test case CTUB00200 failed");
            }
        }
    }
}

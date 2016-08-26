using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Mujin;
using System.Xml;
using System.Diagnostics;

namespace getsceneinfo
{
    class getsceneinfo
    {
        static void Main(string[] args)
        {
            ControllerClient controllerClient = new ControllerClient("testuser", "pass", "http://controller38");

            SceneResource scene = controllerClient.GetScene("askulofficepicking2.mujin.dae");
            BinPickingTask task = (BinPickingTask)scene.GetOrCreateTaskFromName("test", "binpicking", "localhost", 11000);
            Console.WriteLine(task.GetBinpickingState());

            // BinPickingTask task = (BinPickingTask)scene.GetOrCreateTaskFromName("testTask001", "binpicking", "bla", 123);

            // Test1: GetJointValues
            // RobotState robotState = task.GetJointValues();
        }
    }
}

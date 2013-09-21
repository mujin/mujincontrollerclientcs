using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using mujincontrollerclient;
using System.IO;

namespace getsceneinfo
{
    class getsceneinfo
    {
        static void Main(string[] args)
        {
            string mujinIpAddress = "";
            string scenePrimaryKey = "";
            string username = "";
            string password = "";
            string taskName = "";
            string taskType = "";
            string controllerip = "";
            int controllerport = 0;

            ControllerClient controllerClient = new ControllerClient(username, password, mujinIpAddress);
            Scene scene = controllerClient.GetScene(scenePrimaryKey);
            Task task = scene.GetOrCreateTaskFromName(taskName, taskType, controllerip, controllerport);

            //List<object> jointValues = task.GetJointValues();

            //List<double> jointValues = new List<double>() { 0.5, 0.5, 0.5, 0.5, 0.5 };
            //List<int> jointIndices = new List<int>() { 1, 2, 3, 4, 5 };
            //task.MoveJoints(jointValues, jointIndices);

        }
    }
}

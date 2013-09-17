using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using mujincontrollerclient;
using mujincontrollerclientcs.DataObjects;

namespace getsceneinfo
{
    class getsceneinfo
    {
        static void Main(string[] args)
        {
            string controllerIpAddress = "http://192.168.11.30:8000/";
            string scenePrimaryKey = "irex2013.mujin.dae";
            string username = "testuser";
            string password = "pass";
            string taskName = "testTask001";
            string taskType = "binpicking";

            ControllerClient controllerClient = new ControllerClient(username, password, controllerIpAddress);
            Scene scene = controllerClient.GetScene(scenePrimaryKey);
            Task task = scene.GetOrCreateTaskFromName(taskName, taskType);

            //List<object> jointValues = task.GetJointValues();
          
            List<double> jointValues = new List<double>() { 0.0, 0.5, 0.5, 0.5, 0, 0, 0 };
            List<int> jointIndices = new List<int>() { 0, 1, 2, 3, 4, 5, 6 };

            task.MoveJoints(jointValues, jointIndices);

            System.Threading.Thread.Sleep(2000);
            List<object> jointValuesUpdated = task.GetJointValues();
            string test = "";
            
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using mujincontrollerclient;

namespace getsceneinfo
{
    class getsceneinfo
    {
        static void Main(string[] args)
        {
            string mujinIpAddress = "http://192.168.11.12:80/";
            string scenePrimaryKey = "irex2013.mujin.dae";
            string username = "testuser";
            string password = "pass";
            string taskName = "testTask001";
            string taskType = "binpicking";
            string controllerip = "192.168.11.110";
            int controllerport = 5008;

            ControllerClient controllerClient = new ControllerClient(username, password, mujinIpAddress);

            controllerClient.Initialize("mujin:/irex2013/irex2013.WPJ", "mujincollada", "wincaps", "mujin:/irex2013.muin.dae");
            
            Scene scene = controllerClient.GetScene(scenePrimaryKey);
            
            BinPickingTask task = (BinPickingTask)scene.GetOrCreateTaskFromName(taskName, taskType, controllerip, controllerport);

            //http://192.168.11.12/api/v1/eventitem/?format=json&limit=10&order_by=-datecreated

            //List<object> jointValues = task.GetJointValues();

            //List<double> jointValues = new List<double>() { 0.5, 0.5, 0.5, 0.5, 0.5 };
            //List<int> jointIndices = new List<int>() { 1, 2, 3, 4, 5 };
            //task.MoveJoints(jointValues, jointIndices);

            //System.Threading.Thread.Sleep(2000);
            BinPickingTask.RobotState state = task.GetJointValues();
            //state.jointValues;
           
        }
    }
}

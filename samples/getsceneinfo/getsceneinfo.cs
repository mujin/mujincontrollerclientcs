using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
//using Mujin;
using mujincontrollerclient;
using System.Xml;
using System.Diagnostics;

namespace getsceneinfo
{
    class getsceneinfo
    {
        static void Main(string[] args)
        {
            // Default XML path:
            // string xmlFilepath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "..\\..\\samples\\getsceneinfo\\config\\connection.xml";

            // Custom XML path;
            string xmlFilepath = "C:\\connection.xml";

            XmlDocument document = new XmlDocument();
            StreamReader reader = new StreamReader(xmlFilepath, Encoding.UTF8);
            document.Load(reader);

            string mujinIpAddress = document.GetElementsByTagName("ipaddress")[0].InnerText;
            string scenePrimaryKey = document.GetElementsByTagName("scenepk")[0].InnerText;
            string username = document.GetElementrobotStatesByTagName("username")[0].InnerText;
            string password = document.GetElementsByTagName("password")[0].InnerText;
            string taskName = document.GetElementsByTagName("taskname")[0].InnerText;
            string taskType = document.GetElementsByTagName("tasktype")[0].InnerText;
            string controllerip = document.GetElementsByTagName("controllerip")[0].InnerText;
            int controllerport = int.Parse(document.GetElementsByTagName("controllerport")[0].InnerText);

            ControllerClient controllerClient = new ControllerClient(username, password, mujinIpAddress);

            //controllerClient.Initialize("mujin:/irex2013/irex2013.WPJ", "mujincollada", "wincaps", "mujin:/irex2013.mujin.dae");

            Scene scene = controllerClient.GetScene(scenePrimaryKey);
            BinPickingTask task = (BinPickingTask)scene.GetOrCreateTaskFromName(taskName, taskType, controllerip, controllerport);

            // Test1: GetJointValues
            //BinPickingTask.RobotState  = task.GetJointValues();

            /* 
            // Test2: MoveJoints
            robotState.jointValues[1] += 30;
            robotState.jointValues[3] -= 30;
            List<double> jointValues = new List<double>() { robotState.jointValues[1], robotState.jointValues[3] };
            List<int> jointIndices = new List<int>(){1, 3};
            task.MoveJoints(jointValues, jointIndices);
           */

            List<double> jointValues = new List<double>() { 0,0,0,0,0,0,0 };
            List<int> jointIndices = new List<int>() { 0,1,2,3,4,5,6 };
            task.MoveJoints(jointValues, jointIndices);

            /*
            // Test3: MoveToHandPosition
            double[] basepos = robotState.tools["1Base"];
            basepos[1] += 1;
            basepos[3] -= 1;
            task.MoveToHandPosition(new List<double>(basepos), "transform6d", "1Base");
             */

            // Test4: PickAndMove
            task.PickAndMove("container3", "camera3", "1Base", "translationdirection5d", new List<double>() { 1900, 240, 700, 0, 0, 1 });
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using mujincontrollerclient;
using System.Xml;

namespace getsceneinfo
{
    class getsceneinfo
    {
        static void Main(string[] args)
        {
            string xmlFilepath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "..\\..\\samples\\getsceneinfo\\config\\connection.xml";

            XmlDocument document = new XmlDocument();
            StreamReader reader = new StreamReader(xmlFilepath, Encoding.UTF8);
            document.Load(reader);

            string mujinIpAddress   = document.GetElementsByTagName("ipaddress")[0].InnerText;
            string scenePrimaryKey  = document.GetElementsByTagName("scenepk")[0].InnerText;
            string username         = document.GetElementsByTagName("username")[0].InnerText;
            string password         = document.GetElementsByTagName("password")[0].InnerText;
            string taskName         = document.GetElementsByTagName("taskname")[0].InnerText;
            string taskType         = document.GetElementsByTagName("tasktype")[0].InnerText;
            string controllerip     = document.GetElementsByTagName("controllerip")[0].InnerText;
            int controllerport      = int.Parse(document.GetElementsByTagName("controllerport")[0].InnerText);

            ControllerClient controllerClient = new ControllerClient(username, password, mujinIpAddress);

            //controllerClient.Initialize("mujin:/irex2013/irex2013.WPJ", "mujincollada", "wincaps", "mujin:/irex2013.muin.dae");
            
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

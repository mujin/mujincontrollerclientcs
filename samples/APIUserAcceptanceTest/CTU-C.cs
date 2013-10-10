using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mujin;
using System.Xml;
using System.IO;

namespace APIUserAcceptanceTest
{
    partial class Program
    {
        // Create a new task and get the task
        public static BinPickingTask CTUC00100()
        {
            // Make sure by browser that no task is present.

            SceneResource scene = CTUB00100();

            string xmlFilepath = "C:\\connection2.xml";

            XmlDocument document = new XmlDocument();
            StreamReader reader = new StreamReader(xmlFilepath, Encoding.UTF8);
            document.Load(reader);

            string taskName = document.GetElementsByTagName("taskname")[0].InnerText;
            string taskType = document.GetElementsByTagName("tasktype")[0].InnerText;
            string controllerip = document.GetElementsByTagName("controllerip")[0].InnerText;
            int controllerport = int.Parse(document.GetElementsByTagName("controllerport")[0].InnerText);
        
            BinPickingTask task = (BinPickingTask)scene.GetOrCreateTaskFromName(
                        taskName, taskType, controllerip, controllerport);

            return (BinPickingTask)scene.GetOrCreateTaskFromName(
                        taskName, taskType, controllerip, controllerport);
        }

        // Create a new task with incorrect ipaddress
        public static void CTUC00200_CTUC00300()
        {
            // Make sure by browser that no task is present.

            SceneResource scene = CTUB00100();

            string xmlFilepath = "C:\\connection2.xml";

            XmlDocument document = new XmlDocument();
            StreamReader reader = new StreamReader(xmlFilepath, Encoding.UTF8);
            document.Load(reader);

            string taskName = document.GetElementsByTagName("taskname")[0].InnerText;
            string taskType = document.GetElementsByTagName("tasktype")[0].InnerText;
            string controllerip = "192.168.9.99";
            int controllerport = int.Parse(document.GetElementsByTagName("controllerport")[0].InnerText);

            BinPickingTask task = (BinPickingTask)scene.GetOrCreateTaskFromName(
                        taskName, taskType, controllerip, controllerport);

            try
            {
                task.GetJointValues();
            }
            catch (ClientException ex)
            {
                if (!ex.ToString().Contains("Controller ip is not correct")) throw new Exception("Test case CTUC00200_CTUC00300 failed");
            }
        }

        // Create a new task with incorrect port number
        public static void CTUC00400_CTUC00500()
        {
            // Make sure by browser that no task is present.

            SceneResource scene = CTUB00100();

            string xmlFilepath = "C:\\connection2.xml";

            XmlDocument document = new XmlDocument();
            StreamReader reader = new StreamReader(xmlFilepath, Encoding.UTF8);
            document.Load(reader);

            string taskName = document.GetElementsByTagName("taskname")[0].InnerText;
            string taskType = document.GetElementsByTagName("tasktype")[0].InnerText;
            string controllerip = document.GetElementsByTagName("controllerip")[0].InnerText;
            int controllerport = 9999;

            BinPickingTask task = (BinPickingTask)scene.GetOrCreateTaskFromName(
                        taskName, taskType, controllerip, controllerport);

            try
            {
                task.GetJointValues();
            }
            catch (ClientException ex)
            {
                if (!ex.ToString().Contains("Controller port number is not correct")) throw new Exception("Test case CTUC00400_CTUC00500 failed");
            }
        }
    }
}

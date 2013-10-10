using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Diagnostics;
using Mujin;

namespace APIUserAcceptanceTest
{
    partial class Program
    {
        // Create ControllerClient instance and try login
        private static ControllerClient CTUA00100()
        {
            string xmlFilepath = "C:\\connection2.xml";

            XmlDocument document = new XmlDocument();
            StreamReader reader = new StreamReader(xmlFilepath, Encoding.UTF8);
            document.Load(reader);

            string mujinIpAddress = document.GetElementsByTagName("ipaddress")[0].InnerText;
            string scenePrimaryKey = document.GetElementsByTagName("scenepk")[0].InnerText;
            string username = document.GetElementsByTagName("username")[0].InnerText;
            string password = document.GetElementsByTagName("password")[0].InnerText;
            string taskName = document.GetElementsByTagName("taskname")[0].InnerText;
            string taskType = document.GetElementsByTagName("tasktype")[0].InnerText;
            string controllerip = document.GetElementsByTagName("controllerip")[0].InnerText;
            int controllerport = int.Parse(document.GetElementsByTagName("controllerport")[0].InnerText);

            ControllerClient controllerClient = new ControllerClient(username, password, mujinIpAddress);
            return controllerClient;
        }

        // Create ControllerClient instance and try login - Invalid username
        private static void CTUA00200()
        {
            string xmlFilepath = "C:\\connection2.xml";

            XmlDocument document = new XmlDocument();
            StreamReader reader = new StreamReader(xmlFilepath, Encoding.UTF8);
            document.Load(reader);

            string mujinIpAddress = document.GetElementsByTagName("ipaddress")[0].InnerText;
            string scenePrimaryKey = document.GetElementsByTagName("scenepk")[0].InnerText;
            string username = "hogehoge";
            string password = document.GetElementsByTagName("password")[0].InnerText;
            string taskName = document.GetElementsByTagName("taskname")[0].InnerText;
            string taskType = document.GetElementsByTagName("tasktype")[0].InnerText;
            string controllerip = document.GetElementsByTagName("controllerip")[0].InnerText;
            int controllerport = int.Parse(document.GetElementsByTagName("controllerport")[0].InnerText);

            try
            {
                ControllerClient controllerClient = new ControllerClient(username, password, mujinIpAddress);
            }
            catch (ClientException ex)
            {
                if (!ex.ToString().Contains("Confirm username and password")) throw new Exception("Test case CTUA00200 failed");
            }
        }

        // Create ControllerClient instance and try login - Invalid password
        private static void CTUA00300()
        {
            string xmlFilepath = "C:\\connection2.xml";

            XmlDocument document = new XmlDocument();
            StreamReader reader = new StreamReader(xmlFilepath, Encoding.UTF8);
            document.Load(reader);

            string mujinIpAddress = document.GetElementsByTagName("ipaddress")[0].InnerText;
            string scenePrimaryKey = document.GetElementsByTagName("scenepk")[0].InnerText;
            string username = document.GetElementsByTagName("password")[0].InnerText;
            string password = "foobar";
            string taskName = document.GetElementsByTagName("taskname")[0].InnerText;
            string taskType = document.GetElementsByTagName("tasktype")[0].InnerText;
            string controllerip = document.GetElementsByTagName("controllerip")[0].InnerText;
            int controllerport = int.Parse(document.GetElementsByTagName("controllerport")[0].InnerText);

            try
            {
                ControllerClient controllerClient = new ControllerClient(username, password, mujinIpAddress);
            }
            catch (ClientException ex)
            {
                if (!ex.ToString().Contains("Confirm username and password")) throw new Exception("Test case CTUA00300 failed");
            }
        }

        // Create ControllerClient instance and try login - Invalid URI
        private static void CTUA00400()
        {
            string xmlFilepath = "C:\\connection2.xml";

            XmlDocument document = new XmlDocument();
            StreamReader reader = new StreamReader(xmlFilepath, Encoding.UTF8);
            document.Load(reader);

            string mujinIpAddress = "http://www.hogehogehogehogehogehogehogehogeogehogehogehog.co.jp";
            string scenePrimaryKey = document.GetElementsByTagName("scenepk")[0].InnerText;
            string username = document.GetElementsByTagName("username")[0].InnerText;
            string password = document.GetElementsByTagName("password")[0].InnerText;
            string taskName = document.GetElementsByTagName("taskname")[0].InnerText;
            string taskType = document.GetElementsByTagName("tasktype")[0].InnerText;
            string controllerip = document.GetElementsByTagName("controllerip")[0].InnerText;
            int controllerport = int.Parse(document.GetElementsByTagName("controllerport")[0].InnerText);

            try
            {
                ControllerClient controllerClient = new ControllerClient(username, password, mujinIpAddress);
            }
            catch (ClientException ex)
            {
                if (!ex.ToString().Contains("Confirm Mujin Controller server address")) throw new Exception("Test case CTUA00400 failed");
            }
        }

        // Create ControllerClient instance and try login - Invalid URI
        private static void CTUA00401()
        {
            string xmlFilepath = "C:\\connection2.xml";

            XmlDocument document = new XmlDocument();
            StreamReader reader = new StreamReader(xmlFilepath, Encoding.UTF8);
            document.Load(reader);

            string mujinIpAddress = "http://mujin.co.jp:5899/";
            string scenePrimaryKey = document.GetElementsByTagName("scenepk")[0].InnerText;
            string username = document.GetElementsByTagName("username")[0].InnerText;
            string password = document.GetElementsByTagName("password")[0].InnerText;
            string taskName = document.GetElementsByTagName("taskname")[0].InnerText;
            string taskType = document.GetElementsByTagName("tasktype")[0].InnerText;
            string controllerip = document.GetElementsByTagName("controllerip")[0].InnerText;
            int controllerport = int.Parse(document.GetElementsByTagName("controllerport")[0].InnerText);

            try
            {
                ControllerClient controllerClient = new ControllerClient(username, password, mujinIpAddress);
            }
            catch (ClientException ex)
            {
                if (!ex.ToString().Contains("Confirm Mujin Controller port number, etc")) throw new Exception("Test case CTUA00401 failed");
            }
        }
    }
}

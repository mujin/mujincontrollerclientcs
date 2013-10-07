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
            // Default XML path:
            // string xmlFilepath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "..\\..\\samples\\getsceneinfo\\config\\connection.xml";
            
            // Custom XML path;
            string xmlFilepath = "C:\\connection.xml";

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
            Scene scene = controllerClient.GetScene(scenePrimaryKey);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Mujin;
using System.Xml;
using System.Diagnostics;

namespace updateobjects
{
    class updateobjects
    {
        static void Main(string[] args)
        {
            string hostname = "controller65";
            string username = "testuser";
            string password = "pass";
            string scenepk = "knowledgepicking1.mujin.dae";

            ControllerClient controllerClient = new ControllerClient(username, password, "http://" + hostname);

            SceneResource scene = controllerClient.GetScene(scenepk);
            BinPickingTask task = (BinPickingTask)scene.GetOrCreateTaskFromName("test_task_1", "binpicking");

            // slave request id for the target slave, usually hostname + "_slave0"
            task.SlaveRequestID = hostname + "_slave0";

            // remove objects in the scene whose names have this prefix, optional
            string prefix = "detected_sourcecontainer_";

            // list of objects and their poses to be updated in the scene
            List<Dictionary<string, object>> envstate = new List<Dictionary<string, object>>() {
                new Dictionary<string, object>() {
                    { "name", prefix + "1" },
                    { "quat_", new List<double>() { 1, 0, 0, 0 } },
                    { "translation_", new List<double>() { 0, 0, 300 }  },
                    { "object_uri", "mujin:/work1.mujin.dae" },
                },
                new Dictionary<string, object>() {
                    { "name", prefix + "2" },
                    { "quat_", new List<double>() { 1, 0, 0, 0 } },
                    { "translation_", new List<double>() { 0, 100, 400 }  },
                    { "object_uri", "mujin:/work2_b.mujin.dae" },
                }
            };

            task.UpdateObjects(envstate, prefix);
        }
    }
}

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
            string hostname = "controller32";
            ControllerClient controllerClient = new ControllerClient("testuser", "pass", "http://" + hostname);

            SceneResource scene = controllerClient.GetScene("metalringntnpicking4.mujin.dae");
            BinPickingTask task = (BinPickingTask)scene.GetOrCreateTaskFromName("test", "binpicking");

            // slave request id for the target slave, usually hostname + "_slave0"
            task.SlaveRequestID = hostname + "_slave0";

            // uri to the object model file
            string objecturi = "mujin:/metalring_ntn2.mujin.dae";

            // remove objects in the scene whose names have this prefix, optional
            string prefix = "testobject_";

            // list of objects and their poses to be updated in the scene
            List<Dictionary<string, object>> envstate = new List<Dictionary<string, object>>() {
                new Dictionary<string, object>() {
                    { "name", prefix + "1" },
                    { "quat_", new List<double>() { 1, 0, 0, 0 } },
                    { "translation_", new List<double>() { 0, 0, 300 }  },
                    { "object_uri", objecturi },
                },
                new Dictionary<string, object>() {
                    { "name", prefix + "2" },
                    { "quat_", new List<double>() { 1, 0, 0, 0 } },
                    { "translation_", new List<double>() { 0, 100, 400 }  },
                    { "object_uri", objecturi },
                }
            };

            task.UpdateObjects(envstate, prefix, objecturi);
        }
    }
}

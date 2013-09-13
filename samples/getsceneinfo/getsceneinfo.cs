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
            string controllerIpAddress = "http://192.168.11.28/";
            string scenePrimaryKey = "office_clean2_calibration.mujin.dae"/*"irex2013.mujin.dae"*/;



            ControllerClient controllerClient = new ControllerClient("testuser", "pass", controllerIpAddress);
            string [] scenePrimaryKeys = controllerClient.GetScenePrimaryKeys();

            if (Array.IndexOf(scenePrimaryKeys, scenePrimaryKey) < 0)
            {
                // Cannot find the dae file.
                // Exit
            }

            string taskName = "testTask001";
            string taskType = "binpicking";

            TaskResource taskResource = controllerClient.GetOrCreateTaskFromName(scenePrimaryKey, taskName, taskType);

            controllerClient.SetTaskParameters(taskResource);

            controllerClient.TaskExecute(taskResource);

            controllerClient.TaskGetResult(taskResource);









            //controllerClient.GetSceneFromPrimaryKey("irex2013.mujin.dae");

            
            
            
           // SceneResource s = new SceneResource("irex2013.mujin.dae"); 
           // s.Get("instobjects");


            // TaskResource task = s.GetOrCreateTaskFromName("task1","binpicking")
            // task.Set("taskparameters", {"controllerip":"1.1.1.1", ...})
            // task.Execute();

            // GetScene

            // GetOrCreateTaskFromName



            /*
            ControllerClient client = new ControllerClient("testuser", "pass", "http://192.168.11.28/");
            string[] pks = client.GetScenePrimaryKeys();
            Console.WriteLine("Have {0} scenes: ", pks.Count());
            foreach (string pk in pks)
            {
                Console.WriteLine(pk);
            }
             * */


           // ControllerClient client = new ControllerClient("testuser", "pass", "http://192.168.11.28/");
           // JobStatus jobStatus = client.GetRunTimeStatuses();

        }
    }
}

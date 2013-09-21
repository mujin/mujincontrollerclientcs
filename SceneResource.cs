using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mujin
{
    public class SceneResource
    {
        private string scenePrimaryKey;
        private ControllerClient controllerClient;

        public SceneResource(string scenePrimaryKey, ControllerClient controllerClient)
        {
            this.scenePrimaryKey = scenePrimaryKey;
            this.controllerClient = controllerClient;
        }
        
        public TaskResource GetOrCreateTaskFromName(string taskName, string taskType, string controllerip, int controllerport)
        {
            throw new NotImplementedException("This method has not been tested.");

            // Query existing tasks.
            string apiParameters = string.Format("scene/{0}/task/?format=json&limit=1&name={1}&fields=pk,tasktype",
                this.scenePrimaryKey, taskName);
            Dictionary<string, object> jsonMessage = controllerClient.Get(apiParameters);
            List<object> tasks = (List<object>)jsonMessage["objects"];

            if (tasks.Count > 0)
            {
                foreach (Dictionary<string, object> keyValuePair in tasks)
                {
                    string taskPrimaryKey = keyValuePair["pk"].ToString();
                    return new TaskResource(taskPrimaryKey, taskName, controllerip, controllerport, this.controllerClient);
                }
            }

            // Create a new task.
            apiParameters = string.Format("scene/{0}/task/?format=json&fields=pk", this.scenePrimaryKey);
            var command = new Command();
            command.Add("name", taskName);
            command.Add("tasktype", taskType);
            command.Add("scenepk", this.scenePrimaryKey);

            string body = command.GetString();

            jsonMessage = controllerClient.PostOrPut(apiParameters, body, ControllerClient.HttpMethod.POST);

            string taskPrimaryKeyNew = jsonMessage["pk"].ToString();
            return new TaskResource(taskPrimaryKeyNew, taskName, controllerip, controllerport, this.controllerClient);
        }
    }
}

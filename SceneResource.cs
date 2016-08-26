using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using fastJSON;

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

        public Task GetOrCreateTaskFromName(string taskName, string taskType, string controllerip, int controllerport)
        {
            return this.GetTaskFromName(taskName, taskType, controllerip, controllerport)
                ?? this.CreateTaskFromName(taskName, taskType, controllerip, controllerport);
        }

        private Task GetTaskFromName(string taskName, string taskType, string controllerip, int controllerport)
        {
            string apiParameters = string.Format("scene/{0}/task/?format=json&limit=1&name={1}&fields=pk,tasktype",
                this.scenePrimaryKey, taskName);

            Dictionary<string, object> jsonMessage = controllerClient.GetJsonMessage(HttpMethod.GET, apiParameters);

            List<object> tasks = (List<object>)jsonMessage["objects"];

            if (tasks.Count == 0) return null;

            Dictionary<string, object> resultMap = (Dictionary<string, object>)tasks[0];

            string taskPrimaryKey = (string)resultMap["pk"];
            string taskTypeResult = (string)resultMap["tasktype"];

            if (!taskType.Equals(taskTypeResult)) throw new ClientException("unsupported task type: " + taskTypeResult);
            return new BinPickingTask(taskPrimaryKey, taskName, controllerip, controllerport, this.controllerClient);
        }

        private Task CreateTaskFromName(string taskName, string taskType, string controllerip, int controllerport)
        {
            string apiParameters = string.Format("scene/{0}/task/?format=json&fields=pk", this.scenePrimaryKey);

            Dictionary<string, object> command = new Dictionary<string, object>();
            command["name"] = taskName;
            command["tasktype"] = taskType;
            command["scenepk"] = this.scenePrimaryKey;
            string message = JSON.Instance.ToJSON(command);

            Dictionary<string, object> jsonMessage = controllerClient.GetJsonMessage(HttpMethod.POST, apiParameters, message);

            string taskPrimaryKeyNew = jsonMessage["pk"].ToString();
            string taskTypeNew = jsonMessage["tasktype"].ToString();

            if (!taskType.Equals(taskTypeNew)) throw new ClientException("unsupported task type: " + taskTypeNew);
            return new BinPickingTask(taskPrimaryKeyNew, taskName, controllerip, controllerport, this.controllerClient);
        }

        // private Dictionary<string, object> ExceuteTaskSync(string taskType, Command taskparameters, string slaverequestid = "")
        // {
        //     string apiParameters = string.Format("scene/{0}/resultget/?format=json", this.scenePrimaryKey);

        //     Dictionary<string, object> command = new Dictionary<string, object>();
        //     command["tasktype"] = taskType;
        //     command["taskparameters"] = taskparameters;
        //     command["slaverequestid"] = slaverequestid;
        //     return controllerClient.GetJsonMessage(HttpMethod.GET, apiParameters, command.GetString());
        // }

        // public void UpdateObjects(string taskType, Dictionary<string, object> envstate, string objectname, string objecturi, string unit = "mm", string slaverequestid = "")
        // {
        //     Command taskparameters = new Command();
        //     taskparameters.Add("command", "UpdateObjects");
        //     taskparameters.Add("envstate", envstate);
        //     taskparameters.Add("unit", unit);
        //     taskparameters.Add("objectname", objectname);
        //     taskparameters.Add("object_uri", objecturi);

        //     this.ExceuteTaskSync(taskType, taskparameters, slaverequestid);
        // }

        // public Dictionary<string, object> GetBinpickingState(string taskType, string slaverequestid = "")
        // {
        //     Command taskparameters = new Command();
        //     taskparameters.Add("command", "GetBinpickingState");
        //     return this.ExceuteTaskSync(taskType, taskparameters, slaverequestid);
        // }
    }
}

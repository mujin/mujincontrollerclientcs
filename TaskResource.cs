using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Mujin
{
    public class TaskResource
    {
        private string taskPrimaryKey;
        private string taskName;
        private string controllerip;
        private int controllerport;
        private ControllerClient controllerClient;

        public TaskResource(string taskPrimaryKey, string taskName, string controllerip, int controllerport, ControllerClient controllerClient)
        {
            this.taskPrimaryKey = taskPrimaryKey;
            this.taskName = taskName;
            this.controllerClient = controllerClient;
            this.controllerport = controllerport;
            this.controllerClient = controllerClient;
        }

        public List<object> GetJointValues(long timeOutMilliseconds = 10000)
        {
            throw new NotImplementedException("This method has not been tested.");

            string apiParameters = string.Format("task/{0}/?format=json&fields=pk", this.taskPrimaryKey);

            Command apistring = new Command();
            apistring.Add("controllerip", this.controllerip);
            apistring.Add("controllerport", this.controllerport);
            apistring.Add("command", "GetJointValues");
            Command command = new Command();
            command.Add("tasktype", "binpicking");
            command.Add("taskparameters", apistring);

            string body = command.GetString();

            Dictionary<string, object> jsonMessage = controllerClient.PostOrPut(apiParameters, body, ControllerClient.HttpMethod.PUT);

            List<object> result = this.Execute(timeOutMilliseconds);

            Dictionary<string, object> pair = (Dictionary<string, object>)result[0];
            Dictionary<string, object> jointValuesMap = (Dictionary<string, object>)pair["output"];

            List<object> jointValues = (List<object>)jointValuesMap["currentjointvalues"];

            return jointValues;
        }

        public void MoveJoints(List<double> jointValues, List<int> jointIndices, long timeOutMilliseconds = 60000)
        {
            throw new NotImplementedException("This method has not been tested.");

            string apiParameters = string.Format("task/{0}/?format=json&fields=pk", this.taskPrimaryKey);

            Command apistring = new Command();
            apistring.Add("controllerip", this.controllerip);
            apistring.Add("controllerport", this.controllerport);
            apistring.Add("command", "MoveJoints");
            //apistring.Add("robot","VP-5243I");
            apistring.Add("goaljoints", jointValues);
            apistring.Add("jointindices", jointIndices);
            Command command = new Command();
            command.Add("tasktype", "binpicking");
            command.Add("taskparameters", apistring);

            string message = command.GetString();

            Dictionary<string, object> jsonMessage = controllerClient.PostOrPut(apiParameters, message, ControllerClient.HttpMethod.PUT);

            List<object> result = this.Execute(timeOutMilliseconds);
        }

        public void MoveToHandPosition(long timeOutMilliseconds = 5000)
        {
            string apiParameters = string.Format("task/{0}/?format=json&fields=pk", this.taskPrimaryKey);

            Command apistring = new Command();
            apistring.Add("controllerip", this.controllerip);
            apistring.Add("controllerport", this.controllerport);
            apistring.Add("command", "MoveToHandPosition");
            //apistring.Add("goals", );
            Command command = new Command();
            command.Add("tasktype", "binpicking");
            command.Add("taskparameters", apistring);

            string message = command.GetString();

            Dictionary<string, object> jsonMessage = controllerClient.PostOrPut(apiParameters, message, ControllerClient.HttpMethod.PUT);

            List<object> result = this.Execute(timeOutMilliseconds);
        }


        public void MoveToArea(long timeOutMilliseconds = 60000)
        {

        }


        public void PickAndPlace(long timeOutMilliseconds = 60000)
        {

        }

        public void MoveToSensorVisibility(long timeOutMilliseconds = 60000)
        {

        }


        private List<object> Execute(long timeOutMilliseconds)
        {
            string apiParameters = string.Format("task/{0}/", this.taskPrimaryKey);
            string body = "";   // Not sure what this means.

            Dictionary<string, object> jsonMessage = controllerClient.PostOrPut(apiParameters, body, ControllerClient.HttpMethod.PUT);

            List<object> result = new List<object>();

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            do
            {
                result = this.GetResult();

                if (stopWatch.ElapsedMilliseconds > timeOutMilliseconds) return result;
            } while (result.Count == 0);

            return result;
        }

        private List<object> GetResult()
        {
            string apiParameters = string.Format("task/{0}/result/?format=json&limit=1&optimization=None", this.taskPrimaryKey);

            List<object> objects = new List<object>();

            Dictionary<string, object> jsonMessage = controllerClient.Get(apiParameters);
            objects = (List<object>)jsonMessage["objects"];

            return objects;
        }

    }
}

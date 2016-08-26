using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using fastJSON;

namespace Mujin
{
    public class Task
    {
    };

    public class RobotState
    {
        public string[] jointNames;
        public double[] jointValues;
        public Dictionary<string, double[]> tools; // each tool is X,Y,Z,RX,RY,RZ similar to densowave format
    }

    public class BinPickingTask : Task
    {
        private string taskPrimaryKey;
        private string taskName;
        private string controllerip;
        private int controllerport;
        private ControllerClient controllerClient;

        public BinPickingTask(string taskPrimaryKey, string taskName, string controllerip, int controllerport, ControllerClient controllerClient)
        {
            this.taskPrimaryKey = taskPrimaryKey;
            this.taskName = taskName;
            this.controllerip = controllerip;
            this.controllerport = controllerport;
            this.controllerClient = controllerClient;
        }

        public RobotState GetJointValues(long timeOutMilliseconds = 30000)
        {
            string apiParameters = string.Format("task/{0}/?format=json&fields=pk", this.taskPrimaryKey);

            Dictionary<string, object> apistring = new Dictionary<string, object>();
            apistring["controllerip"] = this.controllerip;
            apistring["controllerport"] = this.controllerport;
            apistring["command"] = "GetJointValues";
            Dictionary<string, object> command = new Dictionary<string, object>();
            command["tasktype"] = "binpicking";
            command["taskparameters"] = apistring;

            string message = JSON.Instance.ToJSON(command);

            // Update task
            controllerClient.GetJsonMessage(HttpMethod.PUT, apiParameters, message);

            Dictionary<string, object> result = this.Execute(timeOutMilliseconds);
            Dictionary<string, object> jointValuesMap = (Dictionary<string, object>)result["output"];

            RobotState state = new RobotState();
            List<object> jointnames = (List<object>)jointValuesMap["jointnames"];
            state.jointNames = new string[jointnames.Count];
            for (int i = 0; i < jointnames.Count; i++)
            {
                state.jointNames[i] = (string)jointnames[i];
            }
            List<object> currentjointvalues = (List<object>)jointValuesMap["currentjointvalues"];
            Debug.Assert(currentjointvalues.Count == jointnames.Count);
            state.jointValues = new double[currentjointvalues.Count];
            for (int i = 0; i < currentjointvalues.Count; ++i)
            {
                state.jointValues[i] = (double)currentjointvalues[i];
            }
            Dictionary<string, object> tools = (Dictionary<string, object>)jointValuesMap["tools"];
            state.tools = new Dictionary<string, double[]>();
            foreach (KeyValuePair<string, object> keyvalue in tools)
            {
                List<object> toolvalues = (List<object>)keyvalue.Value;
                Debug.Assert(toolvalues.Count == 6);
                double[] dsttoolvalues = new double[6];
                for (int i = 0; i < 6; ++i)
                {
                    dsttoolvalues[i] = (double)toolvalues[i];
                }
                state.tools[keyvalue.Key] = dsttoolvalues;

            }
            return state;
        }

        public void MoveJoints(List<double> jointValues, List<int> jointIndices, double clearance, double speed,
            string robot, long timeOutMilliseconds = 30000)
        {
            //ClientValidator.ValidateJointValues(jointValues, jointIndices);
            //ClientValidator.ValidateClearance(clearance);
            //ClientValidator.ValidateSpeed(speed);

            string apiParameters = string.Format("task/{0}/?format=json&fields=pk", this.taskPrimaryKey);

            Dictionary<string, object> apistring = new Dictionary<string, object>();
            apistring["controllerip"] = this.controllerip;
            apistring["controllerport"] = this.controllerport;
            apistring["command"] = "MoveJoints";
            //apistring.Add("robot",robot);
            apistring["goaljoints"] = jointValues;
            apistring["jointindices"] = jointIndices;
            apistring["envclearance"] = clearance;
            apistring["speed"] = speed;
            Dictionary<string, object> command = new Dictionary<string, object>();
            command["tasktype"] = "binpicking";
            command["taskparameters"] = apistring;

            string message = JSON.Instance.ToJSON(command);

            Dictionary<string, object> jsonMessage = controllerClient.GetJsonMessage(HttpMethod.PUT, apiParameters, message);
            Dictionary<string, object> result = this.Execute(timeOutMilliseconds);
        }

        public void MoveToHandPosition(List<double> goals, GoalType goalType, string toolname, double speed, long timeOutMilliseconds = 30000)
        {
            //ClientValidator.ValidateGoalPositions(goals, goalType);
            //ClientValidator.ValidateSpeed(speed);

            string apiParameters = string.Format("task/{0}/?format=json&fields=pk", this.taskPrimaryKey);

            Dictionary<string, object> apistring = new Dictionary<string, object>();
            apistring["controllerip"] = this.controllerip;
            apistring["controllerport"] = this.controllerport;
            apistring["command"] = "MoveToHandPosition";
            apistring["toolname"] = toolname;
            apistring["goaltype"] = goalType.ToString();
            apistring["goals"] = goals;
            apistring["speed"] = speed;
            Dictionary<string, object> command = new Dictionary<string, object>();
            command["tasktype"] = "binpicking";
            command["taskparameters"] = apistring;

            string message = JSON.Instance.ToJSON(command);

            Dictionary<string, object> jsonMessage = controllerClient.GetJsonMessage(HttpMethod.PUT, apiParameters, message);
            Dictionary<string, object> result = this.Execute(timeOutMilliseconds);
        }

        public WorkProperties PickAndMove(string boxname, string sensorName, string toolname, GoalType goaltype, List<double> goalTranslationDirections,
            int movetodestination, int deletetarget, double speed, long timeOutMilliseconds = 60000)
        {
            string apiParameters = string.Format("task/{0}/?format=json&fields=pk", this.taskPrimaryKey);

            Dictionary<string, object> apistring = new Dictionary<string, object>();
            apistring["controllerip"] = this.controllerip;
            apistring["controllerport"] = this.controllerport;
            apistring["command"] = "PickAndMove";
            apistring["boxname"] = boxname;
            apistring["sensorname"] = sensorName;
            apistring["toolname"] = toolname;
            apistring["speed"] = speed;
            apistring["goaltype"] = goaltype.ToString();
            apistring["goals"] = goalTranslationDirections;
            apistring["movetodestination"] = movetodestination;
            apistring["deletetarget"] = deletetarget;
            Dictionary<string, object> command = new Dictionary<string, object>();
            command["tasktype"] = "binpicking";
            command["taskparameters"] = apistring;

            string message = JSON.Instance.ToJSON(command);

            Dictionary<string, object> jsonMessage = controllerClient.GetJsonMessage(HttpMethod.PUT, apiParameters, message);
            Dictionary<string, object> result = this.Execute(timeOutMilliseconds);

            Dictionary<string, object> resultMap = result.ContainsKey("output") ? (Dictionary<string, object>)result["output"] : null;
            
            string graspedname = resultMap.ContainsKey("graspedname") ? (string)resultMap["graspedname"] : null;
            List<object> destinationgoals = resultMap.ContainsKey("destinationgoals") ? (List<object>)resultMap["destinationgoals"] : null;

            List<double> list = new List<double>();
            if (destinationgoals != null)
            {
                foreach (object element in destinationgoals)
                {
                    list.Add((double)element);
                }
            }

            return new WorkProperties(graspedname, list);
        }

        public void OpenGripper(string robot, string toolname, string targetname, double speed, long timeOutMilliseconds = 60000)
        {

            string apiParameters = string.Format("task/{0}/?format=json&fields=pk", this.taskPrimaryKey);

            Dictionary<string, object> apistring = new Dictionary<string, object>();
            apistring["controllerip"] = this.controllerip;
            apistring["controllerport"] = this.controllerport;
            apistring["command"] = "OpenGripper";
            apistring["speed"] = speed; 
            apistring["robot"] = robot;
            apistring["toolname"] = toolname;
            apistring["targetname"] = targetname;
            Dictionary<string, object> command = new Dictionary<string, object>();
            command["tasktype"] = "binpicking";
            command["taskparameters"] = apistring;

            string message = JSON.Instance.ToJSON(command);

            Dictionary<string, object> jsonMessage = controllerClient.GetJsonMessage(HttpMethod.PUT, apiParameters, message);
            Dictionary<string, object> result = this.Execute(timeOutMilliseconds);
        }

        public string GetIOVariable(int index, long timeOutMilliseconds = 60000)
        {

            string apiParameters = string.Format("task/{0}/?format=json&fields=pk", this.taskPrimaryKey);

            Dictionary<string, object> apistring = new Dictionary<string, object>();
            apistring["controllerip"] = this.controllerip;
            apistring["controllerport"] = this.controllerport;
            apistring["command"] = "GetIOVariable";
            apistring["index"] = index;
            Dictionary<string, object> command = new Dictionary<string, object>();
            command["tasktype"] = "binpicking";
            command["taskparameters"] = apistring;

            string message = JSON.Instance.ToJSON(command);

            Dictionary<string, object> jsonMessage = controllerClient.GetJsonMessage(HttpMethod.PUT, apiParameters, message);
            Dictionary<string, object> result = this.Execute(timeOutMilliseconds);


            Dictionary<string, object> resultMap = result.ContainsKey("output") ? (Dictionary<string, object>)result["output"] : null;

            string variable = resultMap.ContainsKey("vakye") ? resultMap["value"].ToString() : null;
            return variable;
        }

        public void Grab(string toolname, string targetname, long timeOutMilliseconds = 60000)
        {

            string apiParameters = string.Format("task/{0}/?format=json&fields=pk", this.taskPrimaryKey);

            Dictionary<string, object> apistring = new Dictionary<string, object>();
            apistring["controllerip"] = this.controllerip;
            apistring["controllerport"] = this.controllerport;
            apistring["command"] = "Grab";
            apistring["toolname"] = toolname;
            apistring["targetname"] = targetname;
            Dictionary<string, object> command = new Dictionary<string, object>();
            command["tasktype"] = "binpicking";
            command["taskparameters"] = apistring;

            string message = JSON.Instance.ToJSON(command);

            Dictionary<string, object> jsonMessage = controllerClient.GetJsonMessage(HttpMethod.PUT, apiParameters, message);
            Dictionary<string, object> result = this.Execute(timeOutMilliseconds);
        }

        public Dictionary<string, object> GetBinpickingState(long timeOutMilliseconds = 60000)
        {
            string apiParameters = string.Format("task/{0}/?format=json&fields=pk", this.taskPrimaryKey);

            Dictionary<string, object> taskparameters = new Dictionary<string, object>();
            taskparameters["command"] = "GetBinpickingState";

            Dictionary<string, object> command = new Dictionary<string, object>();
            command["tasktype"] = "binpicking";
            command["taskparameters"] = taskparameters;
            
            string message = JSON.Instance.ToJSON(command);

            Dictionary<string, object> jsonMessage = controllerClient.GetJsonMessage(HttpMethod.PUT, apiParameters, message);
            Dictionary<string, object> result = this.Execute(timeOutMilliseconds);
            return result;
        }

        public void UpdateObjects(string taskType, Dictionary<string, object> envstate, string objectname, string objecturi, string unit = "mm", long timeOutMilliseconds = 60000)
        {
            string apiParameters = string.Format("task/{0}/?format=json&fields=pk", this.taskPrimaryKey);

            Dictionary<string, object> taskparameters = new Dictionary<string, object>();
            taskparameters["command"] = "UpdateObjects";
            taskparameters["envstate"] = envstate;
            taskparameters["unit"] = unit;
            taskparameters["objectname"] = objectname;
            taskparameters["object_uri"] = objecturi;

            Dictionary<string, object> command = new Dictionary<string, object>();
            command["tasktype"] = "binpicking";
            command["taskparameters"] = taskparameters;

            string message = JSON.Instance.ToJSON(command);

            Dictionary<string, object> jsonMessage = controllerClient.GetJsonMessage(HttpMethod.PUT, apiParameters, message);
            Dictionary<string, object> result = this.Execute(timeOutMilliseconds);
        }

        private Dictionary<string, object> Execute(long timeOutMilliseconds)
        {
            string apiParameters = string.Format("task/{0}/", this.taskPrimaryKey);
            // Seems to be an empty message.
            string message = "";

            Dictionary<string, object> jsonMessage = controllerClient.GetJsonMessage(HttpMethod.POST, apiParameters, message);

            object result = null;
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            do
            {
                if (stopWatch.ElapsedMilliseconds > timeOutMilliseconds)
                {
                    // should cancel the task using jsonMessage["jobpk"]
                    throw new ClientException("timed out");
                }
                result = this.GetResult();
            } while (result == null);

            System.Threading.Thread.Sleep(100); //  very important sleep otherwise final results will be wrong
            result = this.GetResult();
            Dictionary<string, object> resultdict = (Dictionary<string, object>)result;
            if (resultdict.ContainsKey("errormessage"))
            {
                string errormessage = (string)resultdict["errormessage"];
                if (errormessage.Count() > 0)
                {
                    if(errormessage.Contains("timeout to get response"))
                    {
                        errormessage += "... MC cannot connect to the controller. Possible error reason: " +
                        "(a) Controller ip is not correct, (c) Controller port number is not correct " +
                        "(d) Robot controller is down. (e) b-CAP listener is not up. " +
                        "(f) Firewall blocks packets. (g) Server is not restarted.";
                    }
                    throw new ClientException(errormessage);
                }
            }
            return resultdict;
        }

        private object GetResult()
        {
            string apiParameters = string.Format("task/{0}/result/?format=json&limit=1&optimization=None", this.taskPrimaryKey);
            Dictionary<string, object> jsonMessage = controllerClient.GetJsonMessage(HttpMethod.GET, apiParameters);
            List<object> objects = (List<object>)jsonMessage["objects"];
            if (objects.Count == 0)
            {
                return null;
            }
            return objects[0];
        }
    }
}

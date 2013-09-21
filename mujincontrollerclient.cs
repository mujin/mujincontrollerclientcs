// -*- coding: utf-8 -*-
// Copyright (C) 2013 MUJIN Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// \author Rosen Diankov <rosen.diankov@mujin.co.jp> and Takuya Murakita

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using fastJSON;
using System.Diagnostics;

namespace mujincontrollerclient
{
    // thrown when robot connection is lost
    [Serializable()]
    public class ClientException : System.Exception
    {
        public ClientException() : base() { }
        public ClientException(string message) : base(message) { }
        public ClientException(string message, System.Exception inner) : base(message, inner) { }
        protected ClientException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
    }

    public enum HttpMethod { GET, POST, PUT }

    public class ControllerClient
    {
        private string _baseuri, _baseapiuri;
        private CredentialCache _credentials;
        private string _basewebdavuri;
        private string _username, _password;
        private CookieContainer _cookies;
        private string _csrftoken;

        public ControllerClient(string username, string password, string baseuri = null, int options = 0)
        {
            if (baseuri != null)
            {
                _baseuri = baseuri;
                // ensure trailing slash
                if (_baseuri.Last() != '/')
                {
                    _baseuri += '/';
                }
            }
            else
            {
                // use the default
                _baseuri = "https://controller.mujin.co.jp/";
            }
            _baseapiuri = _baseuri + "api/v1/";
            // hack for now since webdav server and api server could be running on different ports
            if (_baseuri.EndsWith(":8000/") || (options & 0x80000000) != 0)
            {
                // testing on localhost, however the webdav server is running on port 80...
                _basewebdavuri = string.Format("%s/u/%s/", _baseuri.Substring(0, _baseuri.Count() - 6), username);
            }
            else
            {
                _basewebdavuri = string.Format("%su/%s/", _baseuri, username);
            }

            //_credentials = new System.Net.NetworkCredential(username, password);
            _cookies = new CookieContainer();
            _username = username;
            _password = password;
            _credentials = new CredentialCache();
            _credentials.Add(new Uri("https://controller.mujin.co.jp/"), "Basic", new NetworkCredential(_username, _password));


            HttpWebRequest request1 = _GetWebRequest(_baseuri + "login/", "GET");
            using (HttpWebResponse response = (HttpWebResponse)request1.GetResponse())
            {
                if (true) //response.StatusCode == HttpStatusCode.Redirect)
                {
                    // Response is ignored.
                    HttpWebRequest request2 = _GetWebRequest(_baseapiuri, "GET");
                    using (HttpWebResponse response2 = (HttpWebResponse)request2.GetResponse())
                    {
                        // extract csrf token from cookies?
                        //response2.Cookies;
                        // get CSRF from cookies
                        foreach (Cookie item in response2.Cookies)
                        {
                            if (item.Name == "csrftoken")
                            {
                                _csrftoken = item.Value;
                            }
                        }
                    }
                }
                else if (response.StatusCode == HttpStatusCode.OK)
                {
                    // get CSRF from cookies
                    foreach (Cookie item in response.Cookies)
                    {
                        if (item.Name == "csrftoken")
                        {
                            _csrftoken = item.Value;
                        }
                    }
                    HttpWebRequest request3 = _GetWebRequest(_baseuri + "login/", "POST");
                    request3.ContentType = "application/x-www-form-urlencoded";
                    request3.Referer = _baseuri + "login/";
                    request3.AllowAutoRedirect = false; // redirecting messes things up for some reason...
                    using (StreamWriter streamWrite = new StreamWriter(request3.GetRequestStream()))
                    {
                        streamWrite.Write(string.Format("username={0}&password={1}&this_is_the_login_form=1&next=%2F&csrfmiddlewaretoken={2}", username, password, _csrftoken));
                    }
                    using (HttpWebResponse response3 = (HttpWebResponse)request3.GetResponse())
                    {
                        foreach (Cookie item in response3.Cookies)
                        {
                            if (item.Name == "csrftoken")
                            {
                                _csrftoken = item.Value;
                            }
                        }
                    }
                }
            }
        }

        public void SetBasicAuthHeader(WebRequest req, String userName, String userPassword)
        {
            string authInfo = userName + ":" + userPassword;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            req.Headers["Authorization"] = "Basic " + authInfo;
        }

        private HttpWebRequest _GetWebRequest(string url, string method)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.Method = method;
            httpWebRequest.Credentials = _credentials;
            SetBasicAuthHeader(httpWebRequest, _username, _password);
            httpWebRequest.ContentType = "application/json; charset=UTF-8";
            httpWebRequest.CookieContainer = _cookies;
            httpWebRequest.PreAuthenticate = false;
            httpWebRequest.UserAgent = "controllerclientcs";
            if (_csrftoken != null)
            {
                httpWebRequest.Headers.Add("X-CSRFToken", _csrftoken);
                httpWebRequest.Referer = _baseuri;
            }
            return httpWebRequest;
        }

        public Dictionary<string, object> GetJsonMessage(HttpMethod method, string apiParameters, string message = null)
        {
            switch (method)
            {
                case HttpMethod.GET: return this.GetJsonMessage(apiParameters);
                case HttpMethod.POST: return this.GetJsonMessage(apiParameters, message, "POST");
                case HttpMethod.PUT: return this.GetJsonMessage(apiParameters, message, "PUT");
                default: return null;
            }
        }

        private Dictionary<string, object> GetJsonMessage(string apiParameters)
        {
            HttpWebRequest request = _GetWebRequest(_baseapiuri + apiParameters, "GET");

            Dictionary<string, object> jsonmsg = null;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string responsestring = reader.ReadToEnd();
                    jsonmsg = (Dictionary<string, object>)JSON.Instance.Parse(responsestring);
                }
            }

            return jsonmsg;
        }

        private Dictionary<string, object> GetJsonMessage(string apiParameters, string message, string method)
        {
            HttpWebRequest postWebRequest = _GetWebRequest(_baseapiuri + apiParameters, method);

            Dictionary<string, object> jsonmsg = null;

            using (StreamWriter streamWrite = new StreamWriter(postWebRequest.GetRequestStream()))
            {
                streamWrite.Write(message);
            }
            using (HttpWebResponse postWebResponse = (HttpWebResponse)postWebRequest.GetResponse())
            {
                using (StreamReader reader = new StreamReader(postWebResponse.GetResponseStream()))
                {
                    string response = reader.ReadToEnd();
                    jsonmsg = (Dictionary<string, object>)JSON.Instance.Parse(response);
                    return jsonmsg;
                }
            }
        }

        public Scene GetScene(string scenePrimaryKey)
        {
            string apiParameters = string.Format("scene/{0}/?format=json&fields=pk", scenePrimaryKey);
            Dictionary<string, object> jsonMessage = this.GetJsonMessage(apiParameters);

            System.Diagnostics.Debug.Assert(jsonMessage["pk"].Equals(scenePrimaryKey));

            return new Scene(scenePrimaryKey, this);
        }

        public string[] GetScenePrimaryKeys()
        {
            HttpWebRequest request = _GetWebRequest(_baseapiuri + "scene/?format=json&fields=pk", "GET");
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string responsestring = reader.ReadToEnd();
                    Dictionary<string, object> jsonmsg = (Dictionary<string, object>)JSON.Instance.Parse(responsestring);
                    List<object> scenes = (List<object>)jsonmsg["objects"];
                    string[] pks = new string[scenes.Count()];
                    for (int i = 0; i < scenes.Count(); ++i)
                    {
                        Dictionary<string, object> scene = (Dictionary<string, object>)scenes[i];
                        pks[i] = (string)scene["pk"];
                    }
                    return pks;
                }
            }
        }
    }

    public class Scene
    {
        private string scenePrimaryKey;
        private ControllerClient controllerClient;

        public Scene(string scenePrimaryKey, ControllerClient controllerClient)
        {
            this.scenePrimaryKey = scenePrimaryKey;
            this.controllerClient = controllerClient;
        }

        public Task GetOrCreateTaskFromName(string taskName, string taskType, string controllerip, int controllerport)
        {
            // Query existing tasks.
            string apiParameters = string.Format("scene/{0}/task/?format=json&limit=1&name={1}&fields=pk,tasktype",
                this.scenePrimaryKey, taskName);
            Dictionary<string, object> jsonMessage = controllerClient.GetJsonMessage(HttpMethod.GET, apiParameters);

            List<object> tasks = (List<object>)jsonMessage["objects"];

            if (tasks.Count > 0)
            {
                foreach (Dictionary<string, object> keyValuePair in tasks)
                {
                    string taskPrimaryKey = keyValuePair["pk"].ToString();
                    return new Task(taskPrimaryKey, taskName, controllerip, controllerport, this.controllerClient);
                }
            }

            // Create a new task.
            apiParameters = string.Format("scene/{0}/task/?format=json&fields=pk", this.scenePrimaryKey);
            string message = string.Format("{{\"name\":\"{0}\", \"tasktype\":\"{1}\", \"scenepk\":\"{2}\"}}",
                taskName, taskType, this.scenePrimaryKey);

            jsonMessage = controllerClient.GetJsonMessage(HttpMethod.POST, apiParameters, message);

            string taskPrimaryKeyNew = jsonMessage["pk"].ToString();
            return new Task(taskPrimaryKeyNew, taskName, controllerip, controllerport, this.controllerClient);
        }
    }

    public class Task
    {
        private string taskPrimaryKey;
        private string taskName;
        private string controllerip;
        private int controllerport;
        private ControllerClient controllerClient;

        // This class should be replaced with fastJson.
        private class Command
        {
            private string command = null;

            public Command Add(string key, string value)
            {
                command = (command == null) ? string.Format("\"{0}\": \"{1}\"", key, value)
                    : command += string.Format(", \"{0}\": \"{1}\"", key, value);
                return this;
            }

            public Command Add(string key, int value)
            {
                command = (command == null) ? string.Format("\"{0}\": {1}", key, value)
                    : command += string.Format(", \"{0}\": {1}", key, value);
                return this;
            }

            public Command Add(string key, float value)
            {
                command = (command == null) ? string.Format("\"{0}\": {1}", key, value)
                    : command += string.Format(", \"{0}\": {1}", key, value);
                return this;
            }

            public Command Add<T>(string key, List<T> objects)
            {
                string arrayString = null;

                foreach (T obj in objects)
                {
                    arrayString += (arrayString == null) ? obj.ToString() : string.Format(", {0}", obj.ToString());
                }

                command += (command == null) ? string.Format("\"{0}\": [{1}]", key, arrayString)
                    : string.Format(", \"{0}\": [{1}]", key, arrayString);

                return this;
            }

            public Command Add(string key, Command addCommand)
            {
                command = (command == null) ? string.Format("\"{0}\": {1}", key, addCommand.GetString())
                    : command += string.Format(", \"{0}\": {1}", key, addCommand.GetString());
                return this;
            }

            public string GetString()
            {
                return string.Format("{{{0}}}", this.command);
            }
        }

        public Task(string taskPrimaryKey, string taskName, string controllerip, int controllerport, ControllerClient controllerClient)
        {
            this.taskPrimaryKey = taskPrimaryKey;
            this.taskName = taskName;
            this.controllerClient = controllerClient;
            this.controllerport = controllerport;
            this.controllerClient = controllerClient;
            this.controllerip = controllerip;
        }

        public List<object> GetJointValues(long timeOutMilliseconds = 60000)
        {
            string apiParameters = string.Format("task/{0}/?format=json&fields=pk", this.taskPrimaryKey);

            Command apistring = new Command();
            apistring.Add("controllerip", this.controllerip);
            apistring.Add("controllerport", this.controllerport);
            apistring.Add("command", "GetJointValues");
            Command command = new Command();
            command.Add("tasktype", "binpicking");
            command.Add("taskparameters", apistring);

            string message = command.GetString();

            // 結果は無視します。
            Dictionary<string, object> jsonMessage = controllerClient.GetJsonMessage(HttpMethod.PUT, apiParameters, message);

            List<object> result = this.Execute(timeOutMilliseconds);

            Dictionary<string, object> pair = (Dictionary<string, object>)result[0];
            Dictionary<string, object> jointValuesMap = (Dictionary<string, object>)pair["output"];
            // 走行軸、J1,J2,J3,J5,J6,ハンド
            List<object> jointValues = (List<object>)jointValuesMap["currentjointvalues"];

            return jointValues;
        }

        public void MoveJoints(List<double> jointValues, List<int> jointIndices, long timeOutMilliseconds = 60000)
        {
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

            // 結果は無視します。
            Dictionary<string, object> jsonMessage = controllerClient.GetJsonMessage(HttpMethod.PUT, apiParameters, message);

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

            Dictionary<string, object> jsonMessage = controllerClient.GetJsonMessage(HttpMethod.PUT, apiParameters, message);

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
            // 空のメッセージを送るようです。
            string message = "";

            Dictionary<string, object> jsonMessage = controllerClient.GetJsonMessage(HttpMethod.POST, apiParameters, message);

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

            Dictionary<string, object> jsonMessage = controllerClient.GetJsonMessage(HttpMethod.GET, apiParameters);
            objects = (List<object>)jsonMessage["objects"];

            return objects;
        }

    }
}

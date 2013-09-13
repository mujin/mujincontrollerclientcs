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
using mujincontrollerclientcs.DataObjects;

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

    public class ControllerClient
    {
        private string _baseuri, _baseapiuri;
        private System.Net.NetworkCredential _credentials;
        private string _basewebdavuri;
        private CookieContainer _cookies;
        private string _csrftoken;

        public ControllerClient(string username, string password, string baseuri=null, int options=0)
        {
            if( baseuri != null ) {
                _baseuri = baseuri;
                // ensure trailing slash
                if( _baseuri.Last() != '/' ) {
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
            if( _baseuri.EndsWith(":8000/") || (options&0x80000000)!=0 )
            {
                // testing on localhost, however the webdav server is running on port 80...
                _basewebdavuri = string.Format("%s/u/%s/", _baseuri.Substring(0, _baseuri.Count()-6), username);
            }
            else {
                _basewebdavuri = string.Format("%su/%s/", _baseuri, username);
            }

            _credentials = new System.Net.NetworkCredential(username, password);
            _cookies = new CookieContainer();

            HttpWebRequest request1 = _GetWebRequest(_baseuri+"login/", "GET");
            using (HttpWebResponse response = (HttpWebResponse)request1.GetResponse())
            {
                if (response.StatusCode == HttpStatusCode.Redirect)
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


                    }
                }
            }

           

            //byte[] requestMessage = "{ "username": testuser, "csrfmiddlewaretoken": firstcsrftoken, 'password': config.PASSWORD,
            //    'this_is_the_login_form': '1',
            //    'next': '/'";

            //}
            //httpWebRequest.GetRequestStream.Write();
        }

        /*
        public SceneResource GetSceneFromPrimaryKey(string sceneDaeFilename)
        {
            new SceneResource(this, sceneDaeFilename);


            HttpWebRequest request = _GetWebRequest(_baseapiuri + "scene/?format=json", "GET");
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string responsestring = reader.ReadToEnd();
                    Dictionary<string, object> jsonmsg = (Dictionary<string, object>)JSON.Instance.Parse(responsestring);


                }
            }
            return null;
        }
        */

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
            //httpWebRequest.Credentials = _credentials;
            SetBasicAuthHeader(httpWebRequest, "testuser", "pass");
            httpWebRequest.ContentType = "application/json; charset=UTF-8";
            httpWebRequest.CookieContainer = _cookies;
            httpWebRequest.PreAuthenticate = true;
            httpWebRequest.UserAgent = "controllerclientcs";
            if (_csrftoken != null)
            {
                httpWebRequest.Headers.Add("X-CSRFToken", _csrftoken);
            }
            return httpWebRequest;
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
                    for(int i = 0; i < scenes.Count(); ++i)
                    {
                        Dictionary<string, object> scene = (Dictionary<string, object>)scenes[i];
                        pks[i] = (string)scene["pk"];
                    }
                    return pks;
                }
            }
        }

        public void TaskGetResult(TaskResource taskResource)
        {
            string message = string.Format("task/{0}/result/?format=json&limit=1&optimization=None&fields=pk", taskResource.getPrimaryKey());
            HttpWebRequest request = _GetWebRequest(_baseapiuri + message, "GET");

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string responsestring = reader.ReadToEnd();
                    Dictionary<string, object> jsonmsg = (Dictionary<string, object>)JSON.Instance.Parse(responsestring);
                    List<object> objects = (List<object>)jsonmsg["objects"];
                    string[] pks = new string[objects.Count()];


                }
            }
        }

        public void TaskExecute(TaskResource taskResource)
        {
            string message = string.Format("task/{0}/", taskResource.getPrimaryKey());
            HttpWebRequest putWebRequest = _GetWebRequest(_baseapiuri + message, "POST");

            using (StreamWriter streamWrite = new StreamWriter(putWebRequest.GetRequestStream()))
            {
                string putMessage = "";
                streamWrite.Write(putMessage);
            }
            using (HttpWebResponse putWebResponse = (HttpWebResponse)putWebRequest.GetResponse())
            {
                using (StreamReader reader = new StreamReader(putWebResponse.GetResponseStream()))
                {
                    string responsestring = reader.ReadToEnd();
                    Dictionary<string, object> jsonmsg = (Dictionary<string, object>)JSON.Instance.Parse(responsestring);
                    string jobPrimaryKey = jsonmsg["jobpk"].ToString();

                    //return new TaskResource(this, taskName, taskPrimaryKey);
                }

            }
        }


        // This is only for GetJointValues for now.
        public void SetTaskParameters(TaskResource taskResource)
        {
            string message = string.Format("task/{0}/?format=json&fields=pk", taskResource.getPrimaryKey());
            HttpWebRequest putWebRequest = _GetWebRequest(_baseapiuri + message, "PUT");
           
            using (StreamWriter streamWrite = new StreamWriter(putWebRequest.GetRequestStream()))
            {
                // Implement the API here
                string putMessage = string.Format("{{\"tasktype\": \"binpicking\", \"taskparameters\":{{\"controllerip\":\"192.168.11.29\", \"controllerport\":5007, \"command\":\"GetJointValues\"}} }}");
                        
                streamWrite.Write(putMessage);
            }
            
              using (HttpWebResponse putWebResponse = (HttpWebResponse)putWebRequest.GetResponse())
              {
                  using (StreamReader reader = new StreamReader(putWebResponse.GetResponseStream()))
                  {
                      string responsestring = reader.ReadToEnd();
                      Dictionary<string, object> jsonmsg = (Dictionary<string, object>)JSON.Instance.Parse(responsestring);
                      //string taskPrimaryKey = jsonmsg2["pk"].ToString();

                      //return new TaskResource(this, taskName, taskPrimaryKey);
                  }

              }
       
        }


        public TaskResource GetOrCreateTaskFromName(string scenePrimaryKey, string taskName, string taskType)
        {
            string message = string.Format("scene/{0}/task/?format=json&limit=1&name={1}&fields=pk,tasktype", scenePrimaryKey, taskName);
            HttpWebRequest request = _GetWebRequest(_baseapiuri + message, "GET");

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string responsestring = reader.ReadToEnd();
                    Dictionary<string, object> jsonmsg = (Dictionary<string, object>)JSON.Instance.Parse(responsestring);
                    List<object> tasks = (List<object>)jsonmsg["objects"];

                    if (tasks.Count > 0)
                    {
                        foreach (Dictionary<string, object> keyValuePair in tasks)
                        {
                            string taskPrimaryKey = keyValuePair["pk"].ToString();
                            return new TaskResource(this, taskName, taskPrimaryKey);
                        }
                    }

                    //controller->CallPost(str(boost::format("scene/%s/task/?format=json&fields=pk")%GetPrimaryKey()), str(boost::format("{\"name\":\"%s\", \"tasktype\":\"%s\", \"scenepk\":\"%s\"}")%taskname%tasktype%GetPrimaryKey()), pt);
   
                    message = string.Format("scene/{0}/task/?format=json&fields=pk", scenePrimaryKey);

                    HttpWebRequest postWebRequest = _GetWebRequest(_baseapiuri + message, "POST");
                    //postWebRequest.ContentType = "application/x-www-form-urlencoded";
                    //postWebRequest.Referer = _baseuri + "/";
                    //postWebRequest.AllowAutoRedirect = false; // redirecting messes things up for some reason...
                    using (StreamWriter streamWrite = new StreamWriter(postWebRequest.GetRequestStream()))
                    {
                        //string postMessage = string.Format("{\"name\":\"{0}\", \"tasktype\":\"{1}\", \"scenepk\":\"{2}\"}", taskName, taskType, scenePrimaryKey);
                        string postMessage = string.Format("{{\"name\":\"{0}\", \"tasktype\":\"{1}\", \"scenepk\":\"{2}\"}}", taskName, taskType, scenePrimaryKey);
                        
                        streamWrite.Write(postMessage);
                        //
                    }
                    using (HttpWebResponse postWebResponse = (HttpWebResponse)postWebRequest.GetResponse())
                    {
                        using (StreamReader reader2 = new StreamReader(postWebResponse.GetResponseStream()))
                        {
                            string responsestring2 = reader2.ReadToEnd();
                            Dictionary<string, object> jsonmsg2 = (Dictionary<string, object>)JSON.Instance.Parse(responsestring2);
                            string taskPrimaryKey = jsonmsg2["pk"].ToString();

                            return new TaskResource(this, taskName, taskPrimaryKey);
                        }

                    }
                }
            }
        }

        public JobStatus GetRunTimeStatuses()
        {
            HttpWebRequest request = _GetWebRequest(_baseapiuri + "job/?format=json&fields=pk,status,fnname,elapsedtime", "GET");
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string responsestring = reader.ReadToEnd();
                    Dictionary<string, object> jsonmsg = (Dictionary<string, object>)JSON.Instance.Parse(responsestring);

                    foreach (KeyValuePair<string, object> keyValuePair in jsonmsg)
                    {
                        
                    }

                }
            }

            return null;
        }


        // SceneResource s = new SceneResource("irex2013.mujin.dae"); s.Get("instobjects")
        // TaskResource task = s.GetOrCreateTaskFromName("task1","binpicking")
        // task.Set("taskparameters", {"controllerip":"1.1.1.1", ...})
        // task.Execute();

        // GetScene

        // GetOrCreateTaskFromName



        //  TODO
        //  GetJointValues
        //  GetScene
        //  RestartServer
        //  MoveJoints
        //  MoveToHandPosition
        //  PickAndPlace
        //  MoveToSensorVisibility
        //  MoveToArea
        //  タスクを取得する。

   }

   
    public class WebResource
    {
        public ControllerClient controllerClient;
        public string resoureName;
        public string resoucePrimaryKey;


        public WebResource(ControllerClient controllerClient, string resoureName, string resoucePrimaryKey)
        {

        }

       // string Get(string fieldname);
        //void Set(string fieldname, string value);
    }



    public class SceneResource
    {
        public ControllerClient controllerClient;
        public string resoureName;
        public string resoucePrimaryKey;

        public SceneResource(ControllerClient controllerClient, string resoureName, string resoucePrimaryKey)
        {

        }
        
       
    }

    public class TaskResource
   {
       public ControllerClient controllerClient;
       public string resourceName;
       public string resourcePrimaryKey;

       public TaskResource(ControllerClient controllerClient, string resourceName, string resourcePrimaryKey)
       {
           this.controllerClient = controllerClient;
           this.resourcePrimaryKey = resourcePrimaryKey;
           this.resourceName = resourceName;
       }

       public string getPrimaryKey()
       {
           return resourcePrimaryKey;

       }


   }

    public class TaskParameters
    {


    }


       

}

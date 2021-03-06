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
using System.Xml;

namespace Mujin
{
    public class ControllerClient
    {
        private string username;
        private string password;
        private string baseUri;
        private string baseApiUri;

        private string csrftoken;

        public const string version = "0.1.0";
        private const string DEFAULT_URI = "http://controllerXX/";
        private const string DEFAULT_API_PATH = "api/v1/";


        public ControllerClient(string username, string password, string baseUri = null)
        {
            this.username = username;
            this.password = password;
            this.baseUri = (baseUri == null) ? DEFAULT_URI : this.EnsureLastSlash(baseUri);
            this.baseApiUri = this.baseUri + DEFAULT_API_PATH;
            this.csrftoken = "csrftoken";
        }

        public string GetCurrentSceneURI()
        {
            Dictionary<string, object> jsonMessage = this.GetJsonMessage(HttpMethod.GET, "/config/");
            return (string)jsonMessage["sceneuri"];
        }

        public SceneResource GetScene(string scenePrimaryKey)
        {
            string apiParameters = string.Format("scene/{0}/?format=json&fields=pk", scenePrimaryKey);
            Dictionary<string, object> jsonMessage = this.GetJsonMessage(HttpMethod.GET, apiParameters);
            return new SceneResource(scenePrimaryKey, this);
        }

        public Dictionary<string, object> GetJobs()
        {
            string apiParameters = string.Format("job/?limit=0");
            return this.GetJsonMessage(HttpMethod.GET, apiParameters);
        }

        public Dictionary<string, object> GetJob(string jobPK)
        {
            string apiParameters = string.Format("job/{0}/?limit=0", jobPK);
            return this.GetJsonMessage(HttpMethod.GET, apiParameters);
        }

        public void DeleteJob(string jobPK)
        {
            this.Delete(string.Format("job/{0}/", jobPK));
        }

        public void DeleteJobs()
        {
            this.Delete(string.Format("job/"));
        }

        public Dictionary<string, object> GetSceneTasks(string scenePrimaryKey)
        {
            string apiParameters = string.Format("scene/{0}/task/?format=json", scenePrimaryKey);
            Dictionary<string, object> jsonMessage = this.GetJsonMessage(HttpMethod.GET, apiParameters);
            return jsonMessage;
        }

        public Dictionary<string, object> GetSceneTask(string scenePrimaryKey, string taskPK)
        {
            string apiParameters = string.Format("scene/{0}/task/{1}/?format=json", scenePrimaryKey, taskPK);
            Dictionary<string, object> jsonMessage = this.GetJsonMessage(HttpMethod.GET, apiParameters);
            return jsonMessage;
        }
        public Dictionary<string, object> CreateSceneTask(string scenePrimaryKey, Dictionary<string, object> taskdata, Dictionary<string, string> fields = null, bool usewebapi = true)
        {
            string apiParams = "";
            string url = String.Format("scene/{0}/task/?format=json", scenePrimaryKey);
            if (fields != null)
            {
                apiParams = StringfyAPIParams(fields);
                url += String.Format("&fields={0}", apiParams);
            }

            Dictionary<string, object> response = GetJsonMessage(HttpMethod.POST, url, JSON.ToJSON(taskdata));
            return response;
        }

        public void DeleteSceneTask(string scenePrimaryKey, string taskPK)
        {
            string apiParameters = String.Format("scene/{0}/task/{1}/", scenePrimaryKey, taskPK);
            this.Delete(apiParameters);
        }

        public Dictionary<string, object> RunScenetaskAsync(string scenePrimaryKey, string taskpk, Dictionary<string, object> fields = null, bool usewebapi = true)
        {
            Dictionary<string, object> taskdata = new Dictionary<string, object>();
            taskdata.Add("scenepk", scenePrimaryKey);
            taskdata.Add("target_pk", taskpk);
            taskdata.Add("resource_type", "task");
            Dictionary<string, object> response = GetJsonMessage(HttpMethod.POST, "job/", JSON.ToJSON(taskdata));
            return response;
        }

        public Dictionary<string, object> GetJsonMessage(HttpMethod method, string apiParameters, string message = null)
        {
            switch (method)
            {
                case HttpMethod.GET:
                    {
                        if (!String.IsNullOrEmpty(message)) {
                            return this.GetMessagePostOrPut(apiParameters, message, HttpMethod.GET);
                            // throw new ClientException("Cannot add message body to GET method.");
                        }
                        return this.GetMessageGet(apiParameters);
                    }
                case HttpMethod.POST: return this.GetMessagePostOrPut(apiParameters, message, HttpMethod.POST);
                case HttpMethod.PUT: return this.GetMessagePostOrPut(apiParameters, message, HttpMethod.PUT);
                default: return null;
            }
        }

        public Dictionary<string, object> GetXMLMessage(HttpMethod method, string apiParameters, string message = null)
        {
            switch (method)
            {
                case HttpMethod.GET:
                    {
                        if (!String.IsNullOrEmpty(message)) throw new ClientException("Cannot add message body to GET method.");
                        return this.GetMessageGet(apiParameters, "application/xml; charset=UTF-8");
                    }
                case HttpMethod.POST:
                    {
                        return this.GetMessagePostOrPut(apiParameters, message, HttpMethod.POST, "application/xml; charset=UTF-8");
                    }
                case HttpMethod.PUT:
                    {
                        return this.GetMessagePostOrPut(apiParameters, message, HttpMethod.PUT, "application/xml; charset=UTF-8");
                    }
                default: return null;
            }
        }

        public void Initialize(string referenceUri, string sceneType, string referenceSceneType, string uri)
        {
            Dictionary<string, object> command = new Dictionary<string, object>();
            command["reference_uri"] = referenceUri;
            command["scenetype"] = sceneType;
            command["reference_scenetype"] = referenceSceneType;
            command["uri"] = uri;
            string messageBody = JSON.ToJSON(command);

            Dictionary<string, object> jsonMessage = this.GetJsonMessage(HttpMethod.POST, "scene/?format=json&fields=name,pk,uri&overwrite=1", messageBody);

            string primaryKey = (string)jsonMessage["pk"];
        }

        private string StringfyAPIParams(Dictionary<string, string> apiParams)
        {
            string res = "";
            foreach(KeyValuePair<string, string> entry in apiParams)
            {
                if(!String.IsNullOrEmpty(res))
                {
                    res += "&";
                }
                res += String.Format("{0}={1}", entry.Key, entry.Value);
            }
            return res;
        }
        private string EnsureLastSlash(string uri)
        {
            return uri.EndsWith("/") ? uri : uri += "/";
        }

        private string RemovePortNumberString()
        {
            return this.baseUri.Substring(0, this.baseUri.Length - 6);
        }

        private HttpWebRequest CreateWebRequest(string path, HttpMethod method)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(path);
            httpWebRequest.Method = method.ToString();
            httpWebRequest.UserAgent = String.Format("controllerclientcs/{0}", version);
            httpWebRequest.ContentType = "application/json; charset=UTF-8";
            httpWebRequest.Accept = "application/json";
            httpWebRequest.AllowAutoRedirect = false;

            this.AddAuthorizationHeader(httpWebRequest); // messes up logging
            this.AddCsrfTokenHeader(httpWebRequest);

            return httpWebRequest;
        }

        private void AddAuthorizationHeader(HttpWebRequest httpWebRequest)
        {
            string authInfo = username + ":" + password;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));

            httpWebRequest.PreAuthenticate = true;
            httpWebRequest.Headers.Add("Authorization", "Basic " + authInfo);
        }

        private void AddCsrfTokenHeader(HttpWebRequest httpWebRequest)
        {
            if (csrftoken != null)
            {
                httpWebRequest.Headers.Add("X-CSRFToken", csrftoken);
                httpWebRequest.Headers.Add("Cookie", String.Format("csrftoken={0}", csrftoken));
            }
        }

        private HttpWebResponse GetResponse(HttpWebRequest webRequest, List<HttpStatusCode> expectedStatusCodes)
        {
            HttpWebResponse response = null;

            try
            {
                response = (HttpWebResponse)webRequest.GetResponse();
            }
            catch (WebException ex)
            {
                if (ex.Status.Equals(WebExceptionStatus.ProtocolError) && ex.ToString().Contains("401"))
                {
                    throw new ClientException("401 error. Confirm username and password.");
                }
                if (ex.Status.Equals(WebExceptionStatus.NameResolutionFailure))
                {
                    throw new ClientException("Name resolution error. Confirm Mujin Controller server address.");
                }
                if (ex.Status.Equals(WebExceptionStatus.ConnectFailure))
                {
                    throw new ClientException("Cannot connect to server. Confirm Mujin Controller port number, etc.");
                }
                throw ex;
            }

            if (!expectedStatusCodes.Contains(response.StatusCode))
            {
                response.Close();
                string message = "Server returned error status (recognized as an error): " + response.StatusCode.ToString();
                throw new ClientException(message);
            }

            return response;
        }

        private Dictionary<string, object> GetMessageGet(string apiParameters, string contentType=null)
        {
            if (!apiParameters.StartsWith("/"))
            {
                apiParameters = baseApiUri + apiParameters;
            }
            else
            {
                apiParameters = baseUri + apiParameters.Substring(1);
            }
            HttpWebRequest request = CreateWebRequest(apiParameters, HttpMethod.GET);
            if (contentType != null)
            {
                request.ContentType = contentType;
            }
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                if (ex.Status.Equals(WebExceptionStatus.ProtocolError) && ex.ToString().Contains("500"))
                {
                    throw new ClientException("Mujin controller error. Possible error reason : (a) invalid scene primary key");
                }
                throw ex;
            }

            Dictionary<string, object> jsonMessage = null;
            try
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());
                try
                {
                    string responsestring = reader.ReadToEnd();
                    jsonMessage = (Dictionary<string, object>)JSON.Parse(responsestring);
                }
                finally
                {
                    reader.Close();
                }
            }
            finally
            {
                response.Close();
            }

            return jsonMessage;
        }

        private HttpStatusCode Delete(string apiParameters)
        {
            if (!apiParameters.StartsWith("/"))
            {
                apiParameters = baseApiUri + apiParameters;
            }
            else
            {
                apiParameters = baseUri + apiParameters.Substring(1);
            }
            HttpWebRequest deleteRequest = CreateWebRequest(apiParameters, HttpMethod.DELETE);
            HttpWebResponse response = (HttpWebResponse)deleteRequest.GetResponse();
            HttpStatusCode statusCode = response.StatusCode;
            response.Close();
            return statusCode;
        }

        private Dictionary<string, object> GetMessagePostOrPut(string apiParameters, string message, HttpMethod method, string contentType=null)
        {
            if (!apiParameters.StartsWith("/"))
            {
                apiParameters = baseApiUri + apiParameters;
            }
            else
            {
                apiParameters = baseUri + apiParameters.Substring(1);
            }
            HttpWebRequest postWebRequest = CreateWebRequest(apiParameters, method);
            if (contentType != null)
            {
                postWebRequest.ContentType = contentType;
            }
            StreamWriter writer = new StreamWriter(postWebRequest.GetRequestStream());
            writer.Write(message);
            writer.Close();

            Dictionary<string, object> messagedata;
            HttpWebResponse postWebResponse = (HttpWebResponse)postWebRequest.GetResponse();
            try
            {
                StreamReader reader = new StreamReader(postWebResponse.GetResponseStream());
                try
                {
                    string responsestring = reader.ReadToEnd();
                    if (contentType != null && contentType.IndexOf("xml") >= 0)
                    {
                        System.Xml.Linq.XDocument doc = System.Xml.Linq.XDocument.Parse(responsestring);
                        messagedata = new Dictionary<string, object>();

                        // TEMPORARY until can figure out how to make it consistent with JSON
                        foreach (System.Xml.Linq.XElement element in doc.Descendants().Where(p => p.HasElements == false))
                        {
                            int keyInt = 0;
                            string keyName = element.Name.LocalName;

                            while (messagedata.ContainsKey(keyName))
                            {
                                keyName = element.Name.LocalName + "_" + keyInt++;
                            }

                            messagedata.Add(keyName, element.Value);
                        }
                    }
                    else
                    {
                        messagedata = (Dictionary<string, object>)JSON.Parse(responsestring);
                    }

                }
                finally
                {
                    reader.Close();
                }
            }
            finally
            {
                postWebResponse.Close();
            }

            return messagedata;
        }
    }
}

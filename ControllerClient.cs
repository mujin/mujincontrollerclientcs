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
        private string baseWebDavUri;

        private CredentialCache credentials;
        private CookieContainer cookies;
        private string csrftoken;

        public const string version = "0.1.0";
        private const string DEFAULT_URI = "http://controllerXX/";
        private const string DEFAULT_API_PATH = "api/v1/";
        private const string DEFULAT_LOGIN_PATH = "login/";


        public ControllerClient(string username, string password, string baseUri = null)
        {
            this.username = username;
            this.password = password;
            this.baseUri = (baseUri == null) ? DEFAULT_URI : this.EnsureLastSlash(baseUri);
            this.baseApiUri = this.baseUri + DEFAULT_API_PATH;

            string tempuri = this.baseUri.EndsWith(":8000/") ? this.RemovePortNumberString() : this.baseUri;
            this.baseWebDavUri = string.Format("{0}u/{1}/", tempuri, username);

            this.credentials = new CredentialCache();
            this.credentials.Add(new Uri(this.baseUri), "Basic", new NetworkCredential(this.username, this.password));

            cookies = new CookieContainer();

            this.Login();
        }

        public string GetCurrentSceneURI()
        {
            Dictionary<string, object> jsonMessage = this.Get("config/");
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

        public Dictionary<string, object> GetSceneTasks(string scenePrimaryKey)
        {
            string apiParameters = string.Format("scene/{0}/task/?format=json", scenePrimaryKey);
            Dictionary<string, object> jsonMessage = this.GetJsonMessage(HttpMethod.GET, apiParameters);
            return jsonMessage;
        }

        public Dictionary<string, object> GetSceneTask(string scenePrimaryKey, string taskPK)
        {
            string apiParameters = string.Format("scene/{0}/task/{1}?format=json", scenePrimaryKey, taskPK);
            Dictionary<string, object> jsonMessage = this.GetJsonMessage(HttpMethod.GET, apiParameters);
            return jsonMessage;
        }
        public Dictionary<string, object> CreateSceneTask(string scenePrimaryKey, Dictionary<string, object> taskdata, Dictionary<string, string> fields = null, bool usewebapi = true)
        {
            string apiParams = "";
            string url = String.Format("scene/{0}/task/", scenePrimaryKey);
            if (fields != null)
            {
                apiParams = StringfyAPIParams(fields);
                url += String.Format("fields={0}", apiParams);
            }

            Dictionary<string, object> response = GetJsonMessage(HttpMethod.POST, url, JSON.Instance.ToJSON(taskdata));
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
            Dictionary<string, object> response = GetJsonMessage(HttpMethod.POST, "job/", JSON.Instance.ToJSON(taskdata));
            return response;
        }

        public Dictionary<string, object> Get(string path, Dictionary<string, object> headers=null)
        {
            HttpWebRequest request = CreateWebRequest(baseUri + path, HttpMethod.GET);
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
            }

            StreamReader reader = new StreamReader(response.GetResponseStream());
            string responsestring = reader.ReadToEnd();
            Dictionary<string, object> jsonMessage = (Dictionary<string, object>)JSON.Instance.Parse(responsestring);

            reader.Close();
            response.Close();

            return jsonMessage;
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
            string messageBody = JSON.Instance.ToJSON(command);

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
            httpWebRequest.Credentials = credentials;
            httpWebRequest.ContentType = "application/json; charset=UTF-8";
            httpWebRequest.CookieContainer = this.cookies;
            httpWebRequest.PreAuthenticate = true;
            httpWebRequest.UserAgent = "controllerclientcs";

            this.AddAuthorizationHeader(httpWebRequest); // messes up logging
            this.AddCsrfTokenHeader(httpWebRequest);

            return httpWebRequest;
        }

        private void AddAuthorizationHeader(HttpWebRequest httpWebRequest)
        {
            string authInfo = username + ":" + password;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            httpWebRequest.Headers["Authorization"] = "Basic " + authInfo;
        }

        private void AddCsrfTokenHeader(HttpWebRequest httpWebRequest)
        {
            if (csrftoken != null)
            {
                httpWebRequest.Headers.Add("X-CSRFToken", csrftoken);
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
                string message = "Server returned error status (recognized as an error): " + response.StatusCode.ToString();
                throw new ClientException(message);
            }

            return response;
        }

        private void Login()
        {
            this.TryLogin();
            this.SubmitLoginForm();
        }

        private void TryLogin()
        {
            HttpWebRequest webRequestAuth = CreateWebRequest(baseUri + DEFULAT_LOGIN_PATH, HttpMethod.GET);
            webRequestAuth.AllowAutoRedirect = false;
            HttpWebResponse responseAuth = this.GetResponse(webRequestAuth, new List<HttpStatusCode>() { HttpStatusCode.OK, HttpStatusCode.Redirect });

            Cookie item = responseAuth.Cookies["csrftoken"];
            if (item == null)
            {
                HttpWebRequest webRequestAuth2 = CreateWebRequest(baseApiUri, HttpMethod.GET);
                HttpWebResponse responseAuth2 = this.GetResponse(webRequestAuth2, new List<HttpStatusCode>() { HttpStatusCode.OK });

                item = responseAuth2.Cookies["csrftoken"];
                responseAuth2.Close();
            }

            if (item != null)
            {
                this.csrftoken = item.Value;
                //httpWebRequest.Referer = this.baseUri;
            }

            responseAuth.Close();
        }

        private void SubmitLoginForm()
        {
            HttpWebRequest webRequest = CreateWebRequest(baseUri + DEFULAT_LOGIN_PATH, HttpMethod.POST);
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Referer = this.baseUri + DEFULAT_LOGIN_PATH;
            webRequest.AllowAutoRedirect = false;

            if (this.csrftoken == null)
            {
                throw new ClientException("Need csrftoken for login");
            }
            
            StreamWriter streamWrite = new StreamWriter(webRequest.GetRequestStream());
            //streamWrite.Write(string.Format("username={0}&password={1}&this_is_the_login_form=1&next=%2F&csrfmiddlewaretoken={2}", username, password, this.csrftoken));
            streamWrite.Write(string.Format("username={0}&password={1}&this_is_the_login_form=1&next=%2F", username, password));
            streamWrite.Close();

            HttpWebResponse responseApi = this.GetResponse(webRequest, new List<HttpStatusCode>() { HttpStatusCode.Found, HttpStatusCode.OK });

            responseApi.Close();
        }

        private Dictionary<string, object> GetMessageGet(string apiParameters, string contentType=null)
        {
            HttpWebRequest request = CreateWebRequest(baseApiUri + apiParameters, HttpMethod.GET);
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

            }
            
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string responsestring = reader.ReadToEnd();
            Dictionary<string, object> jsonMessage = (Dictionary<string, object>)JSON.Instance.Parse(responsestring);

            reader.Close();
            response.Close();

            return jsonMessage;
        }

        private HttpStatusCode Delete(string apiParameters)
        {
            HttpWebRequest deleteRequest = CreateWebRequest(this.baseApiUri + apiParameters, HttpMethod.DELETE);
            HttpWebResponse response = (HttpWebResponse)deleteRequest.GetResponse();
            return response.StatusCode;
        }
        private Dictionary<string, object> GetMessagePostOrPut(string apiParameters, string message, HttpMethod method, string contentType=null)
        {
            HttpWebRequest postWebRequest = CreateWebRequest(baseApiUri + apiParameters, method);
            if (contentType != null)
            {
                postWebRequest.ContentType = contentType;
            }
            StreamWriter writer = new StreamWriter(postWebRequest.GetRequestStream());
            writer.Write(message);
            writer.Close();

            HttpWebResponse postWebResponse = (HttpWebResponse)postWebRequest.GetResponse();
            StreamReader reader = new StreamReader(postWebResponse.GetResponseStream());
            string responsestring = reader.ReadToEnd();
            Dictionary<string, object> messagedata;
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
                messagedata = (Dictionary<string, object>)JSON.Instance.Parse(responsestring);
            }

            reader.Close();
            postWebResponse.Close();

            return messagedata;
        }
    }
}

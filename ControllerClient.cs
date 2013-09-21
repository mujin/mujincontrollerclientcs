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
using System.Net;

namespace Mujin
{
    [Serializable()]
    public class ClientException : System.Exception
    {
        public ClientException() : base() { }
        public ClientException(string message) : base(message) { }
        public ClientException(string message, System.Exception inner) : base(message, inner) { }
        protected ClientException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
    }

    // This class should be replaced with fastJson.
    public class Command
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

        private const string DEFAULT_URI = "https://controller.mujin.co.jp/";
        private const string DEFAULT_API_PATH = "api/v1/";
        private const string DEFULAT_LOGIN_PATH = "login/";

        public enum HttpMethod{ GET, POST, PUT}

        public ControllerClient(string username, string password, string baseUri = null)
        {
            this.username = username;
            this.password = password;
            this.baseUri = (baseUri == null) ? DEFAULT_URI : this.EnsureLastSlash(baseUri);
            this.baseApiUri = this.baseUri + DEFAULT_API_PATH;

            string tempuri = this.baseUri.EndsWith(":8000/") ? this.baseUri.Substring(0, this.baseUri.Length - 6) : this.baseUri;
            baseWebDavUri = string.Format("{0}u/{1}/", tempuri, username);
        }

        private string EnsureLastSlash(string uri)
        {
            return uri.EndsWith("/") ? uri : uri += "/";
        }
        
        private HttpWebRequest CreateWebRequest(string path, HttpMethod method)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(path);
            httpWebRequest.Method = method.ToString();
            httpWebRequest.Credentials = credentials;

            string authInfo = username + ":" + password;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            httpWebRequest.Headers["Authorization"] = "Basic " + authInfo;

            httpWebRequest.ContentType = "application/json; charset=UTF-8";
            httpWebRequest.CookieContainer = cookies;
            httpWebRequest.PreAuthenticate = false;
            httpWebRequest.UserAgent = "controllerclientcs";

            if (csrftoken != null)
            {
                httpWebRequest.Headers.Add("X-CSRFToken", csrftoken);
                httpWebRequest.Referer = this.baseUri;
            }
            return httpWebRequest;
        }

        private HttpWebResponse GetResponse(HttpWebRequest webRequest, List<HttpStatusCode> expectedStatusCodes)
        {
            HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();

            if(!expectedStatusCodes.Contains(response.StatusCode)) 
            {
                string message = "Server returned error status: " + response.StatusCode.ToString();
                throw new ClientException(message);
            }

            return response;
        }

        public void Login()
        {
            cookies = new CookieContainer();

            credentials = new CredentialCache();
            //credentials.Add(new Uri(this.baseUri), "Basic", networkCredentials);
            credentials.Add(new Uri(this.baseUri), "Basic", new NetworkCredential(this.username, this.password));

       

            HttpWebRequest webRequestAuth   = CreateWebRequest(baseUri + DEFULAT_LOGIN_PATH, HttpMethod.GET);
            HttpWebResponse responseAuth    = this.GetResponse(webRequestAuth, new List<HttpStatusCode>(){HttpStatusCode.OK});

            HttpWebRequest webRequestApi    = CreateWebRequest(baseApiUri, HttpMethod.GET);
            HttpWebResponse responseApi     = this.GetResponse(webRequestApi, new List<HttpStatusCode>(){ HttpStatusCode.OK});

            Cookie item = responseApi.Cookies["csrftoken"];
            if (item != null) csrftoken = item.Value;

            //credentials = null;
            //cookies = null;
        }

        public List<String> GetScenePrimaryKeys()
        {
            Dictionary<string, object> jsonMessage = this.Get("scene/?format=json&fields=pk");
            List<object> scenes = (List<object>)jsonMessage["objects"];

            throw new NotImplementedException("This method is to be implemented.");
        }

        public SceneResource GetScene(string scenePrimaryKey)
        {
            string apiParameters = string.Format("scene/{0}/?format=json&fields=pk", scenePrimaryKey);
            Dictionary<string, object> jsonMessage = this.Get(apiParameters);

            if (!jsonMessage["pk"].Equals(scenePrimaryKey))
            {
                throw new ClientException("Scene primary key not found.");
            }

            return new SceneResource(scenePrimaryKey, this);
        }

        public Dictionary<string, object> Get(string apiParameters)
        {
            HttpWebRequest request      = CreateWebRequest(baseApiUri + apiParameters, HttpMethod.GET);
            HttpWebResponse response    = this.GetResponse(request, new List<HttpStatusCode>() { HttpStatusCode.OK });
            StreamReader reader         = new StreamReader(response.GetResponseStream());
            Dictionary<string, object> jsonMessage = (Dictionary<string, object>)JSON.Instance.Parse(reader.ReadToEnd());
            reader.Close();

            return jsonMessage;
        }

        public Dictionary<string, object> PostOrPut(string apiParameters, string body, HttpMethod method)
        {
            HttpWebRequest request = CreateWebRequest(baseApiUri + apiParameters, method);

            StreamWriter streamWriter = new StreamWriter(request.GetRequestStream());
            streamWriter.Write(body);
            streamWriter.Close();

            HttpWebResponse response = this.GetResponse(request, new List<HttpStatusCode>() { HttpStatusCode.OK });
            StreamReader reader = new StreamReader(response.GetResponseStream());
            Dictionary<string, object> jsonMessage = (Dictionary<string, object>)JSON.Instance.Parse(reader.ReadToEnd());
            reader.Close();

            return jsonMessage;
        }
    }
}

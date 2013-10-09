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

        private const string DEFAULT_URI = "http://mujin.co.jp:9007/";
        private const string DEFAULT_API_PATH = "api/v1/";
        private const string DEFULAT_LOGIN_PATH = "login/";

        public ControllerClient(string username, string password, string baseUri = null)
        {
            this.username   = username;
            this.password   = password;
            this.baseUri    = (baseUri == null) ? DEFAULT_URI : this.EnsureLastSlash(baseUri);
            this.baseApiUri = this.baseUri + DEFAULT_API_PATH;

            string tempuri  = this.baseUri.EndsWith(":8000/") ? this.RemovePortNumberString() : this.baseUri;
            baseWebDavUri   = string.Format("{0}u/{1}/", tempuri, username);

            credentials = new CredentialCache();
            credentials.Add(new Uri(this.baseUri), "Basic", new NetworkCredential(this.username, this.password));

            cookies = new CookieContainer();

            this.Login();
        }

        public SceneResource GetScene(string scenePrimaryKey)
        {
            string apiParameters = string.Format("scene/{0}/?format=json&fields=pk", scenePrimaryKey);
            Dictionary<string, object> jsonMessage = this.GetJsonMessage(HttpMethod.GET, apiParameters);
            return new SceneResource(scenePrimaryKey, this);
        }

        public Dictionary<string, object> GetJsonMessage(HttpMethod method, string apiParameters, string message = null)
        {
            switch (method)
            {
                case HttpMethod.GET:
                    {
                        if (!String.IsNullOrEmpty(message)) throw new ClientException("Cannot add message body to GET method.");
                        return this.GetJsonMessageGet(apiParameters);
                    }
                case HttpMethod.POST: return this.GetJsonMessagePostOrPut(apiParameters, message, HttpMethod.POST);
                case HttpMethod.PUT: return this.GetJsonMessagePostOrPut(apiParameters, message, HttpMethod.PUT);
                default: return null;
            }
        }

        public void Initialize(string referenceUri, string sceneType, string referenceSceneType, string uri)
        {
            Command command = new Command();
            command.Add("reference_uri", referenceUri);
            command.Add("scenetype", sceneType);
            command.Add("reference_scenetype", referenceSceneType);
            command.Add("uri", uri);
            string messageBody = command.GetString();

            Dictionary<string, object> jsonMessage = this.GetJsonMessage(HttpMethod.POST, 
                "scene/?format=json&fields=name,pk,uri&overwrite=1", messageBody);

            string primaryKey = (string)jsonMessage["pk"];
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
            HttpWebRequest httpWebRequest   = (HttpWebRequest)WebRequest.Create(path);
            httpWebRequest.Method           = method.ToString();
            httpWebRequest.Credentials      = credentials;
            httpWebRequest.ContentType      = "application/json; charset=UTF-8";
            httpWebRequest.CookieContainer  = cookies;
            httpWebRequest.PreAuthenticate  = true;
            httpWebRequest.UserAgent        = "controllerclientcs";

            this.AddAuthorizationHeader(httpWebRequest);
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
            HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();

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
            HttpWebResponse responseAuth = this.GetResponse(webRequestAuth, new List<HttpStatusCode>() { HttpStatusCode.OK });

            Cookie item = responseAuth.Cookies["csrftoken"];
            if (item != null) csrftoken = item.Value;

            responseAuth.Close();
        }

        private void SubmitLoginForm()
        {
            HttpWebRequest webRequest = CreateWebRequest(baseUri + DEFULAT_LOGIN_PATH, HttpMethod.POST);
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Referer = this.baseUri + DEFULAT_LOGIN_PATH;
            webRequest.AllowAutoRedirect = false;

            StreamWriter streamWrite = new StreamWriter(webRequest.GetRequestStream());
            streamWrite.Write(string.Format("username={0}&password={1}&this_is_the_login_form=1&next=%2F&csrfmiddlewaretoken={2}",
                username, password, csrftoken));
            streamWrite.Close();

            HttpWebResponse responseApi = this.GetResponse(webRequest, new List<HttpStatusCode>() { HttpStatusCode.Found });

            responseApi.Close();
        }

        private Dictionary<string, object> GetJsonMessageGet(string apiParameters)
        {
            HttpWebRequest request      = CreateWebRequest(baseApiUri + apiParameters, HttpMethod.GET);
            HttpWebResponse response    = (HttpWebResponse)request.GetResponse();
            StreamReader reader         = new StreamReader(response.GetResponseStream());
            string responsestring       = reader.ReadToEnd();
            Dictionary<string, object> jsonMessage = (Dictionary<string, object>)JSON.Instance.Parse(responsestring);

            reader.Close();
            response.Close();

            return jsonMessage;
        }

        private Dictionary<string, object> GetJsonMessagePostOrPut(string apiParameters, string message, HttpMethod method)
        {
            HttpWebRequest postWebRequest   = CreateWebRequest(baseApiUri + apiParameters, method);
            StreamWriter writer = new StreamWriter(postWebRequest.GetRequestStream());
            writer.Write(message);
            writer.Close();
            
            HttpWebResponse postWebResponse = (HttpWebResponse)postWebRequest.GetResponse();
            StreamReader reader             = new StreamReader(postWebResponse.GetResponseStream());
            string responsestring = reader.ReadToEnd();
            Dictionary<string, object> jsonMessage = (Dictionary<string, object>)JSON.Instance.Parse(responsestring);

            reader.Close();
            postWebResponse.Close();

            return jsonMessage;
        }
    }
}

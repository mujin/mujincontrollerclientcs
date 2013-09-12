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
   }
}

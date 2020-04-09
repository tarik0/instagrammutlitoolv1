using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Instagram_Class
{
    class InstagramClient
    {
        public static string IG_MAIN_PAGE = "https://www.instagram.com";
        public static string IG_LOGIN_PAGE = "https://www.instagram.com/accounts/login/";
        public static string IG_LOGIN_AJAX = "https://www.instagram.com/accounts/login/ajax/";
        public static string FACEBOOK_MAIN_PAGE = "https://www.facebook.com";
        public static string USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:75.0) Gecko/20100101 Firefox/75.0";

        public string _username;
        private string _password;
        public string _proxy;

        public bool _is_logged_in = false;
        private string _ig_ajax;
        private string _datr;

        private HttpClient _httpClient;
        private CookieContainer _cookieContainer;

        public InstagramClient(string username, string password, string proxy, int request_timeout)
        {
            this._username = username;
            this._password = password;
            this._proxy = proxy;

            // Check if inputs are good.
            if (this._username.IndexOf(" ") != -1 || this._username == " ")
            {
                throw new InvalidUsername(this._username);
            }
            if (this._password.IndexOf(" ") != -1 || this._password == " ")
            {
                throw new InvalidPassword(this._password);
            }
            if (this._proxy.IndexOf(" ") != -1 || this._proxy == " ")
            {
                throw new InvalidProxy(this._proxy);
            }
            if (!this._proxy.StartsWith("https://"))
            {
                throw new InvalidProxy(this._proxy);
            }

            // Init proxy, client and cookies
            this._cookieContainer = new CookieContainer();
            var webProxy = new WebProxy
            {
                Address = new Uri(this._proxy),
                UseDefaultCredentials = true,
            };
            var httpClientHandler = new HttpClientHandler
            {
                Proxy = webProxy,
                UseCookies = true,
                CookieContainer = this._cookieContainer
            };
            
            this._httpClient = new HttpClient(handler: httpClientHandler, disposeHandler: true);
            this._httpClient.Timeout = TimeSpan.FromSeconds(request_timeout);

            // Get datr

            this._httpClient.DefaultRequestHeaders.Clear();
            this._httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            this._httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US;q=0.5,en;q=0.3");
            this._httpClient.DefaultRequestHeaders.Add("Connection", "close");
            this._httpClient.DefaultRequestHeaders.Add("Host", "www.facebook.com");
            this._httpClient.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);

            Task<HttpResponseMessage> t1 = this._httpClient.GetAsync(FACEBOOK_MAIN_PAGE);
            HttpResponseMessage response = t1.Result;

            if (!response.IsSuccessStatusCode)
            {
                throw new ResponseIsNotOK("Status Code is " + response.StatusCode.ToString());
            }

            string body = this.GetTextFromResponse(response);

            if (body.IndexOf("[\"_js_datr\",\"") == -1)
            {
                throw new CookieNotFound("Datr");
            }

            this._datr = body.Split(new string[] { "[\"_js_datr\",\"" }, StringSplitOptions.None)[1]
                .Split(new string[] { "\"," }, StringSplitOptions.None)[0];
        }
        public InstagramClient(string username, string password, int request_timeout)
        {
            this._username = username;
            this._password = password;
            this._proxy = null;

            // Check if inputs are good.
            if (this._username.IndexOf(" ") != -1 || this._username == " ")
            {
                throw new InvalidUsername(this._username);
            }
            if (this._password.IndexOf(" ") != -1 || this._password == " ")
            {
                throw new InvalidPassword(this._password);
            }

            // Init client and cookies
            this._cookieContainer = new CookieContainer();
            var httpClientHandler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = this._cookieContainer
            };

            this._httpClient = new HttpClient(handler: httpClientHandler, disposeHandler: true);
            this._httpClient.Timeout = TimeSpan.FromSeconds(request_timeout);

            // Get datr
            this._httpClient.DefaultRequestHeaders.Clear();
            this._httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            this._httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US;q=0.5,en;q=0.3");
            this._httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
            this._httpClient.DefaultRequestHeaders.Add("DNT", "1");
            this._httpClient.DefaultRequestHeaders.Add("Host", "www.facebook.com");
            this._httpClient.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);

            Task<HttpResponseMessage> t1 = this._httpClient.GetAsync(FACEBOOK_MAIN_PAGE);
            HttpResponseMessage response = t1.Result;

            if (!response.IsSuccessStatusCode)
            {
                throw new ResponseIsNotOK("Status Code is " + response.StatusCode.ToString());
            }

            string body = this.GetTextFromResponse(response);

            if (body.IndexOf("[\"_js_datr\",\"") == -1)
            {
                throw new CookieNotFound("Datr");
            }

            this._datr = body.Split(new string[] { "[\"_js_datr\",\"" }, StringSplitOptions.None)[1]
                .Split(new string[] { "\"," }, StringSplitOptions.None)[0];
        }

        public List<Cookie> GetCookies(string url)
        {
            var uri = new Uri(url);
            return this._cookieContainer.GetCookies(uri).Cast<Cookie>().ToList();
        }
        public List<Cookie> GetCookies()
        {
            var uri = new Uri(IG_MAIN_PAGE);
            return this._cookieContainer.GetCookies(uri).Cast<Cookie>().ToList();
        }
        public Dictionary<string, string> GetCookiesAsDict(string url)
        {
            List<Cookie> cookies = GetCookies(url);
            Dictionary<string, string> dict = new Dictionary<string, string>();

            foreach (Cookie cookie in cookies)
            {
                dict.Add(cookie.Name, cookie.Value);
            }

            return dict;
        }
        public Dictionary<string, string> GetCookiesAsDict()
        {
            List<Cookie> cookies = GetCookies();
            Dictionary<string, string> dict = new Dictionary<string, string>();

            foreach (Cookie cookie in cookies)
            {
                dict.Add(cookie.Name, cookie.Value);
            }

            return dict;
        }
        public static string GetInstagramPostId(string mediaId)
        {
            string postId = "";
            try
            {
                long id = long.Parse(mediaId.Substring(0, mediaId.IndexOf('_')));
                string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";
                while (id > 0)
                {
                    long remainder = (id % 64);
                    id = (id - remainder) / 64;

                    postId = alphabet.ElementAt((int)remainder) + postId;
                }
            }
            catch (Exception e)
            {
                Console.Write(e.StackTrace);
            }

            return postId;
        }

        public string GetTextFromResponse(HttpResponseMessage response)
        {
            Task<byte[]> t1 = response.Content.ReadAsByteArrayAsync();
            t1.Wait();

            string response_text = Encoding.UTF8.GetString(t1.Result, 0, t1.Result.Length);

            return response_text;
        }

        public HttpResponseMessage GetLoginPage()
        {
            // Init headers
            this._httpClient.DefaultRequestHeaders.Clear();
            this._httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            this._httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US;q=0.5,en;q=0.3");
            this._httpClient.DefaultRequestHeaders.Add("Connection", "close");
            this._httpClient.DefaultRequestHeaders.Add("Host", "www.instagram.com");
            this._httpClient.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);

            Task<HttpResponseMessage> t1 = this._httpClient.GetAsync(IG_LOGIN_PAGE);
            t1.Wait();

            HttpResponseMessage response = t1.Result;

            if (!response.IsSuccessStatusCode)
            {
                throw new ResponseIsNotOK("Status Code is " + response.StatusCode.ToString());
            }

            // Update X-Instagram-AJAX
            string body = GetTextFromResponse(response);

            if (body.IndexOf("\"rollout_hash\":\"") == -1)
            {
                throw new CookieNotFound("Instagram AJAX");
            }

            this._ig_ajax = body.Split(new string[] { "\"rollout_hash\":\"" }, StringSplitOptions.None)[1]
                .Split(new string[] { "\"," }, StringSplitOptions.None)[0];

            return response;
        }
        public HttpResponseMessage GetInstagramPage(string url)
        {
            // Init headers
            this._httpClient.DefaultRequestHeaders.Clear();
            this._httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            this._httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US;q=0.5,en;q=0.3");
            this._httpClient.DefaultRequestHeaders.Add("Connection", "close");
            this._httpClient.DefaultRequestHeaders.Add("Host", "www.instagram.com");
            this._httpClient.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);
            this._httpClient.DefaultRequestHeaders.Add("X-CSRFToken", GetCookiesAsDict()["csrftoken"]);
            this._httpClient.DefaultRequestHeaders.Add("X-Instagram-AJAX", this._ig_ajax);
            this._httpClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            this._httpClient.DefaultRequestHeaders.Add("X-IG-WWW-Claim", "0");

            Task<HttpResponseMessage> t1 = this._httpClient.GetAsync(url);
            t1.Wait();

            HttpResponseMessage response = t1.Result;

            if (!response.IsSuccessStatusCode)
            {
                throw new ResponseIsNotOK("Status Code is " + response.StatusCode.ToString());
            }

            // Update X-Instagram-AJAX
            string body = GetTextFromResponse(response);

            if (body.IndexOf("\"rollout_hash\":\"") == -1)
            {
                throw new CookieNotFound("Instagram AJAX");
            }

            this._ig_ajax = body.Split(new string[] { "\"rollout_hash\":\"" }, StringSplitOptions.None)[1]
                .Split(new string[] { "\"," }, StringSplitOptions.None)[0];

            return response;
        }
        public HttpResponseMessage GetUserInfo(string username)
        {
            // Init headers
            this._httpClient.DefaultRequestHeaders.Clear();
            this._httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            this._httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US;q=0.5,en;q=0.3");
            this._httpClient.DefaultRequestHeaders.Add("Connection", "close");
            this._httpClient.DefaultRequestHeaders.Add("Host", "www.instagram.com");
            this._httpClient.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);

            Task<HttpResponseMessage> t1 = this._httpClient.GetAsync("https://www.instagram.com/" + username + "/?__a=1");
            t1.Wait();

            HttpResponseMessage response = t1.Result;

            if (!response.IsSuccessStatusCode)
            {
                throw new ResponseIsNotOK("Status Code is " + response.StatusCode.ToString());
            }

            return response;
        }

        public HttpResponseMessage Login()
        {
            // Check cookies and logged in status
            var t = this.GetCookiesAsDict();
            if (this.GetCookies().Count < 3)
            {
                throw new CookieNotFound();
            }
            if (this._is_logged_in != false)
            {
                throw new AlreadyLoggedIn(this._username);
            }

            // Init headers
            this._httpClient.DefaultRequestHeaders.Clear();
            this._httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
            this._httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US;q=0.5,en;q=0.3");
            this._httpClient.DefaultRequestHeaders.Add("Origin", IG_MAIN_PAGE);
            this._httpClient.DefaultRequestHeaders.Add("Referer", IG_LOGIN_PAGE);
            this._httpClient.DefaultRequestHeaders.Add("Connection", "close");
            this._httpClient.DefaultRequestHeaders.Add("Host", "www.instagram.com");
            this._httpClient.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);
            this._httpClient.DefaultRequestHeaders.Add("X-CSRFToken", GetCookiesAsDict()["csrftoken"]);
            this._httpClient.DefaultRequestHeaders.Add("X-Instagram-AJAX", this._ig_ajax);
            this._httpClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            this._httpClient.DefaultRequestHeaders.Add("X-IG-WWW-Claim", "0");

            // Form data
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", this._username),
                new KeyValuePair<string, string>("password", this._password),
                new KeyValuePair<string, string>("queryParams", "{}"),
                new KeyValuePair<string, string>("optIntoOneTap", "false")
            });

            Task<HttpResponseMessage> t1 = this._httpClient.PostAsync(IG_LOGIN_AJAX, content);
            t1.Wait();

            HttpResponseMessage response = t1.Result;

            if (!response.IsSuccessStatusCode)
            {
                throw new ResponseIsNotOK("Status Code is " + response.StatusCode.ToString());
            }
        
            // Check if login is success.
            string body = GetTextFromResponse(response);
            JObject json = JObject.Parse(body);

            if (json["authenticated"].Value<bool>() != true)
            {
                throw new LoginFailed();
            }

            this._is_logged_in = true;
            return response;
        }

        public HttpResponseMessage FollowById(string user_id)
        {
            if (this._is_logged_in == false)
            {
                throw new NotLoggedIn();
            }

            // Init headers
            this._httpClient.DefaultRequestHeaders.Clear();
            this._httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
            this._httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US;q=0.5,en;q=0.3");
            this._httpClient.DefaultRequestHeaders.Add("Origin", IG_MAIN_PAGE);
            this._httpClient.DefaultRequestHeaders.Add("Referer", IG_LOGIN_PAGE);
            this._httpClient.DefaultRequestHeaders.Add("Connection", "close");
            this._httpClient.DefaultRequestHeaders.Add("Host", "www.instagram.com");
            this._httpClient.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);
            this._httpClient.DefaultRequestHeaders.Add("X-CSRFToken", GetCookiesAsDict()["csrftoken"]);
            this._httpClient.DefaultRequestHeaders.Add("X-Instagram-AJAX", this._ig_ajax);
            this._httpClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            this._httpClient.DefaultRequestHeaders.Add("X-IG-WWW-Claim", "0");

            // Init datr
            if (!GetCookiesAsDict().ContainsKey("datr"))
            {
                this._cookieContainer.Add(new Uri(IG_MAIN_PAGE), new Cookie("datr", this._datr));
            }


            Task<HttpResponseMessage> t1 = this._httpClient.PostAsync("https://www.instagram.com/web/friendships/" + user_id + "/follow/", null);
            t1.Wait();

            HttpResponseMessage response = t1.Result;

            if (!response.IsSuccessStatusCode)
            {
                throw new ResponseIsNotOK("Status Code is " + response.StatusCode.ToString());
            }

            return response;
        }
        public HttpResponseMessage Follow(string username)
        {
            if (this._is_logged_in == false)
            {
                throw new NotLoggedIn();
            }

            // Get User Data
            GetInstagramPage("https://www.instagram.com/" + username + "/");
            JObject user_info = JObject.Parse(
                this.GetTextFromResponse(GetUserInfo(username))
                );

            string user_id = user_info["graphql"]["user"]["id"].Value<string>();

            // Init headers
            this._httpClient.DefaultRequestHeaders.Clear();
            this._httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
            this._httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US;q=0.5,en;q=0.3");
            this._httpClient.DefaultRequestHeaders.Add("Origin", IG_MAIN_PAGE);
            this._httpClient.DefaultRequestHeaders.Add("Referer", IG_LOGIN_PAGE);
            this._httpClient.DefaultRequestHeaders.Add("Connection", "close");
            this._httpClient.DefaultRequestHeaders.Add("Host", "www.instagram.com");
            this._httpClient.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);
            this._httpClient.DefaultRequestHeaders.Add("X-CSRFToken", GetCookiesAsDict()["csrftoken"]);
            this._httpClient.DefaultRequestHeaders.Add("X-Instagram-AJAX", this._ig_ajax);
            this._httpClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            this._httpClient.DefaultRequestHeaders.Add("X-IG-WWW-Claim", "0");

            // Init datr
            if (!GetCookiesAsDict().ContainsKey("datr"))
            {
                this._cookieContainer.Add(new Uri(IG_MAIN_PAGE), new Cookie("datr", this._datr));
            }


            Task<HttpResponseMessage> t1 = this._httpClient.PostAsync("https://www.instagram.com/web/friendships/" + user_id + "/follow/", null);
            t1.Wait();

            HttpResponseMessage response = t1.Result;

            if (!response.IsSuccessStatusCode)
            {
                throw new ResponseIsNotOK("Status Code is " + response.StatusCode.ToString());
            }

            return response;
        }
        public HttpResponseMessage Unfollow(string username)
        {
            if (this._is_logged_in == false)
            {
                throw new NotLoggedIn();
            }

            // Get User Data
            GetInstagramPage("https://www.instagram.com/" + username + "/");
            JObject user_info = JObject.Parse(
                this.GetTextFromResponse(GetUserInfo(username))
                );

            string user_id = user_info["graphql"]["user"]["id"].Value<string>();

            // Init headers
            this._httpClient.DefaultRequestHeaders.Clear();
            this._httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
            this._httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US;q=0.5,en;q=0.3");
            this._httpClient.DefaultRequestHeaders.Add("Origin", IG_MAIN_PAGE);
            this._httpClient.DefaultRequestHeaders.Add("Referer", IG_LOGIN_PAGE);
            this._httpClient.DefaultRequestHeaders.Add("Connection", "close");
            this._httpClient.DefaultRequestHeaders.Add("Host", "www.instagram.com");
            this._httpClient.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);
            this._httpClient.DefaultRequestHeaders.Add("X-CSRFToken", GetCookiesAsDict()["csrftoken"]);
            this._httpClient.DefaultRequestHeaders.Add("X-Instagram-AJAX", this._ig_ajax);
            this._httpClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            this._httpClient.DefaultRequestHeaders.Add("X-IG-WWW-Claim", "0");

            // Init datr
            if (!GetCookiesAsDict().ContainsKey("datr"))
            {
                this._cookieContainer.Add(new Uri(IG_MAIN_PAGE), new Cookie("datr", this._datr));
            }

            Task<HttpResponseMessage> t1 = this._httpClient.PostAsync("https://www.instagram.com/web/friendships/" + user_id + "/unfollow/", null);
            t1.Wait();

            HttpResponseMessage response = t1.Result;

            if (!response.IsSuccessStatusCode)
            {
                throw new ResponseIsNotOK("Status Code is " + response.StatusCode.ToString());
            }

            return response;
        }
        public HttpResponseMessage Like(string media_id)
        {
            if (this._is_logged_in == false)
            {
                throw new NotLoggedIn();
            }

            // Init headers
            this._httpClient.DefaultRequestHeaders.Clear();
            this._httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
            this._httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US;q=0.5,en;q=0.3");
            this._httpClient.DefaultRequestHeaders.Add("Origin", IG_MAIN_PAGE);
            this._httpClient.DefaultRequestHeaders.Add("Referer", IG_LOGIN_PAGE);
            this._httpClient.DefaultRequestHeaders.Add("Connection", "close");
            this._httpClient.DefaultRequestHeaders.Add("Host", "www.instagram.com");
            this._httpClient.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);
            this._httpClient.DefaultRequestHeaders.Add("X-CSRFToken", GetCookiesAsDict()["csrftoken"]);
            this._httpClient.DefaultRequestHeaders.Add("X-Instagram-AJAX", this._ig_ajax);
            this._httpClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            this._httpClient.DefaultRequestHeaders.Add("X-IG-WWW-Claim", "0");

            // Init datr
            if (!GetCookiesAsDict().ContainsKey("datr"))
            {
                this._cookieContainer.Add(new Uri(IG_MAIN_PAGE), new Cookie("datr", this._datr));
            }


            Task<HttpResponseMessage> t1 = this._httpClient.PostAsync("https://www.instagram.com/web/likes/" + media_id + "/like/", null);
            t1.Wait();

            HttpResponseMessage response = t1.Result;

            if (!response.IsSuccessStatusCode)
            {
                throw new ResponseIsNotOK("Status Code is " + response.StatusCode.ToString());
            }

            return response;
        }
        public HttpResponseMessage Unlike(string media_id)
        {
            if (this._is_logged_in == false)
            {
                throw new NotLoggedIn();
            }

            // Init headers
            this._httpClient.DefaultRequestHeaders.Clear();
            this._httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
            this._httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US;q=0.5,en;q=0.3");
            this._httpClient.DefaultRequestHeaders.Add("Origin", IG_MAIN_PAGE);
            this._httpClient.DefaultRequestHeaders.Add("Referer", IG_LOGIN_PAGE);
            this._httpClient.DefaultRequestHeaders.Add("Connection", "close");
            this._httpClient.DefaultRequestHeaders.Add("Host", "www.instagram.com");
            this._httpClient.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);
            this._httpClient.DefaultRequestHeaders.Add("X-CSRFToken", GetCookiesAsDict()["csrftoken"]);
            this._httpClient.DefaultRequestHeaders.Add("X-Instagram-AJAX", this._ig_ajax);
            this._httpClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            this._httpClient.DefaultRequestHeaders.Add("X-IG-WWW-Claim", "0");

            // Init datr
            if (!GetCookiesAsDict().ContainsKey("datr"))
            {
                this._cookieContainer.Add(new Uri(IG_MAIN_PAGE), new Cookie("datr", this._datr));
            }


            Task<HttpResponseMessage> t1 = this._httpClient.PostAsync("https://www.instagram.com/web/likes/" + media_id + "/unlike/", null);
            t1.Wait();

            HttpResponseMessage response = t1.Result;

            if (!response.IsSuccessStatusCode)
            {
                throw new ResponseIsNotOK("Status Code is " + response.StatusCode.ToString());
            }

            return response;
        }
        public HttpResponseMessage Comment(string media_id, string message)
        {
            if (this._is_logged_in == false)
            {
                throw new NotLoggedIn();
            }

            // Init headers
            this._httpClient.DefaultRequestHeaders.Clear();
            this._httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
            this._httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US;q=0.5,en;q=0.3");
            this._httpClient.DefaultRequestHeaders.Add("Origin", IG_MAIN_PAGE);
            this._httpClient.DefaultRequestHeaders.Add("Referer", IG_LOGIN_PAGE);
            this._httpClient.DefaultRequestHeaders.Add("Connection", "close");
            this._httpClient.DefaultRequestHeaders.Add("Host", "www.instagram.com");
            this._httpClient.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);
            this._httpClient.DefaultRequestHeaders.Add("X-CSRFToken", GetCookiesAsDict()["csrftoken"]);
            this._httpClient.DefaultRequestHeaders.Add("X-Instagram-AJAX", this._ig_ajax);
            this._httpClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            this._httpClient.DefaultRequestHeaders.Add("X-IG-WWW-Claim", "0");

            // Init datr
            if (!GetCookiesAsDict().ContainsKey("datr"))
            {
                this._cookieContainer.Add(new Uri(IG_MAIN_PAGE), new Cookie("datr", this._datr));
            }

            // Form data
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("comment_text", message),
                new KeyValuePair<string, string>("replied_to_comment_id", "")
            });

            Task<HttpResponseMessage> t1 = this._httpClient.PostAsync("https://www.instagram.com/web/comments/" + media_id + "/add/", content);
            t1.Wait();

            HttpResponseMessage response = t1.Result;

            if (!response.IsSuccessStatusCode)
            {
                throw new ResponseIsNotOK("Status Code is " + response.StatusCode.ToString());
            }

            return response;
        }
    }
}
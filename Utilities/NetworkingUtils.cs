using System;
using System.Collections;
using System.IO;
using System.Net;
using UnityEngine;

namespace Fractalscape
{
    public static class NetworkUtils
    {
        public static bool ProcessFinished;


        public static bool IsConnected(string hostedUrl = "http://www.google.com")
        {
            Debug.Log("Checking internet connection.");
            try
            {
                Debug.Log("Internet connection available.");
                var HtmlText = GetHtmlFromUri(hostedUrl);
                AppSession.IsConnectedToInternet = HtmlText != "";
                return true;
            }
            catch(IOException ex)
            {
                Debug.Log("No internet connection.");
                return false;
            }
        }

        private static string GetHtmlFromUri(string resource)
        {
            var html = string.Empty;
            var req = (HttpWebRequest)WebRequest.Create(resource);
            try
            {
                using (var resp = (HttpWebResponse)req.GetResponse())
                {
                    var isSuccess = (int)resp.StatusCode < 299 && (int)resp.StatusCode >= 200;
                    if (isSuccess)
                    {
                        using (var reader = new StreamReader(resp.GetResponseStream()))
                        {
                            //We are limiting the array to 80 so we don't have
                            //to parse the entire html document feel free to
                            //adjust (probably stay under 300)
                            var cs = new char[80];
                            reader.Read(cs, 0, cs.Length);
                            foreach(var ch in cs)
                            {
                                html +=ch;
                            }
                        }
                    }
                }
            }
            catch
            {
                return "";
            }
            return html;
        }
    }
}
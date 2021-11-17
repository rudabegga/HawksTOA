using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HawksTOA
{
    public enum HttpVerb
    {
        GET,
        POST,
        PUT,
        DELETE
    }
    class RestClient
    {
        public string EndPoint { get; set; }
        public HttpVerb Method { get; set; }
        public string ContentType { get; set; }
        public string PostData { get; set; }
        public string LastModified { get; set; }

        public RestClient()
        {
            EndPoint = "";
            Method = HttpVerb.GET;
            ContentType = "application/json";
            PostData = "";
            LastModified = "";
        }
        public RestClient(string endpoint)
        {
            EndPoint = endpoint;
            Method = HttpVerb.GET;
            ContentType = "application/json";
            PostData = "";
            LastModified = "";
        }
        public RestClient(string endpoint, string lastMofified)
        {
            EndPoint = endpoint;
            Method = HttpVerb.GET;
            ContentType = "application/json";
            PostData = "";
            LastModified = lastMofified;
        }

        public RestClient(string endpoint, HttpVerb method)
        {
            EndPoint = endpoint;
            Method = method;
            ContentType = "application/json";
            PostData = "";
            LastModified = "";
        }

        public RestClient(string endpoint, HttpVerb method, string postData)
        {
            EndPoint = endpoint;
            Method = method;
            ContentType = "application/json";
            PostData = postData;
            LastModified = "";
        }

        public RestClient(string endpoint, HttpVerb method, string contentType, string postData)
        {
            EndPoint = endpoint;
            Method = method;
            ContentType = contentType;
            PostData = postData;
            LastModified = "";
        }

        public string[] MakeRequest()
        {
            return MakeRequest("");
        }

        public string[] MakeRequest(string parameters)
        {
            string[] responseVal = new string[2];
            responseVal[0] = LastModified;
            responseVal[1] = "";

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(EndPoint + parameters);

                request.Method = Method.ToString();
                request.ContentLength = 0;
                request.ContentType = ContentType;
                request.Headers.Add("X-TOA-Key", "1c10e5d044624a1adfcc4611f2802d2bce19824ffe4c26241c0e391f0a3848fa");
                request.Headers.Add("X-Application-Origin", "HawksTOA");

                if (LastModified != string.Empty)
                {
                    DateTime dt = Convert.ToDateTime(LastModified);
                    request.IfModifiedSince = Convert.ToDateTime(LastModified);
                }
                if (!string.IsNullOrEmpty(PostData) && Method == HttpVerb.POST)
                {
                    var encoding = new UTF8Encoding();
                    var bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(PostData);
                    request.ContentLength = bytes.Length;

                    using (var writeStream = request.GetRequestStream())
                    {
                        writeStream.Write(bytes, 0, bytes.Length);
                    }
                }

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    try
                    {
                        var responseValue = string.Empty;

                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            if (response.StatusCode == HttpStatusCode.NotModified)
                            {
                                responseVal[0] = LastModified;
                                responseVal[1] = "";
                            }
                            else
                            {
                                var message = String.Format("Request failed. Received HTTP {0}", response.StatusCode);
                                throw new ApplicationException(message);
                            }
                        }

                        // grab the response
                        using (var responseStream = response.GetResponseStream())
                        {
                            if (responseStream != null)
                                using (var reader = new StreamReader(responseStream))
                                {
                                    responseValue = reader.ReadToEnd();
                                    responseVal[0] = response.LastModified.ToString();
                                    responseVal[1] = responseValue;
                                }
                        }

                        return responseVal;

                    }
                    catch (WebException we)
                    {
                        Console.WriteLine("WebException " + we.ToString());
                        return null;
                    }
                }
            }
            catch (WebException wex)
            {
                Console.WriteLine("WebException " + wex.ToString());
                return responseVal;
            }
        }


    } // class
}

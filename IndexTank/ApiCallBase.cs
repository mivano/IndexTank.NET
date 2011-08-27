using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using System.Runtime.Serialization.Formatters;
using System.IO;

namespace IndexTank
{
    public abstract class ApiCallBase
    {

        protected static string GET_METHOD = "GET";
        protected static string PUT_METHOD = "PUT";
        protected static string DELETE_METHOD = "DELETE";

        protected static string SEARCH_URL = "/search";
        protected static string DOCS_URL = "/docs";
        protected static string CATEGORIES_URL = "/docs/categories";
        protected static string VARIABLES_URL = "/docs/variables";
        protected static string PROMOTE_URL = "/promote";
        protected static string FUNCTIONS_URL = "/functions";

        protected static readonly string ISO8601_PARSER = "yyyy-MM-dd'T'HH:mm:ssz";

        protected static T CallApi<T>(string method, string urlstring, ParameterMap parameters, string privatePass)
        {
            return CallApi<T>(method, urlstring, parameters, (string)null, privatePass);
        }

        protected static T CallApi<T>(string method, string urlstring, string privatePass)
        {
            return CallApi<T>(method, urlstring, null, (string)null, privatePass);
        }

        protected static T CallApi<T>(string method, string urlstring, ParameterMap parameters, Dictionary<string, object> data, string privatePass)
        {
            return CallApi<T>(method, urlstring, parameters, data == null ? null : data.ToJsonString(), privatePass);
        }

        protected static T CallApi<T>(string method, string urlstring, ParameterMap parameters, List<Dictionary<string, object>> data, string privatePass)
        {
            return CallApi<T>(method, urlstring, parameters, data == null ? null : data.ToJsonString(), privatePass);
        }
       
        protected static T CallApi<T>(string method, string urlstring, ParameterMap parameters, string data, string privatePass)
        {

            if (parameters != null && !parameters.IsEmpty())
            {
                urlstring += "?" + ParametersToQuerystring(parameters);
            }
            Uri url = new Uri(urlstring);

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method;
            request.AllowAutoRedirect = false;
            request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(privatePass)));

            if (method == (PUT_METHOD) && data != null)
            {
                // write
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(data);
                }
            }

            try
            {


                var httpResponse = (HttpWebResponse)request.GetResponse();
                int responseCode = (int)httpResponse.StatusCode;

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var responseText = streamReader.ReadToEnd();

                    if (responseCode >= 400)
                    {
                        throw new HttpCodeException(responseCode, responseText);
                    }


                    if (responseCode != 200 && responseCode != 201)
                    {
                        throw new HttpCodeException(responseCode, responseText);
                    }

                    if (!string.IsNullOrEmpty(responseText))
                    {                    
                        return JsonConvert.DeserializeObject<T>(responseText);
                    }
                    return default(T);
                }


            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError && ex.Response != null)
                {
                    var resp = (HttpWebResponse)ex.Response;

                    using (var streamReader = new StreamReader(resp.GetResponseStream()))
                    {
                        var responseText = streamReader.ReadToEnd();
                        int responseCode = (int)resp.StatusCode;

                        if (responseCode >= 400)
                        {
                            throw new HttpCodeException(responseCode, responseText);
                        }

                        if (responseCode != 200 && responseCode != 201)
                        {
                            throw new HttpCodeException(responseCode, responseText);
                        }

                        if (!string.IsNullOrEmpty(responseText))
                        {
                            return JsonConvert.DeserializeObject<T>(responseText);
                        }
                        return default(T);
                    }
                }
                return default(T);
            }
        }

        protected static string ParametersToQuerystring(ParameterMap parameters)
        {
            var sb = new StringBuilder();
            foreach (var key in parameters.KeySet())
            {
                foreach (var value in parameters.Get(key))
                {
                    try
                    {
                        sb.Append(HttpUtility.UrlEncode(key));
                        sb.Append("=");
                        sb.Append(HttpUtility.UrlEncode(value));
                        sb.Append("&");
                    }
                    catch (Exception e)
                    {
                    }
                }
            }

            var querystring=  sb.ToString().TrimEnd('&');
            
            return querystring;
        }
    }

    public static class JsonExtension
    {


        public static string ToJsonString(this Dictionary<string, List<string>> data)
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);

            return json;
        }

        public static string ToJsonString(this Dictionary<string, object> data)
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);

            return json;
        }

        public static string ToJsonString(this List<Dictionary<string, object>> data)
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);

            return json;
        }
    }
}

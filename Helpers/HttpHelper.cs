using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;

namespace Helpers.Http
{
    public static class HttpHelper
    {
        private static string byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
        public static ResponseData Post(string Url, PostData Data, Encoding Encoding = null, string Referer = null, bool AllowAutoRedirect = true)
        {
            CookieContainer cc = new CookieContainer();
            return Post(Url, cc, Data, Encoding, Referer, AllowAutoRedirect);
        }

        public static ResponseData Post(string Url, CookieCollection Cookies, PostData Data, Encoding Encoding = null, string Referer = null, bool AllowAutoRedirect = true)
        {
            CookieContainer cc = Cookies.GetContainer();
            return Post(Url, cc, Data, Encoding, Referer, AllowAutoRedirect);
        }

        public static ResponseData Post(string Url, CookieContainer Cookies, PostData Data, Encoding Encoding = null, string Referer = null, bool AllowAutoRedirect = true)
        {
            Encoding = Encoding ?? Encoding.Default;
            //System.Net.ServicePointManager.Expect100Continue = false;

            ResponseData result = new ResponseData();

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(Url);
            request.KeepAlive = true;
            request.Referer = Referer;
            request.CookieContainer = Cookies;
            request.AllowAutoRedirect = AllowAutoRedirect;
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.17 (KHTML, like Gecko) Chrome/24.0.1312.52 Safari/537.17";// " Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)";
            request.ProtocolVersion = HttpVersion.Version11;//.Version10;
            request.ServicePoint.Expect100Continue = false;
            //request.Connection = "keep-alive";/*avito*/

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            //request.ProtocolVersion = new Version(1, 1);

            System.Net.ServicePointManager.ServerCertificateValidationCallback = (s, cert, chain, ssl) =>
            {
                result.Certificate = cert;
                result.Chain = chain;
                result.Ssl = ssl;
                return true;
            };

            if (Data.Certificate != null)
            {
                request.ClientCertificates.Add(Data.Certificate);
            }

            foreach (Parameter par in Data.Headers)
            {
                if (par.Name.ToLower() == "accept")
                {
                    request.Accept = par.Value;
                }
                else if (par.Name.ToLower() == "content-type")
                {
                    request.ContentType = par.Value;
                }
                else if (par.Name.ToLower() == "host")
                {
                    request.Host = par.Value;
                }
                else if (par.Name.ToLower() == "user-agent")
                {
                    request.UserAgent = par.Value;
                }
                else
                {
                    request.Headers.Set(par.Name, par.Value);
                }
            }

            /*\\*/
            //var sp = request.ServicePoint;
            //var prop = sp.GetType().GetProperty("HttpBehaviour", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            //prop.SetValue(sp, (byte)0, null);
            /*//*/

            byte[] postBytes = null;

            postBytes = Encoding.GetBytes(Data.GetUrlEncodedString());
            request.ContentLength = postBytes.Length;
            Stream newStream = request.GetRequestStream();
            newStream.Write(postBytes, 0, postBytes.Length);
            newStream.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            result.Cookies = HttpHelper.ReadCookies(response);
            result.Response = response;

            Stream responseStream = response.GetResponseStream();
            result.BinaryContent = StreamHelper.ReadAllBytes(responseStream);
            result.TextContent = Encoding.GetString(result.BinaryContent);
            if (result.TextContent.Substring(0, byteOrderMarkUtf8.Length) == byteOrderMarkUtf8)
            {
                result.TextContent = result.TextContent.Remove(0, byteOrderMarkUtf8.Length);
            }
            result.Encoding = Encoding;
            response.Close();
            System.Net.ServicePointManager.ServerCertificateValidationCallback = null;

            return result;
        }

        public static ResponseData MultipartPost(string Url, CookieCollection Cookies, PostData Data, Encoding Encoding = null, string Referer = null, bool AllowAutoRedirect = true)
        {
            CookieContainer cc = Cookies.GetContainer();
            return MultipartPost(Url, cc, Data, Encoding, Referer, AllowAutoRedirect);
        }

        public static ResponseData MultipartPost(string Url, CookieContainer Cookies, PostData Data, Encoding Encoding = null, string Referer = null, bool AllowAutoRedirect = true)
        {
            Encoding = Encoding ?? Encoding.Default;
            //System.Net.ServicePointManager.Expect100Continue = false;

            ResponseData result = new ResponseData();

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(Url);
            //request.KeepAlive = true;
            request.AllowAutoRedirect = AllowAutoRedirect;
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.17 (KHTML, like Gecko) Chrome/24.0.1312.52 Safari/537.17";
            request.CookieContainer = Cookies;
            request.ContentType = "multipart/form-data; boundary=" + PostData.boundary;
            request.ProtocolVersion = HttpVersion.Version11;
            request.ServicePoint.Expect100Continue = false;


            //request.Connection = "keep-alive";/*test*/
            //request.Accept = "text/html, application/xhtml+xml, */*";
            request.Referer = Referer;

            request.Headers.Add(HttpRequestHeader.ContentEncoding, Encoding.HeaderName);

            request.Method = "POST";

            foreach (Parameter par in Data.Headers)
            {
                request.Headers.Set(par.Name, par.Value);
            }

            Stream requestStream = null;

            byte[] buffer = Data.GetMultipartFormData(Encoding);// Encoding.GetBytes(Data.GetPostData());

            request.ContentLength = buffer.Length;

            requestStream = request.GetRequestStream();
            requestStream.Write(buffer, 0, buffer.Length);
            requestStream.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            result.Response = response;
            result.Cookies = HttpHelper.ReadCookies(response);

            Stream responseStream = response.GetResponseStream();
            result.BinaryContent = StreamHelper.ReadAllBytes(responseStream);
            result.TextContent = Encoding.GetString(result.BinaryContent);
            result.Encoding = Encoding;
            response.Close();

            return result;
        }

        public static ResponseData PostFile(string Url, string File, CookieCollection Cookies, Encoding Encoding = null)
        {
            Encoding = Encoding ?? Encoding.Default;
            ResponseData result = new ResponseData();
            var request = (HttpWebRequest)WebRequest.Create(Url);
            request.CookieContainer = Cookies != null ? Cookies.GetContainer() : null;
            request.Method = "POST";
            request.ContentType = "application/octet-stream"; // binary data: 
            request.Headers.Add("X-File-Name", System.IO.Path.GetFileName(File));

            // data (bytes) that will be posted in body of request
            Stream requestStream = request.GetRequestStream();
            Stream f = System.IO.File.OpenRead(File);
            f.CopyTo(requestStream);
            f.Dispose();
            requestStream.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            result.Cookies = HttpHelper.ReadCookies(response);

            Stream responseStream = response.GetResponseStream();
            result.BinaryContent = StreamHelper.ReadAllBytes(responseStream);
            result.TextContent = Encoding.GetString(result.BinaryContent);
            result.Encoding = Encoding;
            response.Close();

            return result;
        }

        public static ResponseData JsonPost(string Url, CookieContainer Cookies, PostData Data, Encoding Encoding = null, string Referer = null)
        {
            Encoding = Encoding ?? Encoding.Default;

            ResponseData result = new ResponseData();

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(Url);

            //request.AllowAutoRedirect = AllowAutoRedirect;
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.17 (KHTML, like Gecko) Chrome/24.0.1312.52 Safari/537.17";
            request.CookieContainer = Cookies ?? new CookieContainer();
            request.ContentType = "application/json";
            request.ProtocolVersion = HttpVersion.Version11;
            request.ServicePoint.Expect100Continue = false;

            request.Referer = Referer;

            request.Headers.Add(HttpRequestHeader.ContentEncoding, Encoding.HeaderName);

            request.Method = "POST";

            foreach (Parameter par in Data.Headers)
            {
                request.Headers.Set(par.Name, par.Value);
            }

            Stream requestStream = null;

            byte[] buffer = Encoding.GetBytes(Data.GetPostData(PostDataType.Json));

            request.ContentLength = buffer.Length;

            requestStream = request.GetRequestStream();
            requestStream.Write(buffer, 0, buffer.Length);
            requestStream.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            result.Cookies = HttpHelper.ReadCookies(response);

            Stream responseStream = response.GetResponseStream();
            result.BinaryContent = StreamHelper.ReadAllBytes(responseStream);
            result.TextContent = Encoding.GetString(result.BinaryContent);
            result.Encoding = Encoding;
            response.Close();

            return result;
        }

        public static ResponseData Get(string Url)
        {
            CookieContainer cc = new CookieContainer();

            return Get(Url, cc);
        }

        public static ResponseData Get(string Url, Encoding Encoding)
        {
            CookieContainer cc = new CookieContainer();

            return Get(Url, cc, Encoding);
        }

        public static ResponseData Get(string Url, CookieCollection Cookies, Encoding Encoding = null, string Referer = null, bool AllowAutoRedirect = true)
        {
            CookieContainer cc = Cookies.GetContainer();

            return Get(Url, cc, Encoding, Referer, AllowAutoRedirect);
        }

        public static ResponseData Get(string Url, CookieContainer Cookies, Encoding Encoding = null, string Referer = null, bool AllowAutoRedirect = true)
        {
            Encoding = Encoding ?? Encoding.Default;
            System.Net.ServicePointManager.Expect100Continue = false;

            ResponseData result = new ResponseData();

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(Url);
            request.KeepAlive = true;
            request.Method = "GET";
            request.Referer = Referer;
            request.CookieContainer = Cookies;
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.17 (KHTML, like Gecko) Chrome/24.0.1312.52 Safari/537.17";
            request.AllowAutoRedirect = AllowAutoRedirect;
            request.Headers.Add("Accept-Encoding", "identity");
            //request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

            System.Net.ServicePointManager.ServerCertificateValidationCallback = (s, cert, chain, ssl) =>
            {
                result.Certificate = cert;
                result.Chain = chain;
                result.Ssl = ssl;
                return true;
            };

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            result.Cookies = HttpHelper.ReadCookies(response);
            result.Response = response;

            Stream responseStream = response.GetResponseStream();

            if (response.ContentEncoding.ToLower().Contains("gzip"))
            {
                responseStream = new System.IO.Compression.GZipStream(responseStream, System.IO.Compression.CompressionMode.Decompress);
            }
            else if (response.ContentEncoding.ToLower().Contains("deflate"))
            {
                responseStream = new System.IO.Compression.DeflateStream(responseStream, System.IO.Compression.CompressionMode.Decompress);
            }

            result.BinaryContent = StreamHelper.ReadAllBytes(responseStream);
            result.TextContent = Encoding.GetString(result.BinaryContent);
            result.Encoding = Encoding;
            response.Close();

            System.Net.ServicePointManager.ServerCertificateValidationCallback = null;

            return result;
        }

        public static CookieCollection ReadCookies(HttpWebResponse response)
        {
            string[] cookies = new string[0];
            CookieCollection cookiesCollection = new CookieCollection();
            if (response.Headers["Set-Cookie"] != null)
            {
                cookies = response.Headers["Set-Cookie"].ReplaceRegex(" expires=.*? .*?(;|(?=,))", "").Split(',');
            }

            foreach (string strCook in cookies)
            {
                string[] cookie1 = strCook.Split(';');
                string name, value, path, domain;
                if (cookie1.Length < 2 || !cookie1[0].Contains("="))
                {
                    continue;
                }

                name = cookie1[0].Split(new char[] { '=' }, 2)[0].Trim();
                value = cookie1[0].Split(new char[] { '=' }, 2)[1];
                path = cookie1[1].Split(new char[] { '=' }, 2)[0].Trim() == "path" ? cookie1[1].Split(new char[] { '=' }, 2)[1] : "";
                domain = cookie1.Length > 2 && cookie1[2].Split(new char[] { '=' }, 2)[0].Trim() == "domain" ? cookie1[2].Split(new char[] { '=' }, 2)[1] : "";

                //if (string.IsNullOrEmpty(domain))
                //{
                //    continue;
                //}

                Cookie cook = new Cookie(name, value, path, domain);
                cookiesCollection.Add(cook);
            }
            return cookiesCollection;
        }

        public static CookieContainer AddCookies(this CookieContainer Container, CookieCollection Cookies, string Domain = "")
        {
            foreach (Cookie cook in Cookies)
            {
                Container.Add(new Cookie(cook.Name, cook.Value, cook.Path, cook.Domain.IsNullOrEmpty() ? Domain : cook.Domain));
            }
            return Container;
        }

        public static CookieCollection AddCookies(this CookieCollection Collection, CookieCollection Cookies, string Domain = "")
        {
            foreach (Cookie cook in Cookies)
            {
                Collection.Add(new Cookie(cook.Name, cook.Value, cook.Path, cook.Domain.IsNullOrEmpty() ? Domain : cook.Domain));
            }
            return Collection;
        }

        public static CookieCollection SetCookies(this CookieCollection Collection, CookieCollection Cookies, string Domain = "")
        {
            foreach (Cookie cook in Cookies)
            {
                Collection.SetCookie(cook, Domain);
            }
            return Collection;
        }

        public static Cookie SetCookie(this CookieCollection Collection, String Name, String Value, String Path = "/", String Domain = "")
        {
            Cookie cook = new Cookie(Name, Value, Path, Domain);
            Collection.SetCookie(cook, Domain);
            return cook;
        }

        public static Cookie SetCookie(this CookieCollection Collection, Cookie Cookie, string Domain = "")
        {
            Cookie origin = Collection[Cookie.Name];
            if (Cookie.Value.ToLower() == "deleted" && origin == null)
            {
                return origin;
            }

            if (origin == null)
            {
                origin = new Cookie(Cookie.Name, Cookie.Value, Cookie.Path, Cookie.Domain.IsNullOrEmpty() ? Domain : Cookie.Domain);
                Collection.Add(origin);
            }
            else
            {
                origin.Path = Cookie.Path;
                origin.Comment = Cookie.Comment;
                origin.CommentUri = Cookie.CommentUri;
                origin.Discard = Cookie.Discard;
                origin.Domain = Cookie.Domain.IsNullOrEmpty() ? Domain : Cookie.Domain;
                origin.Expired = Cookie.Expired;
                origin.Expires = Cookie.Expires;
                origin.HttpOnly = Cookie.HttpOnly;
                origin.Port = Cookie.Port;
                origin.Secure = Cookie.Secure;
                origin.Value = Cookie.Value;
                origin.Version = Cookie.Version;
            }
            return origin;
        }

        public static CookieContainer GetContainer(this CookieCollection Collection)
        {
            CookieContainer cc = new CookieContainer();
            foreach (Cookie cook in Collection)
            {
                if (cook.Value.ToLower() != "deleted" && !cook.Expired)
                {
                    cc.Add(cook);
                }
            }
            return cc;
        }

    }

    public class PostData : IDictionary<string, string>
    {
        // Change this if you need to, not necessary
        public static string boundary = "AaB03x";

        public List<PostDataParam> Params { get; set; }
        public List<Parameter> Headers { get; set; }

        public X509Certificate Certificate { get; set; }

        public X509Chain Chain { get; set; }

        public SslPolicyErrors Ssl { get; set; }

        public Encoding Encoding { get; set; }

        public PostData()
        {
            Params = new List<PostDataParam>();
            Headers = new List<Parameter>();
            Encoding = Encoding.Default;
        }

        public PostData(Encoding Encoding)
            : this()
        {
            this.Encoding = Encoding;
        }

        public PostData(PostData data)
            : this(data.Encoding)
        {
            foreach (PostDataParam p in data.Params)
            {
                this.Set(p.Name, p.Value);
            }
        }

        public void AddHeader(string Name, string Value)
        {
            Parameter parameter = this.Headers.FirstOrDefault(val => val.Name == Name);
            if (parameter != null)
            {
                parameter.Value = Value;
            }
            else
            {
                Headers.Add(new Parameter(Name, Value));
            }
        }

        public PostDataParam Add(string Name, string Value)
        {
            return Set(Name, Value);
            //PostDataParam parameter = new PostDataParam(Name, Value);
            //this.Params.Add(parameter);
            //return parameter;
        }

        public PostDataParam Add(string Name, object Value)
        {
            return this.Add(Name, Value.StringAndTrim());
        }

        public void Remove(string Name)
        {
            PostDataParam parameter = this.Params.FirstOrDefault(val => val.Name == Name);
            if (parameter != null)
            {
                this.Params.Remove(parameter);
            }
        }

        public PostDataParam Set(string Name, string Value)
        {
            PostDataParam parameter = this.Params.FirstOrDefault(val => val.Name == Name);
            if (parameter == null)
            {
                parameter = new PostDataParam(Name, Value);
                this.Params.Add(parameter);
            }
            parameter.Value = Value;
            return parameter;
        }

        public PostDataParam Get(string Name, bool IgnoreCase = true)
        {
            PostDataParam parameter = this.Params.FirstOrDefault(val => (IgnoreCase ? val.Name.ToLower() == Name.ToLower() : val.Name == Name));
            return parameter ?? new PostDataParam(Name, "");
        }

        public PostDataParam AddFile(string Name, string FileName, Encoding Encoding = null)
        {
            Encoding = Encoding ?? this.Encoding;

            FileInfo fi = new FileInfo(FileName);

            PostDataParam parameter = new PostDataParam(Name, fi.Name, File.ReadAllBytes(FileName));
            parameter.Encoding = Encoding;
            parameter.Value = Encoding.GetString(parameter.Bytes);
            this.Params.Add(parameter);
            return parameter;
        }

        /// <summary>
        /// Returns the parameters array formatted for multi-part/form data
        /// </summary>
        /// <returns></returns>
        public string GetPostData(PostDataType DataType = PostDataType.Multipart)
        {
            if (DataType == PostDataType.UrlEncoded)
            {
                return this.GetUrlEncodedString();
            }
            else if (DataType == PostDataType.Json)
            {
                return this.Get("json").Value.IsNullOrEmpty() ? this.Get("body").Value : this.Get("json").Value;
            }

            StringBuilder sb = new StringBuilder();
            foreach (PostDataParam p in Params)
            {
                sb.AppendLine("--" + boundary);

                if (p.Type == PostDataParamType.File)
                {
                    string type = System.Net.Mime.MediaTypeNames.Application.Octet;

                    if (p.FileName.RegexHasMatches("\\.jp[e]?g$"))
                    {
                        type = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                    }
                    else if (p.FileName.RegexHasMatches("\\.png$"))
                    {
                        type = "image/png";
                    }

                    sb.AppendLine(string.Format("Content-Disposition: file; name=\"{0}\"; filename=\"{1}\"", p.Name, p.FileName));
                    sb.AppendLine("Content-Type: " + type);
                    sb.AppendLine();
                    sb.AppendLine(p.Value);
                }
                else
                {
                    sb.AppendLine(string.Format("Content-Disposition: form-data; name=\"{0}\"", p.Name));
                    sb.AppendLine();
                    sb.AppendLine(p.Value);
                }
            }

            sb.AppendLine("--" + boundary + "--");

            return sb.ToString();
        }

        public byte[] GetMultipartFormData(Encoding Encoding = null)
        {
            Stream formDataStream = new System.IO.MemoryStream();
            bool needsCLRF = false;
            Encoding = Encoding ?? this.Encoding;

            foreach (PostDataParam param in this.Params)
            {
                Encoding encoding = param.Encoding ?? Encoding;

                // Thanks to feedback from commenters, add a CRLF to allow multiple parameters to be added.
                // Skip it on the first parameter, add it to subsequent parameters.
                if (needsCLRF)
                    formDataStream.Write(encoding.GetBytes("\r\n"), 0, encoding.GetByteCount("\r\n"));

                needsCLRF = true;

                if (param.Type == PostDataParamType.File)
                {
                    string type = System.Net.Mime.MediaTypeNames.Application.Octet;

                    if (param.FileName.RegexHasMatches("\\.jp[e]?g$"))
                    {
                        type = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                    }
                    else if (param.FileName.RegexHasMatches("\\.png$"))
                    {
                        type = "image/png";
                    }

                    string header = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\";\r\nContent-Type: {3}\r\n\r\n", boundary, param.Name, param.FileName, type);

                    formDataStream.Write(encoding.GetBytes(header), 0, encoding.GetByteCount(header));

                    formDataStream.Write(param.Bytes, 0, param.Bytes.Length);
                }
                else
                {
                    string postData = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}", boundary, param.Name, param.Value);
                    formDataStream.Write(encoding.GetBytes(postData), 0, encoding.GetByteCount(postData));
                }
            }

            // Add the end of the request.  Start with a newline
            string footer = "\r\n--" + boundary + "--\r\n";
            formDataStream.Write(Encoding.GetBytes(footer), 0, Encoding.GetByteCount(footer));

            // Dump the Stream into a byte[]
            formDataStream.Position = 0;
            byte[] formData = new byte[formDataStream.Length];
            formDataStream.Read(formData, 0, formData.Length);
            formDataStream.Close();

            return formData;
        }

        public string GetUrlEncodedString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (PostDataParam p in Params)
            {
                if (p.Type == PostDataParamType.File)
                {
                    continue;
                }
                else
                {
                    sb.Append(HttpUtility.UrlEncode(p.Name));
                    sb.Append("=");
                    sb.Append(HttpUtility.UrlEncode(p.Value));
                    sb.Append("&");
                }
            }

            return sb.ToString().Trim('&');
        }

        public void AddBasicAuth(string login, string password)
        {
            string credentials = Convert.ToBase64String(Encoding.GetBytes(login + ":" + password));
            AddHeader(HttpRequestHeader.Authorization.ToString(), string.Format("Basic {0}", credentials));
        }

        #region IDictionary
        void IDictionary<string, string>.Add(string key, string value)
        {
            this.Add(key, value);
        }

        bool IDictionary<string, string>.ContainsKey(string key)
        {
            return this.Params.Any(val => val.Name.StringAndTrim().ToLower() == key.StringAndTrim().ToLower());
        }

        ICollection<string> IDictionary<string, string>.Keys
        {
            get
            {
                return this.Params.Select(val => val.Name).ToList();
            }
        }

        bool IDictionary<string, string>.Remove(string key)
        {
            this.Remove(key);
            return true;
        }

        bool IDictionary<string, string>.TryGetValue(string key, out string value)
        {
            value = "";
            var p = this.Params.FirstOrDefault(val => val.Name.StringAndTrim().ToLower() == key.StringAndTrim().ToLower());
            if (p == null)
            {
                return false;
            }
            value = p.Value;
            return true;
        }

        ICollection<string> IDictionary<string, string>.Values
        {
            get
            {
                return this.Params.Select(val => val.Value).ToList();
            }
        }

        string IDictionary<string, string>.this[string key]
        {
            get
            {
                return this.Get(key).Value;
            }
            set
            {
                this.Set(key, value);
            }
        }

        void ICollection<KeyValuePair<string, string>>.Add(KeyValuePair<string, string> item)
        {
            this.Add(item.Key, item.Value);
        }

        void ICollection<KeyValuePair<string, string>>.Clear()
        {
            this.Params.Clear();
        }

        bool ICollection<KeyValuePair<string, string>>.Contains(KeyValuePair<string, string> item)
        {
            return this.Params.Any(val => val.Name.StringAndTrim().ToLower() == item.Key.StringAndTrim().ToLower() && val.Value == item.Value);
        }

        void ICollection<KeyValuePair<string, string>>.CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        int ICollection<KeyValuePair<string, string>>.Count
        {
            get { return this.Params.Count; }
        }

        bool ICollection<KeyValuePair<string, string>>.IsReadOnly
        {
            get { return false; }
        }

        bool ICollection<KeyValuePair<string, string>>.Remove(KeyValuePair<string, string> item)
        {
            throw new NotImplementedException();
        }

        IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator()
        {
            return this.Params.Select(val => new KeyValuePair<string, string>(val.Name, val.Value)).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.Params.Select(val => new KeyValuePair<string, string>(val.Name, val.Value)).GetEnumerator();
        }
        #endregion
    }

    public enum PostDataParamType
    {
        Field,
        File
    }

    public enum PostDataType
    {
        Multipart,
        UrlEncoded,
        Json
    }

    public class Parameter
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public Parameter() { }
        public Parameter(string Name, string Value)
        {
            this.Name = Name;
            this.Value = Value;
        }

        public override string ToString()
        {
            string value = this.Value;
            value = value.Length > 200 ? value.Substring(0, 200) + "..." : value;
            return string.Format("{0} = {1}", this.Name, value);
        }
    }

    public class PostDataParam : Parameter
    {
        public PostDataParam(string name, string value)
            : base(name, value)
        {
            Type = PostDataParamType.Field;
        }

        public PostDataParam(string name, string filename, string value)
            : base(name, value)
        {
            FileName = filename;
            Type = PostDataParamType.File;
        }

        public PostDataParam(string name, string filename, byte[] content)
            : base(name, "")
        {
            FileName = filename;
            Type = PostDataParamType.File;
            Bytes = content;
        }

        public string FileName { get; set; }
        public PostDataParamType Type { get; set; }

        public override string ToString()
        {
            string value = this.Type == PostDataParamType.Field ? this.Value : this.FileName;
            value = value.Length > 200 ? value.Substring(0, 200) + "..." : value;
            return string.Format("{0} = {1}", this.Name, value);
        }

        public Encoding Encoding { get; set; }
        public Byte[] Bytes { get; set; }
    }

    public class ResponseData
    {
        public string TextContent { get; set; }
        public byte[] BinaryContent { get; set; }
        public CookieCollection Cookies { get; set; }

        public Encoding Encoding { get; set; }

        public X509Certificate Certificate { get; set; }

        public X509Chain Chain { get; set; }

        public SslPolicyErrors Ssl { get; set; }

        public HttpWebResponse Response { get; set; }
    }

}

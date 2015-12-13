using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Helpers
{
    public abstract class Log
    {
        public static string Path = "/Log/";
        private static object Locker = new object();

        public void Fatal(string Text)
        {
            this.Message("--{0}: Fatal error in application--", Text);
        }

        public void Message(string Text)
        {
            this.Message("--{0}: Message in application--", Text);
        }

        public void Warning(string Text)
        {
            this.Message("--{0}: Warning in application--", Text);
        }

        public void Info(string Text)
        {
            this.Message("--{0}: Information in application--", Text);
        }

        public void Error(string Text)
        {
            this.Message("--{0}: Error in application--", Text);
        }

        public void Error(Exception Ex)
        {
            this.Message("--{0}: Error in application--", Ex.GetExceptionTextAndStackTrace(),
                tw =>
                {
                    Ex.Data.Keys.OfType<object>().ToList().ForEach(key =>
                    {
                        tw.WriteLine(string.Format("ExData: {0} = {1}", key, Ex.Data[key]));
                    });
                },
                fi =>
                {
                    SendEmail(Ex, fi);
                    WriteToDb(Ex, fi);
                });
        }

        public void Message(string TitleTemplate, string Text, Action<TextWriter> Additional = null, Action<FileInfo> Finish = null)
        {
            lock (Locker)
            {
                FileInfo fi = GetFilePath();
                TextWriter tw = new StreamWriter(fi.FullName, true);

                tw.WriteLine(string.Format(TitleTemplate, DateTime.Now));

                try
                {
                    System.Web.HttpContext context = System.Web.HttpContext.Current;
                    if (context != null)
                    {
                        System.Web.HttpRequest request = context.Request;
                        System.IO.TextReader tr;

                        tw.WriteLine(string.Format("Url: {0}", request.Path));

                        request.Params.AllKeys.ToList().ForEach(key =>
                        {
                            tw.WriteLine(string.Format("Param: {0} = {1}", key, request.Params[key]));
                        });

                        request.Form.AllKeys.ToList().ForEach(key =>
                        {
                            tw.WriteLine(string.Format("Form: {0} = {1}", key, request.Form[key]));
                        });

                        request.ServerVariables.AllKeys.ToList().ForEach(key =>
                        {
                            tw.WriteLine(string.Format("ServerVariable: {0} = {1}", key, request.ServerVariables[key]));
                        });

                        request.QueryString.AllKeys.ToList().ForEach(key =>
                        {
                            tw.WriteLine(string.Format("QueryString: {0} = {1}", key, request.QueryString[key]));
                        });

                        if (Additional != null)
                        {
                            Additional(tw);
                        }

                        tw.WriteLine();
                        tw.WriteLine("Request context:");
                        request.InputStream.Position = 0;
                        tr = new StreamReader(request.InputStream);
                        tw.Write(tr.ReadToEnd());
                        tr.Close();
                        tr.Dispose();
                        request.InputStream.Position = 0;
                        tw.WriteLine();
                        tw.WriteLine();
                    }
                }
                catch (Exception ex)
                {
                    tw.WriteLine(string.Format("HttpRequest is not available: {0}", ex.Message));
                }

                tw.WriteLine("Full text:");
                tw.Write(Text);

                tw.Close();
                tw.Dispose();

                if (Finish != null)
                {
                    Finish(fi);
                }
            }
        }

        public abstract void SendEmail(Exception Ex, FileInfo LogFile);
        public abstract void WriteToDb(Exception Ex, FileInfo LogFile);

        protected virtual FileInfo GetFilePath()
        {
            DirectoryInfo di = GetFullPath();
            FileInfo fi = new FileInfo(System.IO.Path.Combine(di.FullName, DateTime.Now.ToString("yyyyMMddHHmm") + ".txt"));

            if (!fi.Exists)
            {
                fi.Create().Dispose();
            }

            return fi;
        }

        protected virtual DirectoryInfo GetFullPath()
        {
            DateTime date = DateTime.Now;
            string path = string.Format("{0}{1}{2}/{3}/", System.Web.HttpRuntime.AppDomainAppPath, Path, date.Year, date.Month);
            DirectoryInfo di = new DirectoryInfo(path);
            DirectoryInfo old = null;

            if (!di.Exists)
            {
                di.Create();
            }
            else
            {
                FileInfo[] files = di.GetFiles();
                foreach (FileInfo file in files)
                {
                    if (DateTime.UtcNow - file.CreationTimeUtc > TimeSpan.FromDays(1))
                    {
                        file.Delete();
                    }
                }
            }

            if (date.Month > 1)
            {
                path = string.Format("{0}{1}{2}/{3}/", System.Web.HttpRuntime.AppDomainAppPath, Path, date.Year, date.Month - 1);
                old = new DirectoryInfo(path);
                if (old.Exists)
                {
                    old.Delete(true);
                }
            }

            path = string.Format("{0}{1}{2}/", System.Web.HttpRuntime.AppDomainAppPath, Path, date.Year - 1);
            old = new DirectoryInfo(path);
            if (old.Exists)
            {
                old.Delete(true);
            }

            return di;
        }
    }
}

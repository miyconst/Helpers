using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Helpers
{
    public class PostedFile
    {
        public string ContentType { get; set; }
        public string FileName { get; set; }
        public byte[] FileContent { get; set; }

        public PostedFile()
        {
        }

        public PostedFile(HttpPostedFileBase file)
        {
            ContentType = file.ContentType;
            FileName = file.FileName;
            FileContent = new Byte[file.InputStream.Length];
            file.InputStream.Read(FileContent, 0, FileContent.Length);
        }
    }
}
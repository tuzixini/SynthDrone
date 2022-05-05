using System;
using System.IO;

namespace fileIO
{
    public class Image
    { 
        public static void Write(string path, byte[] buffer)
        {
            BinaryWriter bw;
            bw = new BinaryWriter(new FileStream(path,FileMode.Create));
            bw.Write(buffer);
            bw.Close();
        }
    }

    public class Log
    {
        public static void Write(string filePath, string msg)
        {
            StreamWriter logger;
            if (!File.Exists(filePath))
            { logger = File.CreateText(filePath); }
            else
            { logger = File.AppendText(filePath); }
            logger.WriteLine(msg);
            logger.Close();
        }
        public static void WriteLog(string filePath, string msg)
        {
            
            string time = DateTime.Now.ToString();
            msg = time + ":   " + msg;
            Write(filePath, msg);
        }
        public static void WriteXml(string filePath, string msg)
        {
            Write(filePath, msg);
        }
    }

}
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Core;

namespace MetroGraphicsMaker
{
    class ChainsLogger
    {
        protected String fullPath;

        protected StringBuilder buffer;

        protected TextWriter writer;

        //public ChainsLogger(String filepath, Line line)
        //{
        //    var todayString = DateTime.Now.ToString("yyyy-MM-dd hh-mm-ss.ffffff");

        //    var linename = line.name;
        //    fullPath = String.Format("{0}{1}{2}_{3}.txt", filepath, Path.DirectorySeparatorChar, todayString, linename);

        //    if (File.Exists(fullPath))
        //        throw new Exception("File already exist!");

        //    writer = new StreamWriter(fullPath);
        //    buffer = new StringBuilder(linename);
        //    buffer.AppendLine(todayString);
        //}

        public ChainsLogger(String filepath, Line line)
        {
            var todayString = DateTime.Now.ToString("yyyy-MM-dd hh-mm-ss.ffffff");

            var linename = line.name;
            fullPath = String.Format("{0}{1}{2}_{3}.txt", filepath, Path.DirectorySeparatorChar, todayString, linename);

            if (File.Exists(fullPath))
                throw new Exception("File already exist!");

            writer = new StreamWriter(fullPath);
            buffer = new StringBuilder(linename);
            buffer.AppendLine(todayString);
        }

        public void Write(IEnumerable<Chain> chains)
        {
            foreach (var chain in chains)
                WriteChain(chain);
        }

        public void WriteChain(Chain chain)
        {
            foreach (var link in chain.Links)
                WriteLink(link);
        }

        public void WriteLink(Link link)
        {
            writer.WriteLine(link);
            writer.Flush();
        }
    }
}


using System;
using System.IO;
using System.Text;

namespace MiaX
{
    struct Arguments
    {
        public int WorldLength;
        public string Charset;
        public byte[] AsciiCharset;
        public string OutputPath;
        public int ProcessCount;

        public Arguments(string[] args)
        {
            if (!int.TryParse(args[0], out WorldLength))
            {
                throw new Exception("Valide word length!");
            }

            Charset = args[1];
            AsciiCharset = Encoding.ASCII.GetBytes(Charset);

            try
            {
                OutputPath = Path.GetFullPath(args[2]);
            }
            catch
            {
                throw new Exception("Invalide output file path");
            }

            if (args.Length == 5 && args[4] == "-x")
            {
                if(!int.TryParse(args[5], out ProcessCount))
                {
                    throw new Exception("Invalide process count");
                }
                if(ProcessCount > Environment.ProcessorCount)
                {
                    throw new Exception("process count bust be lest or equals " + Environment.ProcessorCount);
                }
            } else
            {
                // never happen
                ProcessCount = -1;
            }
        }

        public Arguments(string[] args, int processeCount)
        {
            if(!int.TryParse(args[0], out WorldLength))
            {
                throw new Exception("Valide word length!");
            }

            Charset = args[1];
            AsciiCharset = Encoding.ASCII.GetBytes(Charset);

            try
            {
                OutputPath = Path.GetFullPath(args[2]);
            }catch
            {
                throw new Exception("Invalide output file path");
            }

            ProcessCount = processeCount;
        }

        public static void Usage()
        {
            Console.WriteLine("Usage: *.exe word-length charset output-file [-x process-count]");
            Console.WriteLine();
            Console.WriteLine("\tword-length: The length of the words");
            Console.WriteLine("\tcharset    : ASCII-characters to build the word");
            Console.WriteLine("\toutput-file: Local file to write the words");
            Console.WriteLine("\t-x process-count");
            Console.WriteLine("\t\tprocess-count: How many process spawn");
            Console.WriteLine();
        }
    }
}

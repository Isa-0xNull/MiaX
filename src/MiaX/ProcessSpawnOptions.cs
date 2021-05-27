using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MiaX
{
    class ProcessSpawnOptions
    {
        public string    ExecuteName;
        public int       WordLength;
        public Process[] Processes;
        public Task[]    ProcessesWaiter;
        public string    CharSet;
        public int       BufferLength;
        public byte[]    Buffer;

        public ProcessSpawnOptions(int processCount, int wordLength, string charSet, int bufferLength)
        {
            ExecuteName     = AppDomain.CurrentDomain.FriendlyName;
            WordLength      = wordLength;
            Processes       = new Process[processCount];
            ProcessesWaiter = new Task[processCount];
            CharSet         = charSet;
            BufferLength    = bufferLength;
            Buffer          = new byte[bufferLength];
        }
    }
}
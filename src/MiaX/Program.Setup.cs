using System;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace MiaX
{
    internal static partial class Program
    {
        private static async Task SetupAsync(Arguments arguments)
        {
            
            CreateCancellation(out CancellationTokenSource tokenSource, out CancellationToken token);

            Task monitor = MonitoringAsync(arguments.ProcessCount, token);

            int                 charIndex    = 0;
            int                 charIndexEnd = arguments.AsciiCharset.Length;
            ProcessSpawnOptions options      = new(arguments.ProcessCount, arguments.WorldLength, arguments.Charset, 1024 * 1024 * 100);

            // setup
            for (; charIndex < charIndexEnd && charIndex < arguments.ProcessCount; charIndex++)
            {
                options.ProcessesWaiter[charIndex] = SpawnNewProcess(arguments.AsciiCharset, charIndex, options, charIndex);
            }
            // processing
            while (charIndex < charIndexEnd)
            {
                int pid = Task.WaitAny(options.ProcessesWaiter);
                await CopyFileIntoAsync(pid, options.Buffer, options.BufferLength, arguments.Charset);

                options.ProcessesWaiter[pid] = SpawnNewProcess(arguments.AsciiCharset, charIndex, options, pid);
                
                charIndex++;
            }
            // cleanup
            Task.WaitAll(options.ProcessesWaiter);
            for (int pid = 0; pid < arguments.ProcessCount; pid++)
            {
                string pidFile = await CopyFileIntoAsync(pid, options.Buffer, options.BufferLength, arguments.OutputPath);
                File.Delete(pidFile);
            }

            tokenSource.Cancel();
            await monitor;
        }

        private static async Task MonitoringAsync(int processCount, CancellationToken token)
        {
            int                            memoryMappedFileCapacity = processCount * sizeof(ulong);
            using MemoryMappedFile         mmf                      = MemoryMappedFile.CreateNew(MEMORY_MAPPED_FILE_NAME, memoryMappedFileCapacity);
            using MemoryMappedViewAccessor mmfView                  = mmf.CreateViewAccessor();

            ulong[] snapshot0 = new ulong[processCount];
            ulong[] snapshot1 = new ulong[processCount];

            while (!token.IsCancellationRequested)
            {
                mmfView.ReadArray(0, snapshot0, 0, processCount);
                await Task.Delay(1000);
                mmfView.ReadArray(0, snapshot1, 0, processCount);
                Console.Clear();
                ulong sum = 0;
                for (int i = 0; i < processCount; i++)
                {
                    ulong delta = snapshot1[i] - snapshot0[i];
                    sum += delta;
                    Console.WriteLine($"PID[{i:00}]:{delta}pws/sec");
                }
                Console.WriteLine($"Delta: {sum}pws/sec");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Task SpawnNewProcess(byte[] charSet, int charIndex, ProcessSpawnOptions options, int pid)
        {
            byte startChar = charSet[charIndex];
            ProcessStartInfo pStartInfo = new(options.ExecuteName, $"{options.WordLength} {options.CharSet} {startChar} {pid}")
            {
                CreateNoWindow  = true,
                UseShellExecute = false,
            };

            return Process.Start(pStartInfo).WaitForExitAsync();
        }

        private static async Task<string> CopyFileIntoAsync(int pid, byte[] buffer, int bufferLength, string outputFile)
        {
            string sourceFile = "Process" + pid + ".txt";

            await using FileStream sourceFileStream = File.Open(sourceFile, FileMode.Open,         FileAccess.Read,  FileShare.None);
            await using FileStream outputFileStream = File.Open(outputFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);

            outputFileStream.Seek(0, SeekOrigin.End);
            int readedBytes = 0;
            while ((readedBytes = await sourceFileStream.ReadAsync(buffer, 0, bufferLength)) != 0)
            {
                await outputFileStream.WriteAsync(buffer, 0, readedBytes);
            }
            await outputFileStream.FlushAsync();

            return Path.GetFullPath(sourceFile);
        }
    }
}
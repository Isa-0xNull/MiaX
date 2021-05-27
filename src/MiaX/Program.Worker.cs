using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MiaX
{
    internal static partial class Program
    {
        private static ConcurrentQueue<string> _products;

        private static async Task WordGenAsync(string[] args)
        {
            int    wordLen   = int.Parse(args[0]);
            byte[] charSet   = Encoding.ASCII.GetBytes(args[1]);
            sbyte  startChar = sbyte.Parse(args[2]);
            int    pid       = int.Parse(args[3]);

            _products = new ConcurrentQueue<string>();

            Unsafe.SkipInit(out CreateWordOptions options);

            using MemoryMappedFile         mmf     = MemoryMappedFile.OpenExisting(MEMORY_MAPPED_FILE_NAME);
            using MemoryMappedViewAccessor mmfView = mmf.CreateViewAccessor();

            int mmfOffset = pid * sizeof(ulong);
            mmfView.Write(mmfOffset, (ulong)0);

            unsafe
            {
                sbyte* word           = stackalloc sbyte[wordLen + 1];
                *word                 = startChar;
                *(word + wordLen)     = (sbyte) ' ';
                sbyte* charSetStart   = stackalloc sbyte[charSet.Length];
                sbyte* charSetEnd     = charSetStart + charSet.Length;

                Marshal.Copy(charSet, 0, (IntPtr)charSetStart, charSet.Length);

                options = new CreateWordOptions
                {
                    Word             = word,
                    WordOffset       = word + 1,
                    StartOffset      = charSetStart,
                    EndOffset        = charSetEnd,
                    WordIndex        = wordLen - 1,
                    WordLength       = wordLen,
                    CounterOffset    = mmfOffset,
                    MemoryMappedView = mmfView
                };
            }

            CreateCancellation(out CancellationTokenSource tokenSource, out CancellationToken token);
            Task worker = FileWriteWorker(pid, token);

            CreateWords(options);

            tokenSource.Cancel();
            await worker;
        }

        private static unsafe void CreateWords(CreateWordOptions options)
        {
            sbyte* innerStart = options.StartOffset;
            sbyte* innerEnd   = options.EndOffset;

            if (options.WordIndex > 1) // if not last char
            {
                CreateWordOptions wordOptions = options;
                wordOptions.WordOffset += 1;
                wordOptions.WordIndex  -= 1;
                while (innerStart + 2 < innerEnd)
                {
                    *options.WordOffset = *innerStart;
                    CreateWords(wordOptions);
                    innerStart++;

                    *options.WordOffset = *innerStart;
                    CreateWords(wordOptions);
                    innerStart++;
                }

                if (innerStart < innerEnd)
                {
                    *options.WordOffset = *innerStart;
                    CreateWords(wordOptions);
                    innerStart++;
                }
            }
            else // is last char
            {
                sbyte* word = options.Word;
                int wordLength =  options.WordLength + 1;
                int counterOffset =  options.CounterOffset;

                do
                {
                    *options.WordOffset = *innerStart;
                    _products.Enqueue(new string(word, 0, wordLength));

                    // update count;
                    ulong count = options.MemoryMappedView.ReadUInt64(counterOffset);
                    options.MemoryMappedView.Write(counterOffset, count + 1);

                    innerStart++;
                }
                while (innerStart < innerEnd);
            }
        }

        private static Task FileWriteWorker(int pid, CancellationToken token)
        {
            return Task.Run(async () =>
            {
                string                   processTaskFile    = "Process" + pid + ".txt";
                await using FileStream   outputFileStream   = File.Open(processTaskFile, FileMode.Create, FileAccess.Write, FileShare.None);
                await using StreamWriter outputStreamWriter = new(outputFileStream, Encoding.ASCII, 1024 * 1024 * 100);

                while (!token.IsCancellationRequested || !_products.IsEmpty)
                {
                    if (_products.TryDequeue(out string word))
                    {
                        await outputStreamWriter.WriteAsync(word);
                    }
                }

                await outputStreamWriter.FlushAsync();
            }, token);
        }
    }
}
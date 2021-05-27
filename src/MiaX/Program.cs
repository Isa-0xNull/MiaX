using System;
using System.Threading;
using System.Threading.Tasks;

namespace MiaX
{
    internal static partial class Program
    {
        private const string MEMORY_MAPPED_FILE_NAME = nameof(MEMORY_MAPPED_FILE_NAME);

        private static async Task Main(string[] args)
        {
            try
            {
                switch (args.Length)
                {
                    case 3:
                        await SetupAsync(new Arguments(args, Environment.ProcessorCount));
                        break;
                    case 5:
                        await SetupAsync(new Arguments(args));
                        break;
                    case 4:
                        await WordGenAsync(args);
                        return;
                    default:
                        Arguments.Usage();
                        return;
                }

                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void CreateCancellation(out CancellationTokenSource tokenSource, out CancellationToken token)
        {
            tokenSource = new CancellationTokenSource();
            token       = tokenSource.Token;
        }
    }
}
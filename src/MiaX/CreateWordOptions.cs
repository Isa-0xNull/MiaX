using System.IO.MemoryMappedFiles;

namespace MiaX
{
    static partial class Program
    {
        private unsafe struct CreateWordOptions
        {
            public sbyte*                   Word;
            public sbyte*                   WordOffset;
            public sbyte*                   StartOffset;
            public sbyte*                   EndOffset;
            public int                      WordIndex;
            public int                      WordLength;
            public int                      CounterOffset;
            public MemoryMappedViewAccessor MemoryMappedView;
        }
    }
}
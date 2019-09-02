using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace SubFinder.Extensions
{
    public static class StreamExtensions
    {
        private const string SubtitleFileExtension = ".srt";

        public static async Task<Memory<byte>> ExtractZippedSubtitleAsync(this Stream stream)
        {
            using (var archive = new ZipArchive(stream))
            {
                var subtitleEntry = archive.Entries.First(entry => entry.Name.EndsWith(SubtitleFileExtension));
                var memory = new Memory<byte>(new byte[subtitleEntry.Length]);

                using (var entryStream = subtitleEntry.Open())
                {
                    await entryStream.ReadAsync(memory);
                }

                return memory;
            }
        }
    }
}

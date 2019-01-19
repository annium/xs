using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Xs.Registry.Dotnet.Helpers
{
    internal static class StreamExtensions
    {
        // We pick a value that is the largest multiple of 4096 that is still smaller than the large object heap threshold (85K).
        // The CopyTo/CopyToAsync buffer is short-lived and is likely to be collected at Gen0, and it offers a significant
        // improvement in Copy performance.
        private const int DefaultCopyBufferSize = 81920;

        /// <summary>
        /// Copies a stream to a file, and returns that file as a stream. The underlying file will be
        /// deleted when the resulting stream is disposed.
        /// </summary>
        /// <param name="original">The stream to be copied, at its current position.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The copied stream, with its position reset to the beginning.</returns>
        public static async Task<FileStream> AsTemporaryFileStreamAsync(
            this Stream original,
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            var result = new FileStream(
                Path.GetTempFileName(),
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.None,
                DefaultCopyBufferSize,
                FileOptions.DeleteOnClose
            );

            try
            {
                await original.CopyToAsync(result, DefaultCopyBufferSize, cancellationToken);
                result.Position = 0;
            }
            catch (Exception)
            {
                result.Dispose();
                throw;
            }

            return result;
        }
    }
}
using System.IO.Compression;

namespace GeneralUpdateService.Utils
{
    public static class ZipHelper
    {
        public static async Task ExtractAsync(
            string zipPath,
            string destinationDirectory,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default)
        {
            Directory.CreateDirectory(destinationDirectory);

            using var archive = ZipFile.OpenRead(zipPath);
            long totalBytes = archive.Entries.Sum(e => e.Length);
            long processedBytes = 0;

            foreach (var entry in archive.Entries)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (entry.FullName.EndsWith("/")) // 跳过文件夹
                {
                    Directory.CreateDirectory(Path.Combine(destinationDirectory, entry.FullName));
                    continue;
                }

                string destinationPath = Path.GetFullPath(Path.Combine(destinationDirectory, entry.FullName));

                // 防止路径穿越攻击
                if (!destinationPath.StartsWith(destinationDirectory, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("路径穿越检测");

                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);

                using var entryStream = entry.Open();
                using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);

                var buffer = new byte[81920]; // 80KB 缓冲区

                while (true)
                {
                    int bytesRead = await entryStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    if (bytesRead == 0) break;

                    await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);

                    processedBytes += bytesRead;

                    // 每处理一定量才报告，避免频繁 UI 更新
                    if (totalBytes > 0 && processedBytes % (totalBytes / 100 + 1) == 0)
                    {
                        double percent = (double)processedBytes / totalBytes * 100;
                        progress?.Report(percent);
                    }
                }

                // 确保最后一个文件也报告
                if (totalBytes > 0)
                    progress?.Report(100.0);
            }
        }
    }
}

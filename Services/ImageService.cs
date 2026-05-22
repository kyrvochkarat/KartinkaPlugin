namespace KartinkaSpawner.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using DrawingBitmap = System.Drawing.Bitmap;
    using DrawingColor = System.Drawing.Color;
    using UnityEngine;

    internal static class ImageService
    {
        public static readonly string[] SupportedExtensions =
        {
            ".png",
            ".jpg",
            ".jpeg",
            ".bmp",
        };

        public static IReadOnlyList<string> GetImageFiles(string directory)
        {
            if (!Directory.Exists(directory))
                return Array.Empty<string>();

            return Directory.GetFiles(directory)
                .Where(file => SupportedExtensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
                .OrderBy(file => Path.GetFileName(file), StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        public static bool TryResolveImage(string directory, string imageName, out string path, out string error)
        {
            path = null;
            error = null;

            if (string.IsNullOrWhiteSpace(imageName))
            {
                error = "Укажи имя картинки.";
                return false;
            }

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string safeName = imageName.Trim().Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

            if (Path.IsPathRooted(safeName) || safeName.Split(Path.DirectorySeparatorChar).Any(part => part == ".."))
            {
                error = "Имя картинки не должно быть абсолютным путем или содержать '..'.";
                return false;
            }

            string extension = Path.GetExtension(safeName);
            if (!string.IsNullOrWhiteSpace(extension))
            {
                if (!SupportedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
                {
                    error = $"Формат '{extension}' не поддерживается. Поддерживаются: {string.Join(", ", SupportedExtensions)}";
                    return false;
                }

                string exact = Path.Combine(directory, safeName);
                if (File.Exists(exact))
                {
                    path = exact;
                    return true;
                }

                error = $"Файл '{safeName}' не найден в папке {directory}";
                return false;
            }

            string[] matches = GetImageFiles(directory)
                .Where(file => string.Equals(Path.GetFileNameWithoutExtension(file), safeName, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (matches.Length == 0)
            {
                error = $"Картинка '{safeName}' не найдена. Используй 'kartinka list'.";
                return false;
            }

            if (matches.Length > 1)
            {
                error = "Найдено несколько файлов с таким именем. Укажи расширение: " +
                        string.Join(", ", matches.Select(Path.GetFileName));
                return false;
            }

            path = matches[0];
            return true;
        }

        public static DecodedImage LoadAndResize(string path, int maxWidth, int maxHeight)
        {
            maxWidth = Math.Max(1, maxWidth);
            maxHeight = Math.Max(1, maxHeight);

            // Не используем UnityEngine.ImageConversionModule, потому что в некоторых сборках SCP:SL
            // он тянет netstandard 2.1 и ломает сборку net48-плагина.
            // System.Drawing входит в .NET Framework/net48 и нормально собирается без Unity ImageConversion.
            using (DrawingBitmap bitmap = new DrawingBitmap(path))
            {
                int srcWidth = bitmap.Width;
                int srcHeight = bitmap.Height;

                float multiplier = Math.Min(1f, Math.Min(maxWidth / (float)srcWidth, maxHeight / (float)srcHeight));
                int dstWidth = Math.Max(1, (int)Math.Round(srcWidth * multiplier));
                int dstHeight = Math.Max(1, (int)Math.Round(srcHeight * multiplier));

                Color32[] result = new Color32[dstWidth * dstHeight];

                for (int y = 0; y < dstHeight; y++)
                {
                    int srcY = Math.Min(srcHeight - 1, (int)((y + 0.5f) * srcHeight / dstHeight));

                    for (int x = 0; x < dstWidth; x++)
                    {
                        int srcX = Math.Min(srcWidth - 1, (int)((x + 0.5f) * srcWidth / dstWidth));
                        DrawingColor color = bitmap.GetPixel(srcX, srcY);

                        result[x + (y * dstWidth)] = new Color32(color.R, color.G, color.B, color.A);
                    }
                }

                return new DecodedImage(dstWidth, dstHeight, result);
            }
        }
    }
}

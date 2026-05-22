namespace KartinkaSpawner.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Exiled.API.Features;
    using Exiled.API.Features.Toys;
    using UnityEngine;

    public sealed class PictureSpawner
    {
        private readonly Plugin plugin;
        private readonly List<SpawnedPicture> spawnedPictures = new List<SpawnedPicture>();

        public PictureSpawner(Plugin plugin)
        {
            this.plugin = plugin;
        }

        public int PictureCount => spawnedPictures.Count;

        public int PrimitiveCount
        {
            get
            {
                int count = 0;
                foreach (SpawnedPicture picture in spawnedPictures)
                    count += picture.Primitives.Count;
                return count;
            }
        }

        public SpawnResult Spawn(Player player, string imagePath, float? pixelSizeOverride = null, int? maxSideOverride = null)
        {
            if (player == null)
                return SpawnResult.Fail("Команду spawn нужно выполнять от игрока в RemoteAdmin, чтобы взять позицию.");

            Config config = plugin.Config;

            int maxWidth = Math.Max(1, maxSideOverride ?? config.MaxWidth);
            int maxHeight = Math.Max(1, maxSideOverride ?? config.MaxHeight);
            float pixelSize = Math.Max(0.005f, pixelSizeOverride ?? config.PixelSize);
            float thickness = Math.Max(0.001f, config.PixelThickness);

            DecodedImage image = ImageService.LoadAndResize(imagePath, maxWidth, maxHeight);

            int visiblePixels = 0;
            foreach (Color32 pixel in image.PixelsTopToBottom)
            {
                if (pixel.a > config.AlphaThreshold)
                    visiblePixels++;
            }

            if (visiblePixels <= 0)
                return SpawnResult.Fail("В картинке не осталось видимых пикселей после alpha-фильтра.");

            if (visiblePixels > Math.Max(1, config.MaxPrimitivesPerImage))
            {
                return SpawnResult.Fail(
                    $"Слишком много видимых пикселей: {visiblePixels}. Лимит: {config.MaxPrimitivesPerImage}. " +
                    "Уменьши max_width/max_height в конфиге или укажи меньший max_side: kartinka spawn <name> <pixel_size> <max_side>");
            }

            Transform camera = player.CameraTransform;
            Quaternion rotation = config.UseCameraPitch
                ? camera.rotation
                : Quaternion.Euler(0f, camera.eulerAngles.y, 0f);

            Vector3 forward = rotation * Vector3.forward;
            Vector3 right = rotation * Vector3.right;
            Vector3 up = rotation * Vector3.up;
            Vector3 center = camera.position + (forward * config.SpawnDistance) + (Vector3.up * config.VerticalOffset);
            Vector3 euler = rotation.eulerAngles;
            Vector3 scale = new Vector3(pixelSize, pixelSize, thickness);

            float halfWidth = (image.Width - 1) / 2f;
            float halfHeight = (image.Height - 1) / 2f;

            List<Primitive> primitives = new List<Primitive>(visiblePixels);

            try
            {
                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        Color32 pixel = image.PixelsTopToBottom[x + (y * image.Width)];
                        if (pixel.a <= config.AlphaThreshold)
                            continue;

                        Vector3 offset = (right * ((x - halfWidth) * pixelSize)) +
                                         (up * ((halfHeight - y) * pixelSize));

                        Color color = new Color(pixel.r / 255f, pixel.g / 255f, pixel.b / 255f, pixel.a / 255f);
                        Primitive primitive = Primitive.Create(PrimitiveType.Cube, center + offset, euler, scale, false, color);
                        primitive.Visible = true;
                        primitive.Collidable = config.Collidable;
                        primitive.Spawn();
                        primitives.Add(primitive);
                    }
                }
            }
            catch
            {
                DestroyPrimitives(primitives);
                throw;
            }

            spawnedPictures.Add(new SpawnedPicture(Path.GetFileName(imagePath), primitives));

            return SpawnResult.Ok(
                $"Заспавнено: {Path.GetFileName(imagePath)} | размер {image.Width}x{image.Height} | кубов: {visiblePixels}.");
        }

        public bool UndoLast(out string message)
        {
            if (spawnedPictures.Count == 0)
            {
                message = "Нет заспавненных картинок для удаления.";
                return false;
            }

            SpawnedPicture last = spawnedPictures[spawnedPictures.Count - 1];
            spawnedPictures.RemoveAt(spawnedPictures.Count - 1);
            DestroyPrimitives(last.Primitives);
            message = $"Удалена последняя картинка: {last.Name} ({last.Primitives.Count} кубов).";
            return true;
        }

        public int Clear()
        {
            int count = PrimitiveCount;

            foreach (SpawnedPicture picture in spawnedPictures)
                DestroyPrimitives(picture.Primitives);

            spawnedPictures.Clear();
            return count;
        }

        private static void DestroyPrimitives(IEnumerable<Primitive> primitives)
        {
            foreach (Primitive primitive in primitives)
            {
                try
                {
                    primitive.Destroy();
                }
                catch (Exception exception)
                {
                    Log.Debug($"Не удалось удалить primitive: {exception}");
                }
            }
        }

        private sealed class SpawnedPicture
        {
            public SpawnedPicture(string name, List<Primitive> primitives)
            {
                Name = name;
                Primitives = primitives;
            }

            public string Name { get; }

            public List<Primitive> Primitives { get; }
        }
    }

    public sealed class SpawnResult
    {
        private SpawnResult(bool success, string message)
        {
            Success = success;
            Message = message;
        }

        public bool Success { get; }

        public string Message { get; }

        public static SpawnResult Ok(string message) => new SpawnResult(true, message);

        public static SpawnResult Fail(string message) => new SpawnResult(false, message);
    }
}

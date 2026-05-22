namespace KartinkaSpawner.Commands
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using CommandSystem;
    using Exiled.API.Features;
    using Exiled.Permissions.Extensions;
    using KartinkaSpawner.Services;

    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public sealed class KartinkaCommand : ICommand
    {
        public string Command => "kartinka";

        public string[] Aliases => new[] { "kt", "picture" };

        public string Description => "Спавн картинок из папки конфига через primitive-кубы.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Plugin plugin = Plugin.Instance;
            if (plugin == null)
            {
                response = "KartinkaSpawner не загружен.";
                return false;
            }

            if (!HasPermission(plugin, sender, out response))
                return false;

            if (arguments.Count == 0)
            {
                response = GetUsage(plugin);
                return false;
            }

            string subCommand = arguments.At(0).ToLowerInvariant();

            try
            {
                switch (subCommand)
                {
                    case "list":
                    case "ls":
                        return List(plugin, out response);

                    case "spawn":
                    case "s":
                        return Spawn(plugin, arguments, sender, out response);

                    case "clear":
                    case "removeall":
                        int removed = plugin.Spawner.Clear();
                        response = $"Удалено primitive-объектов: {removed}.";
                        return true;

                    case "undo":
                    case "remove_last":
                    case "last":
                        return plugin.Spawner.UndoLast(out response);

                    case "folder":
                    case "path":
                        response = $"Папка картинок: {plugin.ImagesDirectory}";
                        return true;

                    default:
                        response = $"Неизвестная подкоманда '{subCommand}'.\n" + GetUsage(plugin);
                        return false;
                }
            }
            catch (Exception exception)
            {
                Log.Error(exception);
                response = "Ошибка KartinkaSpawner: " + exception.Message;
                return false;
            }
        }

        private static bool List(Plugin plugin, out string response)
        {
            string[] files = ImageService.GetImageFiles(plugin.ImagesDirectory)
                .Select(Path.GetFileName)
                .ToArray();

            if (files.Length == 0)
            {
                response = $"Картинок нет. Закинь PNG/JPG в папку: {plugin.ImagesDirectory}";
                return true;
            }

            response = $"Картинки в {plugin.ImagesDirectory} ({files.Length}):\n" + string.Join("\n", files);
            return true;
        }

        private static bool Spawn(Plugin plugin, ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 2)
            {
                response = "Использование: kartinka spawn <имя_картинки> [pixel_size] [max_side]";
                return false;
            }

            string imageName = arguments.At(1);

            float? pixelSize = null;
            int? maxSide = null;

            if (arguments.Count >= 3)
            {
                if (!float.TryParse(arguments.At(2), NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedPixelSize) || parsedPixelSize <= 0f)
                {
                    response = "pixel_size должен быть положительным числом. Пример: kartinka spawn logo 0.06";
                    return false;
                }

                pixelSize = parsedPixelSize;
            }

            if (arguments.Count >= 4)
            {
                if (!int.TryParse(arguments.At(3), NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedMaxSide) || parsedMaxSide <= 0)
                {
                    response = "max_side должен быть положительным целым числом. Пример: kartinka spawn logo 0.06 48";
                    return false;
                }

                maxSide = parsedMaxSide;
            }

            if (!ImageService.TryResolveImage(plugin.ImagesDirectory, imageName, out string path, out string error))
            {
                response = error;
                return false;
            }

            Player player = Player.Get(sender);
            SpawnResult result = plugin.Spawner.Spawn(player, path, pixelSize, maxSide);
            response = result.Message;
            return result.Success;
        }

        private static bool HasPermission(Plugin plugin, ICommandSender sender, out string response)
        {
            response = null;

            string requiredPermission = plugin.Config.RequiredPermission;
            if (string.IsNullOrWhiteSpace(requiredPermission))
                return true;

            if (sender is CommandSender commandSender && commandSender.CheckPermission(requiredPermission))
                return true;

            response = $"Нет прав. Требуется permission: {requiredPermission}";
            return false;
        }

        private static string GetUsage(Plugin plugin)
        {
            return "KartinkaSpawner:\n" +
                   "  kartinka list - показать все картинки\n" +
                   "  kartinka spawn <name> [pixel_size] [max_side] - заспавнить картинку перед собой\n" +
                   "  kartinka undo - удалить последнюю заспавненную картинку\n" +
                   "  kartinka clear - удалить все картинки, заспавненные этим плагином\n" +
                   "  kartinka folder - показать папку с картинками\n" +
                   $"Текущая папка: {plugin.ImagesDirectory}";
        }
    }
}

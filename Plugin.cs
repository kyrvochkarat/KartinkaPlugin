namespace KartinkaSpawner
{
    using System;
    using System.IO;
    using Exiled.API.Features;
    using KartinkaSpawner.Services;
    using Server = Exiled.Events.Handlers.Server;

    public sealed class Plugin : Exiled.API.Features.Plugin<Config>
    {
        public static Plugin Instance { get; private set; }

        public override string Name => "KartinkaSpawner";
        public override string Author => "vityanvsk";
        public override string Prefix => "kartinka";
        public override Version Version => new Version(1, 0, 0);
        public override Version RequiredExiledVersion => new Version(9, 13, 2);

        public PictureSpawner Spawner { get; private set; }

        public string ImagesDirectory
        {
            get
            {
                if (Path.IsPathRooted(Config.ImagesFolder))
                    return Config.ImagesFolder;

                return Path.Combine(Paths.Configs, Config.ImagesFolder);
            }
        }

        public override void OnEnabled()
        {
            Instance = this;
            Spawner = new PictureSpawner(this);

            Directory.CreateDirectory(ImagesDirectory);

            if (Config.ClearOnRoundRestart)
                Server.RestartingRound += OnRestartingRound;

            base.OnEnabled();
            Log.Info($"Папка картинок: {ImagesDirectory}");
        }

        public override void OnDisabled()
        {
            if (Config.ClearOnRoundRestart)
                Server.RestartingRound -= OnRestartingRound;

            Spawner?.Clear();
            Spawner = null;
            Instance = null;

            base.OnDisabled();
        }

        private void OnRestartingRound()
        {
            Spawner?.Clear();
        }
    }
}

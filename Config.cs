namespace KartinkaSpawner
{
    using System.ComponentModel;
    using Exiled.API.Interfaces;

    public sealed class Config : IConfig
    {
        [Description("Включен ли плагин.")]
        public bool IsEnabled { get; set; } = true;

        [Description("Включить debug-логи.")]
        public bool Debug { get; set; } = false;

        [Description("Папка с картинками внутри EXILED/Configs. Если указать абсолютный путь — будет использован он.")]
        public string ImagesFolder { get; set; } = "KartinkaImages";

        [Description("Максимальная ширина картинки после уменьшения. Чем больше — тем больше primitive-объектов и нагрузка.")]
        public int MaxWidth { get; set; } = 64;

        [Description("Максимальная высота картинки после уменьшения. Чем больше — тем больше primitive-объектов и нагрузка.")]
        public int MaxHeight { get; set; } = 64;

        [Description("Жесткий лимит создаваемых кубов на одну картинку. Защищает сервер от случайного спавна огромных изображений.")]
        public int MaxPrimitivesPerImage { get; set; } = 2500;

        [Description("Размер одного пикселя в игровых метрах.")]
        public float PixelSize { get; set; } = 0.08f;

        [Description("Толщина пикселей-кубов.")]
        public float PixelThickness { get; set; } = 0.01f;

        [Description("На сколько метров перед игроком спавнить центр картинки.")]
        public float SpawnDistance { get; set; } = 1.5f;

        [Description("Дополнительное смещение центра картинки вверх/вниз относительно камеры игрока.")]
        public float VerticalOffset { get; set; } = 0f;

        [Description("Если true — картинка будет повторять pitch/наклон камеры. Если false — будет вертикальной и повернутой только по yaw игрока.")]
        public bool UseCameraPitch { get; set; } = false;

        [Description("Будут ли кубы иметь коллизию.")]
        public bool Collidable { get; set; } = false;

        [Description("Пиксели с alpha <= этого значения не спавнятся. 0 — спавнить все, кроме полностью прозрачных.")]
        public byte AlphaThreshold { get; set; } = 10;

        [Description("Необязательное EXILED permission для команды. Пусто — не проверять.")]
        public string RequiredPermission { get; set; } = string.Empty;

        [Description("Удалять все заспавненные картинки при рестарте раунда.")]
        public bool ClearOnRoundRestart { get; set; } = true;
    }
}

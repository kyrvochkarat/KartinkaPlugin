namespace KartinkaSpawner.Services
{
    using UnityEngine;

    internal sealed class DecodedImage
    {
        public DecodedImage(int width, int height, Color32[] pixelsTopToBottom)
        {
            Width = width;
            Height = height;
            PixelsTopToBottom = pixelsTopToBottom;
        }

        public int Width { get; }

        public int Height { get; }

        /// <summary>
        /// Пиксели идут слева направо, сверху вниз.
        /// Индекс: x + y * Width, где y=0 — верхняя строка.
        /// </summary>
        public Color32[] PixelsTopToBottom { get; }
    }
}

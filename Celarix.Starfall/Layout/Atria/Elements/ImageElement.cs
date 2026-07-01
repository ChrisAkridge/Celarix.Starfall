using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Atria.Elements
{
    public sealed class ImageElement : AtriaElement
    {
        private readonly SImage _image;

        private ImageElement(SImage image, string atriaIdString)
        {
            _image = image;
            Id = AtriaId.Parse(atriaIdString);
        }

        public static ImageElement FromFile(string filePath, string atriaIdString)
        {
            var image = SImage.FromFile(filePath);
            var element = new ImageElement(image, atriaIdString)
            {
                Size = new SSizeF(image.Width, image.Height)
            };
            return element;
        }

        public override void Render(IRenderTarget target)
        {
            target.DrawImage(_image, Bounds, Opacity);
        }
    }
}

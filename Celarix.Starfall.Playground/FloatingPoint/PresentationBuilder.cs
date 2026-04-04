using Celarix.Starfall.Layout.Helium;
using Celarix.Starfall.Layout.Helium.Components;
using Celarix.Starfall.Layout.Helium.Elements;
using Celarix.Starfall.Layout.Helium.Elements.Containers;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Playground.FloatingPoint
{
    internal static class PresentationBuilder
    {
        private static readonly SColor DefaultBackground = new(8, 0, 130, 255);
        private static readonly SColor DefaultText = SColor.White;

        internal static List<HeliumScene> BuildSlides()
        {
            var slides = new List<HeliumScene>
            {
                CreateTitleSlide()
            };
            return slides;
        }

        private static HeliumScene CreateTitleSlide()
        {
            var titleElement = new TextElement
            {
                Text = "How Floating-Point Numbers Work",
                Font = new SFontFamily("Calibri", 48f),
                Color = DefaultText
            };
            var authorElement = new TextElement
            {
                Text = "by Chris Akridge",
                Font = new SFontFamily("Calibri", 24f),
                Color = DefaultText
            };
            var topContainer = new SingleElementContainer
            {
                Alignment = Alignment.BottomCenter,
                Padding = new Padding(0d, 0d, 0d, 0.05d)
            };
            var bottomContainer = new SingleElementContainer
            {
                Alignment = Alignment.TopCenter,
                Padding = new Padding(0d, 0.05d, 0d, 0d)
            };
            topContainer.Child = titleElement;
            bottomContainer.Child = authorElement;
            var binaryContainer = new BinaryElementContainer();
            binaryContainer.SplitVertical(1, 1);
            binaryContainer.FirstSplit!.SetSingleChild(topContainer);
            binaryContainer.SecondSplit!.SetSingleChild(bottomContainer);

            var slide = new HeliumScene
            {
                BackgroundColor = DefaultBackground,
                Root = binaryContainer
            };
            return slide;
        }
    }
}

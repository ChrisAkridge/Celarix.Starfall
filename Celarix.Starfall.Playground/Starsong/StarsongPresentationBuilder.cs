using Celarix.Starfall.Layout.Helium;
using Celarix.Starfall.Layout.Helium.Components;
using Celarix.Starfall.Layout.Helium.Elements;
using Celarix.Starfall.Layout.Helium.Elements.Containers;
using Celarix.Starfall.Layout.Helium.Transitions;
using Celarix.Starfall.Layout.Helium.Transitions.Transforms.Building;
using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Rendering;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using ExCSS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Celarix.Starfall.Playground.Starsong
{
    internal static class StarsongPresentationBuilder
    {
        private static readonly SColor DefaultBackground = new(0, 0, 137, 255);
        private static readonly SColor DefaultTextColor = SColor.White;
        private static SFont DefaultFont => new SFontFamily("Calibri", 48f);

        public static List<HeliumScene> BuildSlides()
        {
            SFontFamily calibri48 = new("Calibri", 48f, FontWeight.Normal, FontWidth.Normal, FontSlant.Upright);
            SFontFamily calibri36 = new("Calibri", 36f, FontWeight.Normal, FontWidth.Normal, FontSlant.Italic);
            var slides = new List<HeliumScene>
            {
                CreateTitleSlide(),
                CreateDefaultTextSlide("violet", "The Violet Facet",
                    "A focus on productivity, task tracking, and addressing ADHD-PI.",
                    backgroundColor: SColor.FromHSV(270, 0.65, 0.7),
                    topFont: calibri48,
                    bottomFont: calibri36),
                CreateDefaultTextSlide("citrine", "The Citrine Facet",
                    "Low leisure, wasted time, simple entertainment. A lazy Saturday.",
                    backgroundColor: SColor.FromHSV(60, 0.7, 0.9),
                    topFont: calibri48,
                    bottomFont: calibri36),
                CreateDefaultTextSlide("crimson", "The Crimson Facet",
                    "Passion, romance.",
                    backgroundColor: SColor.FromHSV(340, 1, 1),
                    topFont: calibri48,
                    bottomFont: calibri36),
                CreateDefaultTextSlide("coral", "The Coral Facet",
                    "A little rebellious energy against a \"should\" that never existed.",
                    backgroundColor: SColor.FromHSV(305, 0.8, 0.85),
                    topFont: calibri48,
                    bottomFont: calibri36),
                CreateDefaultTextSlide("cyan", "The Cyan Facet",
                    "A balancing force.",
                    backgroundColor: SColor.FromHSV(170, 1, 1),
                    topFont: calibri48,
                    bottomFont: calibri36),
                CreateDefaultTextSlide("emerald", "The Emerald Facet",
                    "Diet and exercise",
                    backgroundColor: SColor.FromHSV(120, 1, 0.9),
                    topFont: calibri48,
                    bottomFont: calibri36),
                CreateDefaultTextSlide("stone", "The Stone Facet",
                    "Darkness. Anxiety and depression.",
                    backgroundColor: SColor.FromHSV(0, 0, 0.3),
                    topFont: calibri48,
                    bottomFont: calibri36),
                CreateDefaultTextSlide("saffron", "The Saffron Facet",
                    "Physical and mental health.",
                    backgroundColor: SColor.FromHSV(30, 1, 0.9),
                    topFont: calibri48,
                    bottomFont: calibri36),
                CreateDefaultTextSlide("moonstone", "The Moonstone Facet",
                    "Socialization, making friends.",
                    backgroundColor: SColor.FromHSV(200, 0.35, 0.8),
                    topFont: calibri48,
                    bottomFont: calibri36),
                CreateDefaultTextSlide("carnation", "The Carnation Facet",
                    "Falling in love.",
                    backgroundColor: SColor.FromHSV(305, 0.35, 0.8),
                    topFont: calibri48,
                    bottomFont: calibri36),
                CreateDefaultTextSlide("sapphire", "The Sapphire Facet",
                    "Creative works, drawing and writing.",
                    backgroundColor: SColor.FromHSV(190, 0.6, 0.75),
                    topFont: calibri48,
                    bottomFont: calibri36),
                CreateDefaultTextSlide("sienna", "The Sienna Facet",
                    "Movies, TV shows, games, videos, recordings.",
                    backgroundColor: SColor.FromHSV(10, 1, 0.3),
                    topFont: calibri48,
                    bottomFont: calibri36),
                CreateDefaultTextSlide("bluebell", "The Bluebell Facet",
                    "Professional programming.",
                    backgroundColor: SColor.FromHSV(190, 0.3, 0.9),
                    topFont: calibri48,
                    bottomFont: calibri36),
                CreateDefaultTextSlide("starflower", "The Starflower Facet",
                    "Hobbyist programming.",
                    backgroundColor: SColor.FromHSV(240, 0.4, 0.5),
                    topFont: calibri48,
                    bottomFont: calibri36),
                CreateDefaultTextSlide("bronze", "The Bronze Facet",
                    "Sacred duty.",
                    backgroundColor: SColor.FromHSV(30, 0.6, 0.45),
                    topFont: calibri48,
                    bottomFont: calibri36),
                CreateDefaultTextSlide("seashell", "The Seashell Facet",
                    "Life administration. Bills, taxes, maintenance.",
                    backgroundColor: SColor.FromHSV(25, 0.05, 0.95),
                    topFont: calibri48,
                    bottomFont: calibri36),
            };
            return slides;
        }

        private static HeliumScene CreateTitleSlide() => CreateDefaultTextSlide("title", "Give light...", "and the people will find their own way.");

        private static HeliumScene CreateDefaultTextSlide(string id, string top,
            string bottom, SColor? backgroundColor = null, SFont? topFont = null,
            SFont? bottomFont = null, SColor? topColor = null, SColor? bottomColor = null)
        {
            backgroundColor ??= DefaultBackground;
            topFont ??= DefaultFont;
            bottomFont ??= DefaultFont;
            topColor ??= DefaultTextColor;
            bottomColor ??= DefaultTextColor;

            var topTextElement = new TextElement
            {
                Text = top,
                Font = topFont,
                Color = topColor!.Value,
                Alignment = Alignment.BottomCenter,
                Id = $"{id}-top"
            };
            var bottomTextElement = new TextElement
            {
                Text = bottom,
                Font = bottomFont,
                Color = bottomColor!.Value,
                Alignment = Alignment.TopCenter,
                Id = $"{id}-bottom"
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
            topContainer.Child = topTextElement;
            bottomContainer.Child = bottomTextElement;
            var binaryContainer = new BinaryElementContainer();
            binaryContainer.SplitHorizontal(1, 1);
            binaryContainer.FirstSplit!.SetSingleChild(topContainer);
            binaryContainer.SecondSplit!.SetSingleChild(bottomContainer);
            var slide = new HeliumScene
            {
                BackgroundColor = backgroundColor!.Value,
                Root = binaryContainer
            };
            return slide;
        }

        public static DelegateTransition BuildSlideToSlideTransition(HeliumScene from, HeliumScene to, SSizeF maxSize,
            MeasurementService measurementService)
        {
            var builder = TransformBuilder.Start(from, to, maxSize, measurementService)
                .WhenDeparting(builder => builder.ForAll().FadeOut(Easings.Smoothstep))
                .WhenArriving(builder => builder.ForAll().FadeIn(Easings.Smoothstep));
            var transformSet = builder.BuildSet();
            return new DelegateTransition(1d,
                (p, r) =>
                {
                    r.Clear(MathHelpers.InterpolateColor(from.BackgroundColor,
                        to.BackgroundColor,
                        Easings.Linear(p)));
                    var renderables = transformSet.ApplyAll(p);
                    foreach (var renderable in renderables)
                    {
                        renderable.Render(r);
                    }
                });
        }
    }
}

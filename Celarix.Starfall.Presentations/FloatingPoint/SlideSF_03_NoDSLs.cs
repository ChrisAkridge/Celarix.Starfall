using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Layout.Atria.Animation;
using Celarix.Starfall.Layout.Atria.Basis;
using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Presentations.FloatingPoint
{
    internal sealed class SlideSF_03_NoDSLs : AtriaSlide
    {
        private int _state;

        public SlideSF_03_NoDSLs(int width, int height) : base(width, height)
        {
        }

        public override void Initialize()
        {
            BackgroundColor = Constants.StarfallBackground;
        }

        public override SlideAdvanceResult Advance()
        {
            if (_state == 0)
            {
                var awkLogo = ImageElement.FromFile("Assets/Images/awk_logo.png", "#awkLogo");
                var awkAnchor = new BasisPoint(new(Center.X, Size.Height * (1d / 4d)), "#awkAnchor");
                awkLogo.AnchorCenterTo(awkAnchor);
                awkLogo.Size = new SSizeF(awkLogo.Size.Width / 6d, awkLogo.Size.Height / 6d);
                Add([awkAnchor, awkLogo])
                    .AnimateBasic(0.5d, AnimationTypes.FadeIn, Easings.Linear);
                _state = 1;
                return SlideAdvanceResult.InternalStateChanged;
            }
            else if (_state == 1)
            {
                var povrayLogo = ImageElement.FromFile("Assets/Images/povray_logo.png", "#povrayLogo");
                var povrayAnchor = new BasisPoint(new(Center.X, Size.Height * (3d / 4d)), "#povrayAnchor");
                povrayLogo.AnchorCenterTo(povrayAnchor);
                povrayLogo.Size = new SSizeF(povrayLogo.Size.Width / 6d, povrayLogo.Size.Height / 6d);
                Add([povrayAnchor, povrayLogo])
                    .AnimateBasic(0.5d, AnimationTypes.FadeIn, Easings.Linear);
                _state = 2;
                return SlideAdvanceResult.InternalStateChanged;
            }
            else if (_state == 2)
            {
                var vhdlText = new TextBlock("#vhdlText")
                {
                    Text = "VHDL",
                    FontFamily = "Calibri",
                    FontSize = 60d,
                    Color = SColor.White
                };
                var vhdlAnchor = new BasisPoint(new(Size.Width * (1d / 4d), Center.Y), "#vhdlAnchor");
                vhdlText.AnchorCenterTo(vhdlAnchor);
                Add([vhdlAnchor, vhdlText])
                    .AnimateBasic(0.5d, AnimationTypes.FadeIn, Easings.Linear);
                _state = 3;
                return SlideAdvanceResult.InternalStateChanged;
            }
            else if (_state == 3)
            {
                var verilogText = new TextBlock("#verilogText")
                {
                    Text = "Verilog",
                    FontFamily = "Calibri",
                    FontSize = 60d,
                    Color = SColor.White
                };
                var verilogAnchor = new BasisPoint(new(Size.Width * (3d / 4d), Center.Y), "#verilogAnchor");
                verilogText.AnchorCenterTo(verilogAnchor);
                Add([verilogAnchor, verilogText])
                    .AnimateBasic(0.5d, AnimationTypes.FadeIn, Easings.Linear);
                _state = 4;
                return SlideAdvanceResult.InternalStateChanged;
            }
            return SlideAdvanceResult.CanAdvance;
        }
    }
}

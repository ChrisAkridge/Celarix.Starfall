using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Layout.Atria.Basis;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Playground.AtriaTests
{
    public sealed class MathFunSlide : AtriaSlide
    {
        private static readonly SColor _backgroundColor = new SColor(8, 0, 130, 255);

        public MathFunSlide(int width, int height) : base(width, height)
        {
        }

        public override void Initialize()
        {
            BackgroundColor = _backgroundColor;

            var additionTable = new AdditionTableElement("#additionTable")
            {
                Size = new SSizeF(600d, 600d),
                CarryingOne = true,
            };
            var anchor = new BasisPoint(Center, "#additionAnchor");
            additionTable.AnchorCenterTo(anchor);
            Add([additionTable, anchor]);
        }

        public override void KeyUp(SKeyboardEvent keyboardEvent)
        {
            if (keyboardEvent.Key == SKey.Space)
            {
                var additionTable = (AdditionTableElement)Query("#additionTable").Single();
                additionTable.CarryingOne = !additionTable.CarryingOne;
            }
        }
    }
}

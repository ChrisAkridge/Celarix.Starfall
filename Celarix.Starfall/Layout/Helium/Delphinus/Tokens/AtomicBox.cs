using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Delphinus.Tokens
{
    internal sealed class AtomicBox : Token
    {
        private readonly List<Token> children;

        /// <summary>
        /// Gets a value indicating how much the content of this box should be scaled relative to the
        /// size of its parent's font size. For example, a value of 1.0 means the content should be rendered at the
        /// parent's font size, while a value of 0.5 means it should be rendered at half the parent's font size.
        /// </summary>
        public double ScaleFactor { get; }

        /// <summary>
        /// Gets a value indicating how much the content of this box should be offset vertically
        /// relative to the baseline of its parent. The unit is in multiples of the parent's font size.
        /// For example, a value of 0.5 means the content should be offset upwards by half the parent's
        /// font size, while a value of -0.5 means it should be offset downwards by half the parent's font
        /// size.
        /// </summary>
        public double VerticalOffset { get; }

        /// <summary>
        /// Gets a value indicating how much the content of this box should be offset horizontally
        /// relative to the baseline of its parent. The unit is in multiples of the parent's font size
        /// - specifically, the size of an "em" in the parent's font. For example, a value of 0.5 means
        /// the content should be offset to the right by half an "em" in the parent's font, while a value of -0.5
        /// means it should be offset to the left by half an "em" in the parent's font.
        /// </summary>
        public double HorizontalOffset { get; }

        public IList<Token> Children => children;

        public AtomicBox(StyleContext styleContext, double scaleFactor, double verticalOffset, double horizontalOffset)
        {
            StyleContext = styleContext;
            ScaleFactor = scaleFactor;
            VerticalOffset = verticalOffset;
            HorizontalOffset = horizontalOffset;
            children = new List<Token>();
        }

        public void AddChild(Token child)
        {
            children.Add(child);
        }
    }
}

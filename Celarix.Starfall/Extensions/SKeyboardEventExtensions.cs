using Celarix.Starfall.Rendering.Models;
using OpenTK.Windowing.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Extensions
{
    public static class SKeyboardEventExtensions
    {
        public static SKeyboardEvent ToSKeyboardEvent(this KeyboardKeyEventArgs args)
        {
            return new(args.Key.ToSKey(),
                args.ScanCode,
                args.Modifiers.ToSKeyModifiers(),
                args.IsRepeat);
        }
    }
}

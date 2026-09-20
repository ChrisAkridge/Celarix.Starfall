using Celarix.Starfall.Rendering.Models;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Extensions
{
    public static class SKeyModifiersExtensions
    {
        public static SKeyModifiers ToSKeyModifiers(this KeyModifiers modifiers)
        {
            var result = 0;
            if (modifiers.HasFlag(KeyModifiers.Shift))
            {
                result |= (int)SKeyModifiers.Shift;
            }
            if (modifiers.HasFlag(KeyModifiers.Control))
            {
                result |= (int)SKeyModifiers.Control;
            }
            if (modifiers.HasFlag(KeyModifiers.Alt))
            {
                result |= (int)SKeyModifiers.Alt;
            }
            if (modifiers.HasFlag(KeyModifiers.Super))
            {
                result |= (int)SKeyModifiers.Super;
            }
            if (modifiers.HasFlag(KeyModifiers.CapsLock))
            {
                result |= (int)SKeyModifiers.CapsLock;
            }
            if (modifiers.HasFlag(KeyModifiers.NumLock))
            {
                result |= (int)SKeyModifiers.NumLock;
            }
            return (SKeyModifiers)result;
        }
    }
}

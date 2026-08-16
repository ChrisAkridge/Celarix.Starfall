using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Rendering.Models
{
    public readonly struct SKeyboardEvent
    {
        public SKey Key { get; }
        public int ScanCode { get; }
        public SKeyModifiers Modifiers { get; }
        public bool IsRepeat { get; }

        public bool Alt => Modifiers.HasFlag(SKeyModifiers.Alt);
        public bool Control => Modifiers.HasFlag(SKeyModifiers.Control);
        public bool Shift => Modifiers.HasFlag(SKeyModifiers.Shift);
        public bool Command => Modifiers.HasFlag(SKeyModifiers.Super);

        public SKeyboardEvent(SKey key, int scanCode, SKeyModifiers modifiers, bool isRepeat)
        {
            Key = key;
            ScanCode = scanCode;
            Modifiers = modifiers;
            IsRepeat = isRepeat;
        }
    }
}

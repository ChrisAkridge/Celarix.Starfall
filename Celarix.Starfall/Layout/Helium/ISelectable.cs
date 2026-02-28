using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium
{
    public interface ISelectable<T> where T : ISelectable<T>
    {
        string? Id { get; }
        IReadOnlyList<string> Classes { get; }
        IReadOnlyList<T> Children { get; }
        bool HasClass(string className);
        void AddClass(string className);
        void RemoveClass(string className);
    }
}

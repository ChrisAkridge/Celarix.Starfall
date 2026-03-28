using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Delphinus.Tokens
{
    internal sealed class OpenSelectionSpan : Token
    {
        private readonly string[] _classes;

        public string Id { get; }
        public IReadOnlyList<string> Classes => _classes;

        public OpenSelectionSpan(string id, IEnumerable<string> classes)
        {
            Id = id;
            _classes = [.. classes];
        }
    }
}

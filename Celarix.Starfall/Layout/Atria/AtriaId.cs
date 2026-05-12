using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Atria
{
    public sealed class AtriaId
    {
        private readonly List<string> _classes = new();

        public string? Id { get; }
        public IReadOnlyList<string> Classes => _classes;

        private AtriaId(string? id, IEnumerable<string> classes)
        {
            Id = id;
            _classes.AddRange(classes);
        }

        public static AtriaId Parse(string idString)
        {
            var parts = idString.Split(['.', '#'], StringSplitOptions.RemoveEmptyEntries);
            string? id = null;
            var classes = new List<string>();
            foreach (var part in parts)
            {
                if (id == null && idString.Contains('#' + part))
                {
                    id = part;
                }
                else
                {
                    classes.Add(part);
                }
            }
            return new AtriaId(id, classes);
        }
    }
}

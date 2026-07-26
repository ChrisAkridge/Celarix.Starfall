using Celarix.Starfall.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Atria
{
    public sealed class AtriaId : IIdentifiable
    {
        private readonly List<string> _classes = [];

        public string? Id { get; }
        public IReadOnlyList<string> Classes => _classes;

        private AtriaId(string? id, IEnumerable<string> classes)
        {
            Id = id;
            _classes.AddRange(classes);
        }

        public bool Matches(string selector)
        {
            return Identification.Matches(this, selector);
        }

        public static AtriaId Parse(string idString) => Identification.Parse(idString, (id, classes) => new AtriaId(id, classes));

        public override string ToString()
        {
            string idPart = "";
            if (Id != null)
            {
                idPart = $"#{Id}";
            }

            var classPart = string.Join("", _classes.Select(c => $".{c}"));

            return $"{idPart}{classPart}";
        }
    }
}

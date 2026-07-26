using Celarix.Starfall.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra
{
    public readonly struct LibraId : IIdentifiable
    {
        private readonly List<string> _classes = [];

        public Guid InternalId { get; } = Guid.NewGuid();
        public string? Id { get; }
        public readonly IReadOnlyList<string> Classes => _classes;

        public LibraId(string? id, IEnumerable<string> classes)
        {
            Id = id;
            _classes.AddRange(classes);
        }

        internal LibraId(Guid internalId, string? id, IEnumerable<string> classes)
        {
            InternalId = internalId;
            Id = id;
            _classes.AddRange(classes);
        }

        public LibraRenderableKey RenderableKey(string role) => new(InternalId, role);

        public bool Matches(string selector) => Identification.Matches(this, selector);
        public static LibraId Parse(string? idString)
        {
            if (idString == null)
            {
                return new LibraId(null, []);
            }

            return Identification.Parse(idString, (id, classes) => new LibraId(id, classes));
        }

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

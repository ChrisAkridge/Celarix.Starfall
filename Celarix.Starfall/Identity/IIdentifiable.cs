using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Identity
{
    public interface IIdentifiable
    {
        public string? Id { get; }
        public IReadOnlyList<string> Classes { get; }
    }
}

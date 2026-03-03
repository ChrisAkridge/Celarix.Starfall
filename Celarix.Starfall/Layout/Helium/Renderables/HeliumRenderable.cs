using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Renderables
{
    public abstract class HeliumRenderable : IRenderable, ISelectable<HeliumRenderable>
    {
        private readonly List<string> classes = [];

        public string? Id { get; init; }

        public IReadOnlyList<string> Classes => classes;

        public IReadOnlyList<HeliumRenderable> Children => [];

        public bool HasClass(string className) => classes.Contains(className);

        public void AddClass(string className)
        {
            // TODO: raise error if class is already present
            if (!classes.Contains(className))
            {
                classes.Add(className);
            }
        }

        public void RemoveClass(string className)
        {
            // TODO: raise error if class is not present
            classes.Remove(className);
        }

        public abstract void Render(IRenderTarget target);
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium
{
    public static class QuerySelector
    {
        public static T? ById<T>(T root, string id) where T : ISelectable<T>
        {
            var candidates = new List<T?>();
            if (root.Id == id)
            {
                candidates.Add(root);
            }
            foreach (var child in root.Children)
            {
                candidates.AddRange(ById(child, id));
            }
            
            if (candidates.Count > 1)
            {
                throw new InvalidOperationException($"Expected exactly one element with id '{id}', but found {candidates.Count}.");
            }

            return candidates.SingleOrDefault();
        }

        public static IEnumerable<T> ByClass<T>(T root, string className) where T : ISelectable<T>
        {
            var candidates = new List<T>();
            if (root.HasClass(className))
            {
                candidates.Add(root);
            }
            foreach (var child in root.Children)
            {
                candidates.AddRange(ByClass(child, className));
            }
            return candidates;
        }

        public static IEnumerable<T> ByAnotherClass<T>(IEnumerable<T> elements, string className) where T : ISelectable<T>
        {
            var candidates = new List<T>();
            foreach (var element in elements)
            {
                if (element.HasClass(className))
                {
                    candidates.Add(element);
                }
            }
            return candidates;
        }
    }
}

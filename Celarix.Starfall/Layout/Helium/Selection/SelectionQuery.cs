using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Selection
{
    public sealed class SelectionQuery<T>
        where T : ISelectable<T>
    {
        private readonly List<string> classNames = new();
        private SelectionQuery<T>? nextQuery;

        public string? Id { get; init; }
        public IReadOnlyList<string> ClassNames => classNames;

        public static SelectionQuery<T> StartById(string id) => new SelectionQuery<T> { Id = id };
        public static SelectionQuery<T> StartByClass(string className)
        {
            var query = new SelectionQuery<T>();
            query.classNames.Add(className);
            return query;
        }

        public SelectionQuery<T> ByClass(string nextClassName)
        {
            classNames.Add(nextClassName);
            return this;
        }

        public SelectionQuery<T> ThenBy(SelectionQuery<T> nextQuery)
        {
            this.nextQuery = nextQuery;
            return this;
        }

        public IReadOnlyList<T> Query(ISelectable<T> root)
        {
            var results = new List<T>();

            if (Id != null && root.Id == Id)
            {
                // this is a very good idea
                // i am smart and good programmer
                results.Add((T)root);
            }

            if (ClassNames.Any(c => root.Classes.Contains(c)))
            {
                results.Add((T)root);
            }

            foreach (var child in root.Children)
            {
                results.AddRange(Query(child));
            }

            if (nextQuery != null)
            {
                return nextQuery.Query(results.Cast<ISelectable<T>>());
            }
            else
            {
                return [.. results.Distinct()];
            }
        }

        public IReadOnlyList<T> Query(IEnumerable<ISelectable<T>> items)
        {
            var results = new List<T>();
            foreach (var item in items)
            {
                results.AddRange(Query(item));
            }
            return results;
        }
    }
}

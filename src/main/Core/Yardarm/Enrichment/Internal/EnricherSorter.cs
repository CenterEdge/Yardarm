using System;
using System.Collections.Generic;
using System.Linq;

namespace Yardarm.Enrichment.Internal
{
    /// <summary>
    /// Sorts enrichers as a directed acyclic graph to ensure they are processed
    /// in the desired order.
    /// </summary>
    internal class EnricherSorter
    {
        /// <summary>
        /// Default instance.
        /// </summary>
        public static EnricherSorter Default { get; } = new();

        /// <summary>
        /// Sorts enrichers as a directed acyclic graph to ensure they are processed
        /// in the desired order.
        /// </summary>
        /// <param name="enrichers">Enrichers to sort.</param>
        /// <returns>A sorted list of enrichers.</returns>
        public IEnumerable<T> Sort<T>(IEnumerable<T> enrichers)
            where T : IEnricher
        {
            ArgumentNullException.ThrowIfNull(enrichers, nameof(enrichers));

            if (enrichers is IList<T> list)
            {
                // Short-circuit optimizations for short lists of known length

                if (list.Count == 0)
                {
                    yield break;
                }

                if (list.Count == 1)
                {
                    yield return list[0];
                    yield break;
                }
            }

            List<Node<T>> nodes = enrichers.Select(p => new Node<T>(p)).ToList();
            var sorted = new List<Node<T>>(nodes.Count);

            while (true)
            {
                var nextNode = nodes.FirstOrDefault(p => !p.IsSorted);
                if (nextNode == null)
                {
                    break;
                }

                nextNode.Visit(nodes, sorted);
            }

            for (int i = sorted.Count - 1; i >= 0; i--)
            {
                yield return sorted[i].Enricher;
            }
        }

        private class Node<T>
            where T : IEnricher
        {
            private readonly Type _enricherType;

            // Tracks nodes currently in the recursive visit path to detect circular references
            private bool _temporaryMark;

            public T Enricher { get; }

            /// <summary>
            /// True once the node has been added to the sorted list.
            /// </summary>
            public bool IsSorted { get; private set; }

            public Node(T enricher)
            {
                Enricher = enricher;
                _enricherType = enricher.GetType();
            }

            public void Visit(List<Node<T>> nodes, List<Node<T>> sorted)
            {
                if (IsSorted)
                {
                    return;
                }
                if (_temporaryMark)
                {
                    throw new InvalidOperationException(
                        $"Invalid enricher dependency graph detected, circular reference to {Enricher.GetType()}");
                }

                _temporaryMark = true;

                foreach (var node in nodes.Where(p => p != this))
                {
                    if (DependsOnMe(node))
                    {
                        node.Visit(nodes, sorted);
                    }
                }

                _temporaryMark = false;
                IsSorted = true;

                sorted.Add(this);
            }

            private bool DependsOnMe(Node<T> other)
            {
                // Since this is a hot path, use for loops instead of foreach or LINQ to reduce heap allocations

                Type[] executeAfter = other.Enricher.ExecuteAfter;
                // ReSharper disable once ForCanBeConvertedToForeach
                for (int i = 0; i < executeAfter.Length; i++)
                {
                    if (executeAfter[i] == _enricherType)
                    {
                        return true;
                    }
                }

                Type[] executeBefore = Enricher.ExecuteBefore;
                // ReSharper disable once ForCanBeConvertedToForeach
                for (int i = 0; i < executeBefore.Length; i++)
                {
                    if (executeBefore[i] == other._enricherType)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}

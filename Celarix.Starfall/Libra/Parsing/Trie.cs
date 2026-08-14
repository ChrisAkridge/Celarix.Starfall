using Celarix.Starfall.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra.Parsing
{
    internal sealed class TrieNode
    {
        public string Value { get; set; }
        public TrieNode?[] Children { get; }

        public TrieNode(string value, int alphabetSize)
        {
            Value = value;
            Children = new TrieNode[alphabetSize];
        }
    }

    internal static class Trie
    {
        public static void Insert(TrieNode node, string value, IReadOnlyList<char> alphabet)
        {
            for (var i = 0; i < value.Length; i++)
            {
                var index = Celarix.Starfall.Extensions.IReadOnlyListExtensions.IndexOf(alphabet, value[i]);
                if (node.Children[index] == null)
                {
                    node.Children[index] = new TrieNode(value[..(i + 1)], alphabet.Count);
                }
                node = node.Children[index]!;
            }
            node.Value = value;
        }

        public static bool Search(TrieNode node, string value, IReadOnlyList<char> alphabet)
        {
            for (var i = 0; i < value.Length; i++)
            {
                var index = Celarix.Starfall.Extensions.IReadOnlyListExtensions.IndexOf(alphabet, value[i]);
                if (node.Children[index] == null)
                {
                    return false;
                }
                node = node.Children[index]!;
            }
            return node.Value == value;
        }
    }
}

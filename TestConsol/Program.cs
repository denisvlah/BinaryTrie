using System;
using BinaryTrieImpl;

namespace TestConsol
{
    class Program
    {
        static void Main(string[] args)
        {
            //var container = new ArrayBackedNodesContainer<int>(initialSize:32000000);
            var container = new GrowableArrayBackedNodeContainer<int>();
            var trie = new PlugableBinaryTrie<int>(container);

            for(int i=1000000; i>0; i--)
            {
                trie.Add(i, i);
            }

            var allOk = true;
            for(int i=1; i<=1000000; i++)
            {
                var value = trie.GetValue(i);
                allOk = allOk & value == i;
            }

            if (allOk)
            {
                Console.WriteLine("All Ok.");
            }
            else
            {
                Console.WriteLine("Something is wrong.");
            }
        }
    }

    
}
using System;
using System.Linq;
using BinaryTrieImpl;
using Xunit;

namespace BinaryTrieTests
{
    public class TrieTests
    {
        public PlugableBinaryTrie<int> GetTrie(NodeContainerType t, int? initialSize = null)
        {
            var size = initialSize ?? 90000;
            INodesContainer<int> container;
            if (t == NodeContainerType.ArrayBacked)
            {
                container = new NodesContainer<int>(size);
            }
            else
            {
                container = new MemoryMappedNodeContainer<int>(RName(), 0, size); 
            }
            
            var trie = new PlugableBinaryTrie<int>(container);

            return trie;
        }  
        
        private string RName()
        {
            return "foo_" + Guid.NewGuid().ToString().Replace("-", "");
        }
        
        [Theory]
        [InlineData(NodeContainerType.ArrayBacked)]
        [InlineData(NodeContainerType.MemoryMappedBacked)]
        public void ItemsCanBeAddedAndRetrievedByKey(NodeContainerType t)
        {
            var trie = GetTrie(t);
            Assert.Equal(0, trie.Count);
            
            trie.Add(10,5);
            
            Assert.Equal(1, trie.Count);

            var value = -1;
            var hasKey = trie.TryGetValue(10, out value);
            Assert.True(hasKey);
            Assert.Equal(5, value);
        }

        [Theory]
        [InlineData(NodeContainerType.ArrayBacked)]
        [InlineData(NodeContainerType.MemoryMappedBacked)]
        public void ValueCanBeAddedByKey0(NodeContainerType t)
        {
            var trie = GetTrie(t);
            
            Assert.False(trie.TryGetValue(0, out int result));
            
            trie.Add(0, 100);

            var addedValue = -1;
            Assert.True(trie.TryGetValue(0, out addedValue));
            Assert.Equal(100, addedValue);
        }
        
        [Theory]
        [InlineData(NodeContainerType.ArrayBacked)]
        [InlineData(NodeContainerType.MemoryMappedBacked)]
        public void ValueCanBeAddedByKeyOne(NodeContainerType t)
        {
            var trie = GetTrie(t);
            
            Assert.False(trie.TryGetValue(1, out int result));
            
            trie.Add(1, 100);

            var addedValue = -1;
            Assert.True(trie.TryGetValue(1, out addedValue));
            Assert.Equal(100, addedValue);
        }

        [Theory]
        [InlineData(NodeContainerType.ArrayBacked)]
        [InlineData(NodeContainerType.MemoryMappedBacked)]
        public void ValueCanBeAddedByDoubleKey(NodeContainerType t)
        {
            var trie = GetTrie(t);
            trie.Add(1, 10, false);
            trie.Add(2, 10, true);
            
            Assert.False(trie.TryGetValue(1, out _));
            Assert.False(trie.TryGetValue(2, out _));
            Assert.False(trie.TryGetValue(0, out _));

            var addedValue = -1;
            trie.TryGetValue(1, out addedValue, false);
            Assert.True(trie.TryGetValue(2, out addedValue, true));
            
            Assert.Equal(1, trie.Count);
            
            trie.Add(1,11);
            trie.Add(2,12);
            
            Assert.True(trie.TryGetValue(1, out _));
            Assert.True(trie.TryGetValue(2, out _));
            trie.TryGetValue(1, out _, false);
            Assert.True(trie.TryGetValue(2, out _));
        }

        [Theory]
        [InlineData(NodeContainerType.ArrayBacked)]
        [InlineData(NodeContainerType.MemoryMappedBacked)]
        public void ValueCanRemoved(NodeContainerType t)
        {
            var trie = GetTrie(t);
            
            trie.Add(1,10);
            Assert.True(trie.TryGetValue(1, out _));
            Assert.True(trie.Count==1);
            
            Assert.True(trie.TryRemove(1));
            Assert.Equal(0, trie.Count);
            Assert.False(trie.TryRemove(1));
            
            Assert.False(trie.TryGetValue(1, out _));
        }

        [Theory]
        [InlineData(NodeContainerType.ArrayBacked)]
        [InlineData(NodeContainerType.MemoryMappedBacked)]
        public void KeyValuesCanBeSortedByKey(NodeContainerType t)
        {
            var trie = GetTrie(t, initialSize: 10000000);

            for(int i = 100000; i > 0; i--)
            {
                trie.Add(i, i);
            }

            
            for (int i = 1; i <= 100000; i++)
            {
                var value = -1;
                var hasKey = trie.TryGetValue(i, out value);
                Assert.True(hasKey);
                Assert.Equal(i, value);
            }

            var expectedKey = 1;
            foreach(var (key, value) in trie.GetEntrySet())
            {
                Assert.Equal(1, key.Count);
                var keyValue = key[0];
                Assert.Equal(keyValue, value);
                
                Assert.True(expectedKey == keyValue);
                expectedKey++;
            }
            
            Assert.Equal(100001, expectedKey);
        }

        [Theory]
        [InlineData(NodeContainerType.ArrayBacked)]
        [InlineData(NodeContainerType.MemoryMappedBacked)]
        public void ComplexKeysCanBeSorted(NodeContainerType t)
        {
            var trie = GetTrie(t, initialSize: 100000);
            trie.Add(new []{3, 1}, 1);
            trie.Add(new []{2, 10}, 10);

            Assert.Equal(2, trie.Count);

            var missingValue = trie.GetValue(new []{3, 50}, -1);

            Assert.Equal(-1, missingValue);

            Assert.Equal(1, trie.GetValue(new []{3,1}));
            Assert.Equal(10, trie.GetValue(new []{2, 10}));

            var sortedEntries = trie.GetEntrySet().ToArray();

            Assert.Equal(2, sortedEntries.Length);
            Assert.Equal(10, sortedEntries[0].Item2);
            Assert.Equal(1, sortedEntries[1].Item2);

            Assert.Equal(2, sortedEntries[0].Item1[0]);
            Assert.Equal(10, sortedEntries[0].Item1[1]);

            Assert.Equal(3, sortedEntries[1].Item1[0]);
            Assert.Equal(1, sortedEntries[1].Item1[1]);

        }        
     }

    public enum NodeContainerType
    {
        ArrayBacked,
        MemoryMappedBacked
    }
}
using System;
using System.IO;
using System.Linq;
using BinaryTrieImpl;
using Xunit;

namespace BinaryTrieTests
{
    public class MemoryMappedContainerTests: IDisposable
    {
        private MemoryMappedNodeContainer<int> c;
        
        public MemoryMappedContainerTests()
        {            
            c = new MemoryMappedNodeContainer<int>(RName(),90000);
        }

        public void Dispose()
        {
            c.Dispose();
            var files = Directory.EnumerateFiles(".")
                .Where(x => x.StartsWith("foo_"))
                .ToList();

            foreach (var file in files)
            {
                File.Delete(file);
            }

        }

        
        [Fact]
        public void SetValueWorks()
        {
            var node1 = c.AddNewNode();            
            c.SetValue(node1.CurrentIndex, -10);
            var sNode1 = c.Get(node1.CurrentIndex);
            Assert.Equal(sNode1.Value, -10);

            var node2 = c.AddNewNode();
            node2.Value = -100;
            node2.Node_0 = -10;
            node2.Node_1 = -10;
            var tmpNode = node2;
            c.ReassignNode(ref tmpNode);

            var sNode2 = c.Get(node2.CurrentIndex);
            Assert.Equal(-100, sNode2.Value);
            Assert.Equal(-10, sNode2.Node_0);
            Assert.Equal(-10, sNode2.Node_1);
        }

        [Fact]
        public void ValuesCountAndIndexCountCanBePersistedAndRestored()
        {
            c.InitFirstNode();
            var node2  = c.AddNewNode();
            node2.HasValue = true;
            node2.Value = 100;
            c.ReassignNode(ref node2);

            var node3 = c.AddNewNode();
            node3.HasValue = true;
            node3.Value = 200;
            c.ReassignNode(ref node3);

            c.IncrementValuesCount();
            c.IncrementValuesCount();
            c.IncrementValuesCount();
            c.DecrementValuesCount();

            var expectedValuesCount = 2;
            var expectedNodeIndex = 3;
            var expectedFullSize = c.FileSizeBytes;

            c.Dispose();

            var restoredContainer = new MemoryMappedNodeContainer<int>(c.FileName);

            Assert.Equal(expectedFullSize, c.FileSizeBytes);

            var actualValuesCount = restoredContainer.GetValuesCount();
            var actualNodeIndex = restoredContainer.GetLastNodeIndex();

            Assert.Equal(expectedValuesCount, actualValuesCount);
            Assert.Equal(expectedNodeIndex, actualNodeIndex);

            var actualNode2 = restoredContainer.Get(node2.CurrentIndex);
            var actualNode3 = restoredContainer.Get(node3.CurrentIndex);

            Assert.Equal(node2.HasValue, actualNode2.HasValue);
            Assert.Equal(node2.Value, actualNode2.Value);

            Assert.Equal(node3.HasValue, actualNode3.HasValue);
            Assert.Equal(node3.Value, actualNode3.Value);         

        }

        [Fact]
        public void BinaryTrieCanBeRestoredFromFile()
        {
            var trie = new PlugableBinaryTrie<int>(c);
            var item1 = 5;
            var item2 = 13;
            trie.Add(item1, item1);
            trie.Add(item2, item2);

            var expectedValuesCount = trie.Count;

            trie.Dispose();

            var restoredTrie = new PlugableBinaryTrie<int>(
                new MemoryMappedNodeContainer<int>(c.FileName)
            );

            Assert.Equal(expectedValuesCount, restoredTrie.Count);

            var sortedPairs = restoredTrie.GetEntrySet().ToArray();

            Assert.Equal(expectedValuesCount, sortedPairs.Length);

            Assert.Equal(item1, sortedPairs[0].Item2);
            Assert.Equal(item1, sortedPairs[0].Item1[0]);
            Assert.Equal(item2, sortedPairs[1].Item2);
            Assert.Equal(item2, sortedPairs[1].Item1[0]);

            restoredTrie.Add(5, 10);
            restoredTrie.Add(10,15);

            var storedValue = restoredTrie.GetValue(new []{ 13});

            Assert.Equal(13, storedValue);
        }

        [Fact]
        public void StructContainer(){
            Assert.False(SizeHelper.IsPersistent(typeof(string)));
            Assert.False(SizeHelper.IsPersistent(typeof(StructWithRefField)));
            Assert.True(SizeHelper.IsPersistent(typeof(int)));
            Assert.True(SizeHelper.IsPersistent(typeof(DateTime)));
            Assert.True(SizeHelper.IsPersistent(typeof(decimal)));
            
        }

        private string RName()
        {
            return "foo_" + Guid.NewGuid().ToString().Replace("-", "");
        }
    }

    struct StructWithRefField
    {
        public int F1;
        public string F2;
    }

    
}
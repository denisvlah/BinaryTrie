using System;
using System.IO;
using System.Linq;
using BinaryTrieImpl;
using Xunit;

namespace BinaryTrieTests
{
    public class TrieTests: IDisposable
    {
        PlugableBinaryTrie<int> trie;
        INodesContainer<int> container;
        public PlugableBinaryTrie<int> GetTrie(NodeContainerType t, int? initialSize = null)
        {
            var size = initialSize ?? 90000;
            
            if (t == NodeContainerType.ArrayBacked)
            {
                container = new ArrayBackedNodesContainer<int>(size);
            }
            else if (t == NodeContainerType.MemoryMappedBacked)
            {
                container = new MemoryMappedNodeContainer<int>("./gtrie.bin", size); 
            }
            else if (t == NodeContainerType.GrowableArrayBacked)
            {
                container = new GrowableArrayBackedNodeContainer<int>(100);
            }
            else
            {
                container = new GrowableMemoryMappedNodeContainer<int>("./gtrie","t{0}.bin",100);

            }
            
            trie = new PlugableBinaryTrie<int>(container);

            return trie;
        }

        public void Dispose()
        {
            trie?.Dispose();

            if (container != null)
            {
                if (container is MemoryMappedNodeContainer<int>)
                {
                    File.Delete("./gtrie.bin");
                }

                if (container is GrowableMemoryMappedNodeContainer<int>)
                {
                    Directory.Delete("./gtrie", true);
                }

            }

                        
        }
        
        
        [Theory]
        [InlineData(NodeContainerType.ArrayBacked)]
        [InlineData(NodeContainerType.MemoryMappedBacked)]
        [InlineData(NodeContainerType.GrowableArrayBacked)]
        [InlineData(NodeContainerType.GrowableMemoryMapped)]
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
        [InlineData(NodeContainerType.GrowableArrayBacked)]
        [InlineData(NodeContainerType.GrowableMemoryMapped)]
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
        [InlineData(NodeContainerType.GrowableArrayBacked)]
        [InlineData(NodeContainerType.GrowableMemoryMapped)]
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
        [InlineData(NodeContainerType.GrowableArrayBacked)]
        [InlineData(NodeContainerType.GrowableMemoryMapped)]
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
        [InlineData(NodeContainerType.GrowableArrayBacked)]
        [InlineData(NodeContainerType.GrowableMemoryMapped)]
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
        [InlineData(NodeContainerType.GrowableArrayBacked)]
        [InlineData(NodeContainerType.GrowableMemoryMapped)]
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
        [InlineData(NodeContainerType.GrowableArrayBacked)]
        [InlineData(NodeContainerType.GrowableMemoryMapped)]
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

        [Theory]
        [InlineData(NodeContainerType.ArrayBacked)]
        [InlineData(NodeContainerType.MemoryMappedBacked)]
        [InlineData(NodeContainerType.GrowableArrayBacked)]
        [InlineData(NodeContainerType.GrowableMemoryMapped)]
        public void ComplexKeyOfDifferentSizeCanBeSorted(NodeContainerType t)
        {
            var trie = GetTrie(t, initialSize: 100000);
            trie.Add(new []{2, 10, 232}, 10);
            trie.Add(new []{3, 1}, 1);
            

            Assert.Equal(2, trie.Count);
            var sortedEntries = trie.GetEntrySet().ToArray();

            Assert.Equal(2, sortedEntries.Length);
            
            //Checking values
            Assert.Equal(10, sortedEntries[0].Item2);
            Assert.Equal(1, sortedEntries[1].Item2);

            //Checking keys
            Assert.Equal(2, sortedEntries[0].Item1[0]);
            Assert.Equal(10, sortedEntries[0].Item1[1]);
            Assert.Equal(232, sortedEntries[0].Item1[2]);

            //Checking keys
            Assert.Equal(3, sortedEntries[1].Item1[0]);
            Assert.Equal(1, sortedEntries[1].Item1[1]);

        } 

        [Theory]
        [InlineData(NodeContainerType.ArrayBacked)]
        [InlineData(NodeContainerType.MemoryMappedBacked)]
        [InlineData(NodeContainerType.GrowableArrayBacked)]
        [InlineData(NodeContainerType.GrowableMemoryMapped)]
        public void KeysCanBeSortedWhileReusingKeysContainerForAllItems(NodeContainerType t)
        {
            var trie = GetTrie(t, initialSize: 100000);
            trie.Add(5, 10);
            trie.Add(2, 15);

            Assert.Equal(2, trie.Count);
            var entriesEnumerator = trie.GetEntrySet(true).GetEnumerator();
            Assert.True(entriesEnumerator.MoveNext());
            Assert.Equal(15, entriesEnumerator.Current.Item2);
            Assert.Equal(1, entriesEnumerator.Current.Item1.Count);
            Assert.Equal(2, entriesEnumerator.Current.Item1[0]);

            Assert.True(entriesEnumerator.MoveNext());
            Assert.Equal(10, entriesEnumerator.Current.Item2);
            Assert.Equal(1, entriesEnumerator.Current.Item1.Count);
            Assert.Equal(5, entriesEnumerator.Current.Item1[0]);

            Assert.False(entriesEnumerator.MoveNext());

        }

        [Theory]
        [InlineData(NodeContainerType.ArrayBacked)]
        [InlineData(NodeContainerType.MemoryMappedBacked)]
        [InlineData(NodeContainerType.GrowableArrayBacked)]
        [InlineData(NodeContainerType.GrowableMemoryMapped)]
        public void KeysWithSamePrefixAreSortedCorrectly(NodeContainerType t)
        {
            var trie = GetTrie(t, initialSize: 100000);
            trie.Add(new[]{1,2,3}, 5);
            trie.Add(new[]{1,2}, 9);

            var sortedKvp = trie.GetEntrySet().ToArray();
            Assert.Equal(2, sortedKvp.Length);

            Assert.Equal(9, sortedKvp[0].Item2);
            Assert.Equal(2, sortedKvp[0].Item1.Count);
            Assert.Equal(1, sortedKvp[0].Item1[0]);
            Assert.Equal(2, sortedKvp[0].Item1[1]);

            Assert.Equal(5, sortedKvp[1].Item2);
            Assert.Equal(3, sortedKvp[1].Item1.Count);
            Assert.Equal(1, sortedKvp[1].Item1[0]);
            Assert.Equal(2, sortedKvp[1].Item1[1]);
            Assert.Equal(3, sortedKvp[1].Item1[2]);


        }

        [Theory]
        [InlineData(NodeContainerType.ArrayBacked)]
        [InlineData(NodeContainerType.MemoryMappedBacked)]
        [InlineData(NodeContainerType.GrowableArrayBacked)]
        [InlineData(NodeContainerType.GrowableMemoryMapped)]
        public void UserFunctionCanBeInvokedOnExistingKeyComplexKey(NodeContainerType t)
        {
            var trie = GetTrie(t, initialSize: 100000);
            trie.Add(new[]{1,2,3}, 5);

            var isFunctionInvoked = false;
            
            trie.DoWithValue(new []{1,2,3}, (hasValue, currentValue)=>{
                isFunctionInvoked = true;
                Assert.True(hasValue);
                return currentValue * 2;
            });

            Assert.True(isFunctionInvoked);

            var newValue = trie.GetValue(new []{ 1,2,3});
            Assert.Equal(10, newValue);
            
        }

        [Theory]
        [InlineData(NodeContainerType.ArrayBacked)]
        [InlineData(NodeContainerType.MemoryMappedBacked)]
        [InlineData(NodeContainerType.GrowableArrayBacked)]
        [InlineData(NodeContainerType.GrowableMemoryMapped)]
        public void UserFunctionCanBeInvokedOnExistingKeySimpleKey(NodeContainerType t)
        {
            var trie = GetTrie(t, initialSize: 100000);
            trie.Add(3, 5);

            var isFunctionInvoked = false;
            
            trie.DoWithValue(3, (hasValue, currentValue)=>{
                isFunctionInvoked = true;
                Assert.True(hasValue);
                return currentValue * 2;
            });

            Assert.True(isFunctionInvoked);

            var newValue = trie.GetValue(3);
            Assert.Equal(10, newValue);
            
        }

        [Theory]
        [InlineData(NodeContainerType.ArrayBacked)]
        [InlineData(NodeContainerType.MemoryMappedBacked)]
        [InlineData(NodeContainerType.GrowableArrayBacked)]
        [InlineData(NodeContainerType.GrowableMemoryMapped)]
        public void UserFunctionCanBeInvokedOnNotExistingComplexKeyAndNotAdeed(NodeContainerType t)
        {
            var trie = GetTrie(t, initialSize: 100000);           

            var isFunctionInvoked = false;
            
            trie.DoWithValue(new []{1,2,3}, (hasValue, currentValue)=>{
                isFunctionInvoked = true;
                Assert.False(hasValue);
                return 1000;
            }, false);

            Assert.True(isFunctionInvoked);

            var newValue = trie.GetValue(new []{ 1,2,3}, -10);
            Assert.Equal(-10, newValue);            
        }

        
        [Theory]
        [InlineData(NodeContainerType.ArrayBacked)]
        [InlineData(NodeContainerType.MemoryMappedBacked)]
        [InlineData(NodeContainerType.GrowableArrayBacked)]
        [InlineData(NodeContainerType.GrowableMemoryMapped)]
        public void UserFunctionCanBeInvokedOnNotExistingSimpleKeyAndNotAdeed(NodeContainerType t)
        {
            var trie = GetTrie(t, initialSize: 100000);           

            var isFunctionInvoked = false;
            
            trie.DoWithValue(3, (hasValue, currentValue)=>{
                isFunctionInvoked = true;
                Assert.False(hasValue);
                return 1000;
            }, false);

            Assert.True(isFunctionInvoked);

            var newValue = trie.GetValue(3, -10);
            Assert.Equal(-10, newValue);            
        }

        [Theory]
        [InlineData(NodeContainerType.ArrayBacked)]
        [InlineData(NodeContainerType.MemoryMappedBacked)]
        [InlineData(NodeContainerType.GrowableArrayBacked)]
        [InlineData(NodeContainerType.GrowableMemoryMapped)]
        public void UserFunctionCanBeInvokedOnNotExistingComplexKeyAndAdeed(NodeContainerType t)
        {
            var trie = GetTrie(t, initialSize: 100000);           

            var isFunctionInvoked = false;
            
            trie.DoWithValue(new []{1,2,3}, (hasValue, currentValue)=>{
                isFunctionInvoked = true;
                Assert.False(hasValue);
                return 1000;
            });

            Assert.True(isFunctionInvoked);

            var newValue = trie.GetValue(new []{ 1,2,3}, -10);
            Assert.Equal(1000, newValue);            
        }

        [Theory]
        [InlineData(NodeContainerType.ArrayBacked)]
        [InlineData(NodeContainerType.MemoryMappedBacked)]
        [InlineData(NodeContainerType.GrowableArrayBacked)]
        [InlineData(NodeContainerType.GrowableMemoryMapped)]
        public void UserFunctionCanBeInvokedOnNotExistingSimpleKeyAndAdeed(NodeContainerType t)
        {
            var trie = GetTrie(t, initialSize: 100000);           

            var isFunctionInvoked = false;
            
            trie.DoWithValue(3, (hasValue, currentValue)=>{
                isFunctionInvoked = true;
                Assert.False(hasValue);
                return 1000;
            });

            Assert.True(isFunctionInvoked);

            var newValue = trie.GetValue(3, -10);
            Assert.Equal(1000, newValue);            
        }

       
    }

     

    public enum NodeContainerType
    {
        MemoryMappedBacked,
        ArrayBacked,
        GrowableArrayBacked,
        GrowableMemoryMapped
        
    }
}
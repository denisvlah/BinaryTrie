using System;
using System.Collections.Generic;
using System.Linq;
using BinaryTrieImpl;
using Xunit;
using Xunit.Abstractions;

namespace BinaryTrie.PerformanceTests{
    public class SortingByKeysTests {
        private ITestOutputHelper output;
        private int TotalKeys = 5000000;

        public SortingByKeysTests(ITestOutputHelper output)
        {
            this.output = output;            
        }

        [Fact]
        public void UsingIntKeysOnSortedDictionary(){
            object f1(){
                var dict = new SortedDictionary<int,int>();

                for (int i=TotalKeys; i>=0; i--){
                    dict[i] = i;
                }

                Assert.Equal(TotalKeys + 1, dict.Count);

                var lastKey = -1;
                foreach(var kvp in dict){
                    Assert.Equal(kvp.Key, kvp.Value);
                    Assert.True(kvp.Key >= lastKey);
                    lastKey = kvp.Key;
                }

                return dict;
            }

            var oldCount = TotalKeys;
            TotalKeys = 100000;
            f1();
            TotalKeys = oldCount;

            var execution = H.Run(f1);
            this.output.WriteLine(execution.ToString());            
        }

        [Fact]
        public void UsingIntKeysOnArrayBackedGrowableTrie(){            
            object f1(){
                var container = new GrowableArrayBackedNodeContainer<int>(5000000, true);
                
                var trie = new PlugableBinaryTrie<int>(container);

                for (int i=TotalKeys; i>=0; i--){
                    trie.Add(i, i);
                }

                Assert.Equal(TotalKeys + 1, trie.Count);

                var lastKey = -1;
                var allOk = true;
                foreach(var kvp in trie.GetEntrySet(reuseKeysList: true)){
                    var key = kvp.Item1[0];
                    var value = kvp.Item2;

                    allOk = allOk && key == value;
                    allOk = allOk && key >= lastKey;                                        
                    lastKey = key;
                }

                Assert.True(allOk);

                return trie;
            }

            var oldCount = TotalKeys;
            TotalKeys = 100000;
            var disposable = f1() as IDisposable;
            disposable.Dispose();
            TotalKeys = oldCount;            
            var execution = H.Run(f1);
            this.output.WriteLine(execution.ToString());           
            
        }

        [Fact]
        public void UsingIntKeysOnArrayBackedTrie()
        {
            object f1()
            {
                var container = new ArrayBackedNodesContainer<int>(TotalKeys*32);
                var trie = new PlugableBinaryTrie<int>(container);

                for (int i = TotalKeys; i >= 0; i--)
                {
                    trie.Add(i, i);
                }

                Assert.Equal(TotalKeys + 1, trie.Count);

                var lastKey = -1;
                var allOk = true;
                foreach (var kvp in trie.GetEntrySet(reuseKeysList: true))
                {
                    var key = kvp.Item1[0];
                    var value = kvp.Item2;
                    allOk = allOk && key == value;                    
                    allOk = allOk && key >= lastKey;                    
                    Assert.True(allOk);
                    lastKey = key;
                }

                Assert.True(allOk);

                return trie;
            }

            var oldCount = TotalKeys;
            TotalKeys = 100000;
            var disposable = f1() as IDisposable;
            disposable.Dispose();
            TotalKeys = oldCount;

            var execution = H.Run(f1);
            this.output.WriteLine(execution.ToString());
        }

        [Fact]
        public void UsingIntKeysOnDictionaryWithoutCapacity(){
            object f1(){
                var dict = new Dictionary<int,int>();

                for (int i=TotalKeys; i>=0; i--){
                    dict[i] = i;
                }

                Assert.Equal(TotalKeys +1 , dict.Count);

                var sortedKeys = dict.OrderBy(x=>x.Key);

                var lastKey = -1;
                var allOk = true;
                foreach(var kvp in sortedKeys){
                    allOk = allOk && kvp.Key == kvp.Value;
                    allOk = allOk && kvp.Key >= lastKey;                    
                    lastKey = kvp.Key;
                }
                Assert.True(allOk);

                return dict;
            }

            var oldCount = TotalKeys;
            TotalKeys = 100000;
            f1();
            TotalKeys = oldCount;

            var execution = H.Run(f1);
            this.output.WriteLine(execution.ToString());            
        }

        [Fact]
        public void UsingIntKeysOnDictionaryWithCapacity(){
            object f1(){
                var dict = new Dictionary<int,int>(TotalKeys+1);

                for (int i=TotalKeys; i>=0; i--){
                    dict[i] = i;
                }

                Assert.Equal(TotalKeys +1 , dict.Count);

                var sortedKeys = dict.OrderBy(x=>x.Key);

                var lastKey = -1;
                var allOk = true;
                foreach(var kvp in sortedKeys){
                    allOk = allOk && kvp.Key == kvp.Value;
                    allOk = allOk && kvp.Key >= lastKey;                    
                    lastKey = kvp.Key;
                }

                return dict;
            }

            var oldCount = TotalKeys;
            TotalKeys = 100000;
            f1();
            TotalKeys = oldCount;

            var execution = H.Run(f1);
            this.output.WriteLine(execution.ToString());            
        }          

    }

}
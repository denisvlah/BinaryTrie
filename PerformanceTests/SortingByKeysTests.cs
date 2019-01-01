using System;
using System.Collections.Generic;
using System.Linq;
using BinaryTrieImpl;
using Xunit;
using Xunit.Abstractions;

namespace BinaryTrie.PerformanceTests{
    public class SortingByKeysTests {
        private ITestOutputHelper output;
        private readonly int TotalKeys = 90000000;

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

            var execution = H.Run(f1);
            this.output.WriteLine(execution.ToString());            
        }

        [Fact]
        public void UsingIntKeysOnArrayBackedGrowableTrie(){
            object f1(){
                var container = new GrowableArrayBackedNodeContainer<int>();
                var trie = new PlugableBinaryTrie<int>(container);

                for (int i=TotalKeys; i>=0; i--){
                    trie.Add(i, i);
                }

                Assert.Equal(TotalKeys + 1, trie.Count);

                var lastKey = -1;
                foreach(var kvp in trie.GetEntrySet(reuseKeysList: true)){
                    var key = kvp.Item1[0];
                    var value = kvp.Item2;
                    Assert.Equal(key, value);
                    Assert.True(key >= lastKey);
                    lastKey = key;
                }

                return trie;
            }

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
                foreach(var kvp in sortedKeys){
                    Assert.Equal(kvp.Key, kvp.Value);
                    Assert.True(kvp.Key >= lastKey);
                    lastKey = kvp.Key;
                }

                return dict;
            }

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
                foreach(var kvp in sortedKeys){
                    Assert.Equal(kvp.Key, kvp.Value);
                    Assert.True(kvp.Key >= lastKey);
                    lastKey = kvp.Key;
                }

                return dict;
            }

            var execution = H.Run(f1);
            this.output.WriteLine(execution.ToString());            
        }          

    }

}
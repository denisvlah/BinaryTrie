using System;
using System.Collections.Generic;
using BinaryTrieImpl;
using Xunit;
using Xunit.Abstractions;

namespace BinaryTrie.PerformanceTests{
    public class FillAndRetrieveByIntKeyTests {

        //TODO: implement warmap
        private ITestOutputHelper output;
        private int TotalKeys = 5000000;

        public FillAndRetrieveByIntKeyTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void UsingSortedDictionary(){
            object f1(){
                var dict = new SortedDictionary<int,int>();

                for (int i=TotalKeys; i>=0; i--){
                    dict[i] = i;
                }

                for (int i=TotalKeys; i>=0; i--){
                    var value = dict[i];
                    Assert.Equal(i, value);
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
        public void UsingSortedDictionaryWithSortedInput(){
            object f1(){
                var dict = new SortedDictionary<int,int>();

                for (int i=0; i<=TotalKeys; i++){
                    dict[i] = i;
                }

                for (int i=TotalKeys; i>=0; i--){
                    var value = dict[i];
                    Assert.Equal(i, value);
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
        public void UsingArrayBackedGrowableTrie(){
            object f1(){
                
                var container = new GrowableArrayBackedNodeContainer<int>(10000000, true);
                var trie = new PlugableBinaryTrie<int>(container);

                for (int i=TotalKeys; i>=0; i--){
                    trie.Add(i, i);
                }

                for (int i=TotalKeys; i>=0; i--){
                    var value = trie.GetValue(i, -1);
                    Assert.Equal(i, value);
                }

                return trie;
            }

            var oldCount = TotalKeys;
            TotalKeys = 100000;
            var disp = f1() as IDisposable;
            disp.Dispose();
            TotalKeys = oldCount;

            var execution = H.Run(f1);
            this.output.WriteLine(execution.ToString());            
        }

        [Fact]
        public void UsingArrayBackedGrowableTrieWithSortedInput(){
            object f1(){
                var container = new GrowableArrayBackedNodeContainer<int>();
                var trie = new PlugableBinaryTrie<int>(container);

                for (int i=0; i<=TotalKeys; i++){
                    trie.Add(i, i);
                }

                for (int i=TotalKeys; i>=0; i--){
                    var value = trie.GetValue(i, -1);
                    Assert.Equal(i, value);
                }

                return trie;
            }

            var oldCount = TotalKeys;
            TotalKeys = 100000;
            f1();
            TotalKeys = oldCount;

            var execution = H.Run(f1);
            this.output.WriteLine(execution.ToString());            
        }

        /* 

        [Fact(Skip="Memory mapped containers are slow for now")]
        public void UsingGrowableMemoryMapedTrie(){
            object f1(){
                var container = new GrowableMemoryMappedNodeContainer<int>();
                var trie = new PlugableBinaryTrie<int>(container);

                for (int i=TotalKeys; i>=0; i--){
                    trie.Add(i, i);
                }

                for (int i=TotalKeys; i>=0; i--){
                    var value = trie.GetValue(i, -1);
                    Assert.Equal(i, value);
                }

                return trie;
            }

            var execution = H.Run(f1);
            this.output.WriteLine(execution.ToString());            
        }

        [Fact(Skip="Memory mapped containers are slow for now")]
        public void UsingGrowableMemoryMapedTrieWithSortedInput(){
            object f1(){
                var container = new GrowableMemoryMappedNodeContainer<int>();
                var trie = new PlugableBinaryTrie<int>(container);

                for (int i=0; i<=TotalKeys; i++){
                    trie.Add(i, i);
                }

                for (int i=TotalKeys; i>=0; i--){
                    var value = trie.GetValue(i, -1);
                    Assert.Equal(i, value);
                }

                return trie;
            }

            var execution = H.Run(f1);
            this.output.WriteLine(execution.ToString());            
        }
        */

        [Fact]
        public void UsingDictionaryWithoutCapacity(){
            object f1(){
                var dict = new Dictionary<int,int>();

                for (int i=TotalKeys; i>=0; i--){
                    dict[i] = i;
                }

                for (int i=TotalKeys; i>=0; i--){
                    var value = dict[i];
                    Assert.Equal(i, value);
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
        public void UsingDictionaryWithCapacity(){
            object f1(){
                var dict = new Dictionary<int,int>(TotalKeys);

                for (int i=TotalKeys; i>=0; i--){
                    dict[i] = i;
                }

                for (int i=TotalKeys; i>=0; i--){
                    var value = dict[i];
                    Assert.Equal(i, value);
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
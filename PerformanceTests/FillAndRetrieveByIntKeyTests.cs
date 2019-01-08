using System;
using System.Collections.Generic;
using BinaryTrieImpl;
using Xunit;
using Xunit.Abstractions;
using System.IO;

namespace BinaryTrie.PerformanceTests{
    public class FillAndRetrieveByIntKeyTests: IDisposable {

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

                var allOk = true;

                for (int i=TotalKeys; i>=0; i--){
                    var value = dict[i];
                    allOk = allOk && i == value;                    
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
        public void UsingArrayBackedGrowableTrie(){
            object f1(){
                
                var container = new GrowableArrayBackedNodeContainer<int>(10000000, true);
                var trie = new PlugableBinaryTrie<int>(container);

                for (int i=TotalKeys; i>=0; i--){
                    trie.Add(i, i);
                }

                var allOk = true;
                for (int i=TotalKeys; i>=0; i--){
                    var value = trie.GetValue(i, -1);
                    allOk = allOk && value == i;
                }

                Assert.True(allOk);

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
        public void UsingArrayBackedTrie()
        {
            object f1()
            {

                var container = new ArrayBackedNodesContainer<int>(TotalKeys*32);
                var trie = new PlugableBinaryTrie<int>(container);

                for (int i = TotalKeys; i >= 0; i--)
                {
                    trie.Add(i, i);
                }

                var allOk = true;
                for (int i = TotalKeys; i >= 0; i--)
                {
                    var value = trie.GetValue(i, -1);
                    allOk = allOk && value == i;
                }

                Assert.True(allOk);

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

        

        [Fact()]
        public void UsingGrowableMemoryMapedTrie(){
            object f1(){
                var container = new GrowableMemoryMappedNodeContainer<int>(size:TotalKeys*32);
                var trie = new PlugableBinaryTrie<int>(container);

                for (int i=TotalKeys; i>=0; i--){
                    trie.Add(i, i);
                }

                Assert.Equal(trie.Count, TotalKeys +1);
                var allOk = true;
                for (int i=0; i<=TotalKeys; i++){
                    var value = trie.GetValue(i, -1);
                    allOk = allOk && value == i;                    
                }

                Assert.True(allOk);
                return trie;
            }

            var oldKeysCount = TotalKeys;
            TotalKeys = 10000;
            var testTrie = f1() as IDisposable;
            testTrie.Dispose();
            Dispose();
            TotalKeys = oldKeysCount;

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

                var allOk = true;
                for (int i=TotalKeys; i>=0; i--){
                    var value = dict[i];
                    allOk = allOk && value == i;
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

        public void Dispose()
        {
            var allFiles = Directory.EnumerateFileSystemEntries(".", "data_*.bin", SearchOption.AllDirectories);
            foreach(var path in allFiles){
                File.Delete(path);
            }
        }
    }

}
using System;
using System.Collections.Generic;
using System.Linq;
using BinaryTrieImpl;
using Xunit;
using Xunit.Abstractions;

namespace BinaryTrie.PerformanceTests{
    public class StructRefReturnOverheadTest {
        private ITestOutputHelper output;
        private const int TotalKeys = 6000000;

        private static readonly MyPoint[] Data = new MyPoint[TotalKeys];

        static StructRefReturnOverheadTest(){
            var rand = new System.Random();
            var value = rand.Next();
            for (int i=0; i<TotalKeys; i++){
                Data[i].X = value;
                Data[i].Y = value;
                Data[i].Z = value;
                Data[i].A = value;
                Data[i].B = value;
                Data[i].C = value;
            }
        }

        

        public StructRefReturnOverheadTest(ITestOutputHelper output)
        {
            this.output = output;            
        }

        ref MyPoint GetRefPoint(int index){
            return ref Data[index];
        }

        MyPoint GetPoint(int index){
            return Data[index];
        }

        [Fact]
        public void CheckDataUsingRefs(){
            object f1(){
                var allEqual = true;
                for (int i = 0; i<TotalKeys; i++){
                    ref var point = ref GetRefPoint(i);
                    var x = point.X;
                    var allEqual1 =  
                                x == point.Y
                                && x == point.Z
                                && x == point.A
                                && x == point.B
                                && x == point.C
                                ;  
                    allEqual = allEqual && allEqual1;                                     ;
                    
                }
                Assert.True(allEqual);

                return new object();
                
            }
            f1();

            var execution = H.Run(f1);
            this.output.WriteLine(execution.ToString());            
        }

        [Fact]
        public void CheckDataUsingCopy(){
            object f1(){
                var allEqual = true;
                for (int i = 0; i<TotalKeys; i++){
                    var point = GetPoint(i);
                    var x = point.X;
                    var allEqual1 =  
                                x == point.Y
                                && x == point.Z
                                && x == point.A
                                && x == point.B
                                && x == point.C
                                ;  
                    allEqual = allEqual && allEqual1;                                     ;
                }

                Assert.True(allEqual);               

                return new object();
                
            }
            f1();

            var execution = H.Run(f1);
            this.output.WriteLine(execution.ToString());            
        }

        [Fact]
        public void CheckDataUsingDirect(){
            object f1(){
                var allEqual = true;
                for (int i = 0; i<TotalKeys; i++){                    
                    var x = Data[i].X;
                    var allEqual1 =  
                                x == Data[i].Y
                                && x == Data[i].Z
                                && x == Data[i].A
                                && x == Data[i].B
                                && x == Data[i].C
                                ;  
                    allEqual = allEqual && allEqual1;  
                }

                Assert.True(allEqual);

                return new object();
                
            }
            f1();         

            var execution = H.Run(f1);
            this.output.WriteLine(execution.ToString());            
        }

                  

    }

    struct MyPoint{
        public int X;
        public int Y;
        public int Z;       

        public int A;
        public int B;
        public int C; 
    }

}
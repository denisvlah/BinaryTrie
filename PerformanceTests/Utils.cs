using System;
using System.Diagnostics;
using System.Threading;

namespace BinaryTrie.PerformanceTests{

    public static class H{

        private static ThreadLocal<object> _container = new ThreadLocal<object>();

        public static PerfData Run(Func<object> a){
            var testCaseName = GetTestCaseName();
            Console.Write($"[{testCaseName}]: ");
            _container.Value = null;;
            
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized, blocking: true);
            
            var bytesStart = GC.GetTotalMemory(true);
            var gc0CollectionsStart = GC.CollectionCount(0);
            var gc1CollectionsStart = GC.CollectionCount(1);
            var gc2CollectionsStart = GC.CollectionCount(2);
            
            var stopwatch = Stopwatch.StartNew();
            _container.Value = a();
            stopwatch.Stop();

            var gc0CollectionsEnd = GC.CollectionCount(0);
            var gc1CollectionsEnd = GC.CollectionCount(1);
            var gc2CollectionsEnd = GC.CollectionCount(2);
            
            var bytesEnd = GC.GetTotalMemory(false);
            
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized, blocking: true);
            
            var bytesClean = GC.GetTotalMemory(true);
            
            var bytesConsumed = bytesEnd - bytesStart;
            var containerBytes = bytesClean - bytesStart;
            var ticks = stopwatch.Elapsed.Ticks;

            var gc0 = gc0CollectionsEnd - gc0CollectionsStart;
            var gc1 = gc1CollectionsEnd - gc1CollectionsStart; 
            var gc2 = gc2CollectionsEnd - gc2CollectionsStart;

            var perfData =  new PerfData(ticks, bytesConsumed, containerBytes, gc0, gc1, gc2);            
            
            Console.WriteLine(perfData.ToString());

            var disp = _container.Value as IDisposable;
            if (disp != null){
                disp.Dispose();
            }

            return perfData;
        }

        private static string GetTestCaseName(){                        
            // Get call stack
            StackTrace stackTrace = new StackTrace();

            var method = stackTrace.GetFrame(2).GetMethod();
            var fullTypeName = method.DeclaringType.Name;

            // Get calling method name
            var testCaseName = fullTypeName + "." + method.Name + "()";

            return testCaseName;
        }
    }

    public class PerfData {

        public PerfData(){}

        public PerfData(long executionTimeTicks, long memoryBytes, long containerBytes, int gc0, int gc1, int gc2)
        {
            ExecutionTimeTicks = executionTimeTicks;
            ConsumedMemoryBytes = memoryBytes;
            ContainerBytes = containerBytes;
            Gc0 = gc0;
            Gc1 = gc1;
            Gc2 = gc2;
        }

        public long ExecutionTimeTicks { get;  }
        public long ConsumedMemoryBytes { get; }
        public long ContainerBytes { get; }
        public int Gc0 { get; }
        public int Gc1 { get; }
        public int Gc2 { get; }

        public TimeSpan Time(){
            return TimeSpan.FromTicks(ExecutionTimeTicks);
        }

        public long RamMegabytes(){
            return (ConsumedMemoryBytes / 1024) / 1024;
        }

        public long ContainerMegabytes(){
            return (ContainerBytes / 1024) / 1024;
        }

        public override string ToString(){
            return $"Time: {Time().TotalSeconds}s; RAM: {RamMegabytes()}Mb; Final_RAM: {ContainerMegabytes()}Mb; Gc0: {Gc0}; Gc1: {Gc1}; Gc2: {Gc2}";
        }
    }

}
using System;
using System.Diagnostics;
using System.Threading;

namespace BinaryTrie.PerformanceTests{

    public static class H{

        private static ThreadLocal<object> _container = new ThreadLocal<object>();

        public static PerfData Run(Func<object> a){
            _container.Value = null;;
            
            GC.Collect();
            
            var bytesStart = GC.GetTotalMemory(true);
            
            var stopwatch = Stopwatch.StartNew();
            _container.Value = a();
            stopwatch.Stop();
            
            var bytesEnd = GC.GetTotalMemory(false);
            
            GC.Collect();
            
            var bytesClean = GC.GetTotalMemory(true);
            
            var bytesConsumed = bytesEnd - bytesStart;
            var containerBytes = bytesClean - bytesStart;
            var ticks = stopwatch.Elapsed.Ticks;

            var perfData =  new PerfData(ticks, bytesConsumed, containerBytes);            
            
            H.PrintStatistics(perfData);

            return perfData;
        }

        private static void PrintStatistics(PerfData data){                        
            // Get call stack
            StackTrace stackTrace = new StackTrace();

            var method = stackTrace.GetFrame(2).GetMethod();
            var fullTypeName = method.DeclaringType.Name;

            // Get calling method name
            var testCaseName = fullTypeName + "." + method.Name + "()";

            Console.WriteLine($"[{testCaseName}]: {data.ToString()}");
        }
    }

    public class PerfData {

        public PerfData(){}

        public PerfData(long executionTimeTicks, long memoryBytes, long containerBytes)
        {
            ExecutionTimeTicks = executionTimeTicks;
            ConsumedMemoryBytes = memoryBytes;
            ContainerBytes = containerBytes;
        }

        public long ExecutionTimeTicks { get;  }
        public long ConsumedMemoryBytes { get; }
        public long ContainerBytes { get; }

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
            return $"Time: {Time().TotalSeconds}s; RAM: {RamMegabytes()}Mb; Final_RAM: {ContainerMegabytes()}Mb";
        }
    }

}
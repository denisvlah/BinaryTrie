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

        private string RName()
        {
            return "foo_" + Guid.NewGuid().ToString().Replace("-", "");
        }
    }
}
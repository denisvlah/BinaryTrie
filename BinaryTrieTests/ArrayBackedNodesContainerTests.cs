namespace BinaryTrieTests
{
    using BinaryTrieImpl;
    using Xunit;
    public class ArrayBackedNodesContainerTests
    {
        private ArrayBackedNodesContainer<int> _nodes = new ArrayBackedNodesContainer<int>();

        [Fact]
        public void WhenRetrievedTrieNodeIsModifiedThenItIsModifiedInsideContainer()
        {
            ref var node = ref  _nodes.AddNewNode(out int nodeIndex);
            node.HasValue = true;
            node.Value = 999;

            var actualNode = _nodes.Get(nodeIndex);
            
            Assert.Equal(true, actualNode.HasValue);
            Assert.Equal(999, actualNode.Value);            
            Assert.Equal(-1, actualNode.Node_0);
            Assert.Equal(-1, actualNode.Node_1);
        } 
    }
}
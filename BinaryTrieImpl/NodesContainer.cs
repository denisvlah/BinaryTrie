namespace BinaryTrieImpl
{
    public class NodesContainer<T> : INodesContainer<T>
    {
        //TODO: implement resizing when capacity reached.
        //TODO: implement thread safity
        //TODO: implement custom serialization/deserialization
        TrieNode<T>[] _array = new TrieNode<T>[10000];

        private int _index = 0;

        public ref TrieNode<T> AddNewNode()
        {
            var index = _index;            
            _index++;
            _array[index] = new TrieNode<T>();
            ref var node = ref _array[index];
            node.Node_0 = -1;
            node.Node_1 = -1;
            node.CurrentIndex = index;

            return ref _array[index];
        }

        public ref TrieNode<T> Get(int index)
        {
            return ref _array[index];
        }

        public int Size()
        {
            return _index;
        }

        public void SetValue(int nodeIndex, T value)
        {
            _array[nodeIndex].Value = value;
        }

        public void ReassignNode(ref TrieNode<T> newNode)
        {
            _array[newNode.CurrentIndex] = newNode;
        }
    }
}
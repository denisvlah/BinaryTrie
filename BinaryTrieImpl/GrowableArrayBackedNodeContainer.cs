namespace BinaryTrieImpl
{
    public class GrowableArrayBackedNodeContainer<T> : BaseGrowableNodeContainer<T, ArrayBackedNodesContainer<T>>
    {
        public GrowableArrayBackedNodeContainer(int? size = null, bool useArrayPool = false)
        {
            if (size.HasValue == false)
            {
                size = 100000;
            }
            _size = size.Value;


            var initialData = new ArrayBackedNodesContainer<T>(_size, useArrayPool);
            _data.Add(initialData);

        }

        protected override ArrayBackedNodesContainer<T> CreateNodeContainer()
        {
            return new ArrayBackedNodesContainer<T>(_size);
        }
    }
}
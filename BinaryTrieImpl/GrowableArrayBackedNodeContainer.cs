namespace BinaryTrieImpl
{
    public class GrowableArrayBackedNodeContainer<T> : BaseGrowableNodeContainer<T, ArrayBackedNodesContainer<T>>
    {
        public GrowableArrayBackedNodeContainer(int? size = null)
        {
            if (size.HasValue == false)
            {
                size = 100000;
            }
            _size = size.Value;


            var initialData = new ArrayBackedNodesContainer<T>(_size);
            _data.Add(initialData);

        }

        protected override ArrayBackedNodesContainer<T> CreateNodeContainer()
        {
            return new ArrayBackedNodesContainer<T>(_size);
        }
    }
}
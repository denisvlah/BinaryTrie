using System.Collections.Generic;

namespace BinaryTrieImpl
{
    public class GrowableArrayBackedNodeContainer<T> : INodesContainer<T>
    {
        private List<ArrayBackedNodesContainer<T>> _data = new List<ArrayBackedNodesContainer<T>>(10);
        private int _size;

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
        public ref TrieNode<T> AddNewNode(out int index)
        {   
            var nodeContainer = _data[_data.Count -1];

            if (nodeContainer.Size() == _size)
            {
                var newNodeContainer = new ArrayBackedNodesContainer<T>(_size);
                _data.Add(newNodeContainer);
            }         

            ref var data = ref _data[_data.Count -1].AddNewNode(out index);
            index += (_data.Count-1) * _size;
            return ref data;  
        }

        public void DecrementValuesCount()
        {
            _data[0].DecrementValuesCount();
        }

        public void Dispose()
        {
            foreach(var dataItem in _data){
                dataItem.Dispose();
            }
        }

        public ref TrieNode<T> Get(int index)
        {
            return ref _data[index / _size].Get(index % _size);
        }

        public int GetValuesCount()
        {
            return _data[0].GetValuesCount();
        }

        public void IncrementValuesCount()
        {
            _data[0].IncrementValuesCount();
        }

        public void InitFirstNode()
        {
            _data[0].InitFirstNode();
        }

        public void ReassignNode(ref TrieNode<T> newNode, int index)
        {
            /* 
            var index = newNode.CurrentIndex;
            var localIndex = index % _size;

            _data[index / _size].ReassignNode(ref newNode, localIndex);
            */
        }

        public void SetValue(int nodeIndex, T value)
        {
            _data[nodeIndex / _size].SetValue(nodeIndex % _size, value);
        }

        public int Size()
        {
            return _data[0].Size();
        }
    }
}
using System.Collections.Generic;
using System.Linq;

namespace BinaryTrieImpl
{
    public abstract class BaseGrowableNodeContainer<T, U> : INodesContainer<T> where U: INodesContainer<T>
    {
        protected List<U> _data = new List<U>(10);
        protected int _size;

        
        public ref TrieNode<T> AddNewNode(out int index)
        {   
            var nodeContainer = _data[_data.Count -1];

            if (nodeContainer.Size() == _size)
            {
                var newNodeContainer = CreateNodeContainer();
                _data.Add(newNodeContainer);
            }         

            ref var data = ref _data[_data.Count -1].AddNewNode(out index);
            index += (_data.Count-1) * _size;
            return ref data;
        }

        protected abstract U CreateNodeContainer();
        

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
            var localIndex = index % _size;
            _data[index / _size].ReassignNode(ref newNode, localIndex);
            
        }

        public void SetValue(int nodeIndex, T value)
        {
            _data[nodeIndex / _size].SetValue(nodeIndex % _size, value);
        }

        public int Size()
        {
            return _data.Sum(x=>x.Size());
        }
    }
}
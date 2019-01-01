using System;

namespace BinaryTrieImpl
{
    public class ArrayBackedNodesContainer<T> : INodesContainer<T>
    {
        private TrieNode<T>[] _array;

        private int _index = 0;

        private int _valuesCount = 0;

        public ArrayBackedNodesContainer(int? initialSize = null)
        {
            var sizeValue = initialSize ?? 100000;
            
            _array = System.Buffers.ArrayPool<TrieNode<T>>.Shared.Rent(sizeValue);
        }

        public ref TrieNode<T> AddNewNode(out int index)
        {            
            index = _index;            
            _index++;
            _array[index] = new TrieNode<T>();
            ref var node = ref _array[index];
            node.Node_0 = 0;
            node.Node_1 = 0;            

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

        public void InitFirstNode()
        {
            AddNewNode(out _);
        }

        public void IncrementValuesCount()
        {
            _valuesCount++;
        }

        public void DecrementValuesCount()
        {
            _valuesCount--;
        }

        public int GetValuesCount()
        {
            return _valuesCount;
        }

        public void ReassignNode(ref TrieNode<T> node, int nodeIndex)
        {

        }

        public void Dispose()
        {
            System.Buffers.ArrayPool<TrieNode<T>>.Shared.Return(_array);            
        }
    }
}
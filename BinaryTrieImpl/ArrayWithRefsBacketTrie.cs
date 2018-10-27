using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;

namespace BinaryTrieImpl
{
    public class ArrayWithRefsBackedBinaryTrie<T>
    {
        //TODO: implement thread safity and custom serialization
        private readonly ArrayBackedNodesContainer<T> _nodes = new ArrayBackedNodesContainer<T>();
        private int _count = 0;

        private int _maxKeySize = 0;       

        public ArrayWithRefsBackedBinaryTrie()
        {
            _nodes.AddNewNode();
            
        }

        
        private int _lastNodeIndex = -1;
        private int _currentKeySize = 1;

        private ref TrieNode<T> Node(int index)
        {
            return ref _nodes.Get(index);
        }

        private int AddNewNode(int nodeIndex, ref BitVector32 bitVector, int mask)
        {
            var bit = bitVector[mask];
            var nextNodeIndex = Node(nodeIndex).NextNodeIndex(bit);
            

            if (nextNodeIndex == -1)
            {
                
                ref var newNode = ref _nodes.AddNewNode();
                Node(nodeIndex).AddIndex(bit, newNode.CurrentIndex);
                newNode.Key = bit;
                

                return newNode.CurrentIndex;
            }
            else
            {
                ref var newNode = ref _nodes.Get(nextNodeIndex);

                return newNode.CurrentIndex;
            }
        }

        public void Add(int key, T value, bool isLastElement = true)
        {
            var nodeIndex = 0;
            if (_lastNodeIndex != -1)
            {
                nodeIndex = _lastNodeIndex;
            }
            var bitVector = new BitVector32(key);
            for(int i=0; i<32; i++)
            {
                var mask = 1 << i;
                nodeIndex = AddNewNode(nodeIndex, ref bitVector, mask);
            }

            if (isLastElement)
            {
                _lastNodeIndex = -1;
                Node(nodeIndex).Value = value;
                _count++;
                _maxKeySize = _currentKeySize > _maxKeySize ? _currentKeySize : _maxKeySize;                
            }
            else
            {
                _currentKeySize++;
                _lastNodeIndex = nodeIndex;
            }            
        }

        public bool TryRemove(int key, bool isLastElement = true)
        {
            var bitVector = new BitVector32(key);
            var nodeIndex = 0;
            if (_lastNodeIndex != -1)
            {
                nodeIndex = _lastNodeIndex;
            }
            for(var i=0; i<32; i++)
            {
                var mask = 1 << i;
                var bit = bitVector[mask];
                nodeIndex = Node(nodeIndex).NextNodeIndex(bit);
                if (nodeIndex == -1)
                {
                    return false;
                }
            }

            if (isLastElement)
            {
                var hasValue = Node(nodeIndex).HasValue;
                Node(nodeIndex).RemoveValue();                
                _lastNodeIndex = -1;
                _count--;
                return hasValue;
            }

            _lastNodeIndex = nodeIndex;

            return Node(nodeIndex).HasValue;
        }

        public bool TryGetValue(int keyBits, out T result, bool isLastElement = true)
        {
            var bitVector = new BitVector32(keyBits);
            result = default(T);
            var nodeIndex = 0;
            if (_lastNodeIndex != -1)
            {
                nodeIndex = _lastNodeIndex;
            }
            for(int i=0; i<32; i++)
            {
                var mask = 1 << i;
                var bit = bitVector[mask];
                var nextIndex = Node(nodeIndex).NextNodeIndex(bit);
                if (nextIndex == -1) 
                {                    
                    return false;
                }

                nodeIndex = _nodes.Get(nextIndex).CurrentIndex;
            }

            if (isLastElement)
            {
                _lastNodeIndex = -1;
            }
            else
            {
                _lastNodeIndex = nodeIndex;
            }

            result = Node(nodeIndex).Value;

            return Node(nodeIndex).HasValue;
        }

        public int Count {get { return _count;}}

        public List<(List<int>, T)> GetEntrySet()
        {
            throw new NotImplementedException();
        }

    }    
    
}
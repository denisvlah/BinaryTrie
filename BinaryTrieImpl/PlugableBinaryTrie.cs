using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;

namespace BinaryTrieImpl
{    

    public class PlugableBinaryTrie<T>
    {
        //TODO: implement thread safity and custom serialization
        private readonly INodesContainer<T> _nodes;        
        private int _count = 0;
        private int _maxKeySize = 0;        

        public PlugableBinaryTrie(INodesContainer<T> container)
        {
            _nodes = container ?? throw new ArgumentNullException(nameof(container));
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
            ref var node = ref Node(nodeIndex);
            var nextNodeIndex = node.NextNodeIndex(bit);
            var newNodeParentIndex = node.CurrentIndex;

            if (nextNodeIndex == -1)
            {
                
                ref var newNode = ref _nodes.AddNewNode();
                node.AddIndex(bit, newNode.CurrentIndex);
                newNode.Key = bit;
                newNode.ParentIndex = newNodeParentIndex;

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
                _nodes.SetValue(nodeIndex, value);                
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

            TrieNode<T> node = default;
            for(var i=0; i<32; i++)
            {
                var mask = 1 << i;
                var bit = bitVector[mask];
                node = Node(nodeIndex);
                nodeIndex = node.NextNodeIndex(bit);
                if (nodeIndex == -1)
                {
                    return false;
                }
            }

            node = Node(nodeIndex);

            if (isLastElement)
            {
                var hasValue = node.HasValue;
                node.RemoveValue();
                _nodes.ReassignNode(ref node);
                _lastNodeIndex = -1;
                _count--;
                return hasValue;
            }

            _lastNodeIndex = nodeIndex;

            return node.HasValue;
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

            TrieNode<T> node = default;
            for(int i=0; i<32; i++)
            {
                var mask = 1 << i;
                var bit = bitVector[mask];
                node = Node(nodeIndex);
                var nextIndex = node.NextNodeIndex(bit);
                if (nextIndex == -1) 
                {                    
                    return false;
                }

                nodeIndex = nextIndex;
            }

            node = Node(nodeIndex);

            if (isLastElement)
            {
                _lastNodeIndex = -1;
            }
            else
            {
                _lastNodeIndex = nodeIndex;
            }

            result = node.Value;
            return node.HasValue;
        }

        public int Count {get { return _count;}}

        public IEnumerable<(List<int>, T)> GetEntrySet()
        {
            var nodes = new Stack<TrieNode<T>>(_maxKeySize);
            var node = Node(0);

            var preferLeft = true;

            while(true)
            {

                if (node.IsTerminal())
                {
                    if (nodes.Count == 0)
                    {
                        break;
                    }
                    else
                    {
                        node = nodes.Pop();
                        preferLeft = false;
                    }
                }
                else
                {
                    preferLeft = true;
                    nodes.Push(node);
                    var nextNodeIndex  = node.GetNextIndex(preferLeft);
                    node = Node(nextNodeIndex);
                    
                    if (node.HasValue)
                    {
                        yield return (GetIntKey(nodes, node.Key), node.Value);
                    }

                }

            }
        }

        private List<int> GetIntKey(Stack<TrieNode<T>> nodes, bool lastKey)
        {
            throw new NotImplementedException();
        }
    }
}
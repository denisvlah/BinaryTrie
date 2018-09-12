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
            var node = Node(nodeIndex);
            var nextNodeIndex = node.NextNodeIndex(bit);
            var newNodeParentIndex = node.CurrentIndex;

            if (nextNodeIndex == -1)
            {
                
                ref var newNode = ref _nodes.AddNewNode();
                node.AddIndex(bit, newNode.CurrentIndex);
                newNode.Key = bit;
                newNode.ParentIndex = newNodeParentIndex;
                _nodes.ReassignNode(ref node);
                _nodes.ReassignNode(ref newNode);

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
            var nodes = new Stack<NodeWrapper<T>>(_maxKeySize);
            var node = new NodeWrapper<T>(Node(0));            

            while(true)
            {                
                if (node.AllVisited())
                {
                    if (nodes.Count == 0)
                    {
                        break;
                    }

                    node = nodes.Pop();
                }
                else
                {
                    nodes.Push(node);
                    var nextNodeIndex  = GetNextNodeIndex(
                        node
                    );
                    
                    node = new NodeWrapper<T>(Node(nextNodeIndex) );
                
                    if (node.Node.HasValue)
                    {
                        yield return (GetIntKey(nodes, node.Node.Key), node.Node.Value);
                    }                        

                }

            }
        }

        private int GetNextNodeIndex(NodeWrapper<T> node)
        {
            if (node.LeftVisited == false)
            {                
                node.LeftVisited = true;
                if (node.Node.Node_0 != -1)
                {
                    return node.Node.Node_0;
                }

                node.RightVisited = true;
                return node.Node.Node_1;
            }

            if (node.RightVisited == false)
            {
                node.RightVisited = true;
                if (node.Node.Node_1 != -1)
                {
                    return node.Node.Node_1;
                }
                node.LeftVisited = true;
                return node.Node.Node_0;
            }

            return -1;
        }

        private List<int> GetIntKey(Stack<NodeWrapper<T>> nodes, bool lastKey)
        {
            var resultList = new List<int>();
            var index = 0;
            var bitVector = new BitVector32(0);
            var bitMask = BitVector32.CreateMask();
            foreach(var nodeWrapper in nodes.Reverse().Skip(1))
            {
                var node = nodeWrapper.Node;
                bitVector[bitMask] = node.Key;
                bitMask = BitVector32.CreateMask(bitMask);

                if (index != 0 && index % 32 == 0)
                {
                    resultList.Add(bitVector.Data);
                    bitVector = new BitVector32(0);
                    bitMask = BitVector32.CreateMask();
                }                

                index++;
            }

            bitVector[bitMask] = lastKey;
            resultList.Add(bitVector.Data);

            return resultList;
        }
    }

    class NodeWrapper<T>
    {
        public NodeWrapper(TrieNode<T> node, bool leftVisited = false, bool rightVisited = false)
        {
            Node = node;
            LeftVisited = leftVisited;
            RightVisited = rightVisited;

            
        }
        public TrieNode<T> Node { get; set; }
        public bool LeftVisited {get; set;}
        public bool RightVisited {get; set;}

        public bool AllVisited()
        {
            return Node.IsTerminal() || (LeftVisited && RightVisited);
        }

        public override string ToString()
        {
            return $"{nameof(Node)}: {Node}, {nameof(LeftVisited)}: {LeftVisited}, {nameof(RightVisited)}: {RightVisited}";
        }
    }
}
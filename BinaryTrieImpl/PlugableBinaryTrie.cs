using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;

namespace BinaryTrieImpl
{    

    public class PlugableBinaryTrie<T>: IDisposable
    {
        //TODO: implement thread safity and custom serialization
        private readonly INodesContainer<T> _nodes;                
        private int _maxKeySize = 0;
        private int[] _invertedMasks = new int[32];    

        public PlugableBinaryTrie(INodesContainer<T> container)
        {
            _nodes = container ?? throw new ArgumentNullException(nameof(container));
            _nodes.InitFirstNode();
            for(int i=0; i<32; i++){
                _invertedMasks[i] = 1 << i;
            }

            _invertedMasks = _invertedMasks.Reverse().ToArray();
            
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

            if (nextNodeIndex == -1)
            {
                
                ref var newNode = ref _nodes.AddNewNode();
                node.AddIndex(bit, newNode.CurrentIndex);
                newNode.Key = bit;                
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
                var mask = _invertedMasks[i];                
                nodeIndex = AddNewNode(nodeIndex, ref bitVector, mask);
            }

            if (isLastElement)
            {
                _lastNodeIndex = -1;
                _nodes.SetValue(nodeIndex, value);                
                _nodes.IncrementValuesCount();
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
                var mask = _invertedMasks[i];
                var bit = bitVector[mask];
                ref var node = ref Node(nodeIndex);
                nodeIndex = node.NextNodeIndex(bit);
                if (nodeIndex == -1)
                {
                    return false;
                }
            }

            ref var foundNode = ref Node(nodeIndex);

            if (isLastElement)
            {
                var hasValue = foundNode.HasValue;
                foundNode.RemoveValue();
                _nodes.ReassignNode(ref foundNode);
                _lastNodeIndex = -1;
                _nodes.DecrementValuesCount();                
                return hasValue;
            }

            _lastNodeIndex = nodeIndex;

            return foundNode.HasValue;
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
                var mask = _invertedMasks[i];                
                var bit = bitVector[mask];
                ref var node = ref Node(nodeIndex);
                var nextIndex = node.NextNodeIndex(bit);
                if (nextIndex == -1) 
                {                    
                    return false;
                }

                nodeIndex = nextIndex;
            }

            ref var foundNode = ref Node(nodeIndex);

            if (isLastElement)
            {
                _lastNodeIndex = -1;
            }
            else
            {
                _lastNodeIndex = nodeIndex;
            }

            result = foundNode.Value;
            return foundNode.HasValue;
        }

        public int Count {get { return _nodes.GetValuesCount();}}

        public IEnumerable<(IReadOnlyList<int>, T)> GetEntrySet(bool reuseKeysList = false)
        {
            var keysContainer = reuseKeysList ? new List<int>(_maxKeySize): null;
            var nodes = new List<NodeWrapper<T>>(_maxKeySize);
            var node = new NodeWrapper<T>(Node(0));            

            while(true)
            {                
                if (node.AllVisited())
                {
                    if (nodes.Count == 0)
                    {
                        yield break;
                    }

                    Pop(nodes, ref node);
                }
                else
                {                   
                    var nextNodeIndex  = GetNextNodeIndex(ref node);
                    Push(nodes, ref node);
                    
                    node = new NodeWrapper<T>(Node(nextNodeIndex) );
                
                    if (node.Node.HasValue)
                    {
                        yield return (GetIntKey(nodes, node.Node.Key, keysContainer), node.Node.Value);

                        if (reuseKeysList){
                            keysContainer.Clear();
                        }
                    }
                }
            }
        }       

        public T GetValue(IReadOnlyList<int> keys, T defaultValue = default)
        {
            T result;
            for(int i = 0; i<keys.Count - 1; i++)
            {
                var hasValue = TryGetValue(keys[i], out result, false);              
            }

            if (TryGetValue(keys[keys.Count - 1], out result))
            {
                return result;
            }

            return defaultValue;
        }

        public void Add(IReadOnlyList<int> keys, T v2)
        {
            for (int i =0; i< keys.Count - 1; i++){
                Add(keys[i], v2, false);
            }

            Add(keys[keys.Count -1], v2);
        }

        private void Push(List<NodeWrapper<T>> nodes, ref NodeWrapper<T> node)
        {
            nodes.Add(node);
        }

        private NodeWrapper<T> Pop(List<NodeWrapper<T>> nodes, ref NodeWrapper<T> node)
        {
            node = nodes[nodes.Count - 1];
            nodes.RemoveAt(nodes.Count -1);

            return node;
        }

        private int GetNextNodeIndex(ref NodeWrapper<T> node)
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

        private List<int> GetIntKey(List<NodeWrapper<T>> nodes, bool lastKey, List<int> resultList = null)
        {
            resultList = resultList ?? new List<int>(_maxKeySize);
            var index = 0;
            var bitVector = new BitVector32(0);

            for(int i=1; i<nodes.Count; i++)
            {
                var mask = _invertedMasks[index];
                bitVector[mask] = nodes[i].Node.Key;
                index++;
                
                if (index == 32)
                {
                    resultList.Add(bitVector.Data);
                    bitVector = new BitVector32(0);
                    index = 0;
                }
            }                        

            bitVector[_invertedMasks[31]] = lastKey;
            resultList.Add(bitVector.Data);

            return resultList;
        }

        public void Dispose()
        {            
            _nodes.Dispose();
        }
    }

    struct NodeWrapper<T>
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
            return 
                Node.IsTerminal() || 
                (LeftVisited && RightVisited) ||
                (LeftVisited && Node.Node_1 == -1) ||
                (RightVisited && Node.Node_0 == -1)
                ;
        }

        public override string ToString()
        {
            return $"{nameof(Node)}: {Node}, {nameof(LeftVisited)}: {LeftVisited}, {nameof(RightVisited)}: {RightVisited}";
        }
    }
}
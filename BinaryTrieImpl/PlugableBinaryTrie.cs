using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace BinaryTrieImpl
{

    public class PlugableBinaryTrie<T>: IDisposable
    {
        //TODO: implement thread safity and custom serialization
        private readonly INodesContainer<T> _nodes;                
        private int _maxKeySize = 0;
        private static readonly int[] _invertedMasks = new int[32];

        static PlugableBinaryTrie()
        {
            for (int i = 0; i < 32; i++)
            {
                _invertedMasks[i] = 1 << i;
            }

            _invertedMasks = _invertedMasks.Reverse().ToArray();
        }

        public PlugableBinaryTrie(INodesContainer<T> container)
        {
            _nodes = container ?? throw new ArgumentNullException(nameof(container));
            _nodes.InitFirstNode();
        }

        
        private int _lastNodeIndex = 0;
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

            if (nextNodeIndex == 0)
            {
                int newNodeIndex;
                ref var newNode = ref _nodes.AddNewNode(out newNodeIndex);                
                node.AddIndex(bit, newNodeIndex);
                newNode.Key = bit;                
                _nodes.ReassignNode(ref node, nodeIndex);
                _nodes.ReassignNode(ref newNode, newNodeIndex);

                return newNodeIndex;
            }
            else
            {
                return nextNodeIndex;
            }
        }        

        public void Add(int key, T value, bool isLastElement = true)
        {
            var nodeIndex = 0;
            if (_lastNodeIndex != 0)
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
                _lastNodeIndex = 0;
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
            if (_lastNodeIndex != 0)
            {
                nodeIndex = _lastNodeIndex;
            }
            
            for(var i=0; i<32; i++)
            {
                var mask = _invertedMasks[i];
                var bit = bitVector[mask];
                ref var node = ref Node(nodeIndex);
                nodeIndex = node.NextNodeIndex(bit);                
            }
            if (nodeIndex == 0)
            {
                return false;
            }

            ref var foundNode = ref Node(nodeIndex);

            if (isLastElement)
            {
                var hasValue = foundNode.HasValue;
                foundNode.RemoveValue();
                _nodes.ReassignNode(ref foundNode, nodeIndex);
                _lastNodeIndex = 0;
                _nodes.DecrementValuesCount();                
                return hasValue;
            }

            _lastNodeIndex = nodeIndex;

            return foundNode.HasValue;
        }

        private int TryGetNode(int keyBits, bool isLastElement = true)
        {
            var bitVector = new BitVector32(keyBits);            
            var nodeIndex = 0;
            if (_lastNodeIndex != 0)
            {
                nodeIndex = _lastNodeIndex;
            }            
            
            for(int i=0; i<32; i++)
            {
                var mask = _invertedMasks[i];                
                var bit = bitVector[mask];
                ref var node = ref Node(nodeIndex);
                var nextIndex = node.NextNodeIndex(bit);
                nodeIndex = nextIndex;
            }

            if (isLastElement == false){
                _lastNodeIndex = nodeIndex;
            }
            else {
                _lastNodeIndex = 0;
            }          

                        
            return nodeIndex;
        }

        public bool TryGetValue(int keyBits, out T result, bool isLastElement = true)
        {   
            var nodeIndex = TryGetNode(keyBits, isLastElement);

            if (nodeIndex == 0){
                result = default;
                return false;
            }

            ref var node = ref Node(nodeIndex);

            result = node.Value;
            return node.HasValue || (!isLastElement);
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
                if (!hasValue){
                    return defaultValue;
                }              
            }

            if (TryGetValue(keys[keys.Count - 1], out result))
            {
                return result;
            }

            return defaultValue;
        }

        public T GetValue(int key, T defaultValue = default)
        {
            T result;           

            if (TryGetValue(key, out result))
            {
                return result;
            }

            return defaultValue;
        }

        public void DoWithValue(IReadOnlyList<int> keys, Func<bool, T, T> newValueFunc, bool addValueIfNotExist = true){
            
            for(int i = 0; i<keys.Count - 1; i++)
            {
                var nodeIndex = TryGetNode(keys[i], false);
                if (nodeIndex == 0){
                    var newValue = newValueFunc(false, default);
                    if (addValueIfNotExist){
                        Add(keys, newValue);
                    }
                    return;
                }              
            }

            var lastNodeIndex = TryGetNode(keys[keys.Count - 1], true);
            ref var node = ref Node(lastNodeIndex);
            var newComputedValue = newValueFunc(node.HasValue, node.Value);

            if (node.HasValue == false && addValueIfNotExist){
                Add(keys, newComputedValue);
            }
            else if (node.HasValue){
                node.Value = newComputedValue;
                _nodes.ReassignNode(ref node, lastNodeIndex);
            }
            
        }

        public void DoWithValue(int key, Func<bool, T, T> newValueFunc, bool addValueIfNotExist = true){

            var lastNodeIndex = TryGetNode(key, true);
            ref var node = ref Node(lastNodeIndex);
            var newComputedValue = newValueFunc(node.HasValue, node.Value);

            if (node.HasValue == false && addValueIfNotExist){
                Add(key, newComputedValue);
            }
            else if (node.HasValue){
                node.Value = newComputedValue;
                _nodes.ReassignNode(ref node, lastNodeIndex);
            }            
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
                if (node.Node.Node_0 != 0)
                {
                    return node.Node.Node_0;
                }

                node.RightVisited = true;
                return node.Node.Node_1;
            }

            if (node.RightVisited == false)
            {
                node.RightVisited = true;
                if (node.Node.Node_1 != 0)
                {
                    return node.Node.Node_1;
                }
                node.LeftVisited = true;
                return node.Node.Node_0;
            }

            return 0;
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
                (LeftVisited && Node.Node_1 == 0) ||
                (RightVisited && Node.Node_0 == 0)
                ;
        }

        public override string ToString()
        {
            return $"{nameof(Node)}: {Node}, {nameof(LeftVisited)}: {LeftVisited}, {nameof(RightVisited)}: {RightVisited}";
        }
    }
}
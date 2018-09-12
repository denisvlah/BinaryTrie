using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace BinaryTrieImpl
{
    public class MemoryMappedNodeContainer<T>: INodesContainer<T>, IDisposable
        where T: struct 
    {
        private readonly string _fileName;
        private readonly long _offset;
        private readonly long _size;
        private readonly long _fullSize;
        private MemoryMappedFile _mmf;
        private MemoryMappedViewAccessor _accessor;
        private int _currentIndex;

        public MemoryMappedNodeContainer(string fileName, long offset, long size)
        {
            _fileName = fileName;
            _offset = offset;            
            _size = SizeHelper.SizeOf(typeof(TrieNode<T>));
            _fullSize = size * _size + _offset;
            _mmf = MemoryMappedFile.CreateFromFile(
                _fileName, 
                FileMode.OpenOrCreate, 
                null, 
                _fullSize
            );
            _accessor = _mmf.CreateViewAccessor(offset, _fullSize);
            
            _currentIndex = 0;
        }

        private TrieNode<T> _lastNode;
        public ref TrieNode<T> AddNewNode()
        {
            _lastNode = new TrieNode<T>(_currentIndex, -1, -1);
            ref var node = ref _lastNode;            
            _accessor.Write(_currentIndex*_size, ref node);
            _currentIndex++;

            return ref node;
        }

        public ref TrieNode<T> Get(int index)
        {
            _accessor.Read(index*_size, out _lastNode);
            return ref _lastNode;
        }

        public int Size()
        {
            return _currentIndex;
        }

        public void SetValue(int nodeIndex, T value)
        {
            ref var node = ref Get(nodeIndex);
            node.Value = value;
            ReassignNode(ref node);
        }

        public void ReassignNode(ref TrieNode<T> newNode)
        {
            var index = newNode.CurrentIndex;
            _accessor.Write(index*_size, ref newNode);
        }

        public void Dispose()
        {
            _accessor.Dispose();
            _mmf.Dispose();
        }
    }
    
    static class SizeHelper
    {
        private static Dictionary<Type, int> sizes = new Dictionary<Type, int>();

        public static int SizeOf(Type type)
        {
            int size;
            if (sizes.TryGetValue(type, out size))
            {
                return size;
            }

            size = SizeOfType(type);
            sizes.Add(type, size);
            return size;            
        }

        private static int SizeOfType(Type type)
        {
            var dm = new DynamicMethod("SizeOfType", typeof(int), new Type[] { });
            ILGenerator il = dm.GetILGenerator();
            il.Emit(OpCodes.Sizeof, type);
            il.Emit(OpCodes.Ret);
            return (int)dm.Invoke(null, null);
        }
    }
}
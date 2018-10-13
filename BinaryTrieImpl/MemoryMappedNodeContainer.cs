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
        private int _valuesCount = 0;
        private int _currentIndex;

        private readonly string _fileName;
        private readonly long _offset;
        private readonly long _nodesSizeBytes;
        private readonly long _fullSize;
        private MemoryMappedFile _mmf;
        private MemoryMappedViewAccessor _accessor;
        

        public MemoryMappedNodeContainer(string fileName, long? maxNodesCount = null)
        {
            _offset = sizeof(int) * 2;
            _fileName = fileName;        
            _nodesSizeBytes = SizeHelper.SizeOf(typeof(TrieNode<T>));
            
            var fileInfo = new FileInfo(fileName);

            if (fileInfo.Exists)
            {
                _fullSize = fileInfo.Length;
            }
            else
            {
                if (maxNodesCount.HasValue == false)
                {
                    maxNodesCount = 100000 * 32;
                }
                _fullSize = maxNodesCount.Value * _nodesSizeBytes + _offset;
            }
            
            _mmf = MemoryMappedFile.CreateFromFile(
                _fileName, 
                FileMode.OpenOrCreate, 
                null, 
                _fullSize
            );
            _accessor = _mmf.CreateViewAccessor(0, _fullSize);

            _valuesCount = ReadValuesCount();
            
            _currentIndex = ReadCurrentIndex();
        }

        private int ReadCurrentIndex()
        {
            return _accessor.ReadInt32(sizeof(int) + 1);
        }

        private void WriteCurrentIndex()
        {
            _accessor.Write(sizeof(int) + 1, _currentIndex);
        }

        private int ReadValuesCount()
        {
            return _accessor.ReadInt32(0);
        }

        private void WriteValuesCount()
        {
            _accessor.Write(0, _valuesCount);
        }

        private TrieNode<T> _lastNode;
        public ref TrieNode<T> AddNewNode()
        {
            _lastNode = new TrieNode<T>(_currentIndex, -1, -1);
            ref var node = ref _lastNode;            
            _accessor.Write(_currentIndex*_nodesSizeBytes + _offset, ref node);
            _currentIndex++;
            WriteCurrentIndex();

            return ref node;
        }

        public ref TrieNode<T> Get(int index)
        {
            _accessor.Read(index*_nodesSizeBytes + _offset, out _lastNode);
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
            _accessor.Write(index*_nodesSizeBytes + _offset, ref newNode);
        }

        public void Dispose()
        {
            _accessor.Dispose();
            _mmf.Dispose();
        }

        public void InitFirstNode()
        {
            if (_currentIndex == 0){
                AddNewNode();
            }
        }

        public void IncrementValuesCount()
        {
            _valuesCount++;
            WriteValuesCount();
        }

        public void DecrementValuesCount()
        {
            _valuesCount--;
            WriteValuesCount();
        }

        public int GetValuesCount()
        {
            return _valuesCount;
        }

        public int GetLastNodeIndex()
        {
            return _currentIndex;
        }

        public string FileName {get{ return _fileName;}}

        public long FileSizeBytes{get{return _fullSize;}}
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
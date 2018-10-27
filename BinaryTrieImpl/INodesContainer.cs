using System;

namespace BinaryTrieImpl
{
    public interface INodesContainer<T>: IDisposable
    {
        ref TrieNode<T> AddNewNode(out int index);
        ref TrieNode<T> Get(int index);
        int Size();
        void SetValue(int nodeIndex, T value);
        void ReassignNode(ref TrieNode<T> newNode, int nodeIndex);
        void InitFirstNode();
        void IncrementValuesCount();
        void DecrementValuesCount();
        int GetValuesCount();
    }
}
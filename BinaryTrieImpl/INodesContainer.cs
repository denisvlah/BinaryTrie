using System;

namespace BinaryTrieImpl
{
    public interface INodesContainer<T>: IDisposable
    {
        ref TrieNode<T> AddNewNode();
        ref TrieNode<T> Get(int index);
        int Size();
        void SetValue(int nodeIndex, T value);
        void ReassignNode(ref TrieNode<T> newNode);
        void InitFirstNode();
        void IncrementValuesCount();
        void DecrementValuesCount();
        int GetValuesCount();
    }
}
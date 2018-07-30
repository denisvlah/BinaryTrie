namespace BinaryTrieImpl
{
    public interface INodesContainer<T>
    {
        ref TrieNode<T> AddNewNode();
        ref TrieNode<T> Get(int index);
        int Size();
        void SetValue(int nodeIndex, T value);
        void ReassignNode(ref TrieNode<T> newNode);
    }
}
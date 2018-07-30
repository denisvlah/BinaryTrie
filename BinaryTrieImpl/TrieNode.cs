namespace BinaryTrieImpl
{
    public struct TrieNode<T>
    {
        public bool Key;

        public T Value { get {return _value;} set { _value = value; HasValue = true; } }

        public int CurrentIndex { get;  set; }
        public bool IsTerminal()
        {
            return Node_0 == -1 && Node_1 == -1;
        }

        public int ParentIndex;
        public bool HasValue;
        public int Node_0;
        public int Node_1;
        private T _value;

        public TrieNode(int currentIndex, int i, int i1)
        {
            CurrentIndex = currentIndex;
            Node_0 = i;
            Node_1 = i1;
            _value = default;
            ParentIndex = -1;
            Key = false;
            HasValue = false;
        }       
        

        public int NextNodeIndex(bool key)
        {
            if (key)
            {
                return Node_1;
            }

            return Node_0;
        }

        public void AddIndex(bool key, int index)
        {
            if (key)
            {
                Node_1 = index;
            }
            else
            {
                Node_0 = index;
            }
        }

        public void RemoveValue()
        {
            HasValue = false;
            _value = default(T);
        }
    }
}
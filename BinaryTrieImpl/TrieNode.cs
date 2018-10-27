using System;

namespace BinaryTrieImpl
{
    public struct TrieNode<T>
    {
        public bool Key;

        public T Value { get {return _value;} set { _value = value; HasValue = true; } }
        
        public bool IsTerminal()
        {
            return Node_0 == -1 && Node_1 == -1;
        }
       
        public bool HasValue;
        public int Node_0;
        public int Node_1;
        private T _value;

        public TrieNode(int i, int i1)
        {            
            Node_0 = i;
            Node_1 = i1;
            _value = default;            
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

        public int GetNextIndex(bool preferLeft)
        {
            if (preferLeft)
            {
                if (Node_0 != -1)
                {
                    return Node_0;
                }

                return Node_1;
            }
            else
            {
                if (Node_1 != -1)
                {
                    return Node_1;
                }

                return Node_0;
            }
            
            
        }

        public override string ToString()
        {
            return
                $"{nameof(Key)}: {Key}, {nameof(Node_0)}: {Node_0}, {nameof(Node_1)}: {Node_1}, {nameof(Value)}: {Value}";
        }
    }
}
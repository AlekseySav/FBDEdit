using System.IO;

namespace FBDEdit
{
    enum SerializerObjects
    {
        None, Key, Value, Item, ArrayBegin, ArrayEnd
    }
    class Serializer
    {
        private StreamWriter sw;
        private SerializerObjects objects;
        public Serializer(StreamWriter sw)
        {
            this.sw = sw;
            objects = SerializerObjects.None;
        }

        public void Key(string key)
        {
            if(objects == SerializerObjects.None || objects == SerializerObjects.ArrayBegin)
                sw.Write("{0}=", key);
            else sw.Write(" {0}=", key);
            objects = SerializerObjects.Key;
        }
        public void Value<T>(T value)
        {
            sw.Write("'{0}'", value);
            objects = SerializerObjects.Value;
        }
        public void Item<T>(T item)
        {
            if (objects == SerializerObjects.None || objects == SerializerObjects.ArrayBegin)
                sw.Write("'{0}'", item);
            else sw.Write(",'{0}'", item);
            objects = SerializerObjects.Item;
        }
        public void ArrayBegin()
        {
            sw.Write("[");
            objects = SerializerObjects.ArrayBegin;
        }
        public void ArrayEnd()
        {
            sw.Write("]");
            objects = SerializerObjects.ArrayEnd;
        }
        public void Pair<T>(T a, T b)
        {
            sw.Write("<'{0}','{1}'>", a, b);
        }
        public void Next()
        {
            sw.Write("\n");
            objects = SerializerObjects.None;
        }
    }

    class Deserializer
    {
        private StreamReader sr;
        private char[] line;
        private int i = 0;
        public Deserializer(StreamReader sr)
        {
            this.sr = sr;
        }

        public string Key()
        {
            if (line.Length == i) return null;
            while (!char.IsLetterOrDigit(line[i]))
            {
                i++;
                if (line.Length == i) return null;
            }
            int j = i;
            while (line[j] != '=')
            {
                j++;
                if (line.Length == j) return null;
            }
            string s = new string(line, i, j - i);
            i = j + 1;
            return s;
        }
        public string Value()
        {
            while (line[i++] != '\'');
            int j = i;
            while (line[j] != '\'') j++;
            string s = new string(line, i, j - i);
            i = j + 1;
            return s;
        }
        public void ArrayBegin()
        {
           while (line[i++] != '[');
        }
        public void ArrayEnd()
        {
            while (line[i++] != ']');
        }
        public void Pair(out string a, out string b)
        {
            while (line[i++] != '<');
            a = Value();
            while (line[i++] != ',');
            b = Value();
            while (line[i++] != '>');
        }
        public bool Next()
        {
            string s = sr.ReadLine();
            if(s == null) return false;
            i = 0;
            line = s.ToCharArray();
            return true;
        }
    }
}

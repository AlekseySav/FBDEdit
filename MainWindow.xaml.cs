using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FBDEdit
{
    public partial class MainWindow : Window
    {
        public static RoutedUICommand NewTemplateCommand = new RoutedUICommand();
        public static RoutedUICommand NewClearCommand = new RoutedUICommand();
        public static RoutedUICommand SerializeCommand = new RoutedUICommand();
        public static RoutedUICommand DeserializeCommand = new RoutedUICommand();
        public static RoutedUICommand AddItemCommand = new RoutedUICommand();

        private List<Item> items;

        public MainWindow()
        {
            InitializeComponent();
            items = new List<Item>();
            LoadTemplate();
        }

        private Item AddItem(double x, double y, ItemType t, string name)
        {
            Item item = new Item
            {
                Type = t,
                ItemName = name
            };
            Canvas.SetLeft(item, x);
            Canvas.SetTop(item, y);
            Source.Children.Add(item);
            items.Add(item);
            return item;
        }
        public void RemoveItem(Item item)
        {
            foreach (ItemNode node in item.InputNodes)
                items[items.Count - 1].DeleteNode(node, true);
            foreach (ItemNode node in item.OutputNodes)
                items[items.Count - 1].DeleteNode(node, false);
            Source.Children.Remove(item);
            items.Remove(item);
        }

        private void Clear()
        {
            while(items.Count != 0)
            {
                foreach (ItemNode node in items[items.Count - 1].InputNodes)
                    items[items.Count - 1].DeleteNode(node, true);
                foreach (ItemNode node in items[items.Count - 1].OutputNodes)
                    items[items.Count - 1].DeleteNode(node, false);
                Source.Children.Remove(items[items.Count - 1]);
                items.RemoveAt(items.Count - 1);
            }
        }
        private void LoadTemplate()
        {
            Clear();
            Item i1 = AddItem(10, 10, ItemType.Var, "input_1");
            Item i2 = AddItem(10, 70, ItemType.Var, "input_2");
            Item i3 = AddItem(200, 30, ItemType.Func, "add");
            Item i4 = AddItem(410, 30, ItemType.Func, "end");
            i1.Inputs = 0;
            i2.Inputs = 0;
            i1.Connect(i3, 0, 0);
            i2.Connect(i3, 1, 0);
            i3.Connect(i4, 0, 0);
        }

        private void Serialize(string path)
        {
            using (StreamWriter sw = new StreamWriter(path))
            {
                Serializer serializer = new Serializer(sw);
                foreach (Item item in items)
                {
                    serializer.Key("Type");
                    serializer.Value(item.Type);
                    serializer.Key("Name");
                    serializer.Value(item.ItemName);
                    serializer.Key("InputsCount");
                    serializer.Value(item.Inputs);
                    serializer.Key("OutputsCount");
                    serializer.Value(item.Outputs);
                    serializer.Key("InputNames");
                    serializer.Value(item.InputNames);
                    serializer.Key("OutputNames");
                    serializer.Value(item.InputNames);
                    serializer.Key("Outputs");
                    serializer.ArrayBegin();
                    foreach (ItemNode node in item.OutputNodes)
                    {
                        if(node.over != null) serializer.Pair(items.IndexOf(node.over), node.overn);
                        else serializer.Pair(-1, -1);
                    }
                    serializer.ArrayEnd();
                    serializer.Key("Pos");
                    serializer.Pair(Canvas.GetLeft(item), Canvas.GetTop(item));
                    serializer.Next();
                }
            }
        }
        private void Deserialize(string path)
        {
            List<List<int>> outputs = new List<List<int>>();
            using (StreamReader sr = new StreamReader(path))
            {
                Deserializer deserializer = new Deserializer(sr);
                while(deserializer.Next())
                {
                    Item item = new Item();
                    outputs.Add(new List<int>());
                    while (true)
                    {
                        string key = deserializer.Key();
                        if (key == null) break;
                        switch (key)
                        {
                            case "Type":
                                string s = deserializer.Value();
                                if (s == "Var") item.Type = ItemType.Var;
                                else if (s == "Func") item.Type = ItemType.Func;
                                else item.Type = ItemType.FuncHeader;
                                break;
                            case "Name":
                                item.ItemName = deserializer.Value();
                                break;
                            case "InputsCount":
                                item.Inputs = int.Parse(deserializer.Value());
                                break;
                            case "OutputsCount":
                                item.Outputs = int.Parse(deserializer.Value());
                                break;
                            case "InputNames":
                                item.InputNames = bool.Parse(deserializer.Value());
                                break;
                            case "OutputNames":
                                item.OutputNames = bool.Parse(deserializer.Value());
                                break;
                            case "Outputs":
                                deserializer.ArrayBegin();
                                for(int i = 0; i < item.Outputs; i++)
                                {
                                    deserializer.Pair(out string a, out string b);
                                    outputs[outputs.Count - 1].Add(int.Parse(a));
                                    item.OutputNodes[i].overn = int.Parse(b);
                                }
                                deserializer.ArrayEnd();
                                break;
                            case "Pos":
                                deserializer.Pair(out string x, out string y);
                                Canvas.SetLeft(item, double.Parse(x));
                                Canvas.SetTop(item, double.Parse(y));
                                break;
                        }
                    }
                    Source.Children.Add(item);
                    items.Add(item);
                }
            }
            for (int i = 0; i < items.Count; i++)
                for (int j = 0; j < items[i].Outputs; j++)
                    if(outputs[i][j] != -1)
                        items[i].Connect(items[outputs[i][j]], items[i].OutputNodes[j].overn, j);
        }

        private void NewTemplateCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            LoadTemplate();
        }
        private void NewClearCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Clear();
        }
        private void SerializeCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Serialize("1.fbd");
        }
        private void DeserializeCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Clear();
            Deserialize("1.fbd");
        }
        private void AddItemCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            AddItem(30, 30, ItemType.Func, "nop");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FBDEdit
{
    public enum ItemType : int
    {
        Var = 0,
        Func = 1,
        FuncHeader = 2
    }
    public struct FunctionType
    {
        public string Name;
        public int Inputs, Outputs;
        public FunctionType(string name, int ic, int oc)
        {
            Name = name;
            Inputs = ic;
            Outputs = oc;
        }

        public static HashSet<FunctionType> Types = new HashSet<FunctionType>
        {
            new FunctionType("nop", 0, 0),
            new FunctionType("end", 1, 0),
            new FunctionType("and", 2, 1),
            new FunctionType("or", 2, 1),
            new FunctionType("xor", 2, 1),
            new FunctionType("add", 2, 1),
            new FunctionType("sub", 2, 1),
            new FunctionType("mul", 2, 1),
            new FunctionType("div", 2, 1),
            new FunctionType("neg", 1, 1),
            new FunctionType("not", 1, 1)
        };
    }
    public class ItemNode
    {
        public static Item LastSrc = null, LastDst = null;
        public static int LastSrcN, LastDstN;

        public Item over;
        public int overn;
        public TextBox text;
        public Ellipse node;
        public Line line;
        public ItemNode(Ellipse e)
        {
            node = e;
        }
    }
    public partial class Item : UserControl
    {
        private Point _MousePrev;
        public List<ItemNode> InputNodes, OutputNodes;
        private int _Inputs, _Outputs;
        private bool _InputNames, _OutputNames;
        private ItemType _Type;
        public int Inputs
        {
            get { return _Inputs; }
            set
            {
                Height = ItemGrid.RowDefinitions[1].Height.Value + Math.Max(value, _Outputs) * 20 + 24;
                if (_Inputs < value)
                    for (int i = _Inputs; i < value; i++)
                    {
                        InputNodes.Add(CreateNode(i, true));
                        if (InputNames) CreateNodeName(i, true);
                    }
                else
                    while (InputNodes.Count != value)
                    {
                        DeleteNode(InputNodes[InputNodes.Count - 1], true);
                        InputNodes.RemoveAt(InputNodes.Count - 1);
                    }
                _Inputs = value;
            }
        }
        public int Outputs
        {
            get { return _Outputs; }
            set
            {
                Height = ItemGrid.RowDefinitions[1].Height.Value + Math.Max(value, _Inputs) * 20 + 24;
                if (_Outputs < value)
                    for (int i = _Outputs; i < value; i++)
                    {
                        OutputNodes.Add(CreateNode(i, false));
                        if(OutputNames) CreateNodeName(i, false);
                    }
                else
                    while (OutputNodes.Count != value)
                    {
                        DeleteNode(OutputNodes[OutputNodes.Count - 1], false);
                        OutputNodes.RemoveAt(OutputNodes.Count - 1);
                    }
                _Outputs = value;
            }
        }
        public bool InputNames
        {
            get { return _InputNames; }
            set
            {
                if (_InputNames == value)
                    return;
                if (value)
                    for (int i = 0; i < Inputs; i++)
                    {
                        if (InputNodes[i].text != null) continue;
                        CreateNodeName(i, true);
                    }
                else
                    for(int i = 0; i < InputNodes.Count; i++)
                        DeleteNodeName(i, true);
                _InputNames = value;
            }
        }
        public bool OutputNames
        {
            get { return _OutputNames; }
            set
            {
                if (_OutputNames == value)
                    return;
                if (value)
                    for (int i = 0; i < Outputs; i++)
                    {
                        if (OutputNodes[i].text != null) continue;
                        CreateNodeName(i, false);
                    }
                else
                    for (int i = 0; i < OutputNodes.Count; i++)
                        DeleteNodeName(i, false);
                _OutputNames = value;
            }
        }
        public ItemType Type
        {
            get { return _Type; }
            set
            {
                _Type = value;
                switch (_Type)
                {
                    case ItemType.Var:
                        NameTextBox.FontStyle = FontStyles.Italic;
                        NameTextBox.FontWeight = FontWeights.SemiBold;
                        NameTextBox.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 120));
                        Inputs = 1;
                        Outputs = 1;
                        break;
                    case ItemType.Func:
                        NameTextBox.FontStyle = FontStyles.Italic;
                        NameTextBox.FontWeight = FontWeights.Normal;
                        NameTextBox.Foreground = new SolidColorBrush(Color.FromRgb(30, 30, 30));
                        break;
                    case ItemType.FuncHeader:
                        NameTextBox.FontStyle = FontStyles.Italic;
                        NameTextBox.FontWeight = FontWeights.SemiBold;
                        NameTextBox.Foreground = new SolidColorBrush(Color.FromRgb(30, 30, 30));
                        Inputs = 0;
                        break;
                }
            }
        }
        public string ItemName 
        { 
            get { return NameTextBox.Text; } 
            set { NameTextBox.Text = value; }
        }

        public Item()
        {
            InputNodes = new List<ItemNode>();
            OutputNodes = new List<ItemNode>();
            InitializeComponent();
            NameTextBox_TextChanged(null, null);
            Panel.SetZIndex(this, 1);
        }

        private double NodeIndexToPosition(int n)
        {
            return n * 20 + 7;
        }
        private int NodePositionToIndex(double pos)
        {
            return Convert.ToInt32((pos - 7) / 20);
        }

        public ItemNode CreateNode(int n, bool input)
        {
            Ellipse e = new Ellipse
            {
                Width = 5,
                Height = 5,
                Fill = new SolidColorBrush(Color.FromRgb(0, 0, 0))
            };
            ItemNode node = new ItemNode(e);
            Source.Children.Add(e);
            Canvas.SetTop(e, NodeIndexToPosition(n));
            if (input)
            {
                Canvas.SetLeft(e, 0.0);
                e.MouseLeftButtonDown += InputNode_MouseLeftButtonDown;
            }
            else
            {
                Canvas.SetRight(e, 0.0);
                e.MouseLeftButtonDown += OutputNode_MouseLeftButtonDown;
                node.line = new Line()
                {
                    Stroke = new SolidColorBrush(Color.FromRgb(80, 80, 200)),
                    StrokeThickness = 4
                };
                ((MainWindow)Application.Current.MainWindow).Source.Children.Add(node.line);
                Panel.SetZIndex(node.line, 0);
            }
            return node;
        }
        public void DeleteNode(ItemNode node, bool input)
        {
            if (input)
            {
                if (node.over != null)
                {
                    node.over.OutputNodes[node.overn].over = null;
                    node.over.UpdateNode(node.overn, false);
                }
            }
            else
            {
                ((MainWindow)Application.Current.MainWindow).Source.Children.Remove(node.line);
                if(node.over != null) node.over.InputNodes[node.overn].over = null;
            }
            Source.Children.Remove(node.node);
            if(node.text != null) Source.Children.Remove(node.text);
        }
        private void CreateNodeName(int n, bool input)
        {
            TextBox t = new TextBox
            {
                Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)),
                Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 120)),
                BorderThickness = new Thickness(0),
                MinWidth = 30,
                MaxWidth = 65,
                TextAlignment = TextAlignment.Left,
                FontStyle = FontStyles.Italic
            };
            Canvas.SetTop(t, n * 20);
            Source.Children.Add(t);
            if (input)
            {
                t.Text = "in_" + n;
                Canvas.SetLeft(t, 7.0);
                InputNodes[n].text = t;
            }
            else
            {
                t.Text = "out_" + n;
                Canvas.SetRight(t, 7.0);
                OutputNodes[n].text = t;
            }
        }
        private void DeleteNodeName(int n, bool input)
        {
            if (input)
            {
                Source.Children.Remove(InputNodes[n].text);
                InputNodes[n].text = null;
            }
            else
            {
                Source.Children.Remove(OutputNodes[n].text);
                OutputNodes[n].text = null;
            }
        }
        private void UpdateNode(int n, bool input)
        {
            if (input)
            {
                if (InputNodes[n].over != null)
                {
                    InputNodes[n].line.X1 = Canvas.GetLeft(InputNodes[n].over) + 148;
                    InputNodes[n].line.Y1 = Canvas.GetTop(InputNodes[n].over) + NodeIndexToPosition(InputNodes[n].overn) + 25.5;
                    InputNodes[n].line.X2 = Canvas.GetLeft(this) + 3;
                    InputNodes[n].line.Y2 = Canvas.GetTop(this) + NodeIndexToPosition(n) + 25.5;
                }
            }
            else
            {
                OutputNodes[n].line.X1 = Canvas.GetLeft(this) + 148;
                OutputNodes[n].line.Y1 = Canvas.GetTop(this) + NodeIndexToPosition(n) + 25.5;
                if (OutputNodes[n].over != null)
                {
                    OutputNodes[n].line.X2 = Canvas.GetLeft(OutputNodes[n].over) + 3;
                    OutputNodes[n].line.Y2 = Canvas.GetTop(OutputNodes[n].over) + NodeIndexToPosition(OutputNodes[n].overn) + 25.5;
                }
                else
                {
                    OutputNodes[n].line.X2 = OutputNodes[n].line.X1;
                    OutputNodes[n].line.Y2 = OutputNodes[n].line.Y1;
                }
            }
        }

        public void Connect(Item dst, int dstn, int srcn)
        {
            if (OutputNodes[srcn].over != null)
            {
                OutputNodes[srcn].over.InputNodes[OutputNodes[srcn].overn].line = null;
                OutputNodes[srcn].over.InputNodes[OutputNodes[srcn].overn].over = null;
            }
            OutputNodes[srcn].over = dst;
            OutputNodes[srcn].overn = dstn;
            dst.InputNodes[dstn].over = this;
            dst.InputNodes[dstn].overn = srcn;
            dst.InputNodes[dstn].line = OutputNodes[srcn].line;
            UpdateNode(srcn, false);
        }

        private void NameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Type != ItemType.Func) return;
            foreach (FunctionType type in FunctionType.Types)
                if (type.Name == NameTextBox.Text)
                {
                    Inputs = type.Inputs;
                    Outputs = type.Outputs;
                    NameTextBox.FontWeight = FontWeights.DemiBold;
                    return;
                }
            NameTextBox.FontWeight = FontWeights.Normal;
        }
        private void Properties_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PropertiesWindow p = new PropertiesWindow(this);
            p.ShowDialog();
        }
        private void InputNode_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(ItemNode.LastSrc == null)
            {
                ItemNode.LastDst = this;
                ItemNode.LastDstN = NodePositionToIndex(Canvas.GetTop((Ellipse)sender));
                return;
            }
            ItemNode.LastSrc.Connect(this, NodePositionToIndex(Canvas.GetTop((Ellipse)sender)), ItemNode.LastSrcN);
            ItemNode.LastSrc = null;
        }
        private void OutputNode_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ItemNode.LastDst == null)
            {
                ItemNode.LastSrc = this;
                ItemNode.LastSrcN = NodePositionToIndex(Canvas.GetTop((Ellipse)sender));
                return;
            }
            Connect(ItemNode.LastDst, ItemNode.LastDstN, NodePositionToIndex(Canvas.GetTop((Ellipse)sender)));
            ItemNode.LastDst = null;
        }
        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Released) return;
            _MousePrev = e.GetPosition(Application.Current.MainWindow);
            CaptureMouse();
        }
        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ReleaseMouseCapture();
        }
        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsMouseCaptured)
            {
                Point p = e.GetPosition(Application.Current.MainWindow);
                Canvas.SetLeft(this, p.X - _MousePrev.X + Canvas.GetLeft(this));
                Canvas.SetTop(this, p.Y - _MousePrev.Y + Canvas.GetTop(this));
                _MousePrev = p;
                for (int i = 0; i < InputNodes.Count; i++)
                    UpdateNode(i, true);
                for (int i = 0; i < OutputNodes.Count; i++)
                    UpdateNode(i, false);
            }
        }
    }
}

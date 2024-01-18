using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using TetrisFigures.Interfaces;

namespace TetrisFigures
{
    /// <summary>
    /// Interaction logic for TetrisCellGrid.xaml
    /// </summary>
    public partial class TetrisCellGrid : UserControl, INotifyPropertyChanged
    {
        public TetrisCellGrid(int w, int h) : this()
        {
            cellHeight = h / 20;
            cellWidth = w / 20;
            Width = cellHeight * 20;
            Height = cellWidth * 20;

            for (int i = 0; i < cellHeight; i++)
            {
                WellGrid.RowDefinitions.Add(new RowDefinition());
            }

            for (int j = 0; j < cellWidth; j++)
            {
                WellGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            mainGrid = new ElementaryCell[cellWidth, cellHeight];

            for (int i = 0; i < cellWidth; i++)
            {
                for (int j = 0; j < cellHeight; j++)
                {
                    Rectangle rec = new Rectangle();
                    rec.Stroke = new SolidColorBrush(Colors.White);
                    rec.StrokeThickness = 1;
                    rec.Visibility = Visibility.Visible;
                    Grid.SetRow(rec, i);
                    Grid.SetColumn(rec, j);
                    WellGrid.Children.Add(rec);

                    mainGrid[i, j] = new ElementaryCell() { rect = rec, IsFrozen = false };
                }
            }

            currentFigureCoordinates = new List<Tuple<int, int>>(4);
        }

        private TetrisCellGrid()
        {
            InitializeComponent();
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));

        }

        //number of cells in a horizontal row
        private int _cellWidth;
        //number of cells in a vertical column
        private int _cellHeight;
        //cells in a form of massive, they are actually also
        //the children of the mainGrid
        //the main purpose to hold it here is to trace their visibility
        //through ElementaryCell instances
        private ElementaryCell[,] mainGrid;
        //holds the current figure
        private TetrisUserControl currentFigure;
        private List<Tuple<int, int>>  currentFigureCoordinates;

        public int cellWidth 
        {
            get { return _cellWidth; }
            private set { _cellWidth = value; }
        }
        public int cellHeight 
        {
            get { return _cellHeight;}
            private set { _cellHeight = value; }
        }

        public Grid wellGrid
        {
            get { return WellGrid; }
        }

        public ElementaryCell[,] cells
        {
            get { return mainGrid; }
        }

        /// <summary>
        /// Initiates a new list of tuplets (X,Y) with relative coordinates of the new figure
        /// </summary>
        /// <param name="t"></param>
        public void InsertNewFigure(TetrisUserControl t)
        {
            currentFigureCoordinates.Clear();
            currentFigure = t;
            foreach (Tuple<int, int> tp in t.InitialPosition)
            {
                currentFigureCoordinates.Add(new Tuple<int, int>((_cellWidth / 2) + tp.Item1, tp.Item2));
            }
        }

        public void ClearGrid()
        {
            foreach(ElementaryCell e in mainGrid)
            {
                e.rect.Visibility = Visibility.Hidden;
                e.IsFrozen = false;
            }
        }
    }
}

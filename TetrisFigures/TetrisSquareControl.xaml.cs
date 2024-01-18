using System;
using System.Collections.Generic;
using TetrisFigures.Interfaces;

namespace TetrisFigures
{
    /// <summary>
    /// Interaction logic for TetrisStickControl.xaml
    /// </summary>
    public partial class TetrisSquareControl : TetrisUserControl
    {
        public TetrisSquareControl()
        {
            InitialPosition = new List<Tuple<int, int>>()
            {
                new Tuple<int, int>(-1, -1),
                new Tuple<int, int>(0, -1),
                new Tuple<int, int>(-1, 0),
                new Tuple<int, int>(0, 0)
            };
            InitializeComponent();
            DataContext = this;
        }

        public override void ChangeSize(int size)
        {
            SquareControlWindow.Width = size * 2;
            SquareControlWindow.Height = size * 2;
        }
    }
}

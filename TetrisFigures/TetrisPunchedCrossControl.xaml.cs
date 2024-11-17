using System;
using System.Collections.Generic;
using TetrisFigures.Interfaces;

namespace TetrisFigures
{
    /// <summary>
    /// Interaction logic for TetrisPunchedCrossControl.xaml
    /// </summary>
    public partial class TetrisPunchedCrossControl : TetrisUserControl
    {
        public TetrisPunchedCrossControl()
        {
            InitialPosition = new List<Tuple<int, int>>()
            {
                new Tuple<int, int>(-1, 0),
                new Tuple<int, int>(0, -1),
                new Tuple<int, int>(1, 0),
                new Tuple<int, int>(0, 1)
            };
            InitializeComponent();
            DataContext = this;
        }

        public override void ChangeSize(int size)
        {
            PunchedCrossControlWindow.Width = size * 3;
            PunchedCrossControlWindow.Height = size * 3;
        }
    }
}

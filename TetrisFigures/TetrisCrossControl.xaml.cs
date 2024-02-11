using System;
using System.Collections.Generic;
using TetrisFigures.Interfaces;

namespace TetrisFigures
{
    /// <summary>
    /// Interaction logic for TetrisCrossControl.xaml
    /// </summary>
    public partial class TetrisCrossControl : TetrisUserControl
    {
        public TetrisCrossControl()
        {
            InitialPosition = new List<Tuple<int, int>>()
            {
                new Tuple<int, int>(-1, -1),
                new Tuple<int, int>(0, -1),
                new Tuple<int, int>(1, -1),
                new Tuple<int, int>(0, 0),
                new Tuple<int, int>(0, -2)
            };
            InitializeComponent();
            DataContext = this;
        }

        public override void ChangeSize(int size)
        {
            CrossControlWindow.Width = size * 3;
            CrossControlWindow.Height = size * 3;
        }
    }
}

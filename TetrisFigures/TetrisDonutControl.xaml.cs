using System;
using System.Collections.Generic;
using TetrisFigures.Auxiliary;
using TetrisFigures.Interfaces;

namespace TetrisFigures
{
    /// <summary>
    /// Interaction logic for TetrisDonutControl.xaml
    /// </summary>
    [Complexity(Complexity = GameComplexity.Hard)]
    public partial class TetrisDonutControl : TetrisUserControl
    {
        public TetrisDonutControl()
        {
            InitialPosition = new List<Tuple<int, int>>()
            {
                new Tuple<int, int>(-1, -1),
                new Tuple<int, int>(0, -1),
                new Tuple<int, int>(1, -1),
                new Tuple<int, int>(-1, 0),
                new Tuple<int, int>(1, 0),
                new Tuple<int, int>(-1, 1),
                new Tuple<int, int>(0, 1),
                new Tuple<int, int>(1, 1),
            };
            InitializeComponent();
            DataContext = this;
        }

        public override void ChangeSize(int size)
        {
            DonutControlWindow.Width = size * 3;
            DonutControlWindow.Height = size * 3;
        }
    }
}

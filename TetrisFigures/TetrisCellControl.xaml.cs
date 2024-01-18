using System;
using System.Collections.Generic;
using TetrisFigures.Interfaces;

namespace TetrisFigures
{
    /// <summary>
    /// Interaction logic for TetrisCellControl.xaml
    /// </summary>
    public partial class TetrisCellControl : TetrisUserControl
    {
        public TetrisCellControl()
        {
            InitialPosition = new List<Tuple<int, int>>()
            {
                new Tuple<int, int>(0, 0)
            };
            InitializeComponent();
            DataContext = this;
        }

        public override void ChangeSize(int size)
        {
            CellControlWindow.Width = size;
            CellControlWindow.Height = size;
        }
    }
}
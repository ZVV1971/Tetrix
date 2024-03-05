using System;
using System.Collections.Generic;
using TetrisFigures.Interfaces;

namespace TetrisFigures
{
    /// <summary>
    /// Interaction logic for TetrisStickControl.xaml
    /// </summary>
    public partial class TetrisStickControl : TetrisUserControl
    {
        public TetrisStickControl()
        {
            TransformationMatrix = new List<Tuple<int, int>[]>()
            {
                //transformation for position #0 -- horizontal position
                new Tuple<int, int>[] { 
                    new Tuple<int, int> (2, -1),  //-point #0
                    new Tuple<int, int> (1, 0),   //-point #1 
                    new Tuple<int, int> (0, 1),   //-point #2
                    new Tuple<int, int> (-1, 2)   //-point #3
                },
                //transformation for position #1 -- vertical position
                new Tuple<int, int>[] {
                    new Tuple<int, int> (-2, 1),  //-point #0
                    new Tuple<int, int> (-1, 0),  //-point #1 
                    new Tuple<int, int> (0, -1),  //-point #2
                    new Tuple<int, int> (1, -2)   //-point #3
                }
            };
            InitialPosition = new List<Tuple<int, int>>()
            {
                new Tuple<int, int>(-2, 0),
                new Tuple<int, int>(-1, 0),
                new Tuple<int, int>(0, 0),
                new Tuple<int, int>(1, 0)
            };
            InitializeComponent();
            DataContext = this;
        }

        public override void ChangeSize(int size)
        {
            StickControlWindow.Width = size * 4;
            StickControlWindow.Height = size;
        }

        public override int GetPositionNumber(IList<Tuple<int, int>> tuples)
        {
            //horizontal placement
            if (tuples[0].Item2 == tuples[1].Item2)
                return 0;
            //vertical placement
            else
                return 1;
        }
    }
}

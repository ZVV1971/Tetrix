using System;
using System.Collections.Generic;
using TetrisFigures.Interfaces;

namespace TetrisFigures
{
    /// <summary>
    /// Interaction logic for TetrisRightSnakeControl.xaml
    /// </summary>
    public partial class TetrisRightSnakeControl : TetrisUserControl
    {
        public TetrisRightSnakeControl()
        {
            TransformationMatrix = new List<Tuple<int, int>[]>()
            {
                //transformation for position #0 -- horizontal position
                new Tuple<int, int>[] { 
                    new Tuple<int, int> (0, -2),  //-point #0
                    new Tuple<int, int> (-1, -1), //-point #1 
                    new Tuple<int, int> (0, 0),   //-point #2
                    new Tuple<int, int> (-1, 1)   //-point #3
                },
                //transformation for position #1 -- vertical position
                new Tuple<int, int>[] {
                    new Tuple<int, int> (0, 2),   //-point #0
                    new Tuple<int, int> (1, 1),   //-point #1 
                    new Tuple<int, int> (0, -0),  //-point #2
                    new Tuple<int, int> (1, -1)   //-point #3
                }
            };
            InitialPosition = new List<Tuple<int, int>>()
            {
                new Tuple<int, int>(-1, 0),
                new Tuple<int, int>(0, 0),
                new Tuple<int, int>(0, -1),
                new Tuple<int, int>(1, -1)
            };
            InitializeComponent();
            DataContext = this;
        }

        public override void ChangeSize(int size)
        {
            RightSnakeControlWindow.Width = size * 3;
            RightSnakeControlWindow.Height = size * 2;
        }

        public override int GetPositionNumber(List<Tuple<int, int>> tuples)
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

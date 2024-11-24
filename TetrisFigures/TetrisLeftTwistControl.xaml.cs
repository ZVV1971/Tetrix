using System;
using System.Collections.Generic;
using TetrisFigures.Auxiliary;
using TetrisFigures.Interfaces;

namespace TetrisFigures
{
    /// <summary>
    /// Interaction logic for TetrisLeftTwistControl.xaml
    /// </summary>
    [Complexity(Complexity = GameComplexity.Average)]
    public partial class TetrisLeftTwistControl : TetrisUserControl
    {
        public TetrisLeftTwistControl()
        {
            TransformationMatrix = new List<Tuple<int, int>[]>()
            {
                //transformation for position #0 -- horizontal position
                new Tuple<int, int>[] {
                    new Tuple<int, int> (2, 0),   //-point #0
                    new Tuple<int, int> (1, -1),  //-point #1 
                    new Tuple<int, int> (0, 0),   //-point #2
                    new Tuple<int, int> (-1, 1),  //-point #3
                    new Tuple<int, int> (-2, 0)   //-point #4
                },
                //transformation for position #1 -- vertical position
                new Tuple<int, int>[] {
                    new Tuple<int, int> (-2, 0),  //-point #0
                    new Tuple<int, int> (-1, 1),  //-point #1 
                    new Tuple<int, int> (0, 0),   //-point #2
                    new Tuple<int, int> (1, -1),  //-point #3
                    new Tuple<int, int> (2, 0)    //-point #4
                }
            };
            InitialPosition = new List<Tuple<int, int>>()
            {
                new Tuple<int, int>(-1, -1),
                new Tuple<int, int>(-1, 0),
                new Tuple<int, int>(0, 0),
                new Tuple<int, int>(1, 0),
                new Tuple<int, int>(1, 1)
            };
            InitializeComponent();
            DataContext = this;
        }

        public override void ChangeSize(int size)
        {
            LeftTwistControlWindow.Width = size * 3;
            LeftTwistControlWindow.Height = size * 3;
        }

        public override int GetPositionNumber(IList<Tuple<int, int>> tuples)
        {
            //horizontal placement
            if (tuples[1].Item2 == tuples[2].Item2)
                return 0;
            //vertical placement
            else
                return 1;
        }
    }
}
using System;
using System.Collections.Generic;
using TetrisFigures.Auxiliary;
using TetrisFigures.Interfaces;

namespace TetrisFigures
{
    /// <summary>
    /// Interaction logic for TetrisThreeDotsControl.xaml
    /// </summary>
    [Complexity(Complexity = GameComplexity.Average)]
    public partial class TetrisThreeDotsControl : TetrisUserControl
    {
        public TetrisThreeDotsControl()
        {
            TransformationMatrix = new List<Tuple<int, int>[]>()
            {
                //transformation for position #0 -- diagonal from top-left to bottom-right (/)
                // Initial: (-1,-1), (0,0), (1,1)
                // After 90° CW rotation: (-1,1), (0,0), (1,-1) (\)
                new Tuple<int, int>[] {
                    new Tuple<int, int>(0, 2),   // (-1,-1) to (-1,1)
                    new Tuple<int, int>(0, 0),   // (0,0) stays (0,0)
                    new Tuple<int, int>(0, -2)   // (1,1) to (1,-1)
                },
                //transformation for position #1 -- diagonal from top-right to bottom-left (\)
                // From (-1,1), (0,0), (1,-1)
                // Back to: (-1,-1), (0,0), (1,1) (/)
                new Tuple<int, int>[] {
                    new Tuple<int, int>(0, -2),  // (-1,1) to (-1,-1)
                    new Tuple<int, int>(0, 0),   // (0,0) stays (0,0)
                    new Tuple<int, int>(0, 2)    // (1,-1) to (1,1)
                }
            };
            InitialPosition = new List<Tuple<int, int>>()
            {
                new Tuple<int, int>(-1, -1),
                new Tuple<int, int>(0, 0),
                new Tuple<int, int>(1, 1)
            };
            InitializeComponent();
            DataContext = this;
        }

        public override void ChangeSize(int size)
        {
            ThreeDotsControlWindow.Width = size * 3;
            ThreeDotsControlWindow.Height = size * 3;
        }

        public override int GetPositionNumber(IList<Tuple<int, int>> tuples)
        {
            // Position 0: (-1,-1), (0,0), (1,1)   - top-left to bottom-right (/)
            //   Point 0 is above center (smaller Y), Point 2 is below center (larger Y)
            // Position 1: (-1,1), (0,0), (1,-1)   - top-right to bottom-left (\)
            //   Point 0 is below center (larger Y), Point 2 is above center (smaller Y)

            // Compare relative Y positions: which point is above/below center?
            int firstY = tuples[0].Item2;
            int centerY = tuples[1].Item2;
            int lastY = tuples[2].Item2;

            // Position 0: first point above center (firstY < centerY), last point below center (lastY > centerY)
            // Position 1: first point below center (firstY > centerY), last point above center (lastY < centerY)

            if (firstY < centerY)
                return 0;  // Position 0: /
            else
                return 1;  // Position 1: \
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using TetrisFigures.Interfaces;

namespace TetrisFigures
{
    /// <summary>
    /// Interaction logic for TetrisHalfCrossControl.xaml
    /// </summary>
    public partial class TetrisHalfCrossControl : TetrisUserControl
    {
        public TetrisHalfCrossControl()
        {
            TransformationMatrix = new List<Tuple<int, int>[]>()
            {
                //transformation for position #0 -- initial horizontal position 0 degrees rotation
                new Tuple<int, int>[] {
                    new Tuple<int, int> (1, -1),  //-point #0
                    new Tuple<int, int> (0, 0),   //-point #1 
                    new Tuple<int, int> (-1, 1),  //-point #2
                    new Tuple<int, int> (-1, -1)  //-point #3
                },
                //transformation for position #1 -- vertical position 90 degrees rotation
                new Tuple<int, int>[] {
                    new Tuple<int, int> (1, 1),   //-point #0
                    new Tuple<int, int> (0, 0),   //-point #1 
                    new Tuple<int, int> (-1, -1), //-point #2
                    new Tuple<int, int> (1, -1)   //-point #3
                },
                //transformation for position #2 -- invert horizontal position 180 degrees rotation
                new Tuple<int, int>[] {
                    new Tuple<int, int> (-1, 1),   //-point #0
                    new Tuple<int, int> (0, 0),    //-point #1 
                    new Tuple<int, int> (1, -1),   //-point #2
                    new Tuple<int, int> (1, 1)     //-point #3
                },
                //transformation for position #3 -- invert vertical position 270 degrees rotation
                new Tuple<int, int>[] {
                    new Tuple<int, int> (-1, -1),  //-point #0
                    new Tuple<int, int> (0, 0),    //-point #1 
                    new Tuple<int, int> (1, 1),    //-point #2
                    new Tuple<int, int> (-1, 1)    //-point #3
                }
            };
            InitialPosition = new List<Tuple<int, int>>()
            {
                new Tuple<int, int>(-1, -1),
                new Tuple<int, int>(0, -1),
                new Tuple<int, int>(1, -1),
                new Tuple<int, int>(0, 0)
            };
            InitializeComponent();
            DataContext = this;
        }

        public override void ChangeSize(int size)
        {
            HalfCrossControlWindow.Width = size * 3;
            HalfCrossControlWindow.Height = size * 2;
        }

        public override int GetPositionNumber(List<Tuple<int, int>> tuples)
        {
            if (tuples.Take(3).All(x => x.Item2 == tuples[0].Item2))
            //positions #0 and #2 -- 0 and 180 degrees
            {
                if (tuples[3].Item2 > tuples[2].Item2)
                    //position 0 degrees #0
                    return 0;
                else
                    //position 180 degrees #2
                    return 2;
            }
            else
            //positions #1 and #3 -- 90 and 270 degrees
            {
                if (tuples[3].Item1 > tuples[2].Item1)
                    //position 270 degrees #3
                    return 3;
                else
                    //position 90 degrees #1
                    return 1;
            }
        }
    }
}

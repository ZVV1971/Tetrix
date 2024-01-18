using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TetrisFigures.Auxiliary;

namespace TetrisTests.Auxiliary
{
    [Parallelizable]
    class RowTrackerBasicTests
    {
        public RowTracker rowTracker;
        public List<int> fullRows;

        [SetUp]
        public void Setup()
        {
            rowTracker = new RowTracker(10, 4);
            fullRows = new List<int>();
            rowTracker.RowFull += c_FullRowEvent;
        }

        [Test, Description("Checks if the cleared RowChecker is empty")]
        public void ClearRowTrackerTest()
        {
            rowTracker.Clear();
            Assert.That(rowTracker.Expose().ToList().All(x => x.Item2 == 0));
        }

        [TestCaseSource(nameof(FigurePositions)), Description("Checks rows fullness after one or many figures are added to the grid")]
        public void AddFigureRowTrackerTest(List<List<Tuple<int,int>>> figures, List<int> rows, int top)
        {
            foreach(List<Tuple<int, int>> f in figures)
            {
                rowTracker.AddFigure(f);
            }
            Assert.AreEqual(rows, rowTracker.GetFullRows().ToList());
        }

        [TestCaseSource(nameof(FigurePositions)), Description("Checks the topmost row after one or more figures are added")]
        public void AddFigureTopmostRowRowTrackerTest(List<List<Tuple<int, int>>> figures, List<int> rows, int top)
        {
            foreach (List<Tuple<int, int>> f in figures)
            {
                rowTracker.AddFigure(f);
            }
            Assert.AreEqual(top, rowTracker.Topmost);
        }

        [TestCaseSource(nameof(FigurePositions)), Description("Checks whether full rows were removed after one or more figures added")]
        public void AddFigureRemoveFullRowTrackerTest(List<List<Tuple<int, int>>> figures, List<int> rows, int top)
        {
            foreach (List<Tuple<int, int>> f in figures)
            {
                rowTracker.AddFigure(f);
            }
            rowTracker.RemoveFullRows();
            Assert.IsEmpty(rowTracker.GetFullRows());
        }

        [TestCaseSource(nameof(FigurePositions)), Description("Checks whether full rows do fire the expected event")]
        public void AddFigureFullRowEventTrackerTest(List<List<Tuple<int, int>>> figures, List<int> rows, int top)
        {
            foreach (List<Tuple<int, int>> f in figures)
            {
                rowTracker.AddFigure(f);
            }
            Assert.AreEqual(fullRows, rows);
        }

        public void c_FullRowEvent (object sender, RowFullEventArgs e)
        {
            fullRows.Add(e.RowId);
        }

        public static object[] FigurePositions =
        {
            //a stick in the horizontal position
            new object[]
            {
                new List<List<Tuple<int, int>>>
                {
                    new List<Tuple<int, int>>()
                    {
                        new Tuple<int, int>(0, 9),
                        new Tuple<int, int>(1, 9),
                        new Tuple<int, int>(2, 9),
                        new Tuple<int, int>(3, 9),
                    }
                },
                //list of full rows
                new List<int>() {9},
                //topmost row
                9
            },
            //two left snakes in vertical positions
            new object[]
            {
                new List<List<Tuple<int, int>>>
                {
                    new List<Tuple<int, int>>()
                    {
                        new Tuple<int, int>(0, 7),
                        new Tuple<int, int>(0, 8),
                        new Tuple<int, int>(1, 8),
                        new Tuple<int, int>(1, 9),
                    },
                    new List<Tuple<int, int>>()
                    {
                        new Tuple<int, int>(2, 7),
                        new Tuple<int, int>(2, 8),
                        new Tuple<int, int>(3, 8),
                        new Tuple<int, int>(3, 9),
                    }
                },
                new List<int>() {8},
                7
            },
            //left snake in horzontal position, then left hook in downward horizontal position and finally a stick in vertical position to fill a gap
            new object[]
            {
                new List<List<Tuple<int, int>>>
                {
                    new List<Tuple<int, int>>()
                    {
                        new Tuple<int, int>(0, 9),
                        new Tuple<int, int>(1, 9),
                        new Tuple<int, int>(1, 8),
                        new Tuple<int, int>(2, 8),
                    },
                    new List<Tuple<int, int>>()
                    {
                        new Tuple<int, int>(0, 8),
                        new Tuple<int, int>(0, 7),
                        new Tuple<int, int>(2, 7),
                        new Tuple<int, int>(3, 7),
                    },
                    new List<Tuple<int, int>>()
                    {
                        new Tuple<int, int>(3, 9),
                        new Tuple<int, int>(3, 8),
                        new Tuple<int, int>(3, 7),
                        new Tuple<int, int>(3, 6),
                    }
                },
                new List<int>() {8,7},
                6
            }
        };
    }
}

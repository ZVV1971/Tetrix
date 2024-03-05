using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using TetrisFigures;
using TetrisFigures.Interfaces;

namespace TetrisTests.Figures.Cell
{
    internal class CellTests
    {
        public TetrisUserControl ctrl;

        [SetUp]
        public void Setup()
        {
            ctrl = new TetrisCellControl();
        }

        [TestCaseSource(nameof(CellPositions)), Description("Test whether Cell rotation gives the original Cell position")]
        [Apartment(ApartmentState.STA)]
        public void TestCellRotation(IList<Tuple<int, int>> pos)
        {
            Assert.AreEqual(pos, ctrl.Rotate(pos));
        }

        public static object[] CellPositions =
        {
            new object[]
            {
                new List<Tuple<int, int>>()
                {
                    new Tuple<int, int>(10, 10)
                }
            }
        };
    }
}

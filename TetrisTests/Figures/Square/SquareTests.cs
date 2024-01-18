using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using TetrisFigures;
using TetrisFigures.Interfaces;

namespace TetrisTests.Figures.Square
{
    class SquareTests
    {
        public TetrisUserControl ctrl;

        [SetUp]
        public void Setup()
        {
            ctrl = new TetrisSquareControl();
        }

        [TestCaseSource(nameof(SquarePositions)), Description("Test whether Square rotation gives the original Square position")]
        [Apartment(ApartmentState.STA)]
        public void TestSquareRotation(List<Tuple<int, int>> pos)
        {
            Assert.AreEqual(pos, ctrl.Rotate(pos));
        }

        public static object[] SquarePositions =
        {
            new object[]
            {
                new List<Tuple<int, int>>()
                {
                    new Tuple<int, int>(10, 10),
                    new Tuple<int, int>(11, 10),
                    new Tuple<int, int>(10, 11),
                    new Tuple<int, int>(11, 11),
                }
            }
        };
    }
}

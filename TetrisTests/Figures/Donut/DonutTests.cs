using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using TetrisFigures;
using TetrisFigures.Interfaces;

namespace TetrisTests.Figures.Donut
{
    internal class DonutTests
    {
        public TetrisUserControl ctrl;

        [SetUp]
        public void Setup()
        {
            ctrl = new TetrisDonutControl();
        }

        [TestCaseSource(nameof(DonutPositions)), Description("Test whether Donut rotation gives the original Donut position")]
        [Apartment(ApartmentState.STA)]
        public void TestDonutRotation(IList<Tuple<int, int>> pos)
        {
            Assert.AreEqual(pos, ctrl.Rotate(pos));
        }

        public static object[] DonutPositions =
        {
            new object[]
            {
                new List<Tuple<int, int>>()
                {
                    new Tuple<int, int>(10, 10),
                    new Tuple<int, int>(10, 9),
                    new Tuple<int, int>(10, 8),
                    new Tuple<int, int>(11, 10),
                    new Tuple<int, int>(11, 8),
                    new Tuple<int, int>(12, 10),
                    new Tuple<int, int>(12, 9),
                    new Tuple<int, int>(12, 8)
                }
            }
        };
    }
}

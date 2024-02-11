using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using TetrisFigures;
using TetrisFigures.Interfaces;

namespace TetrisTests.Figures.Cross
{
    internal class CrossTests
    {
        public TetrisUserControl ctrl;

        [SetUp]
        public void Setup()
        {
            ctrl = new TetrisCrossControl();
        }

        [TestCaseSource(nameof(CrossPositions)), Description("Test whether Cross rotation gives the original Cross position")]
        [Apartment(ApartmentState.STA)]
        public void TestCrossRotation(List<Tuple<int, int>> pos)
        {
            Assert.AreEqual(pos, ctrl.Rotate(pos));
        }

        public static object[] CrossPositions =
        {
            new object[]
            {
                new List<Tuple<int, int>>()
                {
                    new Tuple<int, int>(9, 9),
                    new Tuple<int, int>(10, 10),
                    new Tuple<int, int>(10, 9),
                    new Tuple<int, int>(10, 8),
                    new Tuple<int, int>(11, 9)
                }
            }
        };
    }
}

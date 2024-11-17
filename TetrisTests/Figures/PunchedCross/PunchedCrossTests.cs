using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using TetrisFigures;
using TetrisFigures.Interfaces;

namespace TetrisTests.Figures.PunchedCross
{
    internal class PunchedCrossTests
    {
        public TetrisUserControl ctrl;

        [SetUp]
        public void Setup()
        {
            ctrl = new TetrisPunchedCrossControl();
        }

        [TestCaseSource(nameof(PunchedCrossPositions)), Description("Test whether PucnhedCross rotation gives the original PunchedCross position")]
        [Apartment(ApartmentState.STA)]
        public void TestCrossRotation(IList<Tuple<int, int>> pos)
        {
            Assert.AreEqual(pos, ctrl.Rotate(pos));
        }

        public static object[] PunchedCrossPositions =
        {
            new object[]
            {
                new List<Tuple<int, int>>()
                {
                    new Tuple<int, int>(9, 9),
                    new Tuple<int, int>(10, 10),
                    new Tuple<int, int>(10, 8),
                    new Tuple<int, int>(11, 9)
                }
            }
        };
    }
}

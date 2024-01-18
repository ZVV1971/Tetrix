using NUnit.Framework;
using System;
using System.Collections.Generic;
using TetrisFigures.Interfaces;
using TetrisFigures;
using System.Threading;

namespace TetrisTests.Figures.LeftSnake
{
    public class PositionTests
    {
        public TetrisUserControl ctrl;

        [SetUp]
        public void Setup()
        {
            ctrl = new TetrisLeftSnakeControl();
        }

        [TestCaseSource(nameof(LeftSnakePositions)), Description("Test how LeftSnake figure defines positions")]
        [Apartment(ApartmentState.STA)]
        public void TestLeftSnakePositions(List<Tuple<int, int>> pos, int res)
        {
            Assert.AreEqual(res, ctrl.GetPositionNumber(pos));
        }

        [TestCaseSource(nameof(LeftSnakePositions)), Description("Test that LeftSnake figure rotated from one to another position gets unequal position numbers")]
        [Apartment(ApartmentState.STA)]
        public void TestLeftSnakeRotation_OldPos_nonEqual_NewPos(List<Tuple<int, int>> pos, int d)
        {
            Assert.AreNotEqual(ctrl.GetPositionNumber(pos), ctrl.GetPositionNumber(ctrl.Rotate(pos)));
        }

        [TestCaseSource(nameof(LeftSnakePositions)), Description("Test that LeftSnake figure rotated from one to another positions get position numbers deffering in 1")]
        [Apartment(ApartmentState.STA)]
        public void TestLeftSnakeRotation_OldPos_NewPos_diffc_in_1(List<Tuple<int, int>> pos, int d)
        {
            Assert.That(Math.Abs(ctrl.GetPositionNumber(pos) - ctrl.GetPositionNumber(ctrl.Rotate(pos))) == 1);
        }

        [TestCaseSource(nameof(LeftSnakePositions)), Description("Test that LeftSnake figure being twice rotated returns in the same position")]
        [Apartment(ApartmentState.STA)]
        public void TestLeftSnakeRotation_DoubleRotation_theSame(List<Tuple<int, int>> pos, int d)
        {
            Assert.AreEqual(ctrl.GetPositionNumber(pos), ctrl.GetPositionNumber(ctrl.Rotate(ctrl.Rotate(pos))));
        }

        public static object[] LeftSnakePositions =
        {
            //Initial position #0
            new object[]
            {
                new List<Tuple<int, int>>()
                    {
                    new Tuple<int, int>(1, 1),
                    new Tuple<int, int>(2, 1),
                    new Tuple<int, int>(2, 2),
                    new Tuple<int, int>(3, 2)
                    },
                    0
            },
            //Vertical position #1
            new object[]
            {
                new List<Tuple<int, int>>()
                    {
                    new Tuple<int, int>(1, 1),
                    new Tuple<int, int>(1, 2),
                    new Tuple<int, int>(0, 2),
                    new Tuple<int, int>(0, 3)
                    },
                    1
            }
        };
    }
}
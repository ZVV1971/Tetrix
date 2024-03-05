using NUnit.Framework;
using System;
using System.Collections.Generic;
using TetrisFigures.Interfaces;
using TetrisFigures;
using System.Threading;

namespace TetrisTests.Figures.HalfCross
{
    internal class PositionTests
    {
        public TetrisUserControl ctrl;

        [SetUp]
        public void Setup()
        {
            ctrl = new TetrisHalfCrossControl();
        }

        [TestCaseSource(nameof(HalfCrossPositions)), Description("Test how HalfCross figure defines positions")]
        [Apartment(ApartmentState.STA)]
        public void TestHalfCrossPositions(IList<Tuple<int, int>> pos, int res)
        {
            Assert.AreEqual(res, ctrl.GetPositionNumber(pos));
        }

        [TestCaseSource(nameof(HalfCrossPositions)), Description("Test that HalfCross figure rotated from one to another position gets unequal position numbers")]
        [Apartment(ApartmentState.STA)]
        public void TestHalfCrossRotation_OldPos_nonEqual_NewPos(IList<Tuple<int, int>> pos, int d)
        {
            Assert.AreNotEqual(ctrl.GetPositionNumber(pos), ctrl.GetPositionNumber(ctrl.Rotate(pos)));
        }

        [TestCaseSource(nameof(HalfCrossPositions)), Description("Test that HalfCross figure rotated from one to another positions get position numbers deffering in 1 or 3")]
        [Apartment(ApartmentState.STA)]
        public void TestHalfCrossRotation_OldPos_NewPos_diffc_in_1(IList<Tuple<int, int>> pos, int d)
        {
            int posnum = ctrl.GetPositionNumber(pos);
            int res = posnum - ctrl.GetPositionNumber(ctrl.Rotate(pos));

            if (posnum == 3)
                Assert.That(res == 3);
            else
                Assert.That(res == -1);
        }

        [TestCaseSource(nameof(HalfCrossPositions)), Description("Test that HalfCross figure being four times rotated returns in the same position")]
        [Apartment(ApartmentState.STA)]
        public void TestHalfCrossRotation_DoubleRotation_theSame(IList<Tuple<int, int>> pos, int d)
        {
            Assert.AreEqual(ctrl.GetPositionNumber(pos), ctrl.GetPositionNumber(ctrl.Rotate(ctrl.Rotate(ctrl.Rotate(ctrl.Rotate(pos))))));
        }

        [TestCaseSource(nameof(HalfCrossPositions)), Description("Test that HalfCross figure being rotated returns in the next position")]
        [Apartment(ApartmentState.STA)]
        public void TestHalfCrossRotation_SingleRotation_theNext(IList<Tuple<int, int>> pos, int d)
        {
            Assert.AreEqual((d + 1) % HalfCrossPositions.Length, ctrl.GetPositionNumber(ctrl.Rotate(pos)));
        }

        public static object[] HalfCrossPositions =
        {
            //Initial position #0
            new object[]
            {
                new List<Tuple<int, int>>()
                    {
                    new Tuple<int, int>(1, 1),
                    new Tuple<int, int>(2, 1),
                    new Tuple<int, int>(3, 1),
                    new Tuple<int, int>(2, 2)
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
                    new Tuple<int, int>(1, 3),
                    new Tuple<int, int>(0, 2)
                    },
                    1
            },
            //Horizontal position #2
            new object[]
            {
                new List<Tuple<int, int>>()
                    {
                    new Tuple<int, int>(3, 1),
                    new Tuple<int, int>(2, 1),
                    new Tuple<int, int>(1, 1),
                    new Tuple<int, int>(2, 0)
                    },
                    2
            },
            //Inverted vertical position #3
            new object[]
            {
                new List<Tuple<int, int>>()
                    {
                    new Tuple<int, int>(1, 3),
                    new Tuple<int, int>(1, 2),
                    new Tuple<int, int>(1, 1),
                    new Tuple<int, int>(2, 2)
                    },
                    3
            }
        };
    }
}
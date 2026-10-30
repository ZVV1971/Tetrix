using NUnit.Framework;
using System;
using System.Collections.Generic;
using TetrisFigures.Interfaces;
using TetrisFigures;
using System.Threading;

namespace TetrisTests.Figures.ThreeDots
{
    internal class ThreeDotsPositionTests
    {
        public TetrisUserControl ctrl;

        [SetUp]
        public void Setup()
        {
            ctrl = new TetrisThreeDotsControl();
        }

        [TestCaseSource(nameof(ThreeDotsPositions)), Description("Test how ThreeDots figure defines positions")]
        [Apartment(ApartmentState.STA)]
        public void TestThreeDotsPositions(IList<Tuple<int, int>> pos, int res)
        {
            Assert.AreEqual(res, ctrl.GetPositionNumber(pos));
        }

        [TestCaseSource(nameof(ThreeDotsPositions)), Description("Test that ThreeDots figure rotated changes position")]
        [Apartment(ApartmentState.STA)]
        public void TestThreeDotsRotation_OldPos_nonEqual_NewPos(IList<Tuple<int, int>> pos, int _)
        {
            Assert.AreNotEqual(ctrl.GetPositionNumber(pos), ctrl.GetPositionNumber(ctrl.Rotate(pos)));
        }

        [TestCaseSource(nameof(ThreeDotsPositions)), Description("Test that ThreeDots figure rotation changes position by 1")]
        [Apartment(ApartmentState.STA)]
        public void TestThreeDotsRotation_OldPos_NewPos_diff_in_1(IList<Tuple<int, int>> pos, int _)
        {
            int posnum = ctrl.GetPositionNumber(pos);
            int rotatedPosnum = ctrl.GetPositionNumber(ctrl.Rotate(pos));

            // With 2 positions, the difference should be ±1 (wrapping)
            int diff = Math.Abs(posnum - rotatedPosnum);
            Assert.That(diff, Is.EqualTo(1));
        }

        [TestCaseSource(nameof(ThreeDotsPositions)), Description("Test that ThreeDots figure being rotated twice returns to the same position")]
        [Apartment(ApartmentState.STA)]
        public void TestThreeDotsRotation_DoubleRotation_theSame(IList<Tuple<int, int>> pos, int d)
        {
            // Two rotations should return to original position
            Assert.AreEqual(ctrl.GetPositionNumber(pos), ctrl.GetPositionNumber(ctrl.Rotate(ctrl.Rotate(pos))));
        }

        [TestCaseSource(nameof(ThreeDotsPositions)), Description("Test that ThreeDots figure rotated once moves to next position")]
        [Apartment(ApartmentState.STA)]
        public void TestThreeDotsRotation_SingleRotation_theNext(IList<Tuple<int, int>> pos, int d)
        {
            // Single rotation should move to position (d + 1) % 2
            Assert.AreEqual((d + 1) % ThreeDotsPositions.Length, ctrl.GetPositionNumber(ctrl.Rotate(pos)));
        }

        public static object[] ThreeDotsPositions =
        {
            //Initial position #0 (top-left to bottom-right diagonal: /)
            new object[]
            {
                new List<Tuple<int, int>>()
                    {
                    new Tuple<int, int>(-1, -1),
                    new Tuple<int, int>(0, 0),
                    new Tuple<int, int>(1, 1)
                    },
                    0
            },
            //Position #1 (invert diagonal: \)
            new object[]
            {
                new List<Tuple<int, int>>()
                    {
                    new Tuple<int, int>(-1, 1),
                    new Tuple<int, int>(0, 0),
                    new Tuple<int, int>(1, -1)
                    },
                    1
            }
        };
    }
}

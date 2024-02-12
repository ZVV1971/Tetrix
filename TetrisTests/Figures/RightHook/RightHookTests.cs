﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using TetrisFigures.Interfaces;
using TetrisFigures;
using System.Threading;

namespace TetrisTests.Figures.RightHook
{
    internal class PositionTests
    {
        public TetrisUserControl ctrl;

        [SetUp]
        public void Setup()
        {
            ctrl = new TetrisRightHookControl();
        }

        [TestCaseSource(nameof(RightHookPositions)), Description("Test how RightHook figure defines positions")]
        [Apartment(ApartmentState.STA)]
        public void TestRightHookPositions(List<Tuple<int, int>> pos, int res)
        {
            Assert.AreEqual(res, ctrl.GetPositionNumber(pos));
        }

        [TestCaseSource(nameof(RightHookPositions)), Description("Test that RightHook figure rotated from one to another position gets unequal position numbers")]
        [Apartment(ApartmentState.STA)]
        public void TestRightHookRotation_OldPos_nonEqual_NewPos(List<Tuple<int, int>> pos, int d)
        {
            Assert.AreNotEqual(ctrl.GetPositionNumber(pos), ctrl.GetPositionNumber(ctrl.Rotate(pos)));
        }

        [TestCaseSource(nameof(RightHookPositions)), Description("Test that RightHook figure rotated from one to another positions get position numbers deffering in 1 or 3")]
        [Apartment(ApartmentState.STA)]
        public void TestRightHookRotation_OldPos_NewPos_diffc_in_1(List<Tuple<int, int>> pos, int d)
        {
            int posnum = ctrl.GetPositionNumber(pos);
            int res = posnum - ctrl.GetPositionNumber(ctrl.Rotate(pos));

            if (posnum == 3)
                Assert.That(res == 3);
            else
                Assert.That(res == -1);
        }

        [TestCaseSource(nameof(RightHookPositions)), Description("Test that RightHook figure being four times rotated returns in the same position")]
        [Apartment(ApartmentState.STA)]
        public void TestRightHookRotation_DoubleRotation_theSame(List<Tuple<int, int>> pos, int d)
        {
            Assert.AreEqual(ctrl.GetPositionNumber(pos), ctrl.GetPositionNumber(ctrl.Rotate(ctrl.Rotate(ctrl.Rotate(ctrl.Rotate(pos))))));
        }

        [TestCaseSource(nameof(RightHookPositions)), Description("Test that RightHook figure being rotated returns in the next position")]
        [Apartment(ApartmentState.STA)]
        public void TestRightHookRotation_SingleRotation_theNext(List<Tuple<int, int>> pos, int d)
        {
            Assert.AreEqual((d + 1) % RightHookPositions.Length, ctrl.GetPositionNumber(ctrl.Rotate(pos)));
        }

        public static object[] RightHookPositions =
        {
            //Initial position #0
            new object[]
            {
                new List<Tuple<int, int>>()
                    {
                    new Tuple<int, int>(1, 1),
                    new Tuple<int, int>(2, 1),
                    new Tuple<int, int>(3, 1),
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
                    new Tuple<int, int>(1, 3),
                    new Tuple<int, int>(0, 3)
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
                    new Tuple<int, int>(1, 0)
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
                    new Tuple<int, int>(2, 1)
                    },
                    3
            }
        };
    }
}
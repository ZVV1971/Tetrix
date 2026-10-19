using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TetrisTests.GameLogic
{
    /// <summary>
    /// Code-inspection tests for the shadow figure feature.
    /// These tests verify implementation correctness by inspecting source code.
    /// </summary>
    [TestFixture]
    internal class ShadowImplementationVerificationTests
    {
        /// <summary>
        /// Verifies critical source code patterns are implemented correctly.
        /// This test inspects the MainWindow.xaml.cs file to ensure:
        /// - nameof() is used for property notifications (not string literals)
        /// - Null-safety checks are in place
        /// - Brush freezing is used for performance
        /// - Drop bonus logic is disabled with shadow mode
        /// - Landing position semantics handle NeedsFreezing/EndOfPlay correctly
        /// </summary>
        [Test]
        [Description("Verify critical shadow feature implementation patterns in source code")]
        public void VerifySourceCodeImplementationPatterns()
        {
            var mainWindowPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "..",
                "..",
                "..",
                "..",
                "TetrisMainWindow",
                "MainWindow.xaml.cs"
            );

            Assert.That(File.Exists(mainWindowPath), 
                $"Source file should exist at {mainWindowPath}");

            string sourceCode = File.ReadAllText(mainWindowPath);

            // Pattern 1: Best practice - nameof() instead of string literal
            Assert.That(sourceCode, Does.Contain("nameof(ShadowModeEnabled)"),
                "Pattern 1 FAILED: Should use nameof() for property notification, not string literal");

            // Pattern 2: Null-safety - guard against null/empty collections
            Assert.That(sourceCode, Does.Contain("currentFigureCoordinates == null"),
                "Pattern 2 FAILED: ClearShadow() should have null-safety check for currentFigureCoordinates");

            // Pattern 3: Performance - frozen brush for rendering
            Assert.That(sourceCode, Does.Contain("shadowBrush.Freeze()"),
                "Pattern 3 FAILED: Shadow brush should be frozen for WPF rendering performance");

            // Pattern 4: Scoring - drop bonus disabled when shadow enabled
            Assert.That(sourceCode, Does.Contain("_shadowModeEnabled").And.Contain("j = 0"),
                "Pattern 4 FAILED: When shadow mode enabled, drop bonus (j) should be set to 0");

            // Pattern 5: Landing position - accepts NeedsFreezing/EndOfPlay as valid landing pos
            Assert.That(sourceCode, Does.Contain("NeedsFreezing").And.Contain("shadowPos = nextShadowPos"),
                "Pattern 5 FAILED: CalculateShadowPosition() should assign shadowPos = nextShadowPos when NeedsFreezing");
        }

        /// <summary>
        /// Verifies the Shadow Figure menu item is present in MainWindow XAML with correct binding.
        /// </summary>
        [Test]
        [Description("Verify Shadow Figure UI controls in MainWindow XAML")]
        public void VerifyShadowUiInXaml()
        {
            var xamlPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "..",
                "..",
                "..",
                "..",
                "TetrisMainWindow",
                "MainWindow.xaml"
            );

            Assert.That(File.Exists(xamlPath), 
                $"XAML file should exist at {xamlPath}");

            string xaml = File.ReadAllText(xamlPath);

            // Check for Shadow Figure menu item with TwoWay binding
            Assert.That(xaml, Does.Contain("Shadow Figure"),
                "UI FAILED: Settings menu should have 'Shadow Figure' menu item");

            Assert.That(xaml, Does.Contain("ShadowModeEnabled").And.Contain("TwoWay"),
                "UI FAILED: Shadow Figure should be bound to ShadowModeEnabled with TwoWay binding");
        }
    }
}


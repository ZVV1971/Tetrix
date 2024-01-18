using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Windows;
using TetrisFigures;
using TetrisFigures.Interfaces;

namespace TetrisTests.CellGrid
{
    [Parallelizable]
    public class CellGridBasicTests
    {
        public TetrisCellGrid cellGrid;

        [TestCaseSource(nameof(TetrisCellGridSizes)), Description("How many cells are in a row and in a column when construct a standard cell grid assuming defulat cell size = 20")]
        [Apartment(ApartmentState.STA)]
        public void TestCellGridNumberOfRowsAndColumns(int w, int h, int cellw, int cellh)
        {
            cellGrid = new TetrisCellGrid(w, h);
            Assert.That((cellw == cellGrid.cellWidth) & (cellh == cellGrid.cellHeight));
        }

        [TestCaseSource(nameof(TetrisCellGridSizes)), Description("How many cells are in a row and in a column when construct Grid assuming defulat cell size = 20")]
        [Apartment(ApartmentState.STA)]
        public void TEstCellGridInternalGridDimensions(int w, int h, int cellw, int cellh)
        {
            cellGrid = new TetrisCellGrid(w, h);
            Assert.That((cellw == cellGrid.wellGrid.ColumnDefinitions.Count) & (cellh == cellGrid.wellGrid.RowDefinitions.Count));
        }

        public static object[] TetrisCellGridSizes =
        {
            new object[] {401, 802, 20, 40},
            new object[] {399, 802, 19, 40},
            new object[] {401, 799, 20, 39}
        };
    }

    [Parallelizable]
    public class TetrisCellGridClearanceTests
    {
        public TetrisCellGrid tetrisCellGrid;

        [SetUp]
        public void Setup()
        {
            tetrisCellGrid = new TetrisCellGrid(400, 800);
        }

        [TestCaseSource(nameof(TetrisCellGridVisibilites)), Description("Test that the clear method works")]
        [Apartment(ApartmentState.STA)]
        public void TestCellGridCellsVisibility(int w, int h, Visibility v)
        {
            //make one rectangle visible
            tetrisCellGrid.Cells[w, h].rect.Visibility = Visibility.Visible;
            //Call clear method
            tetrisCellGrid.ClearGrid();
            //check if clearance did work
            Assert.AreEqual(tetrisCellGrid.Cells[w,h].rect.Visibility, Visibility.Hidden);
        }

        public static object[] TetrisCellGridVisibilites =
        {
            new object[] {10, 34, Visibility.Hidden}
        };
    }

    [Parallelizable]
    [Apartment(ApartmentState.STA)]
    public class TetrisCellGridInsertionTests
    {
        public TetrisCellGrid tetrisCellGrid;

        [SetUp]
        public void Setup()
        {
            tetrisCellGrid = new TetrisCellGrid(400, 800);
        }

        [TestCaseSource("GetCellControlls"), Description("Test if Figures is inserted into the grid")]
        [Apartment(ApartmentState.STA)]
        public void TestCellGridNewFigure(string figureName)
        {
            Type objectType = Type.GetType(figureName);

            TetrisUserControl c = (TetrisUserControl)Activator.CreateInstance(objectType);
            //insert a new cell figure
            tetrisCellGrid.InsertNewFigure(c);
            //check if the figure did appear
            foreach (Tuple<int, int> tp in c.InitialPosition)
            {
                if (tp.Item2 >= 0)
                {
                    Assert.AreEqual(tetrisCellGrid.Cells[tetrisCellGrid.cellWidth / 2 + tp.Item1, tp.Item2].rect.Visibility, Visibility.Visible);
                }
            }
        }

        [TestCaseSource("GetCellControlls"), Description("Test if Figures is cleared after got inserted into the grid")]
        [Apartment(ApartmentState.STA)]
        public void TestCellGridNewClearFigure(string figureName)
        {
            Type objectType = Type.GetType(figureName);

            TetrisUserControl c = (TetrisUserControl)Activator.CreateInstance(objectType);
            //insert a new cell figure
            tetrisCellGrid.InsertNewFigure(c);
            //cleared it
            tetrisCellGrid.ClearGrid();
            //check if the figure is cleared
            foreach (Tuple<int, int> tp in c.InitialPosition)
            {
                if (tp.Item2 >= 0)
                {
                    Assert.AreEqual(tetrisCellGrid.Cells[tetrisCellGrid.cellWidth / 2 + tp.Item1, tp.Item2].rect.Visibility, Visibility.Hidden);
                }
            }
        }

        [TestCaseSource("GetCellControlls"), Description("Test if Figures is frozen after got inserted into the grid")]
        [Apartment(ApartmentState.STA)]
        public void TestCellGridNewFreezeFigure(string figureName)
        {
            Type objectType = Type.GetType(figureName);

            TetrisUserControl c = (TetrisUserControl)Activator.CreateInstance(objectType);
            //insert a new cell figure
            tetrisCellGrid.InsertNewFigure(c);
            //cleared it
            tetrisCellGrid.SetNeedsFreeze();
            //check if the figure is cleared
            foreach (Tuple<int, int> tp in c.InitialPosition)
            {
                if (tp.Item2 >= 0)
                {
                    Assert.AreEqual(tetrisCellGrid.Cells[tetrisCellGrid.cellWidth / 2 + tp.Item1, tp.Item2].NeedsFreeze, true);
                }
            }
        }

        //Generates a list of UserControll types that are inhereted from the TetrisUserControll, but exludes the latter
        private static object[] GetCellControlls()
        {
            Type t = typeof(TetrisUserControl);

            return AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(s => s.GetTypes())
                        .Where(p => t.IsAssignableFrom(p) & !p.FullName.Contains("Interfaces"))
                        .Select(x => x.FullName + ", TetrisFigures")
                        .ToArray();
        }
    }
}
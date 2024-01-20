using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using TetrisFigures.Auxiliary;
using TetrisFigures.Dialogs;
using TetrisFigures.Helper;
using TetrisFigures.Interfaces;

namespace TetrisMainWindow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            GameStarted = false;
            IsGameOver = false;
            cellSize = 40;
            priceOfTheRow = 10;

            mainGrid = new ElementaryCell[_gridWidth, _gridHeight];

            for (int i = 0; i < mainGrid.GetLength(0); i++)
            {
                for (int j = 0; j < mainGrid.GetLength(1); j++)
                {
                    Rectangle rec = new Rectangle
                    {
                        Stroke = new SolidColorBrush(Colors.White),
                        StrokeThickness = 1,
                        Visibility = Visibility.Visible
                    };

                    Grid.SetColumn(rec, i);
                    Grid.SetRow(rec, j);
                    cellGrid.Children.Add(rec);

                    mainGrid[i, j] = new ElementaryCell() { rect = rec, IsFrozen = false, NeedsFreeze = false };
                }
            }

            if (File.Exists(highScoresFileName))
            {
                byte[] data = File.ReadAllBytes(highScoresFileName);
                try
                {
                    highestScores = (Dictionary<string, int>)ObjectSerialize.DeSerialize(data);
                    int i = highestScores.Max(x => x.Value);
                    TopGamer = highestScores.First(x => x.Value == i).Key;
                    HighestScore = i;
                }
                catch
                {
                    highestScores = new Dictionary<string, int>();
                    HighestScore = 0;
                    TopGamer = "";
                }
            }
            else highestScores = new Dictionary<string, int>();

            VersionNumber = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            _timer = new DispatcherTimer();

            _event_separator = true;
        }

        private string _currentGamer;
        private string _topGamer;
        private readonly string highScoresFileName = "highscores.scr";
        private Dictionary<string, int> highestScores;
        //the highest score to be shown in the StatusBar
        private int _highScore;
        //the size of cell
        private int _gridWidth = 20;
        private int _gridHeight = 40;
        private TetrisUserControl currentFigure;
        private TetrisUserControl nextFigure;
        private bool _gameStarted;
        private readonly int cellSize;
        //keeps track of the score
        private int _score;
        //holds the actual value of the current level
        private int _level;
        //Text on the Start button
        private string _startButtonText = "Start";
        private bool _isGameOver = false;
        //Keeps track of rows to be finished before the next level is started
        private int _rowsToFinish;
        //An array of Elementary cells -- actually the game zone
        //made to be created and filled in App rather than in XAML
        private ElementaryCell[,] mainGrid;
        //keeps track of the current figure coordinates
        private List<Tuple<int, int>> currentFigureCoordinates;
        //an inidicator to keep the highest (lowest Y) frozen cell coordinate on the pile
        //serves to limit the nested cycle
        private int highestCell;
        //number of points added when a row is full
        private int priceOfTheRow = 10;
        //initial timespan in Timer ticks
        private long _initialTimeSpan =
#if DEBUG
            7500000;
#else
            5000000;
#endif
        //percents of the timespan to decrease the intial (previous) one with
        private int _percTimeSpanDecrease =
#if DEBUG
            8;
#else
            15;
#endif
        //rows to be filled for the first level
        private int _initialRowsToFinish =
#if DEBUG
            1;
#else
            10;
#endif
        //_timer for down events
        private DispatcherTimer _timer;
        //holds the current version of the app
        private string _version;
        //indicator-switcher to separate even timer events from odd ones
        private bool _event_separator;
        #region Properties
        public string TopGamer
        {
            get { return _topGamer; }
            private set
            {
                _topGamer = value;
                NotifyPropertyChanged("TopGamer");
            }
        }
        public string startButtonText
        {
            get { return _startButtonText; }
            private set 
            { 
                _startButtonText = value;
                NotifyPropertyChanged("startButtonText");
            }
        }
        public int Level
        {
            get { return _level; }
            private set
            {
                _level = value;
                NotifyPropertyChanged("Level");
            }
        }
        public int Score
        {
            get { return _score; }
            private set
            {
                _score = value;
                NotifyPropertyChanged("Score");
            }
        }
        public bool GameStarted
        {
            get { return _gameStarted; }
            private set 
            { 
                _gameStarted = value;
                NotifyPropertyChanged("GameStarted");
            }
        }
        public bool IsGameOver
        {
            get { return _isGameOver; }
            private set
            {
                _isGameOver = value;
                NotifyPropertyChanged("IsGameOver");
            }
        }
        public int RowsToFinish
        {
            get { return _rowsToFinish; }
            private set
            {
                _rowsToFinish = value;
                NotifyPropertyChanged("RowsToFinish");
            }
        }
        public int HighestScore
        {
            get { return _highScore; }
            private set
            {
                _highScore = value;
                NotifyPropertyChanged("HighestScore");
            }
        }
        public string VersionNumber
        {
            get { return _version; }
            private set
            {
                _version = value;
                NotifyPropertyChanged("VersionNumber");
            }
        }
        #endregion
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));

        }

        /// <summary>
        /// Does preliminary procedures to get the next figure, insert and draw it
        /// </summary>
        private void DoNextFigure()
        {
            GetNextFigure();
            InsertNewFigure(currentFigure);
            DrawFigure(currentFigure, currentFigureCoordinates, true);
        }

        /// <summary>
        /// Calculates possibility of the current figure to come to the new position <paramref name="newPosition"/>
        /// </summary>
        /// <param name="newPosition">The position that the current figure should take next</param>
        /// <returns></returns>
        private MovementOutcomes IsMovementPossible(List<Tuple<int, int>> newPosition)
        {
            //if the position remains the same return impossibility of the transformation
            if (newPosition.Equals(currentFigureCoordinates))
            {
                return MovementOutcomes.Impossible;
            }

            //check whether the new position comes outside the playground borders
            //left, right sides and bottom
            foreach (Tuple<int, int> t in newPosition) 
            {
                if (t.Item1 < 0 || t.Item1 > (_gridWidth - 1) || t.Item2 > (_gridHeight - 1))
                {
                    return MovementOutcomes.Impossible;
                }
                //or overlaps the pile
                if (t.Item2 >= 0 && t.Item2 <= (_gridHeight - 1) && mainGrid[t.Item1, t.Item2].IsFrozen)
                {
                    return MovementOutcomes.Impossible;
                }
            }

            //check whether the new postion would touch the upper layer of the pile 
            //or the bottom
            //and return either End of the Play if any of the cells is on the first line
            //or NeedsFreezing otherwise
            foreach (Tuple<int, int> t in newPosition)
            {
                if (t.Item2 >= -1 && (t.Item2 == (_gridHeight - 1) || mainGrid[t.Item1, t.Item2 + 1].IsFrozen))
                {
                    if (newPosition.Any((x) => x.Item2 <= 0))
                    {
                        return MovementOutcomes.EndOfPlay;
                    }
                    else
                    {
                        return MovementOutcomes.NeedsFreezing;
                    }
                }
            }

            return MovementOutcomes.Possible;
        }

        /// <summary>
        /// Clears the old figure and
        /// draws the current figure on the new position within the playground cells
        /// </summary>
        /// <param name="c">Is used to take the filling color for rectangles</param>
        /// <param name="newPos">Provides new positions of the figure</param>
        private void DrawFigure(TetrisUserControl c, List<Tuple<int, int>> newPos, bool forced = false)
        {
            if (!newPos.Equals(currentFigureCoordinates) || forced)
            {
                foreach (Tuple<int, int> t in currentFigureCoordinates)
                {
                    if (t.Item1 >= 0 && t.Item2 >= 0)
                    {
                        mainGrid[t.Item1, t.Item2].rect.Fill = new SolidColorBrush(Colors.Transparent);
                        mainGrid[t.Item1, t.Item2].rect.Visibility = Visibility.Hidden;
                    }
                }

                foreach (Tuple<int, int> t in newPos)
                {
                    if (t.Item1 >= 0 && t.Item2 >= 0)
                    {
                        mainGrid[t.Item1, t.Item2].rect.Fill = c.color;
                        mainGrid[t.Item1, t.Item2].rect.Visibility = Visibility.Visible;
                    }
                }

                currentFigureCoordinates = newPos;
            }
        }

        /// <summary>
        /// Freezes the current figure -- that's nominally adds it to the pile
        /// and increments the score
        /// </summary>
        private void FreezeCurrentFigure()
        {
            foreach (Tuple<int, int> t in currentFigureCoordinates)
            {
                if (t.Item1 >=0 && t.Item1 <= (_gridWidth - 1) && t.Item2 >=0 && t.Item2 <= (_gridHeight - 1))
                mainGrid[t.Item1, t.Item2].IsFrozen = true;
            }

            Score += currentFigureCoordinates.Count;
            highestCell = Math.Min(highestCell, currentFigureCoordinates.Min(x => x.Item2));
        }

        /// <summary>
        /// Clears the colors of the rectangles on the playground grid and hides them
        /// </summary>
        private void ClearCellGrid()
        {
            for (int i = 0; i < mainGrid.GetLength(0); i++)
            {
                for (int j = 0; j < mainGrid.GetLength(1); j++)
                {
                    mainGrid[i, j].rect.Fill = new SolidColorBrush(Colors.Transparent);
                    mainGrid[i, j].rect.Visibility = Visibility.Hidden;
                    mainGrid[i, j].IsFrozen = false;
                }
            }
        }

        /// <summary>
        /// Asks for new next figure and takes the "current" next figure as the current one
        /// </summary>
        private void GetNextFigure()
        {
            if (!GameStarted)
            {
                currentFigure = GetNewFigure();
                currentFigure.ChangeSize(cellSize);
            }
            else
            {
                nextFigureCell.Children.Remove(nextFigure);
                currentFigure = nextFigure;
            }

            nextFigureCell.Children.Clear();
            nextFigure = GetNewFigure();
            nextFigure.ChangeSize(cellSize);

            Canvas.SetLeft(nextFigure, nextFigureCell.ActualWidth / 2 - nextFigure.Width / 2);
            Canvas.SetBottom(nextFigure, nextFigureCell.ActualHeight / 2 - nextFigure.Height / 2);
            nextFigureCell.Children.Add(nextFigure);
        }

        /// <summary>
        /// Initiates a new list of tuplets (X,Y) with relative coordinates of the new figure
        /// </summary>
        /// <param name="t"></param>
        private void InsertNewFigure(TetrisUserControl t)
        {
            currentFigureCoordinates = new List<Tuple<int, int>>();
            foreach(Tuple<int, int> tp in t.InitialPosition)
            {
                currentFigureCoordinates.Add(new Tuple<int, int>((_gridWidth / 2) + tp.Item1, tp.Item2));
            }
        }

        /// <summary>
        /// Instantiates new TetrisUserControl with random shape and color
        /// </summary>
        /// <returns></returns>
        private TetrisUserControl GetNewFigure()
        {
            TetrisUserControl fig;

            Type t = typeof(TetrisUserControl);

            string[] figureTypes = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(s => s.GetTypes())
                        .Where(q => t.IsAssignableFrom(q) & !q.FullName.Contains("Interfaces"))
                        .Select(x => x.FullName + ", TetrisFigures")
                        .ToArray();

            Random p = new Random();
            int pInt = p.Next(0, figureTypes.Length);

            Type objectType = Type.GetType(figureTypes[pInt]);
            fig = (TetrisUserControl)Activator.CreateInstance(objectType);

            Random r = new Random();
            int rInt = r.Next(0, Enum.GetNames(typeof(TetrisColors)).Length);

            string col = Enum.GetName(typeof(TetrisColors), rInt);

            fig.color = new SolidColorBrush((Color)typeof(Colors).GetProperty(col).GetValue(null, null));

            return fig;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Set focus on the main canvas so as to enable it to catch keyboard pressing
            Keyboard.Focus(cellGrid);
        }

        private void CellGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (!GameStarted) return;

            List<Tuple<int, int>> newPos = new List<Tuple<int, int>>();
            switch (e.Key)
            {
                case Key.Up:
                    newPos = currentFigure.Rotate(currentFigureCoordinates);
                    break;
                case Key.Down:
                    newPos = GetNextFigurePosition();
                    break;
                case Key.Left:
                    foreach (Tuple<int, int> i in currentFigureCoordinates)
                    {
                        newPos.Add(new Tuple<int, int>(i.Item1 - 1, i.Item2));
                    }
                    break;
                case Key.Right:
                    foreach (Tuple<int, int> i in currentFigureCoordinates)
                    {
                        newPos.Add(new Tuple<int, int>(i.Item1 + 1, i.Item2));
                    }
                    break;
                case Key.Space:
                    int k = 1;
                    do
                    {
                        newPos = new List<Tuple<int, int>>();
                        foreach (Tuple<int, int> i in currentFigureCoordinates)
                        {
                            newPos.Add(new Tuple<int, int>(i.Item1, i.Item2 + k));
                        }
                        k += 1;
                    }
                    while (IsMovementPossible(newPos) == MovementOutcomes.Possible);
                    break;
                default:
                    break;
            }

            ProcessMovement(newPos);
        }

        private void EndOfTheGame()
        {
            GameStarted = false;
            IsGameOver = true;
            if (_currentGamer is null || _currentGamer.Equals(string.Empty))
            {
                GamerNameDialog inputDialog = new GamerNameDialog("Enter the name of the current gamer", "Unknown");
                if (inputDialog.ShowDialog() == true)
                    if (!(inputDialog.Answer.Trim()).Equals(string.Empty))
                        _currentGamer = inputDialog.Answer;
                    else
                        _currentGamer = "Unknown";
                else
                    _currentGamer = "Unknown";
            }

            if (highestScores.ContainsKey(_currentGamer.Trim()))
            {
                if (Score > highestScores[_currentGamer.Trim()])
                {
                    highestScores[_currentGamer.Trim()] = Score;
                }
            }
            else
            {
                highestScores.Add(_currentGamer.Trim(), Score);
            }

            if(Score > HighestScore)
            {
                HighestScore = Score;
                TopGamer = _currentGamer;
            }
        }

        /// <summary>
        /// Defines if the current row is full of the frozen cells and removes it
        /// decrementing rows to finish level value
        /// subsequent upper layers moved down and checked for the same condition
        /// </summary>
        private void HideFullRows()
        {
            bool isFull;
            for (int i = _gridHeight - 1; i >= highestCell; i--)
            {
                isFull = true;
                //Check if all the cells in the current row are frozen
                for (int j = _gridWidth - 1; j >= 0; j--)
                {
                    if (!mainGrid[j, i].IsFrozen)
                    {
                        isFull = false;
                        break;
                    }
                }

                //If the current row is full
                if (isFull)
                {
                    //remove the current row from the grid
                    for (int j = _gridWidth - 1; j >= 0; j--)
                    {
                        mainGrid[j, i].rect.Fill = new SolidColorBrush(Colors.Transparent);
                        mainGrid[j, i].rect.Visibility = Visibility.Hidden;
                        mainGrid[j, i].IsFrozen = false;
                    }


                    for (int k = i; k >= highestCell; k--)
                    {
                        for (int m = 0; m <= _gridWidth - 1; m++)
                        {
                            if (mainGrid[m, k - 1].IsFrozen)
                            {
                                mainGrid[m, k].rect.Fill = mainGrid[m, k - 1].rect.Fill;
                                mainGrid[m, k].rect.Visibility = Visibility.Visible;
                                mainGrid[m, k].IsFrozen = true;

                                mainGrid[m, k - 1].rect.Fill = new SolidColorBrush(Colors.Transparent);
                                mainGrid[m, k - 1].rect.Visibility = Visibility.Hidden;
                                mainGrid[m, k - 1].IsFrozen = false;
                            }
                        }
                    }

                    Score += priceOfTheRow;
                    RowsToFinish -= 1;
                    
                    //Need to continue on the same row
                    i += 1;
                    //Correct the highest cell indicator
                    highestCell += 1;
                }
            }
            if (RowsToFinish <= 0)
            {
                ClearCellGrid();
                Level++;
                RowsToFinish = _initialRowsToFinish + Level * 2;
                _timer.Stop();
                _timer.Interval = new TimeSpan(_timer.Interval.Ticks * (100 - _percTimeSpanDecrease) / 100);
                _timer.Start();
            } 
        }

        private void TimerTickerHandler(object sender, EventArgs e)
        {
            if (_event_separator)
            {
                ProcessMovement(GetNextFigurePosition());
            }
            else
            {
                Debug.WriteLine("Even time-event");
            }
            _event_separator = !_event_separator;
        }

        private List<Tuple<int, int>> GetNextFigurePosition()
        {
            List<Tuple<int, int>>  newPos = new List<Tuple<int, int>>();
            foreach (Tuple<int, int> i in currentFigureCoordinates)
            {
                newPos.Add(new Tuple<int, int>(i.Item1, i.Item2 + 1));
            }

            return newPos;
        }

        private void ProcessMovement(List<Tuple<int, int>> nextPos)
        {
            if (nextPos.Count > 0)
            {
                //Check the possible result of the movement
                switch (IsMovementPossible(nextPos))
                {
                    case MovementOutcomes.Impossible:
                        //do nothing
                        break;
                    case MovementOutcomes.Possible:
                        //redraw the figure
                        DrawFigure(currentFigure, nextPos);
                        break;
                    case MovementOutcomes.NeedsFreezing:
                        //redraw the figure and instantiate a new one

                        DrawFigure(currentFigure, nextPos);
                        FreezeCurrentFigure();
                        HideFullRows();
                        DoNextFigure();
                        break;
                    case MovementOutcomes.EndOfPlay:
                        //redraw the figure and fire end of the gave event
                        DrawFigure(currentFigure, nextPos);
                        FreezeCurrentFigure();
                        EndOfTheGame();
                        break;
                }
            }
        }

        private void StartButton_Click(object sender, MouseButtonEventArgs e)
        {
            GameStarted = false;
            IsGameOver = false;
            ClearCellGrid();
            DoNextFigure();

            //Start the game
            GameStarted = true;
            startButtonText = "Restart";
            Level = 0;
            Score = 0;
            highestCell = _gridHeight - 1;
            RowsToFinish = _initialRowsToFinish;

            _timer.Interval = new TimeSpan(_initialTimeSpan);
            _timer.Tick += TimerTickerHandler;
            _timer.Start();
            //Return focus to the main canvas so as to allow it catching keyboard events
            _ = Keyboard.Focus(cellGrid);
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            List<KeyValuePair<string, int>> l = highestScores.ToList();
            l.Sort((pair1, pair2) => - pair1.Value.CompareTo(pair2.Value));
            HighScoresDialog hs = new HighScoresDialog(l);
            _ = hs.ShowDialog();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            using (FileStream fs = new FileStream(highScoresFileName, FileMode.OpenOrCreate))
            {
                List<KeyValuePair<string, int>> l = highestScores.ToList();
                l.Sort((pair1, pair2) => - pair1.Value.CompareTo(pair2.Value));
                Dictionary<string, int> d = new Dictionary<string, int>(10);
                //serialize to binary
                foreach (KeyValuePair<string, int> k in l.Take(10))
                    d.Add(k.Key, k.Value);
                
                byte[] data = ObjectSerialize.Serialize(d);
                fs.Write(data, 0, data.Length);
            }
        }
    }

}
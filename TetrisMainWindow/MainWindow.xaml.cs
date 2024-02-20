using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            IsGameStarted = false;
            IsGamePaused = false;
            IsGameOver = false;
            cellSize = 35;

            if (File.Exists(highScoresFileName))
            {
                byte[] data = File.ReadAllBytes(highScoresFileName);
                try
                {
                    object obj = ObjectSerialize.DeSerialize(data);
                    if (obj is List<Tuple<string, int, int, DateTime, string>> intermediateScores)
                    {
                        highestScores = intermediateScores;
                    }
                    else
                    {
                        List<Tuple<string, int, int, DateTime>> oldScores = obj as List<Tuple<string, int, int, DateTime>>;
                        highestScores = new List<Tuple<string, int, int, DateTime, string>>();
                        oldScores.ForEach(x => highestScores.Add(new Tuple<string, int, int, DateTime, string>(x.Item1, x.Item2, x.Item3, x.Item4, "20✕40")));
                    }
                    SetHighScores();
                }
                catch
                {
                    highestScores = new List<Tuple<string, int, int, DateTime, string>>();
                    HighestScore = 0;
                    TopGamer = "";
                }
            }
            else highestScores = new List<Tuple<string, int, int, DateTime, string>>();
#if DEBUG
            SpeedInfo.Visibility = Visibility.Visible;
#endif

            VersionNumber = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            _timer = new DispatcherTimer();

            _event_interlacer = 0;

            Type t = typeof(TetrisUserControl);

            figureTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(q => t.IsAssignableFrom(q) && !q.FullName.Contains("Interfaces"))
                .Select(x => x.FullName + ", TetrisFigures")
                .ToArray();
        }

        private void SetHighScores()
        {
            int i = 0;
            try
            {
                i = highestScores.Where(y => y.Item5.Equals(GameFieldSize)).Max(x => x.Item2);
                TopGamer = highestScores.Where(y => y.Item5.Equals(GameFieldSize)).First(x => x.Item2 == i).Item1;
            }
            catch
            {
                TopGamer = "";
            }
            HighestScore = i;
        }

        private double _cellSizeForCanvas;
        private string _currentGamer;
        private string _topGamer;
        private readonly string highScoresFileName = "highscores.scr";
        //name, score, level, datetime of the record
        private List<Tuple<string, int, int, DateTime, string>> highestScores;
        //the highest score to be shown in the StatusBar
        private int _highScore;
        //the size of cell
        private byte _gridWidth = 20;
        private byte _gridHeight = 40;
        private TetrisUserControl currentFigure;
        private TetrisUserControl nextFigure;
        private TetrisUserControl beforeNextFigure;
        private bool _gameStarted;
        private bool _gamePaused;
        private readonly int cellSize;
        //keeps track of the score
        private int _score;
        //holds the actual value of the current level
        private int _level;
        //Text on the Start button
        private string _startButtonText = "Start";
        private string _pauseButtonText = "Pause";
        private string _overOrPauseText = "GAME OVER";
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
        private readonly int priceOfTheRow = 100;
        //initial timespan in Timer ticks
        private long _initialTimeSpan =
#if DEBUG
            2000000;
#else
            1500000;
#endif
        //percents of the timespan to decrease the intial (previous) one with
        private int _percTimeSpanDecrease =
#if DEBUG
            10;
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
        //indicator-switcher to let differentiate freezing timer events from moving ones
        private long _event_interlacer;
        //the ration of moving event to freezing ones
        private readonly int _interlace_factor = 3;
        //indicator that game is will be finished if no rows are removed
        private bool _end_of_the_game_indicator;
        //holds the height of the "drop", i.e. the bigger the difference between the current position and the new one is
        //the bigger multiplier is applied when the figure freezes
        private int _height_of_drop = 0;
        //keeps full rows
        private List<int> _full_rows_list;
        //a flag for timer initiated movement event that may follow manual drops
        private static bool _dropped;
        //contains additonal information about the changes in the score
        private string _add_scoring_info;
        private readonly object balanceLock = new object();
        //holds the typenames of all the figures
        private readonly string[] figureTypes;
        #region Properties
        public long Speed => _timer.Interval.Ticks;
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
        public string pauseButtonText
        {
            get { return _pauseButtonText; }
            private set
            {
                _pauseButtonText = value;
                NotifyPropertyChanged("pauseButtonText");
            }
        }
        public string overOrPauseText
        {
            get { return _overOrPauseText; }
            private set
            {
                _overOrPauseText = value;
                NotifyPropertyChanged("overOrPauseText");
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
        public bool IsGameStarted
        {
            get { return _gameStarted; }
            private set 
            { 
                _gameStarted = value;
                NotifyPropertyChanged("IsGameStarted");
            }
        }
        public bool IsGamePaused
        {
            get { return _gamePaused; }
            private set
            {
                _gamePaused = value;
                NotifyPropertyChanged("IsGamePaused");
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
        public string AdditionalScoringInfo
        {
            get { return _add_scoring_info; }
            private set
            {
                _add_scoring_info = value;
                NotifyPropertyChanged("AdditionalScoringInfo");
            }
        }
        public string GameFieldSize 
        {
            get 
            { 
                return (string.Format("{0}✕{1}", _gridWidth, _gridHeight));
            } 
        }
        public byte GridWidth
        {
            get {return _gridWidth; }
            private set
            {
                _gridWidth = value;
                NotifyPropertyChanged("GridWidth");
                NotifyPropertyChanged("GameFieldSize");
            }
        }
        public byte GridHeight
        {
            get { return _gridHeight; }
            private set
            {
                _gridHeight = value;
                NotifyPropertyChanged("GridHeight");
                NotifyPropertyChanged("GameFieldSize");
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

            if (IsMovementPossible(currentFigureCoordinates) == MovementOutcomes.EndOfPlay)
            {
                SetNeedsFreezing(true);
                _end_of_the_game_indicator = true;
            }
        }

        /// <summary>
        /// Calculates possibility of the current figure to come to the new position <paramref name="newPosition"/>
        /// </summary>
        /// <param name="newPosition">The position that the current figure should take next</param>
        /// <returns></returns>
        private MovementOutcomes IsMovementPossible(List<Tuple<int, int>> newPosition)
        {
            //check whether the new position comes outside the playground borders
            //left, right sides and bottom
            //or overlaps the pile
            if (newPosition.Where(x => x.Item2 >= 0).Any(y => y.Item1 < 0 || y.Item1 > (_gridWidth - 1) || y.Item2 > (_gridHeight - 1) || mainGrid[y.Item1, y.Item2].IsFrozen))
            {
                return MovementOutcomes.Impossible;
            }

            foreach (Tuple<int, int> t in newPosition)
            {
            //check whether the new postion would touch the upper layer of the pile 
            //or the bottom
            //and return either End of the Play if any of the cells is on the first line
            //or NeedsFreezing otherwise
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

            _cellSizeForCanvas = mainGrid[1, 1].rect.ActualWidth;
            foreach (int i in currentFigureCoordinates.Select(x => x.Item2).Where(y => y > 0).Distinct().OrderBy(y => y))
            {
                int l = 0;
                for (int k = 0; k < _gridWidth; k++)
                {
                    if (mainGrid[k, i].IsFrozen) l++;
                }
                if (l == _gridWidth) _full_rows_list.Add(i);
            }

            int j = currentFigureCoordinates.Count * (1 + Math.Max(_height_of_drop - 1, 0));
            Score += j;
            ShowPopup((currentFigureCoordinates.Max(x => x.Item1) + currentFigureCoordinates.Min(x => x.Item1)) * _cellSizeForCanvas / 2, (currentFigureCoordinates.Min(x => x.Item2) - 1) * _cellSizeForCanvas, $"{1 + Math.Max(_height_of_drop - 1, 0)}*{currentFigureCoordinates.Count}={j}");
            highestCell = Math.Min(highestCell, currentFigureCoordinates.Min(x => x.Item2));
        }

        /// <summary>
        /// Shows scoring <paramref name="message"/> on the canvas on the top of the cell grid
        /// at the specified <paramref name="left"/> & <paramref name="top"/> position
        /// </summary>
        private void ShowPopup(double left, double top, string message)
        {
            ScoreInfoControl sc = new ScoreInfoControl
            {
                txtScoreInfo = message
            };
            int pos = cnvInformation.Children.Add(sc);
            Canvas.SetLeft(sc, left);
            Canvas.SetTop(sc, top);
            sc.visibility = Visibility.Visible;

            DispatcherTimer hoaring = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            sc.timer = hoaring;

            hoaring.Tick += delegate (object snd, EventArgs eargs)
            {
                Canvas.SetTop(sc, top - 10 * sc.movementCounter++);
            };

            DispatcherTimer tmr = new DispatcherTimer
            {
                //Set the timer interval to the length of the animation.
                Interval = new TimeSpan(0, 0, 4)
            };
            tmr.Tick += delegate (object snd, EventArgs ea)
            {
                // The animation will be over now, remove the popup
                cnvInformation.Children.Remove(sc);
                // Get rid of the timers.
                ((DispatcherTimer)snd).Stop();
                sc.timer.Stop();
                sc.timer = null;
                sc = null;
            };
            hoaring.Start();
            tmr.Start();
        }

        private void FireInfo()
        {
            DispatcherTimer tmr = new DispatcherTimer
            {
                //Set the timer interval to the length of the animation.
                Interval = new TimeSpan(0, 0, 1)
            };
            tmr.Tick += delegate (object snd, EventArgs ea)
            {
                // The animation will be over now, collapse the label.
                lblAdditionalScoringInfo.Visibility = Visibility.Collapsed;
                // Get rid of the timer.
                ((DispatcherTimer)snd).Stop();
            };
            tmr.Start();
        }

        ///<summary>
        ///Set the Needs to Freeze status (depending on the <paramref name="flag"/> value )
        ///for those cell in the elementary cell grid that correspond to the current figure coordinates.
        ///</summary>
        private void SetNeedsFreezing(bool flag)
        {
            foreach(Tuple<int,int> t in currentFigureCoordinates)
            {
                if (t.Item1 >= 0 && t.Item1 <= (_gridWidth - 1) && t.Item2 >= 0 && t.Item2 <= (_gridHeight - 1))
                {
                    mainGrid[t.Item1, t.Item2].NeedsFreeze = flag;
                }
            }
        }

        /// <summary>
        /// Clears the colors of the rectangles on the playground grid and hides them
        /// </summary>
        private void ClearCellGrid()
        {
            if (mainGrid != null)
            {
                for (int i = 0; i < mainGrid.GetLength(0); i++)
                {
                    for (int j = 0; j < mainGrid.GetLength(1); j++)
                    {
                        mainGrid[i, j].Reset();
                    }
                }
            }
        }

        /// <summary>
        /// Cahnges the figures from Next->Current & before the Next -> Next
        /// request the new figures if required
        /// </summary>
        private void GetNextFigure()
        {
            if (!IsGameStarted)
            {
                currentFigure = GetNewFigure();
                currentFigure.ChangeSize((int)(cellSize * 0.75));
                nextFigure = GetNewFigure();
                nextFigure.ChangeSize((int)(cellSize * 0.75));
            }
            else
            {
                nextFigureCell.Children.Remove(nextFigure);
                currentFigure = nextFigure;
                figureBeforeTheNextCell.Children.Remove(beforeNextFigure);
                nextFigure = beforeNextFigure;
            }

            figureBeforeTheNextCell.Children.Clear();
            beforeNextFigure = GetNewFigure();
            beforeNextFigure.ChangeSize((int)(cellSize * 0.75));

            Canvas.SetLeft(nextFigure, nextFigureCell.ActualWidth / 2 - nextFigure.Width / 2);
            Canvas.SetBottom(nextFigure, nextFigureCell.ActualHeight / 2 - nextFigure.Height / 2);
            nextFigureCell.Children.Add(nextFigure);
            nextFigureCell.ToolTip = nextFigure.GetType().Name.Replace("Control", String.Empty).Replace("Tetris", String.Empty);

            Canvas.SetLeft(beforeNextFigure, figureBeforeTheNextCell.ActualWidth / 2 - beforeNextFigure.Width / 2);
            Canvas.SetBottom(beforeNextFigure, figureBeforeTheNextCell.ActualHeight / 2 - beforeNextFigure.Height / 2);
            figureBeforeTheNextCell.Children.Add(beforeNextFigure);
            figureBeforeTheNextCell.ToolTip = beforeNextFigure.GetType().Name.Replace("Control", String.Empty).Replace("Tetris", String.Empty);
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

        ///<summary>
        ///Sets focus on the main canvas so as to enable it to catch keyboard pressing
        ///</summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(cellGrid);
        }

        private void CellGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (!IsGameStarted | IsGamePaused) return;

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
                    lock (balanceLock)
                    {
                        _dropped = true;
                    }
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

        /// <summary>
        /// Settles all necessary stuff before the game is finished and right after it is
        /// </summary>
        private void EndOfTheGame()
        {
            _timer.Stop();

            IsGameStarted = false;
            IsGameOver = true;
            if (_currentGamer is null || _currentGamer.Equals(string.Empty))
            {
                GamerNameDialog inputDialog = new GamerNameDialog("Enter the name of the current gamer", "Unknown");
                if (inputDialog.ShowDialog() == true)
                    if (!inputDialog.Answer.Trim().Equals(string.Empty))
                        _currentGamer = inputDialog.Answer;
                    else
                        _currentGamer = "Unknown";
                else
                    _currentGamer = "Unknown";
            }

            highestScores.Add(new Tuple<string, int, int, DateTime, string>(_currentGamer.Trim(), _score, _level, DateTime.Now, GameFieldSize));

            if(Score > HighestScore)
            {
                HighestScore = Score;
                TopGamer = _currentGamer;
            }

            pnMenuPanel.IsEnabled = true;
            startButtonText = "Start";
        }

        /// <summary>
        /// Defines if the current row is full of the frozen cells and removes it
        /// decrementing rows to finish level value
        /// subsequent upper layers moved down and checked for the same condition
        /// </summary>
        private void HideFullRows()
        {
            int k = _full_rows_list.Last();
            int l = k - 1; //starting from the row next to the full one
            
            for (; k >= highestCell; k--, l--)
            {
                while (_full_rows_list.Contains(l)) l--;
                HandleGridRows(k, l);
            }

            highestCell += _full_rows_list.Count;
            RowsToFinish -= _full_rows_list.Count;
            int j = (int)(priceOfTheRow * (1 + 0.1 * _level)) * Enumerable.Range(1, _full_rows_list.Count).Sum();
            Score += j;
            AdditionalScoringInfo = $"{_full_rows_list.Count} rows completed; {j} points gained.";
            _full_rows_list.Clear();
            lblAdditionalScoringInfo.Visibility = Visibility.Visible;
            FireInfo();

            if (RowsToFinish <= 0)
            {
                ClearCellGrid();
                Level++;
                RowsToFinish = _initialRowsToFinish + Level * 2;
                highestCell = _gridHeight - 1;
                _timer.Stop();
                _timer.Interval = new TimeSpan(_timer.Interval.Ticks * (100 - _percTimeSpanDecrease) / 100);
                NotifyPropertyChanged("Speed");
                _timer.Start();
            }
        }

        /// <summary>
        /// Cleans up the destination row <paramref name="dest_row"/> and if the source row <paramref name="source_row"/> argument is greater than the highest cell
        /// copies cell attributes correspondingly.
        /// No additional cleansing of the source row is done.
        /// </summary>
        private void HandleGridRows(int dest_row, int source_row)
        {
            //Reset the destination row
            for (int i = 0; i <= _gridWidth - 1; i++)
            {
                mainGrid[i, dest_row].Reset();
            }

            //Copy the source row into the destination one if the source is not NULL
            if(source_row > highestCell)
            {
                for (int i = 0; i <= _gridWidth - 1; i++)
                {
                    if (mainGrid[i, source_row].IsFrozen)
                    {
                        mainGrid[i, dest_row].rect.Fill = mainGrid[i, source_row].rect.Fill;
                        mainGrid[i, dest_row].rect.Visibility = Visibility.Visible;
                        mainGrid[i, dest_row].IsFrozen = true;
                    }
                }
            }
        }

        /// <summary>
        /// Timer event handler that defines whether to move the figure down
        /// or to process cells that need to be frozen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerTickerHandler(object sender, EventArgs e)
        {
            if (_event_interlacer++ % _interlace_factor == 0)
            {
                lock (balanceLock)
                {
                    if (_dropped)
                    {
                        return;
                    }
                }
                ProcessMovement(GetNextFigurePosition());
            }
            else
            {
                DoFreezing();
                lock (balanceLock)
                {
                    _dropped = false;
                }
            }
        }

        private void DoFreezing()
        {
            if (currentFigureCoordinates.Any(x => x.Item2 >= 0 && mainGrid[x.Item1, x.Item2].NeedsFreeze))
            {
                FreezeCurrentFigure();
                if (_full_rows_list.Count > 0) HideFullRows();
                if (_end_of_the_game_indicator)
                {
                    EndOfTheGame();
                    return;
                }
                DoNextFigure();
            }
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
                        //redraw the figure and set OFF necessity to be frozen
                        _height_of_drop = 0;
                        DrawFigure(currentFigure, nextPos);
                        SetNeedsFreezing(false);
                        break;
                    case MovementOutcomes.NeedsFreezing:
                        //redraw the figure and set ON necessity to be frozen
                        _height_of_drop = nextPos.First().Item2 - currentFigureCoordinates.First().Item2;
                        DrawFigure(currentFigure, nextPos);
                        SetNeedsFreezing(true);
                        break;
                    case MovementOutcomes.EndOfPlay:
                        //redraw the figure, set needs freezing ON and set indicator EoG
                        _height_of_drop = 0;
                        DrawFigure(currentFigure, nextPos);
                        SetNeedsFreezing(true);
                        _end_of_the_game_indicator = true;
                        lock (balanceLock)
                        {
                            _dropped = true;
                        }
                        break;
                }
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {

            if (IsGameStarted)
            {
                _timer.Tick -= TimerTickerHandler;
                _timer.Stop();
            }
            if (IsGamePaused)
            {
                pauseButtonText = "Pause";
                overOrPauseText = "GAME OVER";
                IsGamePaused = false;
            }

            InitGrid();
            IsGameStarted = false;
            IsGameOver = false;
            ClearCellGrid();
            nextFigureCell.Children.Clear();
            figureBeforeTheNextCell.Children.Clear();
            DoNextFigure();
            //Start the game
            IsGameStarted = true;
            startButtonText = "Restart";
            Level = 0;
            Score = 0;
            highestCell = _gridHeight - 1;
            RowsToFinish = _initialRowsToFinish;
            _end_of_the_game_indicator = false;
            _event_interlacer = 0;
            _timer.Interval = new TimeSpan(_initialTimeSpan);
            _timer.Tick += TimerTickerHandler;
            NotifyPropertyChanged("Speed");
            lock (balanceLock)
            {
                _dropped = false;
            }
            _full_rows_list.Clear();
            _timer.Start();
            pnMenuPanel.IsEnabled = false;

            //Return focus to the main canvas so as to allow it catching keyboard events
            _ = Keyboard.Focus(cellGrid);
        }

        private void ShowHighScores(object sender, MouseButtonEventArgs e)
        {
            highestScores.Sort((p1, p2) => -p1.Item2.CompareTo(p2.Item2));
            HighScoresDialog hs = new HighScoresDialog(highestScores.Where(x => x.Item5.Equals(GameFieldSize)).Take(10));
            _ = hs.ShowDialog();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            using (FileStream fs = new FileStream(highScoresFileName, FileMode.OpenOrCreate))
            {
                List<Tuple<string, int, int, DateTime, string>> restScores = highestScores.Where(x => !x.Item5.Equals(GameFieldSize)).ToList();
                restScores.AddRange(highestScores.Where(x => x.Item5.Equals(GameFieldSize)).Take(10));

                highestScores.Sort((p1, p2) => -p1.Item2.CompareTo(p2.Item2));
                byte[] data = ObjectSerialize.Serialize(restScores);
                fs.Write(data, 0, data.Length);
            }
        }

        private void MenuItemInfo_Click(object sender, RoutedEventArgs e)
        {
            InfoDialog dlg = new InfoDialog();
            _ = dlg.ShowDialog();
        }

        private void MenuItemScores_Click(object sender, RoutedEventArgs e)
        {
            ShowHighScores(sender, null);
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsGamePaused)
            {
                pauseButtonText = "Resume";
                overOrPauseText = "Paused";
                _timer.Stop();
            }
            else
            {
                pauseButtonText = "Pause";
                overOrPauseText = "GAME OVER";
                _timer.Start();
            }
            IsGamePaused = !IsGamePaused;
        }

        private void GameFieldSize_Click(object sender, RoutedEventArgs e)
        {
            GameFieldSizeDialog dlg = new GameFieldSizeDialog(GridWidth, GridHeight);
            bool? res = dlg.ShowDialog();
            if (res != null && res.Value)
            {
                ClearCellGrid();
                GridWidth = dlg.sz.width;
                GridHeight = dlg.sz.height;
                SetHighScores();
            }
        }

        private void InitGrid()
        {
             if (cellGrid.RowDefinitions.Count != _gridHeight)
            {

                mainGrid = new ElementaryCell[_gridWidth, _gridHeight];

                if (cellGrid.RowDefinitions.Count != 0)
                {
                    cellGrid.RowDefinitions.RemoveRange(0, cellGrid.RowDefinitions.Count);
                    cellGrid.ColumnDefinitions.RemoveRange(0, cellGrid.ColumnDefinitions.Count);
                }

                for (int i = 1; i <= _gridWidth; i++)
                {
                    cellGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                }
                for (int j = 1; j <= _gridHeight; j++)
                {
                    cellGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
                }

                _full_rows_list = new List<int>();

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
            }
        }
    }
}
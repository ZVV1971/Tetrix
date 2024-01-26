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
            GameStarted = false;
            IsGameOver = false;
            cellSize = 40;

            mainGrid = new ElementaryCell[_gridWidth, _gridHeight];

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

            if (File.Exists(highScoresFileName))
            {
                byte[] data = File.ReadAllBytes(highScoresFileName);
                try
                {
                    highestScores = (List<Tuple<string, int, int, DateTime>>)ObjectSerialize.DeSerialize(data);
                    int i = highestScores.Max(x => x.Item2);
                    TopGamer = highestScores.First(x => x.Item2 == i).Item1;
                    HighestScore = i;
                }
                catch
                {
                    highestScores = new List<Tuple<string, int, int, DateTime>>();
                    HighestScore = 0;
                    TopGamer = "";
                }
            }
            else highestScores = new List<Tuple<string, int, int, DateTime>>();
#if DEBUG
            SpeedInfo.Visibility = Visibility.Visible;
#endif

            VersionNumber = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            _timer = new DispatcherTimer();

            _event_interlacer = 0;
        }

        private string _currentGamer;
        private string _topGamer;
        private readonly string highScoresFileName = "highscores.scr";
        //name, score, level, datetime of the record
        private List<Tuple<string, int, int, DateTime>> highestScores;
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
        //indicator-switcher to let freezing timer events from moving ones
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
        public string AdditionalScoringInfo 
        {
            get { return _add_scoring_info; }
            private set
            {
                _add_scoring_info = value;
                NotifyPropertyChanged("AdditionalScoringInfo");
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

            foreach (int i in currentFigureCoordinates.Select(x => x.Item2).Distinct().OrderBy(y => y))
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
            AdditionalScoringInfo = $"{currentFigure.GetType().Name.Replace("Tetris","").Replace("Control","")} has been dropped from {_height_of_drop} cells and gave you {j} points.";
            lblAdditionalScoringInfo.Visibility = Visibility.Visible;
            FireInfo();
            highestCell = Math.Min(highestCell, currentFigureCoordinates.Min(x => x.Item2));
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
            for (int i = 0; i < mainGrid.GetLength(0); i++)
            {
                for (int j = 0; j < mainGrid.GetLength(1); j++)
                {
                    mainGrid[i, j].Reset();
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

        ///<summary>
        ///Sets focus on the main canvas so as to enable it to catch keyboard pressing
        ///</summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
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

            GameStarted = false;
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

            highestScores.Add(new Tuple<string, int, int, DateTime>(_currentGamer.Trim(), _score, _level, DateTime.Now));

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

            //Return focus to the main canvas so as to allow it catching keyboard events
            _ = Keyboard.Focus(cellGrid);
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            highestScores.Sort((p1, p2) => -p1.Item2.CompareTo(p2.Item2));
            HighScoresDialog hs = new HighScoresDialog(highestScores.Take(10));
            _ = hs.ShowDialog();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            using (FileStream fs = new FileStream(highScoresFileName, FileMode.OpenOrCreate))
            {

                highestScores.Sort((p1, p2) => -p1.Item2.CompareTo(p2.Item2));
                byte[] data = ObjectSerialize.Serialize(highestScores.Take(10).ToList());
                fs.Write(data, 0, data.Length);
            }
        }
    }
}
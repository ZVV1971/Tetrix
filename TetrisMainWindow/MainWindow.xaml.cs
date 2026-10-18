using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using TetrisFigures.Auxiliary;
using TetrisFigures.Dialogs;
using TetrisFigures.Interfaces;

namespace TetrisMainWindow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// Reusable random instance to avoid creating new instances repeatedly
        /// </summary>
        private static readonly Random _random = new Random();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            InitializeGameState();
            LoadHighScores();

#if DEBUG
            SpeedInfo.Visibility = Visibility.Visible;
#endif

            VersionNumber = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            _timer = new DispatcherTimer();
            _eventInterlacer = 0;
            CurrentGamer = GetCurrentGamerName();
        }

        private void InitializeGameState()
        {
            IsGameStarted = false;
            IsGamePaused = false;
            IsGameOver = false;
            cellSize = 35;
            _gameComplexity = GameComplexity.Average;
        }

        private void LoadHighScores()
        {
            if (!File.Exists(highScoresFileName))
            {
                highestScores = new List<ScoreRecord>();
                return;
            }

            try
            {
                byte[] data = File.ReadAllBytes(highScoresFileName);
                using (MemoryStream outputStream = new MemoryStream())
                {
                    using (MemoryStream inputStream = new MemoryStream(data))
                    {
                        using (GZipStream zip = new GZipStream(inputStream, CompressionMode.Decompress))
                        {
                            zip.CopyTo(outputStream);
                        }
                    }

                    string serializedScores = Encoding.UTF8.GetString(outputStream.ToArray());
                    List<ScoreRecord> intermediateScores = JsonSerializer.Deserialize<List<ScoreRecord>>(serializedScores);
                    highestScores = intermediateScores ?? new List<ScoreRecord>();
                }

                SetHighScores();
            }
            catch
            {
                ResetHighScores();
            }
        }

        private void ResetHighScores()
        {
            highestScores = new List<ScoreRecord>();
            HighestScore = 0;
            TopGamer = string.Empty;
        }

        private static string GetCurrentGamerName()
        {
            var identity = WindowsIdentity.GetCurrent().Name;
            // Handle both "DOMAIN\username" and standalone "username" formats
            var parts = identity.Split('\\');
            return parts.Length > 1 ? parts[parts.Length - 1] : identity;
        }

        private double _cellSizeForCanvas;
        private string _currentGamer;
        private string _topGamer;
        private readonly string highScoresFileName = "highscores.scr";
        // name, score, level, datetime of the record, game field size, complexity level
        private List<ScoreRecord> highestScores;
        // the highest score to be shown in the StatusBar
        private int _highScore;
        // the size of cell
        private byte _gridWidth = 20;
        private byte _gridHeight = 40;
        private TetrisUserControl currentFigure;
        private TetrisUserControl nextFigure;
        private TetrisUserControl beforeNextFigure;
        private bool _gameStarted;
        private bool _gamePaused;
        private int cellSize;
        // keeps track of the score
        private int _score;
        // holds the actual value of the current level
        private int _level;
        // Text on the Start button
        private string _startButtonText = "Start";
        private string _pauseButtonText = "Pause";
        private string _overOrPauseText = "GAME OVER";
        private bool _isGameOver = false;
        // Keeps track of rows to be finished before the next level is started
        private int _rowsToFinish;
        // An array of Elementary cells -- actually the game zone
        // made to be created and filled in App rather than in XAML
        private ElementaryCell[,] mainGrid;
        // keeps track of the current figure coordinates
        private List<Tuple<int, int>> currentFigureCoordinates;
        // an inidicator to keep the highest (lowest Y) frozen cell coordinate on the pile
        // serves to limit the nested cycle
        private int highestCell;
        // number of points added when a row is full
        private readonly int priceOfTheRow = 100;
        // initial timespan in Timer ticks
        private readonly long _initialTimeSpan =
#if DEBUG
            2000000;
#else
            1500000;
#endif
        // percents of the timespan to decrease the intial (previous) one with
        private readonly int _percTimeSpanDecrease =
#if DEBUG
            10;
#else
            15;
#endif
        // rows to be filled for the first level
        private readonly int _initialRowsToFinish =
#if DEBUG
            1;
#else
            10;
#endif
        // _timer for down events
        private readonly DispatcherTimer _timer;
        // holds the current version of the app
        private string _version;
        // indicator-switcher to let differentiate freezing timer events from moving ones
        private long _eventInterlacer;
        // the ration of moving event to freezing ones
        private readonly int _interlaceFactor = 3;
        // indicator that game is will be finished if no rows are removed
        private bool _endOfTheGameIndicator;
        // holds the height of the "drop", i.e. the bigger the difference between the current position and the new one is
        // the bigger multiplier is applied when the figure freezes
        private int _heightOfDrop = 0;
        // keeps full rows
        private List<int> _fullRowsList;
        // a flag for timer initiated movement event that may follow manual drops
        private bool _dropped;
        // contains additonal information about the changes in the score
        private string _addScoringInfo;
        private readonly object balanceLock = new object();
        // holds the typenames of all the figures
        private string[] figureTypes;
        private GameComplexity _gameComplexity;

        #region Properties
        public GameComplexity GameComplexityLevel
        {
            get { return _gameComplexity; }
            set
            {
                _gameComplexity = value;
                NotifyPropertyChanged("GameComplexityLevel");
            }
        }

        public string CurrentGamer
        {
            get { return _currentGamer; }
            private set
            {
                _currentGamer = value;
                NotifyPropertyChanged("CurrentGamer");
            }
        }

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
            get { return _addScoringInfo; }
            private set
            {
                _addScoringInfo = value;
                NotifyPropertyChanged("AdditionalScoringInfo");
            }
        }

        public string GameFieldSize
        {
            get
            {
                return string.Format("{0}✕{1}", _gridWidth, _gridHeight);
            }
        }

        public byte GridWidth
        {
            get { return _gridWidth; }
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
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
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
                _endOfTheGameIndicator = true;
            }
        }

        /// <summary>
        /// Calculates possibility of the current figure to come to the new position <paramref name="newPosition"/>
        /// </summary>
        /// <param name="newPosition">The position that the current figure should take next</param>
        /// <returns></returns>
        private MovementOutcomes IsMovementPossible(IList<Tuple<int, int>> newPosition)
        {
            // check whether the new position comes outside the playground borders
            // left, right sides and bottom or overlaps the pile
            if (newPosition.Where(x => x.Item2 >= 0).Any(y => y.Item1 < 0 || y.Item1 > (_gridWidth - 1) || y.Item2 > (_gridHeight - 1) || mainGrid[y.Item1, y.Item2].IsFrozen))
            {
                return MovementOutcomes.Impossible;
            }

            foreach (Tuple<int, int> t in newPosition)
            {
                // check whether the new postion would touch the upper layer of the pile
                // or the bottom and return either End of the Play if any of the cells is
                // on the first line or NeedsFreezing otherwise
                if (t.Item2 >= -1 && (t.Item2 == (_gridHeight - 1) || mainGrid[t.Item1, t.Item2 + 1].IsFrozen))
                {
                    if (newPosition.Any((x) => x.Item2 <= 0))
                    {
                        return MovementOutcomes.EndOfPlay;
                    }

                    return MovementOutcomes.NeedsFreezing;
                }
            }

            return MovementOutcomes.Possible;
        }

        /// <summary>
        /// Clears the old figure and draws the current figure on the new position within the playground cells
        /// </summary>
        /// <param name="c">Is used to take the filling color for rectangles</param>
        /// <param name="newPos">Provides new positions of the figure</param>
        private void DrawFigure(TetrisUserControl c, IList<Tuple<int, int>> newPos, bool forced = false)
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

                currentFigureCoordinates = newPos.ToList();
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
                if (t.Item1 >= 0 && t.Item1 <= (_gridWidth - 1) && t.Item2 >= 0 && t.Item2 <= (_gridHeight - 1))
                {
                    mainGrid[t.Item1, t.Item2].IsFrozen = true;
                }
            }

            _cellSizeForCanvas = mainGrid[1, 1].rect.ActualWidth;
            foreach (int i in currentFigureCoordinates.Select(x => x.Item2).Where(y => y > 0).Distinct().OrderBy(y => y))
            {
                int l = 0;
                for (int k = 0; k < _gridWidth; k++)
                {
                    if (mainGrid[k, i].IsFrozen)
                    {
                        l++;
                    }
                }

                if (l == _gridWidth)
                {
                    _fullRowsList.Add(i);
                }
            }

            int j = currentFigureCoordinates.Count * (1 + Math.Max(_heightOfDrop - 1, 0));
            Score += j;
            ShowPopup(
                (currentFigureCoordinates.Max(x => x.Item1) + currentFigureCoordinates.Min(x => x.Item1)) * _cellSizeForCanvas / 2,
                (currentFigureCoordinates.Min(x => x.Item2) - 1) * _cellSizeForCanvas,
                $"+{j}");
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
                // Set the timer interval to the length of the animation.
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

        /// <summary>
        /// Starts animation on the additional info about full lines
        /// </summary>
        private void FireInfo()
        {
            DispatcherTimer tmr = new DispatcherTimer
            {
                // Set the timer interval to the length of the animation.
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

        /// <summary>
        /// Set the Needs to Freeze status (depending on the <paramref name="flag"/> value)
        /// for those cell in the elementary cell grid that correspond to the current figure coordinates.
        /// </summary>
        private void SetNeedsFreezing(bool flag)
        {
            foreach (Tuple<int, int> t in currentFigureCoordinates)
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
        /// Changes the figures from Next->Current & before the Next -> Next
        /// request the new figures if required
        /// </summary>
        private void GetNextFigure()
        {
            if (!IsGameStarted)
            {
                currentFigure = GetNewFigure();
                currentFigure.ChangeSize((int)(cellSize * 0.75));
                do
                {
                    nextFigure = GetNewFigure();
                }
                while (currentFigure.GetType().Equals(nextFigure.GetType()));
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
            do
            {
                beforeNextFigure = GetNewFigure();
            }
            while (beforeNextFigure.GetType().Equals(nextFigure.GetType()));
            beforeNextFigure.ChangeSize((int)(cellSize * 0.75));

            Canvas.SetLeft(nextFigure, nextFigureCell.ActualWidth / 2 - nextFigure.Width / 2);
            Canvas.SetBottom(nextFigure, nextFigureCell.ActualHeight / 2 - nextFigure.Height / 2);
            nextFigureCell.Children.Add(nextFigure);
            nextFigureCell.ToolTip = nextFigure.GetType().Name.Replace("Control", string.Empty).Replace("Tetris", string.Empty);

            Canvas.SetLeft(beforeNextFigure, figureBeforeTheNextCell.ActualWidth / 2 - beforeNextFigure.Width / 2);
            Canvas.SetBottom(beforeNextFigure, figureBeforeTheNextCell.ActualHeight / 2 - beforeNextFigure.Height / 2);
            figureBeforeTheNextCell.Children.Add(beforeNextFigure);
            figureBeforeTheNextCell.ToolTip = beforeNextFigure.GetType().Name.Replace("Control", string.Empty).Replace("Tetris", string.Empty);
        }

        /// <summary>
        /// Initiates a new list of tuplets (X,Y) with relative coordinates of the new figure
        /// </summary>
        /// <param name="t"></param>
        private void InsertNewFigure(TetrisUserControl t)
        {
            currentFigureCoordinates = new List<Tuple<int, int>>();
            foreach (Tuple<int, int> tp in t.InitialPosition)
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
            // Select random figure type using shared Random instance
            int figureIndex = _random.Next(0, figureTypes.Length);

            TetrisUserControl fig;
            Type objectType = Type.GetType(figureTypes[figureIndex]);
            fig = (TetrisUserControl)Activator.CreateInstance(objectType);

            // Select random color
            int rInt = _random.Next(0, Enum.GetNames(typeof(TetrisColors)).Length);
            string col = Enum.GetName(typeof(TetrisColors), rInt);
            fig.color = new SolidColorBrush((Color)typeof(Colors).GetProperty(col).GetValue(null, null));

            return fig;
        }

        /// <summary>
        /// Sets focus on the main canvas so as to enable it to catch keyboard pressing
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(cellGrid);
        }

        /// <summary>
        /// Handles game keys through WPF's routed keyboard input, independent of which child has focus.
        /// Guards are ordered by performance: fast checks first, expensive tree traversals last.
        /// </summary>
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Guard 1: Only accept arrow keys and Space for game input
            if (e.Key != Key.Up && e.Key != Key.Down && e.Key != Key.Left && e.Key != Key.Right && e.Key != Key.Space)
            {
                return;
            }

            // Guard 2: Game must be started and not paused
            if (!IsGameStarted || IsGamePaused)
            {
                return;
            }

            // Guard 3: Ignore keystrokes with modifier keys (Ctrl, Alt, Shift, Win)
            // This allows system shortcuts (Ctrl+C, Ctrl+X, Ctrl+V, Ctrl+Left/Right for word navigation, IME switching, etc.)
            // and application-level shortcuts to work properly without being consumed by game input.
            if (Keyboard.Modifiers != ModifierKeys.None)
            {
                return;
            }

            // Guard 4: Allow keyboard navigation if the user is in menu mode (Alt or F10 pressed to activate menu bar).
            // In menu mode, Keyboard.FocusedElement may still be on the game field, but WPF is processing
            // menu navigation keys, so we must not intercept them.
            if (InputManager.Current.IsInMenuMode)
            {
                return;
            }

            var focusedElement = Keyboard.FocusedElement as DependencyObject;
            var originalSource = e.OriginalSource as DependencyObject;

            // Fast path: if the game grid itself is focused, skip tree traversals entirely
            // This is the common case during active gameplay and avoids redundant visual/logical tree walks.
            if (ReferenceEquals(focusedElement, cellGrid))
            {
                ProcessGameKey(e.Key);
                e.Handled = true;
                return;
            }

            // Consolidated check: returns true if the focused element or original source (if different)
            // should consume the keyboard input. This single walk replaces four separate traversals.
            if (ShouldControlConsumeKeyInput(focusedElement, originalSource, e.Key))
            {
                return;
            }

            // All guards passed: consume the game key
            ProcessGameKey(e.Key);
            e.Handled = true;
        }

        /// <summary>
        /// Consolidated check that returns true if either the focused element or original source
        /// (if different) should consume the keyboard input (menu navigation or interactive control).
        /// This replaces four separate tree traversals with a single unified walk up the visual/logical tree.
        /// </summary>
        /// <param name="focusedElement">The element that currently has keyboard focus</param>
        /// <param name="originalSource">The original source of the keyboard event</param>
        /// <param name="key">The key being pressed</param>
        /// <returns>True if the key should be passed to a control rather than consumed for game input</returns>
        private static bool ShouldControlConsumeKeyInput(DependencyObject focusedElement, DependencyObject originalSource, Key key)
        {
            // Check if focusedElement should consume the key
            if (focusedElement != null && ControlShouldConsumeKey(focusedElement, key))
            {
                return true;
            }

            // Only check originalSource if it's different from focusedElement to avoid redundant tree walks
            if (originalSource != null && !ReferenceEquals(focusedElement, originalSource) && ControlShouldConsumeKey(originalSource, key))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Traverses the visual and logical tree to check if an element or any ancestor
        /// is a menu control or interactive control that should consume keyboard input.
        /// Consolidates the tree traversal logic from IsElementInMenu and IsInteractiveControl
        /// to avoid redundant walks up the visual/logical tree on every keystroke.
        /// </summary>
        private static bool ControlShouldConsumeKey(DependencyObject element, Key key)
        {
            while (element != null)
            {
                // Check if the current element is a menu control
                if (element is MenuBase || element is MenuItem)
                {
                    return true;
                }

                // FrameworkContentElement (Run, Span, Hyperlink inside text or menu headers) - handle specially
                // Unwrap non-visual content elements to access their parent.
                if (element is FrameworkContentElement fce)
                {
                    element = fce.Parent;
                    continue;
                }

                // TextBoxBase and derived controls (TextBox, RichTextBox, etc)
                // Handle all text input controls
                if (element is System.Windows.Controls.Primitives.TextBoxBase)
                {
                    return true;
                }

                // PasswordBox does not inherit from TextBoxBase, so check it explicitly
                if (element is PasswordBox)
                {
                    return true;
                }

                // ComboBox text input field (for editable comboboxes)
                if (element is ComboBox)
                {
                    return true;
                }

                // Button activation via Space, and other ButtonBase-derived controls
                // IMPORTANT: When a button has focus via mouse click during active gameplay, Space will activate it.
                // This is intentional for UI accessibility. Only buttons that don't need keyboard interaction
                // should have Focusable="False" in XAML (see StartButton, PauseButton, etc.)
                if (element is ButtonBase && key == Key.Space)
                {
                    return true;
                }

                // RadioButton requires arrow keys (Up, Down, Left, Right) to navigate between options in a group.
                // RadioButton derives from ButtonBase so we check it separately for arrow key navigation.
                if (element is RadioButton && (key == Key.Up || key == Key.Down || key == Key.Left || key == Key.Right))
                {
                    return true;
                }

                // TabControl/TabItem - arrow keys navigate between tabs
                if (element is TabControl && (key == Key.Up || key == Key.Down || key == Key.Left || key == Key.Right))
                {
                    return true;
                }

                // Slider - arrow keys navigate the slider value
                if (element is Slider && (key == Key.Left || key == Key.Right || key == Key.Up || key == Key.Down))
                {
                    return true;
                }

                // ListBox - arrow keys navigate items
                if (element is ListBox && (key == Key.Up || key == Key.Down || key == Key.Left || key == Key.Right))
                {
                    return true;
                }

                // DataGrid - arrow keys navigate cells
                if (element is System.Windows.Controls.DataGrid && (key == Key.Up || key == Key.Down || key == Key.Left || key == Key.Right))
                {
                    return true;
                }

                // TreeView - arrow keys navigate nodes
                if (element is TreeView && (key == Key.Up || key == Key.Down || key == Key.Left || key == Key.Right))
                {
                    return true;
                }

                // Traverse the visual tree (handles template children correctly)
                // with fallback to logical tree when visual parent is null or for non-visual elements.
                // This ensures we don't get stuck at template boundaries where visual parent is null.
                DependencyObject parent = null;
                if (element is Visual || element is System.Windows.Media.Media3D.Visual3D)
                {
                    parent = VisualTreeHelper.GetParent(element);
                }

                // Fallback to logical parent if visual parent is null or if element is non-visual
                element = parent ?? LogicalTreeHelper.GetParent(element);
            }

            return false;
        }

        private void ProcessGameKey(Key key)
        {
            List<Tuple<int, int>> newPos = new List<Tuple<int, int>>();
            switch (key)
            {
                case Key.Up:
                    newPos = currentFigure.Rotate(currentFigureCoordinates).ToList();
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
            _timer.Tick -= TimerTickerHandler;
            _timer.Stop();

            IsGameStarted = false;
            IsGameOver = true;

            highestScores.Add(new ScoreRecord()
            {
                playerName = CurrentGamer.Trim(),
                score = _score,
                level = _level,
                recordTimestamp = DateTime.Now,
                gameFieldSize = GameFieldSize,
                complexityLevel = GameComplexityLevel
            });

            if (Score > HighestScore)
            {
                HighestScore = Score;
                TopGamer = CurrentGamer;
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
            int k = _fullRowsList.Max();
            int l = k - 1; // starting from the row next to the full one

            for (; k >= highestCell; k--, l--)
            {
                while (_fullRowsList.Contains(l))
                {
                    l--;
                }

                HandleGridRows(k, l);
            }

            highestCell += _fullRowsList.Count;
            RowsToFinish -= _fullRowsList.Count;
            int j = (int)(priceOfTheRow * (1 + 0.1 * _level)) * Enumerable.Range(1, _fullRowsList.Count).Sum();
            Score += j;
            AdditionalScoringInfo = $"{_fullRowsList.Count} rows completed; {j} points gained.";
            _fullRowsList.Clear();
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
            // Reset the destination row
            for (int i = 0; i <= _gridWidth - 1; i++)
            {
                mainGrid[i, dest_row].Reset();
            }

            // Copy the source row into the destination
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

        /// <summary>
        /// Timer event handler that defines whether to move the figure down
        /// or to process cells that need to be frozen
        /// </summary>
        private void TimerTickerHandler(object sender, EventArgs e)
        {
            if (_eventInterlacer++ % _interlaceFactor == 0)
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

        /// <summary>
        /// Freezes those cells that're marked and calls End of the game if it's detected
        /// </summary>
        private void DoFreezing()
        {
            if (currentFigureCoordinates.Any(x => x.Item2 >= 0 && mainGrid[x.Item1, x.Item2].NeedsFreeze))
            {
                FreezeCurrentFigure();
                if (_fullRowsList.Count > 0)
                {
                    HideFullRows();
                }

                if (_endOfTheGameIndicator)
                {
                    EndOfTheGame();
                    return;
                }

                DoNextFigure();
            }
        }

        /// <summary>
        /// Returns the next postion for the current figure
        /// </summary>
        /// <returns></returns>
        private List<Tuple<int, int>> GetNextFigurePosition()
        {
            List<Tuple<int, int>> newPos = new List<Tuple<int, int>>();
            foreach (Tuple<int, int> i in currentFigureCoordinates)
            {
                newPos.Add(new Tuple<int, int>(i.Item1, i.Item2 + 1));
            }

            return newPos;
        }

        /// <summary>
        /// Defines and calls the actions depedning on the <paramref name="nextPos"/> next position of the current figure
        /// </summary>
        private void ProcessMovement(IList<Tuple<int, int>> nextPos)
        {
            if (nextPos.Count > 0)
            {
                // Check the possible result of the movement
                switch (IsMovementPossible(nextPos))
                {
                    case MovementOutcomes.Impossible:
                        // do nothing
                        break;
                    case MovementOutcomes.Possible:
                        // redraw the figure and set OFF necessity to be frozen
                        _heightOfDrop = 0;
                        DrawFigure(currentFigure, nextPos);
                        SetNeedsFreezing(false);
                        break;
                    case MovementOutcomes.NeedsFreezing:
                        // redraw the figure and set ON necessity to be frozen
                        _heightOfDrop = nextPos.First().Item2 - currentFigureCoordinates.First().Item2;
                        DrawFigure(currentFigure, nextPos);
                        SetNeedsFreezing(true);
                        break;
                    case MovementOutcomes.EndOfPlay:
                        // redraw the figure, set needs freezing ON and set indicator EoG
                        _heightOfDrop = 0;
                        DrawFigure(currentFigure, nextPos);
                        SetNeedsFreezing(true);
                        _endOfTheGameIndicator = true;
                        lock (balanceLock)
                        {
                            _dropped = true;
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Starts or re-starts the game, calling necessary changes and cleanups meanwhile
        /// </summary>
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

            Type t = typeof(TetrisUserControl);

            figureTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(q => t.IsAssignableFrom(q)
                    && !q.FullName.Contains("Interfaces")
                    && q.GetCustomAttribute<ComplexityAttribute>().Complexity <= GameComplexityLevel)
                .Select(x => x.FullName + ", TetrisFigures")
                .ToArray();

            InitGrid();
            IsGameStarted = false;
            IsGameOver = false;
            ClearCellGrid();
            nextFigureCell.Children.Clear();
            figureBeforeTheNextCell.Children.Clear();
            DoNextFigure();

            // Start the game
            IsGameStarted = true;
            startButtonText = "Restart";
            Level = 0;
            Score = 0;
            highestCell = _gridHeight - 1;
            RowsToFinish = _initialRowsToFinish;
            _endOfTheGameIndicator = false;
            _eventInterlacer = 0;
            _timer.Interval = new TimeSpan(_initialTimeSpan * (100 - (int)GameComplexityLevel * 10) / 100);
            _timer.Tick += TimerTickerHandler;
            NotifyPropertyChanged("Speed");
            lock (balanceLock)
            {
                _dropped = false;
            }

            _fullRowsList.Clear();
            _timer.Start();
            pnMenuPanel.IsEnabled = false;

            // Return focus to the main canvas so as to allow it catching keyboard events
            _ = Keyboard.Focus(cellGrid);
        }

        /// <summary>
        /// Shows the high score window for the current game field size
        /// </summary>
        private void ShowHighScores(object sender, MouseButtonEventArgs e)
        {
            highestScores.Sort((p1, p2) => -p1.score.CompareTo(p2.score));
            HighScoresDialog hs = new HighScoresDialog(highestScores.Where(x => x.gameFieldSize.Equals(GameFieldSize)).Take(10));
            _ = hs.ShowDialog();
        }

        /// <summary>
        /// Attempts to store the score results in the file
        /// </summary>
        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                using (FileStream fs = new FileStream(highScoresFileName, FileMode.OpenOrCreate))
                {
                    List<ScoreRecord> restScores = highestScores.Where(x => !x.gameFieldSize.Equals(GameFieldSize) && !x.complexityLevel.Equals(GameComplexityLevel)).ToList();
                    restScores.AddRange(highestScores.Where(x => x.gameFieldSize.Equals(GameFieldSize) && x.complexityLevel.Equals(GameComplexityLevel)).Take(10));

                    highestScores.Sort((p1, p2) => -p1.score.CompareTo(p2.score));

                    string s = JsonSerializer.Serialize(restScores);
                    byte[] b = Encoding.UTF8.GetBytes(s);

                    using (GZipStream zip = new GZipStream(fs, CompressionMode.Compress))
                    {
                        zip.Write(b, 0, b.Length);
                    }
                }
            }
            catch (Exception)
            {
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

        /// <summary>
        /// Pauses / Resumes the current game
        /// </summary>
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

            if (!IsGamePaused)
            {
                // Restore keyboard input to the game field after resuming.
                _ = Keyboard.Focus(cellGrid);
            }
        }

        /// <summary>
        /// Calls game field size dialog and does corresponding changes if it succeeds
        /// </summary>
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

        /// <summary>
        /// Initiates a new main grid if it 'hasn't been initiated or the game field size got changed
        /// </summary>
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

                _fullRowsList = new List<int>();

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
                        _ = cellGrid.Children.Add(rec);

                        mainGrid[i, j] = new ElementaryCell() { rect = rec, IsFrozen = false, NeedsFreeze = false };
                    }
                }
            }
        }

        /// <summary>
        /// Set high scores info for status bar
        /// </summary>
        private void SetHighScores()
        {
            int i = 0;
            try
            {
                i = highestScores.Where(y => y.gameFieldSize.Equals(GameFieldSize)).Max(x => x.score);
                TopGamer = highestScores.Where(y => y.gameFieldSize.Equals(GameFieldSize)).First(x => x.score == i).playerName;
            }
            catch
            {
                TopGamer = string.Empty;
            }

            HighestScore = i;
        }
    }
}

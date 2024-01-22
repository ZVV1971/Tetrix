using System;
using System.Collections.Generic;

namespace TetrisFigures.Auxiliary
{
    public class RowTracker
    {
        public RowTracker(int depth, int width)
        {
            _counters = new byte[depth];
            _width = width;
            _topmost = depth;
        }

        public void AddFigure(IEnumerable<Tuple<int, int>> coordinates)
        {
            foreach (Tuple<int, int> t in coordinates)
            {
                if (++_counters[t.Item2] == _width)
                {
                    RowFullEventArgs args = new RowFullEventArgs { RowId = t.Item2 };
                    OnRowFull(args);
                }
                _topmost = Math.Min(t.Item2, _topmost);
            }
        }

        public IEnumerable<int> GetFullRows()
        {
            for (int i = _counters.Length - 1; i > 0; i--)
            {
                if (_counters[i] >= _width)
                {
                    yield return i;
                }
            }
        }

        /// <summary>
        /// Resets the counters
        /// </summary>
        public void Clear()
        {
            _topmost = _counters.Length;
            for (int i = 0; i < _counters.Length; i++)
            {
                _counters[i] = 0;
            }
        }

        /// <summary>
        /// Tidies up in the counter
        /// </summary>
        public void RemoveFullRows()
        {
            int j = _counters.Length - 1;
            byte[] _new_counters = new byte[_counters.Length];
            
            for (int i = _counters.Length - 1; i >= 0; i--)
            {
                if (_counters[i] < _width)
                {
                    _new_counters[j--] = _counters[i];
                }
            }

            _counters = new byte[_counters.Length];
            Array.Copy(_new_counters, _counters, _counters.Length);
        }

        public IEnumerable<Tuple<int, byte>> Expose()
        {
            for (int i = 0; i < _counters.Length; i++)
            {
                yield return new Tuple<int, byte>(i, _counters[i]);
            }
        }

        //An array to hold the number of frozen cells in each row
        private byte[] _counters;
        //width of the grid
        private int _width;
        //the topmost non-empty row
        private int _topmost;

        public int Topmost
        {
            get { return _topmost; }
        }

        protected virtual void OnRowFull(RowFullEventArgs e)
        {
            EventHandler<RowFullEventArgs> handler = RowFull;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<RowFullEventArgs> RowFull;
    }
}

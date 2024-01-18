using System;

namespace TetrisFigures.Auxiliary
{
    public class RowFullEventArgs : EventArgs
    {
        public int RowId { get; set; }
    }

    public delegate void RowFullEventHandler(object sender, RowFullEventArgs e);
}

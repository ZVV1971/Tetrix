using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;

namespace TetrisFigures.Interfaces
{

    public class TetrisUserControl : UserControl, INotifyPropertyChanged
    {
        public virtual void ChangeSize(int size) { }

        public virtual int GetPositionNumber(List<Tuple<int, int>> tuples)
        {
            return 0;
        }

        public virtual List<Tuple<int, int>> Rotate(List<Tuple<int, int>> oldpos)
        {
            if (TransformationMatrix != null)
            {

                List<Tuple<int, int>> newPos = new List<Tuple<int, int>>();
                int pos_number = GetPositionNumber(oldpos);

                for (int n = 0; n < oldpos.Count; n++)
                {
                    newPos.Add(new Tuple<int, int>(oldpos[n].Item1 + TransformationMatrix[pos_number][n].Item1,
                        oldpos[n].Item2 + TransformationMatrix[pos_number][n].Item2));
                }

                return newPos;
            }
            else return oldpos;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        private Brush _color;

        public Brush color
        {
            get { return _color; }
            set { _color = value; NotifyPropertyChanged("color"); }
        }

        public virtual List<Tuple<int, int>[]> TransformationMatrix
        {
            get;
            set;
        }

        public virtual List<Tuple<int, int>> InitialPosition
        {
            get;
            set;
        }
    }
}

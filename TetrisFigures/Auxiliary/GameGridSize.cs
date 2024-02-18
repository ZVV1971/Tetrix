using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace TetrisFigures.Auxiliary
{
    public class GameGridSize : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private Dictionary<string, List<string>> propErrors = new Dictionary<string, List<string>>();

        private byte _width;
        private byte _height;
        private float _ratio;
        public byte width
        {
            get { return _width; }
            set
            {
                _width = value;
                OnPropertyChanged("width");
                ratio = (float)_height / (float)_width;
            }
        }
        public byte height
        {
            get { return _height; }
            set
            {
                _height = value;
                OnPropertyChanged("height");
                ratio = (float)_height / (float)_width;
            }
        }
        public float ratio
        {
            get { return _ratio; }
            set
            {
                _ratio = value;
                OnPropertyChanged("ratio");
            }
        }

        private void DataValidation(string name)
        {
            switch (name)
            {
                case nameof(width):
                    if (propErrors.TryGetValue(nameof(width), out List<string> listWErrors) == false)
                        listWErrors = new List<string>();
                    else
                        listWErrors.Clear();

                    if (width > 20)
                        listWErrors.Add("Width cannot be greater than 20!!!");
                    else if (width < 10)
                        listWErrors.Add("Width cannot be less than 10!!!");
                    propErrors[nameof(width)] = listWErrors;

                    if (listWErrors.Count(x => x == nameof(width)) > 0)
                    {
                        OnPropertyErrorsChanged("width");
                    }
                    break;
                case nameof(height):
                    if (propErrors.TryGetValue(nameof(height), out List<string> listHErrors) == false)
                        listHErrors = new List<string>();
                    else
                        listHErrors.Clear();

                    if (height > 40)
                        listHErrors.Add("Height cannot be greater than 40!!!");
                    else if (height < 20)
                        listHErrors.Add("Height cannot be less than 20!!!");
                    propErrors[nameof(height)] = listHErrors;

                    if (listHErrors.Count(x => x == nameof(height)) > 0)
                    {
                        OnPropertyErrorsChanged("height");
                    }
                    break;
                case nameof(ratio):
                    if (propErrors.TryGetValue(nameof(ratio), out List<string> listRErrors) == false)
                        listRErrors = new List<string>();
                    else
                        listRErrors.Clear();

                    if (ratio != 2)
                        listRErrors.Add(string.Format("Ratio must be exactly 2!!! Not {0}", ratio));
                    propErrors[nameof(ratio)] = listRErrors;

                    if (listRErrors.Count(x => x == nameof(height)) > 0)
                    {
                        OnPropertyErrorsChanged("ratio");
                    }
                    break;
                default:
                    break;
            }
        }

        # region INotifyDataErrorInfo
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        private void OnPropertyErrorsChanged(string p)
        {
            if (ErrorsChanged != null)
                ErrorsChanged.Invoke(this, new DataErrorsChangedEventArgs(p));
        }

        public System.Collections.IEnumerable GetErrors(string propertyName)
        {
            if (propertyName != null)
            {
                propErrors.TryGetValue(propertyName, out List<string> errors);
                return errors;
            }

            else
                return null;
        }

        public bool HasErrors
        {
            get
            {
                try
                {
                    List<string> propErrorsCount = propErrors.Values.FirstOrDefault(r => r.Count > 0);
                    if (propErrorsCount != null)
                        return true;
                    else
                        return false;
                }
                catch { }
                return true;
            }
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            DataValidation(name);
        }
        #endregion
    }
}
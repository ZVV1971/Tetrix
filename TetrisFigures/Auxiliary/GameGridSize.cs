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
        private float _ratio = 2;
        public byte width
        {
            get { return _width; }
            set
            {
                _width = value;
                OnPropertyChanged("width");
                OnPropertyChanged("ratio");
            }
        }
        public byte height
        {
            get { return _height; }
            set
            {
                _height = value;
                OnPropertyChanged("height");
                OnPropertyChanged("ratio");
            }
        }
        public float ratio
        {
            get { return (float)_height / (float)_width; }
            set
            {
                _ratio = (float)_height / (float)_width;
                OnPropertyChanged("ratio");
            }
        }

        private void DataValidation(string name)
        {
            //Validate Name property
            List<string> listErrors;
            
            switch (name)
            {
                case nameof(width):
                    if (propErrors.TryGetValue(nameof(width), out listErrors) == false)
                        listErrors = new List<string>();
                    else
                        listErrors.Clear();

                    if (width > 20)
                        listErrors.Add("Width cannot be greater than 20!!!");
                    else if (width < 10)
                        listErrors.Add("Width cannot be less than 10!!!");
                    propErrors[nameof(width)] = listErrors;

                    if (listErrors.Count(x => x == nameof(width)) > 0)
                    {
                        OnPropertyErrorsChanged("width");
                    }
                    break;
                case nameof(height):
                    if (propErrors.TryGetValue(nameof(height), out listErrors) == false)
                        listErrors = new List<string>();
                    else
                        listErrors.Clear();

                    if (height > 40)
                        listErrors.Add("Height cannot be greater than 40!!!");
                    else if (height < 20)
                        listErrors.Add("Height cannot be less than 20!!!");
                    propErrors[nameof(height)] = listErrors;

                    if (listErrors.Count(x => x == nameof(height)) > 0)
                    {
                        OnPropertyErrorsChanged("height");
                    }
                    break;
                case nameof(ratio):
                    if (propErrors.TryGetValue(nameof(ratio), out listErrors) == false)
                        listErrors = new List<string>();
                    else
                        listErrors.Clear();

                    if (ratio != 2)
                        listErrors.Add("Ratio must be exactly 2!!!");
                    propErrors[nameof(ratio)] = listErrors;
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
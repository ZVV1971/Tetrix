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
                ratio = (_width != 0 && _height != 0) ? ((float)_height / (float)_width) : 2;
                OnPropertyChanged("width");
            }
        }
        public byte height
        {
            get { return _height; }
            set
            {
                _height = value;
                ratio = (_width != 0 && _height != 0) ? ((float)_height / (float)_width) : 2;
                OnPropertyChanged("height");
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

        protected async Task Validate(string name)
        {
            await Task.Factory.StartNew(() => { DataValidation(name); });
        }

        private void DataValidation(string name)
        {
            List<string> listErrors;

            if (propErrors.TryGetValue(nameof(width), out listErrors) == false)
                listErrors = new List<string>();
            else
                listErrors.Clear();

            if (width > 20)
                listErrors.Add("Width cannot be greater than 20!!!");
            else if (width < 10)
                listErrors.Add("Width cannot be less than 10!!!");
            propErrors[nameof(width)] = listErrors;

            if (propErrors.TryGetValue(nameof(height), out listErrors) == false)
                listErrors = new List<string>();
            else
                listErrors.Clear();

            if (height > 40)
                listErrors.Add("Height cannot be greater than 40!!!");
            else if (height < 20)
                listErrors.Add("Height cannot be less than 20!!!");
            propErrors[nameof(height)] = listErrors;

            if (name != nameof(ratio))
            {
                if (ratio != 2)
                { listErrors.Add(string.Format("Ratio must be exactly 2!!! Not {0}", ratio)); }
                propErrors[name] = listErrors;
            }

            OnPropertyErrorsChanged(nameof(height));
            OnPropertyErrorsChanged(nameof(width));
        }

        # region INotifyDataErrorInfo
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        private void OnPropertyErrorsChanged(string p)
        {
            if (ErrorsChanged != null)
            { ErrorsChanged.Invoke(this, new DataErrorsChangedEventArgs(p)); }
        }

        public System.Collections.IEnumerable GetErrors(string propertyName)
        {
            if (propertyName != null)
            {
                propErrors.TryGetValue(propertyName, out List<string> errors);
                return errors;
            }
            else { return null; }
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

        public async void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            await Validate(name);
        }
        #endregion
    }
}
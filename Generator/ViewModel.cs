using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Generator
{
    /// <summary>
    /// Base Class for Viewmodels with Property Changed methods implemented
    /// </summary>
    public abstract class ViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        /// <summary>
        /// Raises this object's PropertyChanged event for multiple items
        /// </summary>
        /// <param name="propertyNames">Series of properties which have new values</param>
        protected virtual void OnPropertiesChanged(params string[] propertyNames)
        {
            foreach (string name in propertyNames)
                OnPropertyChanged(name);
        }

        #endregion // INotifyPropertyChanged Members

    }
}

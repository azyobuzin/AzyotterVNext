using System;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Livet;
using Newtonsoft.Json;

namespace Azyotter.Models
{
    public abstract class ModelBase : NotificationObject, IDisposable
    {
        protected void Set<T>(T value, [CallerMemberName] string propName = null)
        {
            PropertyHelper.Set(this, propName, value, this.RaisePropertyChanged);
        }

        protected override void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            base.RaisePropertyChanged(propertyName);
        }

        [NonSerialized]
        private LivetCompositeDisposable compositeDisposable = new LivetCompositeDisposable();

        [XmlIgnore]
        [JsonIgnore]
        public LivetCompositeDisposable CompositeDisposable
        {
            get
            {
                return this.compositeDisposable;
            }
            set
            {
                this.compositeDisposable = value;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.CompositeDisposable != null)
                    this.CompositeDisposable.Dispose();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Azyotter.Models;
using Livet;
using Livet.EventListeners;

namespace Azyotter.ViewModels
{
    public class TabViewModel : ViewModelBase
    {
        public TabViewModel(MainViewModel parent, Tab model)
        {
            this.Parent = parent;
            this.Model = model;

            this.Statuses = ViewModelHelper.CreateReadOnlyDispatcherCollection(
                model.Statuses,
                s => new StatusViewModel(this, s),
                DispatcherHelper.UIDispatcher
            );

            this.CompositeDisposable.Add(new PropertyChangedEventListener(model, (sender, e) =>
            {
                switch (e.PropertyName)
                {
                    case "Name":
                        this.RaisePropertyChanged(e.PropertyName);
                        break;
                }
            }));
        }

        public MainViewModel Parent { get; private set; }
        public Tab Model { get; private set; }

        public string Name
        {
            get
            {
                return this.Model.Name;
            }
        }

        public ReadOnlyDispatcherCollection<StatusViewModel> Statuses { get; private set; }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (this.Statuses != null)
                {
                    this.Statuses.ForEach(s => s.Dispose());
                    this.Statuses = null;
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Azyotter.Models;
using Livet;
using Livet.Commands;
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
            this.StatusesView = new ListCollectionView(this.Statuses);
            this.StatusesView.SortDescriptions.Add(new SortDescription("Id", ListSortDirection.Descending));

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
        public ListCollectionView StatusesView { get; private set; }

        private StatusViewModel selectedStatus = null;
        public StatusViewModel SelectedStatus
        {
            get
            {
                return this.selectedStatus;
            }
            set
            {
                this.Set(value);
                this.FavoriteCommand.RaiseCanExecuteChanged();
                this.RetweetCommand.RaiseCanExecuteChanged();
            }
        }

        private ViewModelCommand favoriteCommand;
        public ViewModelCommand FavoriteCommand
        {
            get
            {
                if (this.favoriteCommand == null)
                    this.favoriteCommand = new ViewModelCommand(
                        () => this.SelectedStatus.FavoriteCommand.Execute(),
                        () => this.SelectedStatus != null && this.SelectedStatus.FavoriteCommand.CanExecute);
                return this.favoriteCommand;
            }
        }

        private ViewModelCommand retweetCommand;
        public ViewModelCommand RetweetCommand
        {
            get
            {
                if (this.retweetCommand == null)
                    this.retweetCommand = new ViewModelCommand(
                        () => this.SelectedStatus.RetweetCommand.Execute(),
                        () => this.SelectedStatus != null && this.SelectedStatus.RetweetCommand.CanExecute);
                return this.retweetCommand;
            }
        }

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

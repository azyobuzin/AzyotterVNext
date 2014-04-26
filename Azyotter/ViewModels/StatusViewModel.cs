using System;
using Azyotter.Models;
using Livet;
using Livet.EventListeners;

namespace Azyotter.ViewModels
{
    public class StatusViewModel : ViewModelBase
    {
        public StatusViewModel(TabViewModel parent, StatusModel model)
        {
            this.Parent = parent;
            this.Model = model;

            this.CompositeDisposable.Add(new PropertyChangedEventListener(model, (sender, e) =>
            {
                switch (e.PropertyName)
                {
                    case "Id":
                    case "CreatedAt":
                    case "Text":
                        this.RaisePropertyChanged(e.PropertyName);
                        break;
                }
            }));
            this.CompositeDisposable.Add(new PropertyChangedEventListener(model.From, (sender, e) =>
            {
                switch (e.PropertyName)
                {
                    case "ScreenName":
                        this.RaisePropertyChanged(() => this.FromScreenName);
                        break;
                    case "Name":
                        this.RaisePropertyChanged(() => this.FromName);
                        break;
                    case "ProfileImage":
                        this.RaisePropertyChanged(() => this.FromProfileImage);
                        break;
                }
            }));
            this.CompositeDisposable.Add(new CollectionChangedEventListener(model.FavoritedAccounts,
                (sender, e) => this.RaisePropertyChanged(() => this.IsFavorited)));
            this.CompositeDisposable.Add(new PropertyChangedEventListener(model.Parent.Settings)
            {
                { "ActiveAccountId", (sender, e) => this.RaisePropertyChanged(() => this.IsFavorited) }
            });
        }

        public TabViewModel Parent { get; private set; }
        public StatusModel Model { get; private set; }

        public long Id
        {
            get
            {
                return this.Model.Id;
            }
        }

        public string FromScreenName
        {
            get
            {
                return this.Model.From.ScreenName;
            }
        }

        public string FromName
        {
            get
            {
                return this.Model.From.Name;
            }
        }

        public string FromProfileImage
        {
            get
            {
                return this.Model.From.ProfileImage;
            }
        }

        public string CreatedAt
        {
            get
            {
                return this.Model.CreatedAt.ToLocalTime().ToString("T");
            }
        }

        public string Text
        {
            get
            {
                return this.Model.Text;
            }
        }

        public bool IsFavorited
        {
            get
            {
                return this.Model.FavoritedAccounts.Contains(this.Model.Parent.Settings.GetActiveAccount());
            }
        }
    }
}

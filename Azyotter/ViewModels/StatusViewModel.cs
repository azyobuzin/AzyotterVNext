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
        }

        public TabViewModel Parent { get; private set; }
        public StatusModel Model { get; private set; }

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
    }
}

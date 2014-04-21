using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Azyotter.Models;
using Livet.EventListeners;
using Livet.Commands;

namespace Azyotter.ViewModels
{
    public class TweetingViewModel : ViewModelBase
    {
        public TweetingViewModel(MainViewModel parent)
        {
            this.Parent = parent;
            this.CompositeDisposable.Add(new PropertyChangedEventListener(parent.Model.Settings, (sender, e) =>
            {
                switch (e.PropertyName)
                {
                    case "ActiveAccountId":
                        this.RaisePropertyChanged(() => this.ActiveAccount);
                        break;
                }
            }));
        }

        public MainViewModel Parent { get; private set; }

        public Account ActiveAccount
        {
            get
            {
                return this.Parent.Model.Settings.GetActiveAccount();
            }
        }

        private string text = "";
        public string Text
        {
            get
            {
                return this.text;
            }
            set
            {
                this.Set(value);
            }
        }

        private ViewModelCommand tweetCommand;
        public ViewModelCommand TweetCommand
        {
            get
            {
                if (this.tweetCommand == null)
                    this.tweetCommand = new ViewModelCommand(() =>
                    {
                        this.Parent.Model.Tweet(this.Text);
                        this.Text = "";
                    }, () => true);
                return this.tweetCommand;
            }
        }
    }
}

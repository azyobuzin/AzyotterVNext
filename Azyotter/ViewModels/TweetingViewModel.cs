using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Azyotter.Models;
using Azyotter.Models.TwitterText;
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
        private Validator validator = new Validator();

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
                this.RemainingCount = Validator.MAX_TWEET_LENGTH - this.validator.GetTweetLength(value);
            }
        }

        private int remainingCount = 140;
        public int RemainingCount
        {
            get
            {
                return this.remainingCount;
            }
            private set
            {
                this.Set(value);
                this.TweetCommand.RaiseCanExecuteChanged();
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
                    }, () => this.Parent.Model.Settings.GetActiveAccount() != null && this.validator.IsValidTweet(this.Text));
                return this.tweetCommand;
            }
        }
    }
}

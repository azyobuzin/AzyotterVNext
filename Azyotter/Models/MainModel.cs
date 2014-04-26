using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreTweet;
using Livet;
using Livet.EventListeners;

namespace Azyotter.Models
{
    public class MainModel : ModelBase
    {
        public MainModel()
        {
            this.Settings = Settings.Load();
            this.Statuses = new StatusStorage(this);
            this.Users = new UserStorage(this);

            this.CompositeDisposable.Add(new CollectionChangedEventListener(this.Settings.Accounts)
            {
                {
                    NotifyCollectionChangedAction.Add,
                    (sender, e) => TaskExEx.RunLong(() => this.Statuses.AddRange(
                        this.GetTwitterClient((Account)e.NewItems[0]).Statuses.HomeTimeline(),
                        (Account)e.NewItems[0]
                    ))
                }
            });
        }

        public Settings Settings { get; private set; }

        public StatusStorage Statuses { get; private set; }
        public UserStorage Users { get; private set; }

        public Tokens GetTwitterClient()
        {
            return this.GetTwitterClient(this.Settings.GetActiveAccount());
        }

        public Tokens GetTwitterClient(Account account)
        {
            return Tokens.Create(
                string.IsNullOrEmpty(account.ConsumerKey) ? Settings.ConsumerKey : account.ConsumerKey,
                string.IsNullOrEmpty(account.ConsumerSecret) ? Settings.ConsumerSecret : account.ConsumerSecret,
                account.OAuthToken,
                account.OAuthTokenSecret
            );
        }

        public void PassNewStatus(StatusModel status)
        {
            this.Settings.Tabs.ForEach(tab => tab.AddStatus(status));
        }

        public void FirstReceive()
        {
            this.Settings.Accounts.ForEach(a => TaskExEx.RunLong(() =>
                this.Statuses.AddRange(this.GetTwitterClient(a).Statuses.HomeTimeline(), a)
            ));
        }

        public Task Tweet(string text)
        {
            return TaskEx.Run(() =>
            {
                try
                {
                    this.Statuses.Add(
                        this.GetTwitterClient().Statuses.Update(status => text),
                        null
                    );
                }
                catch
                {
                    //TODO
                }
             });
        }
    }
}

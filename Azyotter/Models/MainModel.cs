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
            this.Statuses = new ConcurrentDictionary<long, StatusModel>();
            this.Users = new ConcurrentDictionary<long, UserModel>();
            this.Settings = Settings.Load();

            this.CompositeDisposable.Add(new CollectionChangedEventListener(this.Settings.Accounts)
            {
                {
                    NotifyCollectionChangedAction.Add,
                    (sender, e) => TaskExEx.RunLong(() => AddStatuses(
                        this.GetTwitterClient(e.NewItems[0] as Account).Statuses.HomeTimeline()
                    ))
                }
            });
        }

        public Settings Settings { get; private set; }

        public ConcurrentDictionary<long, StatusModel> Statuses { get; private set; }
        public ConcurrentDictionary<long, UserModel> Users { get; private set; }

        public Tokens GetTwitterClient(Account account)
        {
            return Tokens.Create(
                string.IsNullOrEmpty(account.ConsumerKey) ? Settings.ConsumerKey : account.ConsumerKey,
                string.IsNullOrEmpty(account.ConsumerSecret) ? Settings.ConsumerSecret : account.ConsumerSecret,
                account.OAuthToken,
                account.OAuthTokenSecret
            );
        }

        public void AddStatus(Status s)
        {
            var isNewItem = true;
            var model = this.Statuses.AddOrUpdate(s.ID, _ => new StatusModel(this, s), (_, current) =>
            {
                current.Update(this, s);
                isNewItem = false;
                return current;
            });
            if (isNewItem)
                this.Settings.Tabs.ForEach(tab => tab.AddStatus(model));
        }

        public void AddStatuses(IEnumerable<Status> statuses)
        {
            statuses.ForEach(this.AddStatus);
        }

        public UserModel GetOrUpdateUser(User user)
        {
            return this.Users.AddOrUpdate(user.ID.Value, _ => new UserModel(user), (id, current) =>
            {
                current.Update(user);
                return current;
            });
        }

        public void FirstReceive()
        {
            this.Settings.Accounts.ForEach(a => TaskExEx.RunLong(() =>
                AddStatuses(this.GetTwitterClient(a).Statuses.HomeTimeline())
            ));
        }

        public void Tweet(string text)
        {
            TaskExEx.RunLong(() =>{
                try
                {
                    this.GetTwitterClient(this.Settings.GetActiveAccount())
                        .Statuses.Update(status => text);
                }
                catch
                {
                    //TODO
                }
             });
        }
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using CoreTweet;
using Livet;

namespace Azyotter.Models
{
    public class StatusModel : ModelBase
    {
        public StatusModel(MainModel parent)
        {
            this.Parent = parent;
        }

        public StatusModel(MainModel parent, Status status, Account account)
            : this(parent)
        {
            this.Update(status, account);
        }

        public MainModel Parent { get; private set; }

        private bool isDM = false;
        public bool IsDM
        {
            get
            {
                return this.isDM;
            }
            set
            {
                this.Set(value);
            }
        }

        private long id = 0;
        public long Id
        {
            get
            {
                return this.id;
            }
            set
            {
                this.Set(value);
            }
        }

        private DateTimeOffset createdAt = default(DateTimeOffset);
        public DateTimeOffset CreatedAt
        {
            get
            {
                return this.createdAt;
            }
            set
            {
                this.Set(value);
            }
        }

        private string text = null;
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

        private UserModel from = null;
        public UserModel From
        {
            get
            {
                return this.from;
            }
            set
            {
                this.Set(value);
            }
        }

        private ObservableSynchronizedCollection<Account> favoritedAccounts = new ObservableSynchronizedCollection<Account>();
        public ObservableSynchronizedCollection<Account> FavoritedAccounts
        {
            get
            {
                return this.favoritedAccounts;
            }
        }

        private ObservableSynchronizedCollection<RetweetInfo> retweetedAccounts = new ObservableSynchronizedCollection<RetweetInfo>();
        public ObservableSynchronizedCollection<RetweetInfo> RetweetedAccounts
        {
            get
            {
                return this.retweetedAccounts;
            }
        }

        public void Update(Status status, Account account)
        {
            this.Id = status.ID;
            this.CreatedAt = status.CreatedAt;
            this.Text = status.Text;
            this.From = this.Parent.Users.GetOrUpdate(status.User);
            if (this.FavoritedAccounts.Contains(account))
            {
                if (status.IsFavorited.HasValue && !status.IsFavorited.Value)
                    this.FavoritedAccounts.Remove(account);
            }
            else
            {
                if (status.IsFavorited.HasValue && status.IsFavorited.Value)
                    this.FavoritedAccounts.Add(account);
            }
            var retweetInfo = this.RetweetedAccounts.FirstOrDefault(r => r.Account == account);
            if (retweetInfo != null)
            {
                if (status.IsRetweeted.HasValue && !status.IsRetweeted.Value)
                    this.RetweetedAccounts.Remove(retweetInfo);
            }
            else
            {
                if (status.IsRetweeted.HasValue && status.IsRetweeted.Value)
                    this.RetweetedAccounts.Add(new RetweetInfo(account, status.CurrentUserRetweet));
            }
        }

        public Task ToggleFavorite()
        {
            return TaskExEx.RunLong(() =>
            {
                //TODO: RT の処理
                var a = this.Parent.Settings.GetActiveAccount();
                if (this.FavoritedAccounts.Contains(a))
                {
                    this.Parent.GetTwitterClient().Favorites.Destroy(id => this.Id);
                    this.FavoritedAccounts.Remove(a);
                    //TODO: あんふぁぼ済みエラーの処理
                }
                else
                {
                    this.Parent.GetTwitterClient().Favorites.Create(id => this.Id);
                    if (!this.FavoritedAccounts.Contains(a))
                        this.favoritedAccounts.Add(a);
                    //TODO: ふぁぼ済みエラーの処理
                }
            });
        }

        public Task ToggleRetweet()
        {
            return TaskExEx.RunLong(() =>
            {
                //TODO: RT の処理
                var a = this.Parent.Settings.GetActiveAccount();
                var retweetInfo = this.RetweetedAccounts.FirstOrDefault(r => r.Account == a);
                if (retweetInfo != null)
                {
                    var retweetId = retweetInfo.StatusId.HasValue
                        ? retweetInfo.StatusId.Value
                        : this.Parent.GetTwitterClient(a).Statuses
                            .Show(id => this.Id, include_my_retweet => true)
                            .CurrentUserRetweet;
                    this.Parent.GetTwitterClient(a).Statuses.Destroy(id => retweetId);
                    this.RetweetedAccounts.Remove(retweetInfo);
                    //TODO: RT 削除済みエラーの処理
                }
                else
                {
                    var retweet = this.Parent.GetTwitterClient(a).Statuses.Retweet(id => this.Id);
                    if (!this.RetweetedAccounts.Any(r => r.Account == a))
                        this.RetweetedAccounts.Add(new RetweetInfo(a, retweet.ID));
                }
            });
        }
    }
}

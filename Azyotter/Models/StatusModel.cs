using System;
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

        public StatusModel(MainModel parent, Status status)
            : this(parent)
        {
            this.Update(status);
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

        private ObservableSynchronizedCollection<Account> favoritedUsers = new ObservableSynchronizedCollection<Account>();
        public ObservableSynchronizedCollection<Account> FavoritedUsers
        {
            get
            {
                return this.favoritedUsers;
            }
        }

        public void Update(Status status)
        {
            this.Id = status.ID;
            this.CreatedAt = status.CreatedAt;
            this.Text = status.Text;
            this.From = this.Parent.Users.GetOrUpdate(status.User);
        }

        public Task ToggleFavorite()
        {
            return TaskExEx.RunLong(() =>
            {
                //TODO: RT の処理
                var a = this.Parent.Settings.GetActiveAccount();
                if (this.FavoritedUsers.Contains(a))
                {
                    this.Parent.GetTwitterClient().Favorites.Destroy(id => this.Id);
                    this.FavoritedUsers.Remove(a);
                }
                else
                {
                    this.Parent.GetTwitterClient().Favorites.Create(id => this.Id);
                    if (!this.FavoritedUsers.Contains(a))
                        this.favoritedUsers.Add(a);
                }
            });
        }
    }
}

using System;
using CoreTweet;

namespace Azyotter.Models
{
    public class StatusModel : ModelBase
    {
        public StatusModel() { }
        public StatusModel(MainModel mainModel, Status status)
        {
            this.Update(mainModel, status);
        }

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

        public void Update(MainModel mainModel, Status status)
        {
            this.Id = status.ID;
            this.CreatedAt = status.CreatedAt;
            this.Text = status.Text;
            this.From = mainModel.GetOrUpdateUser(status.User);
        }
    }
}

using System;
using CoreTweet;

namespace Azyotter.Models
{
    public class UserModel : ModelBase
    {
        public UserModel() { }
        public UserModel(User user)
        {
            this.Update(user);
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

        private string screenName = null;
        public string ScreenName
        {
            get
            {
                return this.screenName;
            }
            set
            {
                this.Set(value);
            }
        }

        private string name = null;
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.Set(value);
            }
        }

        private string profileImage = null;
        public string ProfileImage
        {
            get
            {
                return this.profileImage;
            }
            set
            {
                this.Set(value);
            }
        }

        public void Update(User user)
        {
            this.Id = user.ID.Value;
            this.ScreenName = user.ScreenName;
            this.Name = user.Name;
            this.ProfileImage = StatusProcessor.GetOriginalProfileImage(user.ProfileImageUrlHttps);
        }
    }
}

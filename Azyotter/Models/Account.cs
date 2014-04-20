namespace Azyotter.Models
{
    public class Account : ModelBase
    {
        private long userId = 0;
        public long UserId
        {
            get
            {
                return this.userId;
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

        private string consumerKey = null;
        public string ConsumerKey
        {
            get
            {
                return this.consumerKey;
            }
            set
            {
                this.Set(value);
            }
        }

        private string consumerSecret = null;
        public string ConsumerSecret
        {
            get
            {
                return this.consumerSecret;
            }
            set
            {
                this.Set(value);
            }
        }

        private string oauthToken = null;
        public string OAuthToken
        {
            get
            {
                return this.oauthToken;
            }
            set
            {
                this.Set(value);
            }
        }

        private string oauthTokenSecret = null;
        public string OAuthTokenSecret
        {
            get
            {
                return this.oauthTokenSecret;
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
    }
}

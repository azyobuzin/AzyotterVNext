using System.Diagnostics;
using System.Threading.Tasks;
using CoreTweet;

namespace Azyotter.Models
{
    public class Authorization : ModelBase
    {
        public Authorization(MainModel parent)
        {
            this.Parent = parent;
        }

        public MainModel Parent { get; private set; }

        private OAuth.OAuthSession session;

        public Task Begin()
        {
            return TaskEx.Run(() =>
            {
                this.session = OAuth.Authorize(Settings.ConsumerKey, Settings.ConsumerSecret);
                Process.Start(session.AuthorizeUri.ToString());
            });
        }

        public async Task End(string pin)
        {
            await TaskEx.Run(() =>
            {
                var t = this.session.GetTokens(pin);
                var profile = t.Account.VerifyCredentials();
                this.Parent.Settings.Accounts.Add(new Account()
                {
                    UserId = profile.ID.Value,
                    ScreenName = profile.ScreenName,
                    ConsumerKey = this.session.ConsumerKey,
                    ConsumerSecret = this.session.ConsumerSecret,
                    OAuthToken = t.AccessToken,
                    OAuthTokenSecret = t.AccessTokenSecret,
                    ProfileImage = StatusProcessor.GetOriginalProfileImage(profile.ProfileImageUrlHttps)
                });
            });
            await this.Parent.Settings.Save();
        }
    }
}

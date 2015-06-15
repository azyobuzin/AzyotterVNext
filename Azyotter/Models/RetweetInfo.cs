namespace Azyotter.Models
{
    public class RetweetInfo : ModelBase
    {
        public RetweetInfo(Account account, long? statusId)
        {
            this.Account = account;
            this.StatusId = statusId;
        }

        public Account Account { get; private set; }

        private long? statusId = null;
        public long? StatusId
        {
            get
            {
                return this.statusId;
            }
            set
            {
                this.Set(value);
            }
        }
    }
}

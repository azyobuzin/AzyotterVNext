using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CoreTweet;

namespace Azyotter.Models
{
    public class StatusStorage : IEnumerable<StatusModel>
    {
        public StatusStorage(MainModel parent)
        {
            this.Parent = parent;
        }

        public MainModel Parent { get; private set; }

        private readonly ConcurrentDictionary<long, StatusModel> dic = new ConcurrentDictionary<long, StatusModel>();

        public IEnumerator<StatusModel> GetEnumerator()
        {
            return this.dic.Select(kvp => kvp.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void Add(Status status, Account account)
        {
            var isNewItem = true;
            var model = this.dic.AddOrUpdate(status.ID, _ => new StatusModel(this.Parent, status, account), (_, current) =>
            {
                current.Update(status, account);
                isNewItem = false;
                return current;
            });
            if (isNewItem)
                this.Parent.PassNewStatus(model);
        }

        public void AddRange(IEnumerable<Status> statuses, Account account)
        {
            statuses.ForEach(s => this.Add(s, account));
        }
    }
}

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

        public void Add(Status s)
        {
            var isNewItem = true;
            var model = this.dic.AddOrUpdate(s.ID, _ => new StatusModel(this.Parent, s), (_, current) =>
            {
                current.Update(s);
                isNewItem = false;
                return current;
            });
            if (isNewItem)
                this.Parent.PassNewStatus(model);
        }

        public void AddRange(IEnumerable<Status> statuses)
        {
            statuses.ForEach(this.Add);
        }
    }
}

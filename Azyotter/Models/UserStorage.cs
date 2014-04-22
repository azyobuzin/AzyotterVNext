using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CoreTweet;

namespace Azyotter.Models
{
    public class UserStorage : IEnumerable<UserModel>
    {
        public UserStorage(MainModel parent)
        {
            this.Parent = parent;
        }

        public MainModel Parent { get; private set; }

        private readonly ConcurrentDictionary<long, UserModel> dic = new ConcurrentDictionary<long, UserModel>();

        public IEnumerator<UserModel> GetEnumerator()
        {
            return this.dic.Select(kvp => kvp.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public UserModel GetOrUpdate(User user)
        {
            return this.dic.AddOrUpdate(user.ID.Value, _ => new UserModel(user), (id, current) =>
            {
                current.Update(user);
                return current;
            });
        }
    }
}

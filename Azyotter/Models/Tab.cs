using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Livet;
using Newtonsoft.Json;

namespace Azyotter.Models
{
    public class Tab : ModelBase
    {
        public Tab()
        {
            this.Statuses = new ObservableSynchronizedCollection<StatusModel>();
        }

        private string name = "NewTab";
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

        [JsonIgnore]
        public ObservableSynchronizedCollection<StatusModel> Statuses { get; private set; }

        public void AddStatus(StatusModel status)
        {
            //TODO: フィルター
            this.Statuses.Add(status);
        }
    }
}

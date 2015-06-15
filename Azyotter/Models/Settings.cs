using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Livet;
using Newtonsoft.Json;

namespace Azyotter.Models
{
    public class Settings : ModelBase
    {
        public const string ConsumerKey = "xx0RBhnHRa0FYfPuDLlBNg";
        public const string ConsumerSecret = "TSMrMmc6uIxT2l8o7p9aC3mxHVMDzp17eXInyuSZgk";

        private static readonly string fileName = Path.Combine(
            Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
            "Settings.json"
        );

        public static Settings GetDefaultSettings()
        {
            var settings = new Settings();

            settings.Tabs.Add(new Tab()
            {
                Name = "Home"
            });

            return settings;
        }

        public static Settings Load()
        {
            if (File.Exists(fileName))
            {
                try
                {
                    return JsonConvert.DeserializeObject<Settings>(File.ReadAllText(fileName));
                }
                catch
                {
                    File.Copy(fileName, Path.Combine(Path.GetDirectoryName(fileName), "Settings"
                        + DateTime.Now.ToString("yyyyMMddHHmmss") + ".json"));
                    return GetDefaultSettings();
                }
            }
            else
                return GetDefaultSettings();
        }

        public Task Save()
        {
            return TaskEx.Run(() => File.WriteAllText(
                fileName,
                JsonConvert.SerializeObject(this)
            ));
        }

        public Settings()
        {
            this.Accounts = new ObservableSynchronizedCollection<Account>();
            this.Tabs = new ObservableSynchronizedCollection<Tab>();
        }

        public ObservableSynchronizedCollection<Account> Accounts { get; private set; }

        public ObservableSynchronizedCollection<Tab> Tabs { get; private set; }

        private long? activeAccountId = null;
        public long? ActiveAccountId
        {
            get
            {
                return this.activeAccountId;
            }
            set
            {
                this.Set(value);
            }
        }

        public Account GetActiveAccount()
        {
            return this.Accounts.FirstOrDefault(a => a.UserId == this.ActiveAccountId)
                ?? this.Accounts.FirstOrDefault();
        }
    }
}

using System;
using System.Linq;
using Azyotter.Models;
using Livet;
using Livet.Messaging;

namespace Azyotter.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            this.Model = new MainModel();
            this.CompositeDisposable.Add(this.Model);
            this.Tabs = ViewModelHelper.CreateReadOnlyDispatcherCollection(
                this.Model.Settings.Tabs,
                model => new TabViewModel(this, model),
                DispatcherHelper.UIDispatcher
            );
        }

        public MainModel Model { get; private set; }

        public void Initialize()
        {
            this.Model.FirstReceive();

            if (!this.Model.Settings.Accounts.Any())
            {
                this.Messenger.RaiseAsync(new TransitionMessage(new AuthorizationViewModel(this), "StartAuthorization"));
            }
        }

        public ReadOnlyDispatcherCollection<TabViewModel> Tabs { get; private set; }
    }
}
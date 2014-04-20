using System;
using System.Windows;
using Azyotter.Models;
using Livet.Commands;
using Livet.EventListeners;
using Livet.Messaging;

namespace Azyotter.ViewModels
{
    public class AuthorizationViewModel : ViewModelBase
    {
        public AuthorizationViewModel() { }
        public AuthorizationViewModel(MainViewModel parent)
        {
            this.Parent = parent;
            this.Model = new Authorization(parent.Model);

            this.CompositeDisposable.Add(new PropertyChangedEventListener(this)
            {
                { "IsWorking", (sender, e) => this.RaisePropertyChanged(() => this.IsIdle) }
            });
        }

        public MainViewModel Parent { get; private set; }
        public Authorization Model { get; private set; }

        private bool isCompleted = false;
        public bool IsCompleted
        {
            get
            {
                return this.isCompleted;
            }
            set
            {
                this.Set(value);
            }
        }

        private string pin = "";
        public string Pin
        {
            get
            {
                return this.pin;
            }
            set
            {
                this.Set(value);
            }
        }

        private bool isWorking = false;
        public bool IsWorking
        {
            get
            {
                return this.isWorking;
            }
            set
            {
                this.Set(value);
            }
        }

        public bool IsIdle
        {
            get
            {
                return !this.IsWorking;
            }
        }

        public async void BeginAuthorization()
        {
            try
            {
                this.IsWorking = true;
                await this.Model.Begin();
                this.IsWorking = false;
            }
            catch (Exception ex)
            {
                this.Messenger.Raise(new InformationMessage(
                    ex.ToString(),
                    "リクエストトークンを取得できませんでした",
                    MessageBoxImage.Error,
                    "ShowMessage"
                ));
                this.Messenger.RaiseAsync(new InteractionMessage("Close"));
            }
        }

        private ViewModelCommand okCommand;
        public ViewModelCommand OkCommand
        {
            get
            {
                if (this.okCommand == null)
                    this.okCommand = new ViewModelCommand(async () =>
                    {
                        try
                        {
                            this.IsWorking = true;
                            await this.Model.End(this.Pin);
                            this.IsCompleted = true;
                            this.Messenger.RaiseAsync(new InteractionMessage("Close"));
                        }
                        catch (Exception ex)
                        {
                            this.IsWorking = false;
                            this.Messenger.RaiseAsync(new InformationMessage(
                                ex.ToString(),
                                "アクセストークンを取得できませんでした",
                                MessageBoxImage.Error,
                                "ShowMessage"
                            ));
                        }
                    });
                return this.okCommand;
            }
        }

        private ViewModelCommand cancelCommand;
        public ViewModelCommand CancelCommand
        {
            get
            {
                if (this.cancelCommand == null)
                    this.cancelCommand = new ViewModelCommand(() =>
                        this.Messenger.RaiseAsync(new InteractionMessage("Close")));
                return this.cancelCommand;
            }
        }
    }
}

using Caliburn.Micro;
using RfidValidator.ViewModels;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Activation;

namespace RfidValidator
{
    sealed partial class App
    {
        private WinRTContainer _container;
        private IEventAggregator _eventAggregator;

        public App()
        {
            InitializeComponent();
        }

        protected override void Configure()
        {
            _container = new WinRTContainer();
            _container.RegisterWinRTServices();
            _container.PerRequest<ShellViewModel>()
                .PerRequest<AccountTabViewModel>()
                .PerRequest<ValidateTabViewModel>();
            _eventAggregator = _container.GetInstance<IEventAggregator>();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            if (args.PreviousExecutionState == ApplicationExecutionState.Running)
            {
                return;
            }

            DisplayRootViewFor<ShellViewModel>();
        }

        protected override object GetInstance(Type service, string key)
        {
            return _container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }
    }
}

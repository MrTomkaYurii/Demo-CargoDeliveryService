using System;
using WPFDrivers.ViewModels;

namespace WPFDrivers.Commands;

public class NavigateCommand : CommandBase
{
    private readonly MainViewModel _mainViewModel;
    //pivate readonly IDriversManager _driversManager; 
    private readonly IServiceProvider _serviceProvider;


    public enum ViewType
    {
        Login,
        Drivers
    }


    public NavigateCommand(MainViewModel mainViewModel, IServiceProvider serviceProvider)
    //IDriversManager driversManager)
    {
        _serviceProvider = serviceProvider;
        _mainViewModel = mainViewModel;
 //       _driversManager = driversManager;
    }

    public override void Execute(object? parameter)
    {
        if (parameter is ViewType viewType)
        {
            switch (viewType)
            {
                case ViewType.Drivers:
                    _mainViewModel.CurrentVM = new DriversViewModel(_serviceProvider);
                    break;
                default:
                    break;
            }
        }
    }
}

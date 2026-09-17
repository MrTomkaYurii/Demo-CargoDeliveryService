using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFDrivers.ViewModels;

namespace WPFDrivers.Commands
{
    public class GetDriverCommand : CommandBaseAsync
    {
        private readonly DriversViewModel _driversViewModel;

        public GetDriverCommand(DriversViewModel driversViewModel)
        {
            _driversViewModel = driversViewModel;
        }

        public override async Task ExecuteAsync(object? parameter)
        {
            await _driversViewModel.LoadDriverAsync();
        }
    }
}

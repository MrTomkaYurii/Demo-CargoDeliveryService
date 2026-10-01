namespace Drivers.DAL.EF.Entities.HelpModels;

public class DriverParameters : QueryStringParameters
{
    public DriverParameters()
    {
        OrderBy = "LastName";
    }
    public uint MinYearOfBirth { get; set; }
    public uint MaxYearOfBirth { get; set; } = (uint)DateTime.Now.Year;

    public bool ValidYearRange => MaxYearOfBirth > MinYearOfBirth;

    public string LastName { get; set; }
}

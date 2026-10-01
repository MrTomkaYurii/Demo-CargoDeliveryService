namespace DriversBlogManagement.SharedTestHelpers.Fakes.Address;

using DriversBlogManagement.Domain.Addresses.Dtos;
using DriversBlogManagement.Domain.Addresses;

public class FakeAddress
{
    public static Address Generate(AddressForCreationDto addressForCreationDto)
    {
        return new Address(addressForCreationDto.Line1,
            addressForCreationDto.Line2,
            addressForCreationDto.City,
            addressForCreationDto.State,
            addressForCreationDto.PostalCode,
            addressForCreationDto.Country);
    }

    public static Address Generate()
    {
        return Generate(new FakeAddressForCreationDto().Generate());
    }
}
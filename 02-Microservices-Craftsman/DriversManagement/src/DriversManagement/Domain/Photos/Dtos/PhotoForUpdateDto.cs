namespace DriversManagement.Domain.Photos.Dtos;

using Destructurama.Attributed;

public sealed record PhotoForUpdateDto
{
    public string PhotoData { get; set; }
    public string ContentType { get; set; }
    public string FileName { get; set; }
    public string FileSizels { get; set; }
}

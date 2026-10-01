namespace DriversManagement.Domain.Photos.Dtos;

using Destructurama.Attributed;

public sealed record PhotoDto
{
    public Guid Id { get; set; }
    public string PhotoData { get; set; }
    public string ContentType { get; set; }
    public string FileName { get; set; }
    public string FileSizels { get; set; }
}

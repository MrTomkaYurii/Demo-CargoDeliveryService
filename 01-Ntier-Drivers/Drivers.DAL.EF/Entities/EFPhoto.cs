namespace Drivers.DAL.EF.Entities;

public class EFPhoto
{
    public int Id { get; set; }
    public byte[] PhotoData { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public int FileSize { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public virtual EFDriver? Driver { get; set; }
}

namespace Drivers.DAL.ADO.Entities;

/// <summary>
/// Сутність фотографії (ADO.NET / Dapper).
/// Відповідає структурі таблиці [dbo].[Photos].
/// </summary>
public class Photo
{
    public int Id { get; set; }
    public byte[]? PhotoData { get; set; }
    public string? ContentType { get; set; }
    public string? FileName { get; set; }
    public int? FileSize { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}

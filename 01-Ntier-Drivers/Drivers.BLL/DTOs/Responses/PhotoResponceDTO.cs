using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.BLL.DTOs.Responses
{
    public class PhotoResponceDTO
    {
        public int Id { get; set; }
        public byte[]? PhotoData { get; set; }
        public string? ContentType { get; set; }
        public string? FileName { get; set; }
        public int? FileSize { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.BLL.DTOs.Requests
{
    public class PhotoReqDTO
    {
        public byte[]? PhotoData { get; set; }
        public string? ContentType { get; set; }
        public string? FileName { get; set; }
        public int? FileSize { get; set; }
    }
}

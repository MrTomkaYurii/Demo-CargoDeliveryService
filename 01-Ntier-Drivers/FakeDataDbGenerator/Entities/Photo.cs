using System;
using System.Collections.Generic;

namespace FakeDataDriverDbGenerator.Entities
{
    public partial class Photo
    {
        public int Id { get; set; }

        public byte[] PhotoData { get; set; }
        public string ContentType { get; set; }
        public string FileName { get; set; }
        public int FileSize { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual Driver? Driver { get; set; }
    }
}

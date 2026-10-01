using System;
using System.Collections.Generic;

namespace FakeDataDriverDbGenerator.Entities
{
    public partial class Repair
    {
        public int Id { get; set; }

        public DateTime RepairDate { get; set; }
        public string? Description { get; set; }
        public decimal Cost { get; set; }


        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public int? TruckId { get; set; }
        public virtual Truck? Truck { get; set; }
    }
}

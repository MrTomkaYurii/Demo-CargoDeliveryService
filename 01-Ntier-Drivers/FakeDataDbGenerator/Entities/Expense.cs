using System;
using System.Collections.Generic;

namespace FakeDataDriverDbGenerator.Entities
{
    public partial class Expense
    {
        public int Id { get; set; }

        public decimal DriverPayment { get; set; }
        public decimal FuelCost { get; set; }
        public decimal MaintenanceCost { get; set; }
        public string Category { get; set; } = null!;
        public DateTime Date { get; set; }
        public string? Note { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public int DriverId { get; set; }
        public int TruckId { get; set; }
        public virtual Driver Driver { get; set; } = null!;
        public virtual Truck Truck { get; set; } = null!;
    }
}

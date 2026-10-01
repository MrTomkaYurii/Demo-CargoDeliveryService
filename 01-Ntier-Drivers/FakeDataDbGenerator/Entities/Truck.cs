using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FakeDataDriverDbGenerator.Entities
{
    public class Truck
    {
        public Truck()
        {
            Expenses = new HashSet<Expense>();
            Inspections = new HashSet<Inspection>();
            Repairs = new HashSet<Repair>();
        }

        public int Id { get; set; }

        public string TruckNumber { get; set; } 
        public string Model { get; set; }
        public int Year { get; set; }
        public int Capacity { get; set; }
        public string? FuelType { get; set; }
        public decimal? FuelConsumption { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? Vin { get; set; }
        public string? EngineNumber { get; set; }
        public string? ChassisNumber { get; set; }
        public string? InsurancePolicyNumber { get; set; }
        public DateTime? InsuranceExpirationDate { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual ICollection<Expense> Expenses { get; set; }
        public virtual ICollection<Inspection> Inspections { get; set; }
        public virtual ICollection<Repair> Repairs { get; set; }
    }
}

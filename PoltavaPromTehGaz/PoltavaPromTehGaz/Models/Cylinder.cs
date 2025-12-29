using System;

namespace PoltavaPromTehGaz.Models
{
    public enum CylinderStatus { Повний, Порожній, В_ремонті, Списаний }
    public enum CylinderLocation { Склад, В_дорозі, У_клієнта, На_заправці, На_обміні }

    public class Cylinder
    {
        public int Id { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
        public string GasType { get; set; } = "Аргон";
        public CylinderStatus Status { get; set; } = CylinderStatus.Повний;
        public CylinderLocation Location { get; set; } = CylinderLocation.Склад;
        public DateTime ManufactureDate { get; set; } = DateTime.Now.AddYears(-1);
        public DateTime LastCheckDate { get; set; } = DateTime.Now;
        public DateTime NextCheckDate { get; set; } = DateTime.Now.AddYears(1);
        public decimal PurchasePrice { get; set; }
        public decimal CurrentValue { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }
    }
}
using System;

namespace PoltavaPromTehGaz.Models
{
    public enum OperationType
    {
        Надходження,
        Продаж,
        Повернення,
        Заправка,
        Обмін,
        Ремонт,
        Списання,
        Переміщення
    }

    public class CylinderOperation
    {
        public int Id { get; set; }
        public int CylinderId { get; set; }
        public Cylinder? Cylinder { get; set; }
        public OperationType OperationType { get; set; }
        public DateTime OperationDate { get; set; } = DateTime.Now;
        public int? EmployeeId { get; set; }
        public Employee? Employee { get; set; }
        public int? ClientId { get; set; }
        public string? ClientName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; } = 1;
        public string? Notes { get; set; }
        public string? DocumentNumber { get; set; }
        public CylinderStatus PreviousStatus { get; set; }
        public CylinderStatus NewStatus { get; set; }
        public CylinderLocation PreviousLocation { get; set; }
        public CylinderLocation NewLocation { get; set; }
    }
}
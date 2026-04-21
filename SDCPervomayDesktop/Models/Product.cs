using System;

namespace SDCPervomayDesktop.Models
{
    public class Product
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public int Quantity { get; set; }
        public string ShelfCode { get; set; }
        public bool HasExpirationDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public bool IsLowStock => Quantity <= 3;
        public bool IsExpired => HasExpirationDate && ExpirationDate.HasValue && ExpirationDate.Value.Date < DateTime.Today;
        public bool IsExpiringSoon =>
            HasExpirationDate &&
            ExpirationDate.HasValue &&
            !IsExpired &&
            (ExpirationDate.Value.Date - DateTime.Today).TotalDays <= 7;
    }
}

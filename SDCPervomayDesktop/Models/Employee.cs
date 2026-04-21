using System;
using System.Collections.ObjectModel;

namespace SDCPervomayDesktop.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Patronymic { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int RoleId { get; set; }
        public decimal Salary { get; set; }
        public byte[] PhotoData { get; set; }
        public ObservableCollection<Achievement> Achievements { get; set; }
        public string FullName => $"{LastName} {FirstName} {Patronymic}".Trim();
    }
}
using SDCPervomayDesktop;
using SDCPervomayDesktop.Entities;
using System.Collections.ObjectModel;
using System.Linq;

namespace SDCPervomayDesktop.ViewModels
{
    public class MenKeysPageViewModel
    {
        private readonly string _lockerGenderCode;

        public ObservableCollection<LockerViewModel> UpperLockers { get; }
        public ObservableCollection<LockerViewModel> LowerLockers { get; }
        public ObservableCollection<ActiveVisitorListItem> ActiveVisitors { get; set; }

        public MenKeysPageViewModel() : this(true)
        {
        }

        public MenKeysPageViewModel(bool isMaleLockerRoom)
        {
            _lockerGenderCode = isMaleLockerRoom ? "М" : "Ж";
            UpperLockers = new ObservableCollection<LockerViewModel>();
            LowerLockers = new ObservableCollection<LockerViewModel>();
            ActiveVisitors = new ObservableCollection<ActiveVisitorListItem>();

            LoadLockersFromDatabase();
        }

        private void LoadLockersFromDatabase()
        {
            var ctx = App.context;

            // Раньше в коде была опечатка «Ш014»; приводим существующую запись к «Ш14», если корректной ещё нет.
            foreach (var legacy in ctx.Lockers.Where(l => l.Number == "Ш014" && l.Gender == _lockerGenderCode).ToList())
            {
                if (!ctx.Lockers.Any(l => l.Number == "Ш14" && l.Gender == _lockerGenderCode))
                    legacy.Number = "Ш14";
            }

            string[] upperNumbers = { "Ш10", "Ш11", "Ш12", "Ш13", "Ш14", "Ш15", "Ш16", "Ш17", "Ш18" };
            string[] lowerNumbers = { "Ш01", "Ш02", "Ш03", "Ш04", "Ш05", "Ш06", "Ш07", "Ш08", "Ш09" };

            foreach (var num in upperNumbers.Concat(lowerNumbers))
                EnsureLockerEntity(ctx, num, _lockerGenderCode);

            ctx.SaveChanges();

            foreach (var num in upperNumbers)
            {
                var entity = ctx.Lockers.First(l => l.Number == num && l.Gender == _lockerGenderCode);
                UpperLockers.Add(new LockerViewModel(entity));
            }

            foreach (var num in lowerNumbers)
            {
                var entity = ctx.Lockers.First(l => l.Number == num && l.Gender == _lockerGenderCode);
                LowerLockers.Add(new LockerViewModel(entity));
            }
        }

        private static void EnsureLockerEntity(SDCPervomayDBEntities ctx, string number, string genderCode)
        {
            if (ctx.Lockers.Any(l => l.Number == number && l.Gender == genderCode))
                return;

            ctx.Lockers.Add(new Lockers
            {
                Number = number,
                Gender = genderCode,
                IsOccupied = false
            });
        }
    }
}

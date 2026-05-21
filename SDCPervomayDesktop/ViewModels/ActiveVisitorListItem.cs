namespace SDCPervomayDesktop.ViewModels
{
    /// <summary>
    /// Элемент списка посетителей для выдачи ключа (ФИО из клиента).
    /// </summary>
    public class ActiveVisitorListItem
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string FullName { get; set; }
    }
}

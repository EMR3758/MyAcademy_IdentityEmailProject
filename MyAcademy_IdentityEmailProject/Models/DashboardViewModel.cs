using MyAcademy_IdentityEmailProject.Entities;

namespace MyAcademy_IdentityEmailProject.Models
{
    public class DashboardViewModel
    {
        public string UserName { get; set; }
        public string UserSurname { get; set; }
        public string UserEmail { get; set; }
        public string? UserImage { get; set; }

        public int TotalInbox { get; set; }
        public int TotalSent { get; set; }
        public int UnreadCount { get; set; }
        public int FavoriteCount { get; set; }
        public int DraftCount { get; set; }
        public int TrashCount { get; set; }

        public List<Message> RecentMessages { get; set; } = new List<Message>();
        public List<Message> RecentSentMessages { get; set; } = new List<Message>();

        public List<DailyTrafficItem> DailyTraffic { get; set; } = new List<DailyTrafficItem>();
        public List<CategoryCountItem> CategoryCounts { get; set; } = new List<CategoryCountItem>();

        public int WeeklySentCount { get; set; }
        public bool EmailConfirmed { get; set; }
    }

    public class DailyTrafficItem
    {
        public DateTime Date { get; set; }
        public int Incoming { get; set; }
        public int Outgoing { get; set; }
    }

    public class CategoryCountItem
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = "";
        public int Count { get; set; }
    }
}

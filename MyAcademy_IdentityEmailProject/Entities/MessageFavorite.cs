namespace MyAcademy_IdentityEmailProject.Entities
{
    public class MessageFavorite
    {
        public int MessageFavoriteId { get; set; }

        public int MessageId { get; set; }
        public Message Message { get; set; }

        public string UserId { get; set; }
        public AppUser User { get; set; }

    }
}

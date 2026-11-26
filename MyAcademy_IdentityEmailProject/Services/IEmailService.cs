namespace MyAcademy_IdentityEmailProject.Services
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string body);
    }
}

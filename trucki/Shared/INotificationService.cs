using AutoMapper.Internal;
using trucki.DTOs;

namespace trucki.Shared
{
    public interface INotificationService
    {
        Task<bool> SendEmailAsync(MailRequest mailRequest);
    }
}

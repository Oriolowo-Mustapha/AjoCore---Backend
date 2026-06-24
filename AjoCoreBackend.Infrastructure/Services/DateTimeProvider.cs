using System;
using AjoCoreBackend.Application.Interfaces.Services;

namespace AjoCoreBackend.Infrastructure.Services
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}

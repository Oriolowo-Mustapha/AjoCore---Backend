using System;
using System.Collections.Generic;
using System.Text;

namespace AjoCoreBackend.Application.Interfaces.Services
{
    public interface IReversalProcessingService
    {
        Task ProcessPendingReversalsAsync(Guid reversalLedgerId);
    }
}

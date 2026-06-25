using System;
using System.Collections.Generic;
using System.Text;

namespace AjoCoreBackend.Domain.Enum
{
    public enum PayoutStatus
    {
        NotDue,
        Pending,
        Settled,
        Failed
    }
}

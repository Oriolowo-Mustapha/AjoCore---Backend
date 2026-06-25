using System;
using System.Collections.Generic;
using System.Text;

namespace AjoCoreBackend.Domain.Enum
{
    public enum ContributionStatus
    {
        Pending,
        FullyPaid,
        UnderPaid,
        OverPaid,
        Late
    }
}

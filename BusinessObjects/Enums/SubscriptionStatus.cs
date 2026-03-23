using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Enums
{
    public enum SubscriptionStatus 
    { 
        PendingPayment = 0,
        Active = 1, 
        Paused = 2, 
        Cancelled = 3,
        Expired = 4
    }
}

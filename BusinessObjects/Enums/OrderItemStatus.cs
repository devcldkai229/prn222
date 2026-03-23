using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Enums
{
    public enum OrderItemStatus
    {
        Planned = 1,
        Preparing = 2,
        Delivering = 3,
        Delivered = 4,
        Cancelled = 5,
    }
}   

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Enums
{
    public enum OrderStatus
    {
        Planned = 1,        // Đã lên kế hoạch
        InProgress = 2,     // (legacy / shipper assigned)
        Delivered = 3,      // Shipper đã giao
        Completed = 4,      // (legacy)
        Cancelled = 5,      // Đã hủy
        Disputed = 6,       // Tranh chấp
        Preparing = 7,      // Nhà bếp đang chuẩn bị
        Delivering = 8,     // Đang trên đường giao
        ConfirmedByUser = 9,// User xác nhận đã nhận hàng
    }
}

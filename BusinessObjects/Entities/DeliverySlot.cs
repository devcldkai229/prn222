using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Entities
{
    public class DeliverySlot
    {
        public int Id { get; set; }

        [Required, StringLength(80)]
        public string Name { get; set; } = "Morning"; // Morning/Evening

        [Range(0, 999)]
        public int Capacity { get; set; } = 100;

        public TimeOnly StartAt { get; set; }

        public TimeOnly EndAt { get; set; }

        public bool IsActive { get; set; } = true;
    }
}

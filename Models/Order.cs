using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace A2.Models
{
    public class Order
    {
        [Key]
        public string UserName { get; set; }
        public int ProductId { get; set; }

    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace A2.Models
{
    public class User
    {
        [Key]
        [Required]
        public string UserName { get; set; }
        public string ? Password { get; set; }
        public string ? Address { get; set; }
    }
}
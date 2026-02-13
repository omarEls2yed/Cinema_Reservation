using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Models
{
    public class UserPaymentMethod
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        [Required]
        public string ProviderToken { get; set; } 

        public string CardBrand { get; set; }    
        public string Last4Digits { get; set; }   
        public bool IsDefault { get; set; }
    }
}

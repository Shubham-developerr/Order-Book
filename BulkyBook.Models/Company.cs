using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Models
{
    public class Company
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        [StringLength(maximumLength:30,MinimumLength =5,ErrorMessage ="Minimum length is 5 and Maxlength 20")]
        public string StreetAddress { get; set; }
        [StringLength(maximumLength:30)]
        public string City { get; set; }
        [StringLength(maximumLength: 30)]
        public string State { get; set; }
        [MaxLength(6, ErrorMessage = "Length should be excat 6")]
        [MinLength(6, ErrorMessage = "Length should be exact 6")]
        public string PostalCode { get; set; }
        [MaxLength(10,ErrorMessage ="Length should be 10")]
        [MinLength(10, ErrorMessage = "Length should be 10")]
        public string PhoneNumber { get; set; }
    }
}

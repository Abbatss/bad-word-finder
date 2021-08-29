using System;
using System.ComponentModel.DataAnnotations;

namespace BWF.Api.Host.Models
{
    public class AddBadWordRequest  
    {
        [Required]
       public string Word { get; set; }
    }
}

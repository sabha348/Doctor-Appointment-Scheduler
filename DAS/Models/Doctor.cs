using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DAS.Models
{
    public class Doctor
    {
        [Key]
        public int DID { get; set; } 

        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Specialist field is required.")]
        public string Specialist { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>(); 
    }
}

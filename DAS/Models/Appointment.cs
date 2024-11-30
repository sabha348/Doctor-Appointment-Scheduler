using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAS.Models
{
    public class Appointment
    {
        [Key]
        public int AID { get; set; } 

        [ForeignKey("Patient")]
        public int PatientID { get; set; }

        [ForeignKey("Doctor")]
        [Required(ErrorMessage = "The Doctor field is required.")] 
        public int DoctorID { get; set; }

        [Required(ErrorMessage = "The Appointment Date is required.")]
        public DateTime AppointmentDate { get; set; }

        public string Reason { get; set; }

        [Required]
        public string Status { get; set; } = "Pending"; 

        public DateTime? AllocatedTime { get; set; }

        public virtual Patient Patient { get; set; }

        public virtual Doctor Doctor { get; set; }
    }
}

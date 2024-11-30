using Microsoft.AspNetCore.Mvc;
using DAS.Data;
using DAS.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using DAS.ViewModels;


namespace DAS.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult CreateAdmin()
        {
            return View(); 
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAdmin(Admin admin)
        {
            if (ModelState.IsValid)
            {
                if (_context.Admins.Any(a => a.Username == admin.Username))
                {
                    ModelState.AddModelError("Username", "Username is already taken.");
                    return View(admin); 
                }

                _context.Admins.Add(admin);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Admin created successfully!";
                return RedirectToAction("AdminDashboard");
            }

            return View(admin);
        }

        public IActionResult Login()
        {
            return View(); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var admin = _context.Admins
                    .FirstOrDefault(a => a.Username == model.Username && a.Password == model.Password);

                if (admin != null)
                {
                    HttpContext.Session.SetInt32("AdminID", admin.Id);

                    return RedirectToAction("AdminDashboard");
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model); 
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Doctors()
        {
            var doctors = _context.Doctors.ToList();
            return View(doctors); 
        }

        
        public IActionResult DeleteDoctor(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = _context.Doctors.FirstOrDefault(d => d.DID == id);
            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor); 
        }

        
        [HttpPost, ActionName("DeleteDoctorConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor != null)
            {
                _context.Doctors.Remove(doctor);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Doctors)); 
        }

       
        public IActionResult Appointments()
        {
            var appointments = _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .ToList();
            return View(appointments); 
        }

        
        public IActionResult SearchAppointments(int? AID, string DoctorName, string PatientName)
        {
            var query = _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .AsQueryable();

            if (AID.HasValue)
            {
                query = query.Where(a => a.AID == AID.Value);
            }

            if (!string.IsNullOrEmpty(DoctorName))
            {
                query = query.Where(a => a.Doctor.Name.Contains(DoctorName));
            }

            if (!string.IsNullOrEmpty(PatientName))
            {
                query = query.Where(a => a.Patient.Name.Contains(PatientName));
            }

            var appointments = query.ToList();

            return View(appointments); 
        }



        
        public IActionResult Patients()
        {
            var patients = _context.Patients.ToList(); 
            return View(patients); 
        }

        
        public IActionResult DeletePatient(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = _context.Patients.FirstOrDefault(d => d.PID == id);
            if (patient == null)
            {
                return NotFound();
            }

            return View(patient); 
        }

        
        [HttpPost, ActionName("DeletePatientConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePatientConfirmed(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient != null)
            {
                _context.Patients.Remove(patient);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Patients)); 
        }




        
        public IActionResult AdminDashboard()
        {
            var model = new AdminDashboardViewModel
            {
                TotalPatients = _context.Patients.Count(),
                TotalDoctors = _context.Doctors.Count(),
                TotalAppointments = _context.Appointments.Count(),
            };

            return View(model); 
        }

    }
}



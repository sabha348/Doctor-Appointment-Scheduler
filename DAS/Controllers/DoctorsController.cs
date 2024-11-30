using Microsoft.AspNetCore.Mvc;
using DAS.Data;
using DAS.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DAS.ViewModels;
using System.Diagnostics;

namespace DAS.Controllers
{
    public class DoctorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DoctorController(ApplicationDbContext context)
        {
            _context = context;
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
                var doctor = _context.Doctors
                    .FirstOrDefault(d => d.Username == model.Username && d.Password == model.Password);

                if (doctor != null)
                {
                    HttpContext.Session.SetInt32("DoctorID", doctor.DID);

                    return RedirectToAction("DoctorDashboard");
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model); 
        }

        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult DoctorDashboard()
        {
            HttpContext.Session.SetString("UserName", "Khanjan");
            return View();
        }

        public IActionResult CreateDoctor()
        {
            return View(); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDoctor(Doctor doctor)
        {
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine("hello"); 
            }

            _context.Add(doctor);
            await _context.SaveChangesAsync();
            return RedirectToAction("DoctorDashboard", "Doctor");
            return View(doctor); 
        }

        public async Task<IActionResult> MyAppointments()
        {
            var doctorId = HttpContext.Session.GetInt32("DoctorID");

            if (doctorId == null)
            {
                return RedirectToAction("Login");
            }

            var appointments = await _context.Appointments
                .Include(a => a.Patient) 
                .Where(a => a.DoctorID == doctorId.Value)
                .ToListAsync();

            return View(appointments); 
        }

        public async Task<IActionResult> ManageProfile()
        {
            var doctorId = HttpContext.Session.GetInt32("DoctorID");

            if (doctorId == null)
            {
                return RedirectToAction("Login");
            }

            var doctor = await _context.Doctors.FindAsync(doctorId);

            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageProfile(Doctor doctor)
        {
            var doctorId = HttpContext.Session.GetInt32("DoctorID");

            if (doctorId == null)
            {
                return RedirectToAction("Login");
            }

            var existingDoctor = await _context.Doctors.FindAsync(doctorId);

            if (existingDoctor == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    existingDoctor.Name = doctor.Name;
                    existingDoctor.Gender = doctor.Gender;
                    existingDoctor.Email = doctor.Email;
                    existingDoctor.Specialist = doctor.Specialist;

                    if (!string.IsNullOrWhiteSpace(doctor.Password))
                    {
                        existingDoctor.Password = doctor.Password;
                    }

                    existingDoctor.Username = doctor.Username;

                    _context.Update(existingDoctor);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Profile updated successfully!";
                    return RedirectToAction("DoctorDashboard");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Doctors.Any(d => d.DID == existingDoctor.DID))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine(error.ErrorMessage);
                }
            }

            return View(doctor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProfile()
        {
            var doctorId = HttpContext.Session.GetInt32("DoctorID");

            if (doctorId == null)
            {
                return RedirectToAction("Login");
            }

            var doctor = await _context.Doctors.FindAsync(doctorId);

            if (doctor == null)
            {
                return NotFound();
            }

            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();
            HttpContext.Session.Clear(); 

            return RedirectToAction("Index", "Home"); 
        }

        public async Task<IActionResult> EditAppointment(int id)
        {

            var appointment = await _context.Appointments
                .Include(a => a.Patient) 
                .Include(a => a.Doctor)  
                .FirstOrDefaultAsync(a => a.AID == id);

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment); 
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAppointmentConfirm(Appointment appointment)
        {
            Debug.WriteLine(appointment.AID);

            
            try
            {
                var existingAppointment = await _context.Appointments.FindAsync(appointment.AID);
                if (existingAppointment == null)
                {
                    Debug.WriteLine(appointment.AppointmentDate);
                    return NotFound();
                }

                existingAppointment.AllocatedTime = appointment.AllocatedTime;
                existingAppointment.Status = appointment.Status;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Appointment updated successfully!";
                return RedirectToAction(nameof(MyAppointments));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AppointmentExists(appointment.AID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            

            return View(appointment);
        }



        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.AID == id);
        }


        
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }
    }
}




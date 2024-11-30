using Microsoft.AspNetCore.Mvc;
using DAS.Data;
using DAS.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DAS.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;

namespace DAS.Controllers
{
    public class PatientController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PatientController(ApplicationDbContext context)
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
                var patient = _context.Patients
                    .FirstOrDefault(p => p.Username == model.Username && p.Password == model.Password);

                if (patient != null)
                {
                    HttpContext.Session.SetInt32("PatientID", patient.PID);

                    return RedirectToAction("PatientDashboard");
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }


        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult PatientDashboard()
        {
            return View();
        }


        public IActionResult CreatePatient()
        {
            return View(); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePatient([Bind("Name,Gender,Username,Password,Email")] Patient patient)
        {
            Console.WriteLine("CreatePatient function called.");

            if (ModelState.IsValid)
            {
                try
                {
                    Console.WriteLine("Model is valid. Proceeding to save patient."); 

                    _context.Add(patient);
                    await _context.SaveChangesAsync();

                    Console.WriteLine("Patient saved successfully!"); 

                    TempData["SuccessMessage"] = "Patient created successfully!";
                    return RedirectToAction(nameof(CreatePatient));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error saving patient: " + ex.Message); 
                    ModelState.AddModelError("", "Unable to save changes: " + ex.Message);
                }
            }
            else
            {
                Console.WriteLine("Model is not valid. Validation errors present."); 
            }

            return View(patient); 
        }


        public async Task<IActionResult> BookAppointment()
        {
            Debug.WriteLine("This is a debug message.");
            ViewBag.Doctors = await _context.Doctors.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookAppointment(Appointment appointment)
        {
            var todaysDate = DateTime.Today;
            var paramDate = appointment.AppointmentDate;
            if (paramDate < todaysDate)
            {
                return View("BookAppointment.cshtml");
            }
            Debug.WriteLine(appointment.AppointmentDate);

            var patientId = HttpContext.Session.GetInt32("PatientID");

            if (patientId == null)
            {
                return RedirectToAction("Login", "Patient");
            }

            appointment.PatientID = patientId.Value;  
            appointment.Status = "Pending";           
            appointment.AllocatedTime = null;         

            _context.Add(appointment);               
            await _context.SaveChangesAsync();        

            TempData["SuccessMessage"] = "Appointment booked successfully!";
            return RedirectToAction(nameof(MyAppointments));  
            ViewBag.Doctors = await _context.Doctors.ToListAsync();
            return View(appointment);
        }







        // GET: Patient/MyAppointments
        public async Task<IActionResult> MyAppointments()
        {
            // Get the PatientID from the session
            var patientId = HttpContext.Session.GetInt32("PatientID");

            if (patientId == null)
            {
                // If patient is not logged in or session has expired, redirect to login
                return RedirectToAction("Login", "Patient");
            }

            // Retrieve appointments for the logged-in patient and include Doctor and Patient information
            var appointments = await _context.Appointments
                .Include(a => a.Doctor)   // Include Doctor details
                .Include(a => a.Patient)  // Include Patient details (if necessary)
                .Where(a => a.PatientID == patientId.Value)
                .ToListAsync();

            return View(appointments);  // Pass the appointments to the view
        }


        // GET: Patient/ManageProfile
        public async Task<IActionResult> ManageProfile()
        {
            var patientId = HttpContext.Session.GetInt32("PatientID");

            if (patientId == null)
            {
                // If the patient is not logged in, redirect to login
                return RedirectToAction("Login");
            }

            // Fetch the patient details from the database
            var patient = await _context.Patients.FindAsync(patientId);

            if (patient == null)
            {
                return NotFound();
            }

            return View(patient); // Pass the patient object to the view
        }

        // POST: Patient/ManageProfile (Update profile)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageProfile(Patient patient)
        {
            var patientId = HttpContext.Session.GetInt32("PatientID");

            if (patientId == null)
            {
                return RedirectToAction("Login");
            }

            // Fetch the existing patient details from the database
            var existingPatient = await _context.Patients.FindAsync(patientId);

            if (existingPatient == null)
            {
                return NotFound();
            }

            // Update only the editable fields
            if (ModelState.IsValid)
            {
                try
                {
                    // Update the existing patient record with the values from the form
                    existingPatient.Name = patient.Name;
                    existingPatient.Gender = patient.Gender;
                    existingPatient.Email = patient.Email;

                    // Only update the password if the user provided a new one
                    if (!string.IsNullOrWhiteSpace(patient.Password))
                    {
                        existingPatient.Password = patient.Password;
                    }

                     existingPatient.Username = patient.Username;

                    // Save the updated patient details to the database
                    _context.Update(existingPatient);
                    await _context.SaveChangesAsync();

                    // Set success message and redirect to Patient Dashboard
                    TempData["SuccessMessage"] = "Profile updated successfully!";
                    return RedirectToAction("PatientDashboard");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Patients.Any(p => p.PID == existingPatient.PID))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }

            // If the model state is invalid, return the same view with the current data
            return View(patient);
        }



        // POST: Patient/DeleteProfile (Delete patient account)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProfile()
        {
            var patientId = HttpContext.Session.GetInt32("PatientID");

            if (patientId == null)
            {
                return RedirectToAction("Login");
            }

            var patient = await _context.Patients.FindAsync(patientId);

            if (patient == null)
            {
                return NotFound();
            }

            _context.Patients.Remove(patient); // Remove the patient record
            await _context.SaveChangesAsync();
            HttpContext.Session.Clear(); // Clear session on account deletion

            return RedirectToAction("Index", "Home"); // Redirect to homepage after deleting the account
        }

        public IActionResult Logout()
        {
            // Clear session on logout
            HttpContext.Session.Clear();

            // Redirect to the Home page after logout
            return RedirectToAction("Index", "Home");
        }

    }
}

document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('appointment-form');
    const submitBtn = document.querySelector('.submit-btn');

    // Real-time validation feedback
    form.addEventListener('input', function (event) {
        if (event.target.classList.contains('form-control')) {
            event.target.classList.remove('invalid-input');
        }
    });

    // Highlight empty fields on submit
    form.addEventListener('submit', function (event) {
        let isValid = true;
        const inputs = form.querySelectorAll('.form-control');

        inputs.forEach(input => {
            if (input.value.trim() === '') {
                input.classList.add('invalid-input');
                isValid = false;
            }
        });

        if (!isValid) {
            event.preventDefault();
            alert('Please fill in all the required fields');
        }
    });

    // Appointment tooltip for guidance (optional)
    const appointmentInput = document.querySelector('input[asp-for="AppointmentDate"]');
    appointmentInput.addEventListener('focus', function () {
        appointmentInput.setAttribute('title', 'Select a convenient date and time for your appointment.');
    });
});
// User Add Modal AJAX Functionality
document.addEventListener('DOMContentLoaded', function() {
    const createUserForm = document.getElementById('createuserform');
    
    // Profile picture preview functionality
    const input = document.getElementById('profilePictureInput');
    const preview = document.getElementById('profilePreview');
    const plus = document.getElementById('plusSign');
    const previewName = document.getElementById('previewName');
    const firstNameInput = document.getElementById('firstName');
    const lastNameInput = document.getElementById('lastName');

    if (input) {
        input.addEventListener('change', function () {
            const file = this.files[0];
            if (file) {
                const reader = new FileReader();
                reader.onload = function (e) {
                    preview.src = e.target.result;
                    preview.classList.remove('hidden');
                    plus.classList.add('hidden');
                };
                reader.readAsDataURL(file);
            }
        });
    }

    // Name preview functionality
    if (firstNameInput) {
        firstNameInput.addEventListener('input', updatePreviewName);
    }
    if (lastNameInput) {
        lastNameInput.addEventListener('input', updatePreviewName);
    }
    
    if (createUserForm) {
        createUserForm.addEventListener('submit', function(e) {
            e.preventDefault();
            
            // Trigger client-side validation
            if (!$(this).valid()) {
                return;
            }
            
            const formData = new FormData(this);
            
            // Show loading state
            const submitButton = this.querySelector('button[type="submit"]');
            const originalText = submitButton.textContent;
            submitButton.textContent = 'Adding User...';
            submitButton.disabled = true;
            
            fetch('/Admin/CreateUser', {
                method: 'POST',
                body: formData
            })
            .then(response => {
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                }
                return response.text();
            })
            .then(data => {
                // Close modal
                closeModal();
                
                // Show success message
                if (typeof toastr !== 'undefined') {
                    toastr.success('User created successfully!');
                } else {
                    alert('User created successfully!');
                }
                
                // Reload the page to show updated user list
                setTimeout(() => {
                    window.location.reload();
                }, 1000);
            })
            .catch(error => {
                console.error('Error creating user:', error);
                
                if (typeof toastr !== 'undefined') {
                    toastr.error('Failed to create user. Please try again.');
                } else {
                    alert('Failed to create user. Please try again.');
                }
            })
            .finally(() => {
                // Reset button state
                submitButton.textContent = originalText;
                submitButton.disabled = false;
            });
        });
    }
});

// Modal control functions
function closeModal() {
    const modal = document.getElementById('addUserModal');
    if (modal) {
        modal.classList.add('hidden');
    }
}

function openAddUserModal() {
    const modal = document.getElementById('addUserModal');
    if (modal) {
        modal.classList.remove('hidden');
    }
}

function updatePreviewName() {
    const firstName = document.getElementById('firstName')?.value || '';
    const lastName = document.getElementById('lastName')?.value || '';
    const fullName = [firstName, lastName].filter(Boolean).join(' ') || 'New User';
    const previewNameElement = document.getElementById('previewName');
    if (previewNameElement) {
        previewNameElement.textContent = fullName.toUpperCase();
    }
}

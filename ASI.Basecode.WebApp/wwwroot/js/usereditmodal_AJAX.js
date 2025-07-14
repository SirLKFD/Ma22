// User Edit Modal AJAX Functionality
document.addEventListener('DOMContentLoaded', function () {
    // Set max date for birthdate input (must be at least 13 years ago)
    const birthdateInput = document.getElementById('editBirthdate');
    if (birthdateInput) {
        const today = new Date();
        const maxDate = new Date(today.getFullYear() - 13, today.getMonth(), today.getDate());
        const yyyy = maxDate.getFullYear();
        const mm = String(maxDate.getMonth() + 1).padStart(2, '0');
        const dd = String(maxDate.getDate()).padStart(2, '0');
        const formattedMaxDate = `${yyyy}-${mm}-${dd}`;
        birthdateInput.max = formattedMaxDate;
        // Optionally set default value to max date
        // birthdateInput.value = formattedMaxDate;
    }
    const editUserForm = document.getElementById('editUserForm');

    // Profile picture preview for edit modal
    const editProfilePictureInput = document.getElementById('editProfilePictureInput');
    if (editProfilePictureInput) {
        editProfilePictureInput.addEventListener('change', function (e) {
            const file = e.target.files[0];
            if (file) {
                const reader = new FileReader();
                reader.onload = function (e) {
                    const img = document.getElementById('editProfileImage');
                    const placeholder = document.getElementById('editProfilePlaceholder');
                    const container = document.getElementById('editProfileImageContainer');

                    img.src = e.target.result;
                    img.classList.remove('hidden');
                    placeholder.classList.add('hidden');
                    container.style.background = 'transparent';
                };
                reader.readAsDataURL(file);
            }
        });
    }

    if (editUserForm) {
        editUserForm.addEventListener('submit', function (e) {
            // Birthdate validation: must be at least 13 years old
            const birthdateInput = document.getElementById('editBirthdate');
            const birthdateErrorId = 'editBirthdateError';
            let birthdateError = document.getElementById(birthdateErrorId);
            if (!birthdateError) {
                birthdateError = document.createElement('span');
                birthdateError.id = birthdateErrorId;
                birthdateError.className = 'block text-sm text-[#EAC231]';
                birthdateInput.parentNode.parentNode.appendChild(birthdateError);
            }
            birthdateError.textContent = '';
            birthdateError.style.display = 'none';

            if (birthdateInput && birthdateInput.value) {
                const birthDate = new Date(birthdateInput.value);
                const today = new Date();
                const minDate = new Date(today.getFullYear() - 13, today.getMonth(), today.getDate());
                if (birthDate > minDate) {
                    birthdateError.textContent = 'User must be at least 13 years old.';
                    birthdateError.style.display = 'block';
                    birthdateInput.focus();
                    e.preventDefault();
                    return;
                }
            }

            e.preventDefault();

            const formData = new FormData(this);

            // Show loading state
            const submitButton = this.querySelector('button[type="submit"]');
            const originalText = submitButton.textContent;
            submitButton.textContent = 'Saving Changes...';
            submitButton.disabled = true;

            fetch('/Admin/UpdateUser', {
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
                    closeEditDetails();

                    // Show success message
                    if (typeof toastr !== 'undefined') {
                        toastr.success('User updated successfully!');
                    } else {
                        alert('User updated successfully!');
                    }

                    // Reload the page to show updated user list
                    setTimeout(() => {
                        window.location.reload();
                    }, 1000);
                })
                .catch(error => {
                    console.error('Error updating user:', error);

                    if (typeof toastr !== 'undefined') {
                        toastr.error('Failed to update user. Please try again.');
                    } else {
                        alert('Failed to update user. Please try again.');
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
function closeEditDetails() {
    const modal = document.getElementById('editUserModal');
    if (modal) {
        modal.classList.add('hidden');
    }
}

function openEditUserModal(userId, firstName, lastName, birthdate, contact, email, role, profilePicture) {
    const modal = document.getElementById('editUserModal');
    if (modal) {
        // Populate form fields
        document.getElementById('editUserIdInput').value = userId;
        document.getElementById('editFirstName').value = firstName || '';
        document.getElementById('editLastName').value = lastName || '';
        document.getElementById('editBirthdate').value = birthdate || '';
        document.getElementById('editContact').value = contact || '';
        document.getElementById('editEmail').value = email || '';
        document.getElementById('editRole').value = role || '1';
        document.getElementById('editExistingProfilePicture').value = profilePicture || '';

        // Update display fields
        document.getElementById('editUserName').textContent = `${firstName} ${lastName}`.toUpperCase();
        document.getElementById('editUserId').textContent = `ID: ${userId}`;
        document.getElementById('editUserEmail').textContent = email;

        // Handle profile picture display
        const img = document.getElementById('editProfileImage');
        const placeholder = document.getElementById('editProfilePlaceholder');
        const container = document.getElementById('editProfileImageContainer');

        if (profilePicture) {
            img.src = profilePicture;
            img.classList.remove('hidden');
            placeholder.classList.add('hidden');
            container.style.background = 'transparent';
        } else {
            img.classList.add('hidden');
            placeholder.classList.remove('hidden');
            container.style.background = '#FFE9C6';
        }

        // Show modal
        modal.classList.remove('hidden');
    }
}
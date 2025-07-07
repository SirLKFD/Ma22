// User Edit Modal AJAX Functionality
document.addEventListener('DOMContentLoaded', function() {
    const editUserForm = document.getElementById('editUserForm');
    
    // Profile picture preview for edit modal
    const editProfilePictureInput = document.getElementById('editProfilePictureInput');
    if (editProfilePictureInput) {
        editProfilePictureInput.addEventListener('change', function(e) {
            const file = e.target.files[0];
            if (file) {
                const reader = new FileReader();
                reader.onload = function(e) {
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
        editUserForm.addEventListener('submit', function(e) {
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

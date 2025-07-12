// UserMaster AJAX Functionality
document.addEventListener("DOMContentLoaded", function () {
  // Modal control functions
  function openModal() {
    const modal = document.getElementById("addUserModal");
    if (modal) modal.classList.remove("hidden");
  }

  function closeModal() {
    const modal = document.getElementById("addUserModal");
    if (modal) modal.classList.add("hidden");

    const form = document.getElementById("createuserform");
    if (form) {
      form.reset();
      const validator = $(form).validate();
      if (validator) {
        validator.resetForm();
        $(form)
          .find(".input-validation-error")
          .removeClass("input-validation-error");
      }
    }
  }

  function editDetails(userId) {
    // Fetch the edit modal HTML and user data, then inject and populate
    fetch(`/Admin/GetUserEditModal?userId=${userId}`)
      .then((response) => {
        if (!response.ok) throw new Error("Unable to fetch edit modal.");
        return response.text();
      })
      .then((html) => {
        const container = document.getElementById("editUserModalContainer");
        if (container) container.innerHTML = html;
        // After injecting, fetch user data and populate fields
        fetch(`/Admin/GetUserForEdit?userId=${userId}`)
          .then((response) => {
            if (!response.ok) throw new Error("Unable to fetch user data.");
            return response.json();
          })
          .then((user) => {
            // Populate the edit modal with user data
            const editUserIdInput = document.getElementById("editUserIdInput");
            const editFirstName = document.getElementById("editFirstName");
            const editLastName = document.getElementById("editLastName");
            const editEmail = document.getElementById("editEmail");
            const editContact = document.getElementById("editContact");
            const editRole = document.getElementById("editRole");
            const editBirthdate = document.getElementById("editBirthdate");

            if (editUserIdInput) editUserIdInput.value = user.id;
            if (editFirstName) editFirstName.value = user.firstName;
            if (editLastName) editLastName.value = user.lastName;
            if (editEmail) editEmail.value = user.emailId;
            if (editContact) editContact.value = user.contact || "";
            if (editRole) editRole.value = user.role;

            // Handle birthdate
            if (editBirthdate && user.birthdate) {
              const birthdate = new Date(user.birthdate);
              editBirthdate.value = birthdate.toISOString().split("T")[0];
            } else if (editBirthdate) {
              editBirthdate.value = "";
            }

            // Handle profile picture
            const profileImage = document.getElementById("editProfileImage");
            const placeholder = document.getElementById(
              "editProfilePlaceholder"
            );
            const container = document.getElementById(
              "editProfileImageContainer"
            );
            const existingProfilePictureInput = document.getElementById(
              "editExistingProfilePicture"
            );

            if (user.profilePicture) {
              if (profileImage) {
                profileImage.src = user.profilePicture;
                profileImage.classList.remove("hidden");
              }
              if (placeholder) placeholder.classList.add("hidden");
              if (container) container.style.background = "transparent";
              if (existingProfilePictureInput)
                existingProfilePictureInput.value = user.profilePicture;
            } else {
              if (profileImage) profileImage.classList.add("hidden");
              if (placeholder) placeholder.classList.remove("hidden");
              if (container) container.style.background = "#FFE9C6";
              if (existingProfilePictureInput)
                existingProfilePictureInput.value = "";
            }

            // Update user info display
            const editUserName = document.getElementById("editUserName");
            const editUserId = document.getElementById("editUserId");
            const editUserEmail = document.getElementById("editUserEmail");
            const editUserCreated = document.getElementById("editUserCreated");

            if (editUserName)
              editUserName.textContent = `${user.firstName} ${user.lastName}`;
            if (editUserId) editUserId.textContent = `ID: ${user.id}`;
            if (editUserEmail) editUserEmail.textContent = user.emailId;

            if (editUserCreated && user.createdTime) {
              const createdDate = new Date(user.createdTime);
              editUserCreated.textContent = `Account Created: ${createdDate.toLocaleDateString()}`;
            } else if (editUserCreated) {
              editUserCreated.textContent = "Account Created: N/A";
            }

            // Show the edit modal
            const editModal = document.getElementById("editUserModal");
            if (editModal) editModal.classList.remove("hidden");
          })
          .catch((error) => {
            alert(error.message);
          });
      })
      .catch((error) => {
        alert(error.message);
      });
  }

  function closeEditDetails() {
    const modal = document.getElementById("editUserModal");
    if (modal) modal.classList.add("hidden");

    // Reset the edit form
    const form = document.getElementById("editUserForm");
    if (form) {
      form.reset();
    }

    // Reset profile image
    const profileImage = document.getElementById("editProfileImage");
    const placeholder = document.getElementById("editProfilePlaceholder");
    const container = document.getElementById("editProfileImageContainer");
    const existingProfilePictureInput = document.getElementById(
      "editExistingProfilePicture"
    );

    if (profileImage) profileImage.classList.add("hidden");
    if (placeholder) placeholder.classList.remove("hidden");
    if (container) container.style.background = "#FFE9C6";
    if (existingProfilePictureInput) existingProfilePictureInput.value = "";
  }

  function closeViewDetails() {
    const modal = document.getElementById("viewUserModal");
    if (modal) {
      modal.remove();
    }
  }

  function viewDetails(id) {
    fetch(`/Admin/ViewUserDetails?userId=${id}`)
      .then((response) => {
        if (!response.ok) throw new Error("Unable to fetch user.");

        return response.text();
      })
      .then((html) => {
        const container = document.getElementById("viewUserModalContainer");
        if (container) container.innerHTML = html;
      })
      .catch((error) => {
        alert(error.message);
      });
  }

  function deleteProfile(userId, userName, profilePicUrl) {
    // Show modal
    const deleteModal = document.getElementById("deleteUserModal");
    if (deleteModal) deleteModal.classList.remove("hidden");

    // Set user info in modal
    const deleteUserName = document.getElementById("deleteUserName");
    const deleteUserId = document.getElementById("deleteUserId");
    if (deleteUserName) deleteUserName.textContent = userName;
    if (deleteUserId) deleteUserId.textContent = `ID: ${userId}`;

    // Set profile picture or emoji
    const profileImg = document.getElementById("deleteUserProfilePic");
    const profileEmoji = document.getElementById("deleteUserProfileEmoji");
    const profileContainer = profileImg?.parentElement;

    if (profilePicUrl && profilePicUrl.trim() !== "") {
      if (profileImg) {
        profileImg.src = profilePicUrl;
        profileImg.classList.remove("hidden");
      }
      if (profileEmoji) profileEmoji.classList.add("hidden");
      if (profileContainer) profileContainer.style.background = "transparent";
    } else {
      if (profileImg) {
        profileImg.src = "";
        profileImg.classList.add("hidden");
      }
      if (profileEmoji) profileEmoji.classList.remove("hidden");
      if (profileContainer) profileContainer.style.background = "#FFE9C6";
    }

    // Remove previous event listeners
    const confirmBtn = document.getElementById("confirmDeleteBtn");
    if (confirmBtn) {
      const newBtn = confirmBtn.cloneNode(true);
      confirmBtn.parentNode.replaceChild(newBtn, confirmBtn);
      // Add new event listener
      newBtn.addEventListener("click", function () {
        handleDeleteUser(userId);
      });
    }
  }

  function closeDeleteProfile() {
    const modal = document.getElementById("deleteUserModal");
    if (modal) modal.classList.add("hidden");
  }

  function handleDeleteUser(userId) {
    fetch(`/Admin/DeleteUser`, {
      method: "POST",
      headers: {
        "Content-Type": "application/x-www-form-urlencoded",
        RequestVerificationToken:
          document.querySelector('input[name="__RequestVerificationToken"]')
            ?.value || "",
      },
      body: `id=${userId}`,
    })
      .then((response) => {
        if (response.redirected) {
          window.location.href = response.url;
        } else if (response.ok) {
          window.location.reload();
        } else {
          alert("Failed to delete user.");
        }
      })
      .catch(() => {
        alert("Failed to delete user.");
      });
  }

  // Attach AJAX submit handler for the edit user form
  document.addEventListener("submit", function (e) {
    var form = e.target;
    if (form && form.id === "editUserForm") {
      e.preventDefault();

      var formData = new FormData(form);

      fetch(form.action, {
        method: "POST",
        body: formData,
      })
        .then((response) => response.json())
        .then((data) => {
          if (data.success) {
            // Close the modal
            closeEditDetails();
            // Optionally show a success toast
            if (window.toastr)
              toastr.success(data.message || "User updated successfully!");
            window.location.reload();
          } else {
            // Show error message
            if (window.toastr) toastr.error(data.message || "Update failed.");
          }
        })
        .catch(() => {
          if (window.toastr)
            toastr.error("An error occurred while updating the user.");
        });
    }
  });

  // Make functions globally available
  window.openModal = openModal;
  window.closeModal = closeModal;
  window.editDetails = editDetails;
  window.closeEditDetails = closeEditDetails;
  window.closeViewDetails = closeViewDetails;
  window.viewDetails = viewDetails;
  window.deleteProfile = deleteProfile;
  window.closeDeleteProfile = closeDeleteProfile;
  window.handleDeleteUser = handleDeleteUser;
});

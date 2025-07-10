// mediaModal.js - reusable modal navigation for videos/images
(function(window, document) {
    let videoList = [];
    let imageList = [];
    let currentMediaType = '';
    let currentMediaIndex = 0;

    function setMediaLists(videos, images) {
        videoList = videos || [];
        imageList = images || [];
    }

    function openMediaModalWithNavigation(type, index) {
        currentMediaType = type;
        currentMediaIndex = index;

        const modal = document.getElementById('mediaModal');
        renderMediaContent();
        updateNavigationButtons();
        modal.classList.remove('hidden');
        modal.classList.add('flex');
    }

    function renderMediaContent() {
        const content = document.getElementById('modalContent');
        let mediaArray = currentMediaType === 'video' ? videoList : imageList;
        let media = mediaArray[currentMediaIndex];

        if (currentMediaType === 'video') {
            content.innerHTML = `
                <div class="relative w-full">
                    <video controls autoplay class="max-h-[70vh] max-w-[80vw] rounded">
                        <source src='${media.url}' type='${media.type}'>
                        Your browser does not support the video tag.
                    </video>
                    <div class="absolute bottom-4 left-0 right-0 flex justify-center">
                        <div class="rounded bg-black/70 px-3 py-1 text-white">
                            ${currentMediaIndex + 1} of ${mediaArray.length}
                        </div>
                    </div>
                </div>`;
        } else if (currentMediaType === 'image') {
            content.innerHTML = `
                <div class="relative">
                    <img src='${media.url}' alt='${media.name}' class='max-h-[80vh] max-w-[90vw] rounded shadow-lg' />
                    <div class="absolute bottom-4 left-0 right-0 flex justify-center">
                        <div class="rounded bg-black/70 px-3 py-1 text-white">
                            ${currentMediaIndex + 1} of ${mediaArray.length}
                        </div>
                    </div>
                </div>`;
        }
    }

    function updateNavigationButtons() {
        const prevBtn = document.getElementById('mediaPrevBtn');
        const nextBtn = document.getElementById('mediaNextBtn');
        let mediaArray = currentMediaType === 'video' ? videoList : imageList;

        if (mediaArray.length > 1) {
            prevBtn.classList.remove('hidden');
            nextBtn.classList.remove('hidden');

            if (currentMediaIndex === 0) {
                prevBtn.style.opacity = '0.5';
                prevBtn.style.cursor = 'not-allowed';
            } else {
                prevBtn.style.opacity = '1';
                prevBtn.style.cursor = 'pointer';
            }

            if (currentMediaIndex === mediaArray.length - 1) {
                nextBtn.style.opacity = '0.5';
                nextBtn.style.cursor = 'not-allowed';
            } else {
                nextBtn.style.opacity = '1';
                nextBtn.style.cursor = 'pointer';
            }
        } else {
            prevBtn.classList.add('hidden');
            nextBtn.classList.add('hidden');
        }
    }

    function navigateMedia(direction) {
        let mediaArray = currentMediaType === 'video' ? videoList : imageList;

        if (direction === -1 && currentMediaIndex > 0) {
            currentMediaIndex--;
            renderMediaContent();
            updateNavigationButtons();
        } else if (direction === 1 && currentMediaIndex < mediaArray.length - 1) {
            currentMediaIndex++;
            renderMediaContent();
            updateNavigationButtons();
        }
    }

    function closeMediaModal() {
        const modal = document.getElementById('mediaModal');
        modal.classList.add('hidden');
        modal.classList.remove('flex');
        document.getElementById('modalContent').innerHTML = '';
    }

    // Keyboard navigation
    document.addEventListener('keydown', function(e) {
        const modal = document.getElementById('mediaModal');
        if (!modal || modal.classList.contains('hidden')) return;

        if (e.key === 'ArrowLeft') {
            e.preventDefault();
            navigateMedia(-1);
        } else if (e.key === 'ArrowRight') {
            e.preventDefault();
            navigateMedia(1);
        } else if (e.key === 'Escape') {
            e.preventDefault();
            closeMediaModal();
        }
    });

    // Close modal when clicking outside
    document.addEventListener('DOMContentLoaded', function() {
        const modal = document.getElementById('mediaModal');
        if (modal) {
            modal.addEventListener('click', function(e) {
                if (e.target === this) closeMediaModal();
            });
        }
    });

    // Add document modal logic
    function openDocumentModal(url, mediaType, isPdf) {
        const modal = document.getElementById('mediaModal');
        const content = document.getElementById('modalContent');
        const prevBtn = document.getElementById('mediaPrevBtn');
        const nextBtn = document.getElementById('mediaNextBtn');
        // Hide navigation buttons
        if (prevBtn) prevBtn.classList.add('hidden');
        if (nextBtn) nextBtn.classList.add('hidden');
        if (isPdf === true || isPdf === 'true') {
            content.innerHTML = `<iframe src='${url}' class='h-[90vh] w-[60vw] rounded' frameborder='0'></iframe>`;
        } else {
            content.innerHTML = `
                <div class='flex flex-col items-center space-y-4 text-white'>
                    <span class='material-symbols-outlined text-6xl'>description</span>
                    <p class='text-lg'>This document cannot be previewed in the browser.</p>
                    <p class='text-sm text-gray-300'>Only PDF files can be previewed directly.</p>
                    <a href='${url}' target='_blank' class='rounded bg-blue-600 px-4 py-2 text-white hover:bg-blue-700'>Download to View</a>
                </div>`;
        }
        modal.classList.remove('hidden');
        modal.classList.add('flex');
    }

    // Add download modal logic
    function openDownloadModal() {
        var modal = document.getElementById('downloadModal');
        if (modal) {
            modal.classList.remove('hidden');
            modal.classList.add('flex');
        }
    }
    function closeDownloadModal() {
        var modal = document.getElementById('downloadModal');
        if (modal) {
            modal.classList.add('hidden');
            modal.classList.remove('flex');
        }
        // Reset form
        var allRadio = document.querySelector('input[name="downloadOption"][value="all"]');
        if (allRadio) allRadio.checked = true;
        var selectedContentList = document.getElementById('selectedContentList');
        if (selectedContentList) selectedContentList.classList.add('hidden');
        var mediaTypeOptions = document.getElementById('mediaTypeOptions');
        if (mediaTypeOptions) mediaTypeOptions.classList.add('hidden');
        var checkboxes = document.querySelectorAll('input[name="selectedMedia"]');
        checkboxes.forEach(function(cb) { cb.checked = false; });
    }

    // Download processing logic
    function processDownload() {
        const selectedOption = document.querySelector('input[name="downloadOption"]:checked').value;
        const topicIdElem = document.querySelector('[name="topicId"]');
        const topicId = topicIdElem ? topicIdElem.value : (window.topicId || null);

        if (!topicId) {
            alert('Topic ID not found.');
            return;
        }

        if (selectedOption === 'selected') {
            const selectedCheckboxes = document.querySelectorAll('input[name="selectedMedia"]:checked');
            if (selectedCheckboxes.length === 0) {
                alert('Please select at least one file to download.');
                return;
            }
            const selectedIds = Array.from(selectedCheckboxes).map(cb => cb.value);
            downloadFiles(topicId, selectedOption, selectedIds);
        } else if (selectedOption === 'mediaonly') {
            const mediaTypeOption = document.querySelector('input[name="mediaTypeOption"]:checked');
            if (!mediaTypeOption) {
                alert('Please select a media type to download.');
                return;
            }
            downloadFiles(topicId, mediaTypeOption.value);
        } else {
            downloadFiles(topicId, selectedOption);
        }
    }

    function downloadFiles(topicId, mode, selectedMediaIds = null) {
        // Get the download URL from the hidden input
        const downloadUrlInput = document.getElementById('downloadUrl');
        const downloadUrl = downloadUrlInput ? downloadUrlInput.value : '/AdminTopic/DownloadMediaZip';
        // Get the topic title from the hidden input
        const topicTitleInput = document.getElementById('topicTitle');
        const topicTitle = topicTitleInput ? topicTitleInput.value : '';
        // Create a form to submit the download request
        const form = document.createElement('form');
        form.method = 'POST';
        form.action = downloadUrl;

        // Add topicId
        const topicInput = document.createElement('input');
        topicInput.type = 'hidden';
        topicInput.name = 'topicId';
        topicInput.value = topicId;
        form.appendChild(topicInput);

        // Add topicTitle
        if (topicTitle) {
            const titleInput = document.createElement('input');
            titleInput.type = 'hidden';
            titleInput.name = 'topicTitle';
            titleInput.value = topicTitle;
            form.appendChild(titleInput);
        }

        // Add mode
        const modeInput = document.createElement('input');
        modeInput.type = 'hidden';
        modeInput.name = 'mode';
        modeInput.value = mode;
        form.appendChild(modeInput);

        // Add selected media IDs if provided
        if (selectedMediaIds && selectedMediaIds.length > 0) {
            selectedMediaIds.forEach(id => {
                const idInput = document.createElement('input');
                idInput.type = 'hidden';
                idInput.name = 'selectedMediaIds';
                idInput.value = id;
                form.appendChild(idInput);
            });
        }

        // Add CSRF token if present
        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        if (token) {
            const tokenInput = document.createElement('input');
            tokenInput.type = 'hidden';
            tokenInput.name = '__RequestVerificationToken';
            tokenInput.value = token.value;
            form.appendChild(tokenInput);
        }

        document.body.appendChild(form);
        form.submit();
        document.body.removeChild(form);

        closeDownloadModal();
    }

    // Show/hide content sections based on radio selection
    function updateDownloadModalSections() {
        const radioButtons = document.querySelectorAll('input[name="downloadOption"]');
        const selectedContentList = document.getElementById('selectedContentList');
        const mediaTypeOptions = document.getElementById('mediaTypeOptions');
        let selectedValue = null;
        radioButtons.forEach(radio => {
            if (radio.checked) selectedValue = radio.value;
        });
        if (selectedValue === 'selected') {
            if (selectedContentList) selectedContentList.classList.remove('hidden');
            if (mediaTypeOptions) mediaTypeOptions.classList.add('hidden');
        } else if (selectedValue === 'mediaonly') {
            if (selectedContentList) selectedContentList.classList.add('hidden');
            if (mediaTypeOptions) mediaTypeOptions.classList.remove('hidden');
        } else {
            if (selectedContentList) selectedContentList.classList.add('hidden');
            if (mediaTypeOptions) mediaTypeOptions.classList.add('hidden');
        }
    }

    document.addEventListener('DOMContentLoaded', function() {
        // ... existing code ...
        // Download modal radio logic
        const radioButtons = document.querySelectorAll('input[name="downloadOption"]');
        radioButtons.forEach(radio => {
            radio.addEventListener('change', updateDownloadModalSections);
        });
        updateDownloadModalSections();

        // --- NEW: Auto-initialize media lists from JSON blobs if present ---
        try {
            var videoJsonTag = document.getElementById('videoMediaJson');
            var imageJsonTag = document.getElementById('imageMediaJson');
            var videos = [];
            var images = [];
            if (videoJsonTag && videoJsonTag.textContent) {
                videos = JSON.parse(videoJsonTag.textContent);
            }
            if (imageJsonTag && imageJsonTag.textContent) {
                images = JSON.parse(imageJsonTag.textContent);
            }
            setMediaLists(videos, images);
        } catch (e) {
            // Ignore if not present or invalid
        }

        // Expose for inline onclicks (for backward compatibility)
        window.openMediaModalWithNavigation = openMediaModalWithNavigation;
        window.closeMediaModal = closeMediaModal;
    });
    // Also call when opening the modal
    const origOpenDownloadModal = window.mediaModal.openDownloadModal;
    window.mediaModal.openDownloadModal = function() {
        origOpenDownloadModal();
        updateDownloadModalSections();
    };

    // Expose to window
    window.mediaModal = {
        setMediaLists,
        openMediaModalWithNavigation,
        closeMediaModal,
        navigateMedia,
        openDocumentModal,
        openDownloadModal,
        closeDownloadModal,
        processDownload
    };
})(window, document); 
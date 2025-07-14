// BrowseTrainings AJAX Functionality
document.addEventListener('DOMContentLoaded', function() {
    // Initialize variables
    let currentlyDisplayed = parseInt(document.getElementById('loadMoreBtn')?.getAttribute('data-currently-displayed') || '0');
    const totalTrainings = parseInt(document.getElementById('loadMoreBtn')?.getAttribute('data-total-trainings') || '0');
    const pageSize = 9;
    let selectedCategoryName = null;

    // Category navigation logic
    let categoryStart = 0;
    const maxVisible = 6;
    const wrapper = document.getElementById('category-cards-wrapper');
    const categories = Array.from(wrapper?.querySelectorAll('.category-card') || []);
    
    function updateCategoryVisibility(animatedDirection = null) {
        if (!wrapper) return;
        
        // Animate slide
        if (animatedDirection) {
            wrapper.style.transition = 'transform 0.7s cubic-bezier(0.22, 1, 0.36, 1)';
            wrapper.style.transform = `translateX(${animatedDirection === 'left' ? '80px' : '-80px'})`;
            setTimeout(() => {
                wrapper.style.transform = 'translateX(0)';
            }, 700);
        }
        categories.forEach((el, idx) => {
            el.style.display = (idx >= categoryStart && idx < categoryStart + maxVisible) ? '' : 'none';
        });
    }
    
    const leftBtn = document.getElementById('category-left-btn');
    const rightBtn = document.getElementById('category-right-btn');
    
    if (leftBtn) {
        leftBtn.addEventListener('click', function() {
            if (categoryStart > 0) {
                categoryStart--;
                updateCategoryVisibility('left');
            }
        });
    }
    
    if (rightBtn) {
        rightBtn.addEventListener('click', function() {
            if (categoryStart + maxVisible < categories.length) {
                categoryStart++;
                updateCategoryVisibility('right');
            }
        });
    }
    
    updateCategoryVisibility();

    // AJAX filter by category
    document.querySelectorAll('.category-card').forEach(card => {
        card.addEventListener('click', function() {
            const categoryId = this.getAttribute('data-category-id');
            const categoryName = this.querySelector('p')?.textContent || '';
            const categoryDescription = this.getAttribute('data-category-description') || '';
            
            // Show loading indicator
            const loadingElement = document.getElementById('trainings-loading');
            if (loadingElement) loadingElement.style.display = '';
            
            fetch(`/UserTraining/TrainingsByCategory?categoryId=${categoryId}`)
                .then(response => response.text())
                .then(html => {
                    const container = document.getElementById('trainings-container');
                    if (container) container.innerHTML = html;

                    AOS.refresh();
                    
                    // Update title and subtitle
                    const titleElement = document.getElementById('trainings-title');
                    const subtitleElement = document.getElementById('trainings-subtitle');
                    if (titleElement) titleElement.textContent = categoryName;
                    if (subtitleElement) subtitleElement.innerHTML = categoryDescription;
                    selectedCategoryName = categoryName;
                    if (titleElement) titleElement.scrollIntoView({ behavior: 'smooth', block: 'start' });
                })
                .finally(() => {
                    if (loadingElement) loadingElement.style.display = 'none';
                });
        });
    });

    // AJAX reload all trainings
    const showAllBtn = document.getElementById('showAllTrainingsBtn');
    if (showAllBtn) {
        showAllBtn.addEventListener('click', function() {
            // Show loading indicator
            const loadingElement = document.getElementById('trainings-loading');
            if (loadingElement) loadingElement.style.display = '';
            
            fetch(`/UserTraining/LoadMoreTrainings?skip=0`)
                .then(response => response.text())
                .then(html => {
                    const container = document.getElementById('trainings-container');
                    if (container) container.innerHTML = html;
                    AOS.refresh();
                    // Reset title and subtitle
                    const titleElement = document.getElementById('trainings-title');
                    const subtitleElement = document.getElementById('trainings-subtitle');
                    if (titleElement) titleElement.textContent = 'Trainings';
                    if (subtitleElement) subtitleElement.innerHTML = 'Hand-picked courses from our expert instructors';
                    selectedCategoryName = null;
                    if (titleElement) titleElement.scrollIntoView({ behavior: 'smooth', block: 'start' });
                })
                .finally(() => {
                    if (loadingElement) loadingElement.style.display = 'none';
                });
        });
    }

    // Load more logic
    const loadMoreBtn = document.getElementById("loadMoreBtn");
    if (loadMoreBtn) {
        loadMoreBtn.addEventListener("click", function () {
            const button = this;
            const buttonIcon = button.querySelector('.material-symbols-outlined');
            
            // Show loading state
            if (buttonIcon) {
                buttonIcon.textContent = 'hourglass_empty';
                buttonIcon.classList.add('animate-spin');
            }
            button.disabled = true;
            
            fetch(`/UserTraining/LoadMoreTrainings?skip=${currentlyDisplayed}`)
                .then(response => {
                    if (!response.ok) {
                        throw new Error('Network response was not ok');
                    }
                    return response.text();
                })
                .then(data => {
                    const container = document.getElementById("trainings-container");
                    if (container) {
                        container.insertAdjacentHTML('beforeend', data);
                    }
                    AOS.refresh();
                    // Update the count
                    currentlyDisplayed += pageSize;
                    
                    // Hide button if all trainings are loaded
                    if (currentlyDisplayed >= totalTrainings) {
                        button.style.display = "none";
                    } else {
                        // Reset button state
                        if (buttonIcon) {
                            buttonIcon.textContent = 'keyboard_double_arrow_down';
                            buttonIcon.classList.remove('animate-spin');
                            buttonIcon.classList.add('animate-bounce');
                        }
                        button.disabled = false;
                    }
                })
                .catch(error => {
                    console.error("Error loading more trainings:", error);
                    // Reset button state on error
                    if (buttonIcon) {
                        buttonIcon.textContent = 'keyboard_double_arrow_down';
                        buttonIcon.classList.remove('animate-spin');
                        buttonIcon.classList.add('animate-bounce');
                    }
                    button.disabled = false;
                    alert('Failed to load more trainings. Please try again.');
                });
        });
    }

    // SEARCH logic
    const searchInput = document.getElementById('training-search-input');
    const searchBtn = document.getElementById('training-search-btn');
    
    function doTrainingSearch() {
        const search = searchInput?.value.trim() || '';
        const loadingElement = document.getElementById('trainings-loading');
        if (loadingElement) loadingElement.style.display = '';
        
        fetch(`/UserTraining/SearchTrainings?search=${encodeURIComponent(search)}`)
            .then(response => response.text())
            .then(html => {
                const container = document.getElementById('trainings-container');
                if (container) container.innerHTML = html;
                AOS.refresh();
                const titleElement = document.getElementById('trainings-title');
                const subtitleElement = document.getElementById('trainings-subtitle');
                if (titleElement) titleElement.textContent = 'Trainings';
                if (subtitleElement) subtitleElement.innerHTML = 'Hand-picked courses from our expert instructors';
                if (titleElement) titleElement.scrollIntoView({ behavior: 'smooth', block: 'start' });
            })
            .finally(() => {
                if (loadingElement) loadingElement.style.display = 'none';
            });
    }


    
    if (searchBtn) {
        searchBtn.addEventListener('click', doTrainingSearch);
    }
    
    if (searchInput) {
        searchInput.addEventListener('keydown', function(e) {
            if (e.key === 'Enter') {
                doTrainingSearch();
            }
        });
    }
}); 
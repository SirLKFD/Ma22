// UserTrainings AJAX Functionality
document.addEventListener('DOMContentLoaded', function() {
    // Category navigation logic
    let userCategoryStart = 0;
    const userMaxVisible = 6;
    const userWrapper = document.getElementById('usertrainings-category-cards-wrapper');
    const userCategories = Array.from(userWrapper?.querySelectorAll('.category-card') || []);

    function updateUserCategoryVisibility(animatedDirection = null) {
        if (!userWrapper) return;
        
        if (animatedDirection) {
            userWrapper.style.transition = 'transform 0.7s cubic-bezier(0.22, 1, 0.36, 1)';
            userWrapper.style.transform = `translateX(${animatedDirection === 'left' ? '80px' : '-80px'})`;
            setTimeout(() => {
                userWrapper.style.transform = 'translateX(0)';
            }, 700);
        }
        userCategories.forEach((el, idx) => {
            el.style.display = (idx >= userCategoryStart && idx < userCategoryStart + userMaxVisible) ? '' : 'none';
        });
    }

    // Category navigation buttons
    const leftBtn = document.getElementById('usertrainings-category-left-btn');
    const rightBtn = document.getElementById('usertrainings-category-right-btn');
    
    if (leftBtn) {
        leftBtn.addEventListener('click', function() {
            if (userCategoryStart > 0) {
                userCategoryStart--;
                updateUserCategoryVisibility('left');
            }
        });
    }
    
    if (rightBtn) {
        rightBtn.addEventListener('click', function() {
            if (userCategoryStart + userMaxVisible < userCategories.length) {
                userCategoryStart++;
                updateUserCategoryVisibility('right');
            }
        });
    }
    
    updateUserCategoryVisibility();

    // SEARCH logic
    const searchInput = document.getElementById('usertrainings-search-input');
    const searchBtn = document.getElementById('usertrainings-search-btn');
    
    function doUserTrainingsSearch() {
        const search = searchInput?.value.trim() || '';
        const loadingElement = document.getElementById('usertrainings-loading');
        if (loadingElement) loadingElement.style.display = '';
        
        fetch(`/User/SearchUserTrainings?search=${encodeURIComponent(search)}`)
            .then(response => response.text())
            .then(html => {
                const grid = document.getElementById('usertrainings-container');
                if (grid) {
                    grid.innerHTML = html;
                }
                const titleElement = document.getElementById('usertrainings-title');
                const subtitleElement = document.getElementById('usertrainings-subtitle');
                if (titleElement) titleElement.textContent = 'Trainings';
                if (subtitleElement) subtitleElement.innerHTML = 'Hand-picked courses from our expert instructors';
                if (titleElement) titleElement.scrollIntoView({ behavior: 'smooth', block: 'start' });
            })
            .finally(() => {
                if (loadingElement) loadingElement.style.display = 'none';
            });
    }
    
    if (searchBtn) {
        searchBtn.addEventListener('click', doUserTrainingsSearch);
    }
    
    if (searchInput) {
        searchInput.addEventListener('keydown', function(e) {
            if (e.key === 'Enter') {
                doUserTrainingsSearch();
            }
        });
    }

    // Category filter logic
    document.querySelectorAll('.category-card').forEach(card => {
        card.addEventListener('click', function() {
            const categoryId = this.getAttribute('data-category-id');
            const categoryName = this.querySelector('p')?.textContent || '';
            const categoryDescription = this.getAttribute('data-category-description') || '';
            const loadingElement = document.getElementById('usertrainings-loading');
            if (loadingElement) loadingElement.style.display = '';
            
            fetch(`/User/UserTrainingsByCategory?categoryId=${categoryId}`)
                .then(response => response.text())
                .then(html => {
                    const container = document.getElementById('usertrainings-container');
                    if (container) container.innerHTML = html;
                    
                    const titleElement = document.getElementById('usertrainings-title');
                    const subtitleElement = document.getElementById('usertrainings-subtitle');
                    if (titleElement) titleElement.textContent = categoryName;
                    if (subtitleElement) subtitleElement.innerHTML = categoryDescription;
                    if (titleElement) titleElement.scrollIntoView({ behavior: 'smooth', block: 'start' });
                })
                .finally(() => {
                    if (loadingElement) loadingElement.style.display = 'none';
                });
        });
    });

    // LOAD MORE logic for user trainings
    const loadMoreBtn = document.getElementById("loadMoreUserTrainingsBtn");
    if (loadMoreBtn) {
        // Get initial values from the page
        let userCurrentlyDisplayed = parseInt(loadMoreBtn.getAttribute('data-currently-displayed') || '0');
        const userTotalTrainings = parseInt(loadMoreBtn.getAttribute('data-total-trainings') || '0');
        const userPageSize = 9;
        
        loadMoreBtn.addEventListener("click", function () {
            const button = this;
            const buttonIcon = button.querySelector('.material-symbols-outlined');
            
            // Show loading state
            if (buttonIcon) {
                buttonIcon.textContent = 'hourglass_empty';
                buttonIcon.classList.add('animate-spin');
            }
            button.disabled = true;
            
            fetch(`/User/LoadMoreUserTrainings?skip=${userCurrentlyDisplayed}`)
                .then(response => {
                    if (!response.ok) {
                        throw new Error('Network response was not ok');
                    }
                    return response.text();
                })
                .then(data => {
                    const container = document.getElementById("usertrainings-container");
                    if (container) {
                        container.insertAdjacentHTML('beforeend', data);
                    }
                    
                    // Update the count
                    userCurrentlyDisplayed += userPageSize;
                    
                    // Hide button if all trainings are loaded
                    if (userCurrentlyDisplayed >= userTotalTrainings) {
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
                    console.error("Error loading more user trainings:", error);
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
}); 
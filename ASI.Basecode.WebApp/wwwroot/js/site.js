// Global error handling for fetch and jQuery AJAX
(function() {
    // Intercept all fetch requests
    const originalFetch = window.fetch;
    window.fetch = async function(...args) {
        const response = await originalFetch.apply(this, args);
        // If the server redirects to /ServerError, force a client-side redirect
        if (response.redirected && response.url.includes('/ServerError')) {
            window.location.href = '/ServerError';
        }
        // If the server returns a 500 error, redirect as well
        if (response.status === 500) {
            window.location.href = '/ServerError';
        }
        return response;
    };

    // jQuery global AJAX error handler
    if (window.jQuery) {
        $(document).ajaxError(function(event, jqxhr, settings, thrownError) {
            if (jqxhr.status === 500 || (jqxhr.responseText && jqxhr.responseText.includes('/ServerError'))) {
                window.location.href = '/ServerError';
            }
        });
    }
})();

let dpicn = document.querySelector(".dpicn");
let dropdown = document.querySelector(".dropdown");

dpicn.addEventListener("click", () => {
    dropdown.classList.toggle("dropdown-open");
})
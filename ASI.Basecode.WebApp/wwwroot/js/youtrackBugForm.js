// YouTrack Feedback Form Script
(function() {
    // Load the YouTrack feedback form script
    const script = document.createElement('script');
    script.src = 'https://ma22.youtrack.cloud/static/simplified/form/form-entry.js?auto=false';
    script.onload = function() {
        // Create the feedback button container
        const feedbackButton = document.createElement('div');
        feedbackButton.id = 'yt-feedback-button';
        feedbackButton.style.position = 'fixed';
        feedbackButton.style.bottom = '20px';
        feedbackButton.style.right = '20px';
        document.body.appendChild(feedbackButton);

        // Initialize the feedback form
        if (typeof YTFeedbackForm !== 'undefined') {
            YTFeedbackForm.renderFeedbackButton(
                feedbackButton,
                {
                    backendURL: 'https://ma22.youtrack.cloud',
                    formUUID: '546c0843-518d-4c66-af67-06fc0623b6d5',
                    theme: 'dark',
                    language: 'en'
                }
            );
        }
    };
    document.head.appendChild(script);
})(); 
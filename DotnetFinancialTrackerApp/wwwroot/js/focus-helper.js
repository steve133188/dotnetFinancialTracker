// Focus helper functions for QuickTransactionEntry
window.focusElement = (selector) => {
    const element = document.querySelector(selector);
    if (element) {
        element.focus();
        element.click();
    }
};

// Optional: Add vibration feedback for mobile devices
window.vibrateDevice = (duration = 50) => {
    if (navigator.vibrate) {
        navigator.vibrate(duration);
    }
};
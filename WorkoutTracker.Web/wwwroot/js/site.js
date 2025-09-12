// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// Custom JavaScript for Workout Tracker

// Initialize app when DOM is loaded
document.addEventListener("DOMContentLoaded", function () {
  // Initialize tooltips
  var tooltipTriggerList = [].slice.call(
    document.querySelectorAll('[data-bs-toggle="tooltip"]'),
  );
  var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
    return new bootstrap.Tooltip(tooltipTriggerEl);
  });

  // Auto-dismiss alerts after 5 seconds
  setTimeout(function () {
    var alerts = document.querySelectorAll(".alert");
    alerts.forEach(function (alert) {
      if (
        alert.classList.contains("alert-success") ||
        alert.classList.contains("alert-info")
      ) {
        var bsAlert = new bootstrap.Alert(alert);
        bsAlert.close();
      }
    });
  }, 5000);

  // Form validation feedback
  const forms = document.querySelectorAll(".needs-validation");
  Array.prototype.slice.call(forms).forEach(function (form) {
    form.addEventListener(
      "submit",
      function (event) {
        if (!form.checkValidity()) {
          event.preventDefault();
          event.stopPropagation();
        }
        form.classList.add("was-validated");
      },
      false,
    );
  });
});

// Utility functions
const WorkoutTracker = {
  // Show loading state on buttons
  showLoading: function (button) {
    const originalText = button.innerHTML;
    button.innerHTML =
      '<span class="spinner-border spinner-border-sm me-2"></span>Loading...';
    button.disabled = true;
    button.dataset.originalText = originalText;
  },

  // Hide loading state on buttons
  hideLoading: function (button) {
    if (button.dataset.originalText) {
      button.innerHTML = button.dataset.originalText;
      button.disabled = false;
      delete button.dataset.originalText;
    }
  },

  // Confirm delete actions
  confirmDelete: function (message) {
    return confirm(
      message ||
        "Are you sure you want to delete this item? This action cannot be undone.",
    );
  },
};

// Global event handlers
document.addEventListener("click", function (e) {
  // Handle delete confirmations
  if (
    e.target.classList.contains("btn-delete") ||
    e.target.closest(".btn-delete")
  ) {
    const button = e.target.classList.contains("btn-delete")
      ? e.target
      : e.target.closest(".btn-delete");
    const message = button.dataset.confirmMessage;
    if (!WorkoutTracker.confirmDelete(message)) {
      e.preventDefault();
      return false;
    }
  }

  // Handle form submissions with loading states
  if (e.target.type === "submit") {
    setTimeout(() => WorkoutTracker.showLoading(e.target), 10);
  }
});

// ===============================
// Menu common JS
// ===============================

let pendingReport = null;

/**
 * Download Excel (open confirm modal)
 */
function downloadExcel(reportName) {
    pendingReport = reportName;

    const msg = document.getElementById("confirmMessage");
    if (msg) {
        msg.textContent = `Do you want to download "${reportName}" report?`;
    }

    const modalEl = document.getElementById("confirmModal");
    if (!modalEl) {
        console.error("confirmModal not found");
        return;
    }

    const modal = new bootstrap.Modal(modalEl);
    modal.show();
}

/**
 * Logout
 */
function logout() {
    window.location.href = "/Account/Login";
}

// ===============================
// Event binding
// ===============================
document.addEventListener("DOMContentLoaded", function () {
    const okBtn = document.getElementById("confirmOk");
    if (!okBtn) return;

    okBtn.addEventListener("click", function () {
        if (!pendingReport) return;

        window.location.href =
            `/Home/ExportToExcel?reportName=${encodeURIComponent(pendingReport)}`;

        const modalEl = document.getElementById("confirmModal");
        const modal = bootstrap.Modal.getInstance(modalEl);
        if (modal) {
            modal.hide();
        }

        pendingReport = null;
    });
});

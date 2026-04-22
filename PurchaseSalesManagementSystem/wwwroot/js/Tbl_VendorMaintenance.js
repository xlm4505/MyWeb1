let currentRows = [];

function escapeHtml(value) {
    if (value === null || value === undefined) {
        return "";
    }

    return String(value)
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#39;");
}

function renderTable(records) {
    currentRows = records || [];

    const body = document.getElementById("dataBody");
    body.innerHTML = "";

    currentRows.forEach((row, index) => {
        body.insertAdjacentHTML(
            "beforeend",
            `<tr>
                <td>${escapeHtml(row.id)}</td>
                <td><input type="text" inputmode="numeric" class="form-control number50-input" id="apDivisionNo_${index}" value="${escapeHtml(row.apDivisionNo)}"></td>
                <td><input type="text" inputmode="numeric" class="form-control number50-input" id="vendorNo_${index}" value="${escapeHtml(row.vendorNo)}"></td>
                <td><input type="text" maxlength="100" class="form-control" id="name_${index}" value="${escapeHtml(row.vendorName)}"></td>
                <td><input type="checkbox" id="chk_${index}"></td>
            </tr>`
        );
    });

    document.getElementById("actionArea").style.display = currentRows.length > 0 ? "block" : "none";
}
function sanitizeNumber50Input(event) {
    const input = event.target;
    if (!(input instanceof HTMLInputElement) || !input.classList.contains("number50-input")) {
        return;
    }

    const digitsOnly = input.value.replace(/\D/g, "").slice(0, 50);
    if (digitsOnly !== input.value) {
        input.value = digitsOnly;
    }
}
function sanitizeModalInput(event) {
    const input = event.target;
    if (!(input instanceof HTMLInputElement)) {
        return;
    }

    if (input.classList.contains("modal-id-number")) {
        const digitsOnly = input.value.replace(/\D/g, "").slice(0, 30);
        if (digitsOnly !== input.value) {
            input.value = digitsOnly;
        }
    }

    if (input.classList.contains("modal-number50")) {
        const digitsOnly = input.value.replace(/\D/g, "").slice(0, 50);
        if (digitsOnly !== input.value) {
            input.value = digitsOnly;
        }
    }
}

function openAddModal() {
    document.getElementById("addId").value = "";
    document.getElementById("addApDivisionNo").value = "";
    document.getElementById("addVendorNo").value = "";
    document.getElementById("addVendorName").value = "";
    document.getElementById("addModal").style.display = "block";
}

function closeAddModal() {
    document.getElementById("addModal").style.display = "none";
}

function validateAddForm() {
    const id = document.getElementById("addId").value.trim();
    const apDivisionNo = document.getElementById("addApDivisionNo").value.trim();
    const vendorNo = document.getElementById("addVendorNo").value.trim();

    if (!id) {
        alert("ID is required.");
        document.getElementById("addId").focus();
        return false;
    }

    if (!/^\d{1,30}$/.test(id)) {
        alert("ID must be numeric and up to 30 digits.");
        document.getElementById("addId").focus();
        return false;
    }

    if (apDivisionNo && !/^\d{1,50}$/.test(apDivisionNo)) {
        alert("APDivisionNo must be numeric and up to 50 digits.");
        document.getElementById("addApDivisionNo").focus();
        return false;
    }

    if (vendorNo && !/^\d{1,50}$/.test(vendorNo)) {
        alert("VendorNo must be numeric and up to 50 digits.");
        document.getElementById("addVendorNo").focus();
        return false;
    }

    return true;
}

async function addItem() {
    if (!validateAddForm()) {
        return;
    }

    if (!confirm("Do you want to register this data?")) {
        return;
    }

    const payload = {
        id: document.getElementById("addId").value.trim(),
        apDivisionNo: document.getElementById("addApDivisionNo").value.trim(),
        vendorNo: document.getElementById("addVendorNo").value.trim(),
        vendorName: document.getElementById("addVendorName").value.trim().slice(0, 100)
    };

    const response = await fetch("/Tbl_Vendor/Add", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload)
    });

    if (!response.ok) {
        let message = "Data registration failed.";
        try {
            const errorResult = await response.json();
            if (errorResult && errorResult.message) {
                message = errorResult.message;
            }
        } catch (error) {
            // ignore JSON parse errors and keep default message
        }

        alert(message);
        return;
    }

    alert("Data has been registered.");
    closeAddModal();
    await searchItems();
}

function requestCloseAddModal() {
    if (!confirm("Do you want to close this screen?")) {
        return;
    }

    closeAddModal();
}
async function searchItems() {
    const id = document.getElementById("searchCode").value.trim();
    const query = id ? `?id=${encodeURIComponent(id)}` : "";

    const response = await fetch(`/Tbl_Vendor/Search${query}`);
    if (!response.ok) {
        alert("Search failed.");
        return;
    }

    const data = await response.json();
    renderTable(data);
}

function getSelectedRows() {
    const selected = [];

    currentRows.forEach((row, index) => {
        const check = document.getElementById(`chk_${index}`);
        if (!check || !check.checked) {
            return;
        }

        selected.push({
            id: row.id,
            apDivisionNo: document.getElementById(`apDivisionNo_${index}`).value.trim(),
            vendorNo: document.getElementById(`vendorNo_${index}`).value.trim(),
            vendorName: document.getElementById(`name_${index}`).value.trim().slice(0, 100)
        });
    });

    return selected;
}

async function updateSelected() {
    const selected = getSelectedRows();
    if (selected.length === 0) {
        alert("Select rows to update.");
        return;
    }

    const response = await fetch("/Tbl_Vendor/Update", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(selected)
    });

    if (!response.ok) {
        alert("Update failed.");
        return;
    }

    const result = await response.json();
    alert(`Updated ${result.updatedCount} row(s).`);
    await searchItems();
}

async function deleteSelected() {
    const selected = getSelectedRows();
    if (selected.length === 0) {
        alert("Select rows to delete.");
        return;
    }

    if (!confirm(`Delete ${selected.length} selected row(s)?`)) {
        return;
    }

    const response = await fetch("/Tbl_Vendor/Delete", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(selected)
    });

    if (!response.ok) {
        alert("Delete failed.");
        return;
    }

    const result = await response.json();
    alert(`Deleted ${result.deletedCount} row(s).`);
    await searchItems();
}
function downloadExcel() {
    const id = document.getElementById("searchCode").value.trim();
    window.location.href = `/Tbl_Vendor/ExportToExcel?id=${encodeURIComponent(id)}`;
}
document.getElementById("searchForm").addEventListener("submit", async function (e) {
    e.preventDefault();
    await searchItems();
});

document.getElementById("dataBody").addEventListener("input", sanitizeNumber50Input);
document.getElementById("addButton").addEventListener("click", openAddModal);
document.getElementById("okAddButton").addEventListener("click", addItem);
document.getElementById("closeAddButton").addEventListener("click", requestCloseAddModal);
document.getElementById("addModal").addEventListener("input", sanitizeModalInput);
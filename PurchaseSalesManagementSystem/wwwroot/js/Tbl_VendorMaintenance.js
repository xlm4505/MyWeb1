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

document.getElementById("searchForm").addEventListener("submit", async function (e) {
    e.preventDefault();
    await searchItems();
});

document.getElementById("dataBody").addEventListener("input", sanitizeNumber50Input);
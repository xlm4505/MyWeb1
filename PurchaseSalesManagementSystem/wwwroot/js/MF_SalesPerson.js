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
                <td>${escapeHtml(row.customerCode)}</td>
                <td><input type="text" class="form-control" id="sp_${index}" value="${escapeHtml(row.salesPerson)}" maxlength="50" pattern="[A-Za-z0-9]*"></td>
                <td><input type="checkbox" id="chk_${index}"></td>
            </tr>`
        );
    });

    document.getElementById("actionArea").style.display = currentRows.length > 0 ? "block" : "none";
    currentRows.forEach((_, index) => {
        const salesPersonInput = document.getElementById(`sp_${index}`);
        if (!salesPersonInput) {
            return;
        }

        salesPersonInput.addEventListener("input", function () {
            this.value = this.value.replace(/[^A-Za-z0-9]/g, "").slice(0, 50);
        });
    });
}

async function searchItems() {
    const customerCode = document.getElementById("searchCode").value.trim();
    const query = customerCode ? `?customerCode=${encodeURIComponent(customerCode)}` : "";

    const response = await fetch(`/MF_SalesPerson/Search${query}`);
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
            customerCode: row.customerCode,
            salesPerson: document.getElementById(`sp_${index}`).value.trim()
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
    const invalidRow = selected.find((row) => !salesPersonRegex.test(row.salesPerson ?? ""));
    if (invalidRow) {
        alert("SalesPerson must be up to 50 half-width alphanumeric characters.");
        return;
    }
    const response = await fetch("/MF_SalesPerson/Update", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(selected)
    });

    if (!response.ok) {
        const error = await response.json().catch(() => null);
        alert(error?.message || "Update failed.");
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

    const response = await fetch("/MF_SalesPerson/Delete", {
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
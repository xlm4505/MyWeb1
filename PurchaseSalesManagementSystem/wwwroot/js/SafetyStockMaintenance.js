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
                <td>${escapeHtml(row.itemCode)}</td>
                <td>${escapeHtml(row.procType)}</td>
                <td>${escapeHtml(row.arDivisionNo)}</td>
                <td>${escapeHtml(row.customerNo)}</td>
                <td>${escapeHtml(row.warehouseCode)}</td>
                <td><input type="text" inputmode="numeric" class="form-control qty-input" id="qty_${index}" value="${escapeHtml(row.quantity)}"></td>
                <td>${escapeHtml(row.itemNo)}</td>
                <td>${escapeHtml(row.comment)}</td>
                <td><input type="checkbox" id="chk_${index}"></td>
            </tr>`
        );
    });

    document.getElementById("actionArea").style.display = currentRows.length > 0 ? "block" : "none";
}
function sanitizeQuantityInput(event) {
    const input = event.target;
    if (!(input instanceof HTMLInputElement) || !input.classList.contains("qty-input")) {
        return;
    }

    const value = input.value;
    if (value === "") {
        return;
    }

    if (!/^\d+$/.test(value)) {
        input.value = "";
        return;
    }

    if (value.length > 7) {
        input.value = value.slice(0, 7);
    }
}

async function searchItems() {
    const itemCode = document.getElementById("searchCode").value.trim();
    const query = itemCode ? `?itemCode=${encodeURIComponent(itemCode)}` : "";

    const response = await fetch(`/SafetyStockMaintenance/Search${query}`);
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

        const qty = parseFloat(document.getElementById(`qty_${index}`).value);

        selected.push({
            itemCode: row.itemCode,
            quantity: Number.isNaN(qty) ? 0 : qty
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

    const response = await fetch("/SafetyStockMaintenance/Update", {
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

    const response = await fetch("/SafetyStockMaintenance/Delete", {
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

document.getElementById("dataBody").addEventListener("input", sanitizeQuantityInput);
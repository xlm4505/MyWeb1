let currentRows = [];
const alphaNumericFieldRules = {
    addItemCode: 30,
    addProcType: 1,
    addArDivisionNo: 2,
    addCustomerNo: 20,
    addWarehouseCode: 3,
    addComment: 30
};
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
                <td><input type="text" class="form-control editable-alnum" data-maxlength="20" id="customerNo_${index}" value="${escapeHtml(row.customerNo)}"></td>
                <td><input type="text" class="form-control editable-alnum" data-maxlength="3" id="warehouseCode_${index}" value="${escapeHtml(row.warehouseCode)}"></td>
                <td><input type="text" inputmode="numeric" class="form-control qty-input" id="qty_${index}" value="${escapeHtml(row.quantity)}"></td>
                <td>${escapeHtml(row.itemNo)}</td>
                <td><input type="text" class="form-control editable-alnum" data-maxlength="30" id="comment_${index}" value="${escapeHtml(row.comment)}"></td>
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

    const digitsOnly = value.replace(/\D/g, "").slice(0, 7);
    if (digitsOnly !== value) {
        input.value = digitsOnly;
    }
}
function sanitizeEditableAlphaNumericInput(event) {
    const input = event.target;
    if (!(input instanceof HTMLInputElement) || !input.classList.contains("editable-alnum")) {
        return;
    }

    const maxLength = parseInt(input.dataset.maxlength ?? "", 10);
    if (Number.isNaN(maxLength) || maxLength <= 0) {
        return;
    }

    const sanitized = input.value.replace(/[^A-Za-z0-9]/g, "").slice(0, maxLength);
    if (sanitized !== input.value) {
        input.value = sanitized;
    }
}
function sanitizeSevenDigitNumberInput(event) {
    const input = event.target;
    if (!(input instanceof HTMLInputElement) || !input.classList.contains("modal-number-7")) {
        return;
    }

    const value = input.value;
    if (value === "") {
        return;
    }

    const digitsOnly = value.replace(/\D/g, "").slice(0, 7);
    if (digitsOnly !== value) {
        input.value = digitsOnly;
    }
}
function sanitizeAlphaNumericInput(event) {
    const input = event.target;
    if (!(input instanceof HTMLInputElement)) {
        return;
    }

    const maxLength = alphaNumericFieldRules[input.id];
    if (!maxLength) {
        return;
    }

    const sanitized = input.value.replace(/[^A-Za-z0-9]/g, "").slice(0, maxLength);
    if (sanitized !== input.value) {
        input.value = sanitized;
    }
}
function openAddModal() {
    document.getElementById("addItemCode").value = "";
    document.getElementById("addProcType").value = "";
    document.getElementById("addArDivisionNo").value = "";
    document.getElementById("addCustomerNo").value = "";
    document.getElementById("addWarehouseCode").value = "";
    document.getElementById("addQuantity").value = "";
    document.getElementById("addItemNo").value = "";
    document.getElementById("addComment").value = "";

    document.getElementById("addModal").style.display = "block";
}

function closeAddModal() {
    document.getElementById("addModal").style.display = "none";
}

function validateAddForm() {
    const itemCode = document.getElementById("addItemCode").value.trim();
    const procType = document.getElementById("addProcType").value.trim();
    const arDivisionNo = document.getElementById("addArDivisionNo").value.trim();
    const customerNo = document.getElementById("addCustomerNo").value.trim();
    const warehouseCode = document.getElementById("addWarehouseCode").value.trim();
    const quantity = document.getElementById("addQuantity").value.trim();
    const itemNo = document.getElementById("addItemNo").value.trim();
    const comment = document.getElementById("addComment").value.trim();
    const requiredChecks = [
        { value: itemCode, fieldId: "addItemCode", message: "ItemCode is required." },
        { value: procType, fieldId: "addProcType", message: "ProcType is required." },
        { value: quantity, fieldId: "addQuantity", message: "Quantity is required." }
    ];

    for (const check of requiredChecks) {
        if (check.value) {
            continue;
        }

        alert(check.message);
        document.getElementById(check.fieldId).focus();
        return false;
    }
    if (!/^[A-Za-z0-9]{1,30}$/.test(itemCode)) {
        alert("ItemCode must be half-width alphanumeric and up to 30 characters.");
        return false;
    }

    if (!/^[A-Za-z0-9]{1}$/.test(procType)) {
        alert("ProcType must be 1 half-width alphanumeric character.");
        return false;
    }

    if (arDivisionNo && !/^[A-Za-z0-9]{1,2}$/.test(arDivisionNo)) {
        alert("ARDivisionNo must be half-width alphanumeric and up to 2 characters.");
        return false;
    }

    if (customerNo && !/^[A-Za-z0-9]{1,20}$/.test(customerNo)) {
        alert("CustomerNo must be half-width alphanumeric and up to 20 characters.");
        return false;
    }

    if (warehouseCode && !/^[A-Za-z0-9]{1,3}$/.test(warehouseCode)) {
        alert("WarehouseCode must be half-width alphanumeric and up to 3 characters.");
        return false;
    }

    if (comment && !/^[A-Za-z0-9]{1,30}$/.test(comment)) {
        alert("Comment must be half-width alphanumeric and up to 30 characters.");
        return false;
    }

    if (!/^\d{1,7}$/.test(quantity)) {
        alert("Quantity must be numeric and up to 7 digits.");
        return false;
    }

    if (itemNo && !/^\d{1,7}$/.test(itemNo)) {
        alert("ItemNo must be numeric and up to 7 digits.");
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
        itemCode: document.getElementById("addItemCode").value.trim(),
        procType: document.getElementById("addProcType").value.trim(),
        arDivisionNo: document.getElementById("addArDivisionNo").value.trim(),
        customerNo: document.getElementById("addCustomerNo").value.trim(),
        warehouseCode: document.getElementById("addWarehouseCode").value.trim(),
        quantity: parseInt(document.getElementById("addQuantity").value.trim(), 10),
        itemNo: document.getElementById("addItemNo").value.trim() === ""
            ? 0
            : parseInt(document.getElementById("addItemNo").value.trim(), 10),
        comment: document.getElementById("addComment").value.trim()
    };

    const response = await fetch("/SafetyStockMaintenance/Add", {
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
}

function requestCloseAddModal() {
    if (!confirm("Do you want to close this screen?")) {
        return;
    }

    closeAddModal();
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

        const qty = parseInt(document.getElementById(`qty_${index}`).value, 10);
        const customerNo = document.getElementById(`customerNo_${index}`).value.trim();
        const warehouseCode = document.getElementById(`warehouseCode_${index}`).value.trim();
        const comment = document.getElementById(`comment_${index}`).value.trim();
        selected.push({
            itemCode: row.itemCode,
            customerNo,
            warehouseCode,
            quantity: Number.isNaN(qty) ? 0 : qty,
            comment
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
document.getElementById("dataBody").addEventListener("input", sanitizeEditableAlphaNumericInput);
document.getElementById("addButton").addEventListener("click", openAddModal);
document.getElementById("okAddButton").addEventListener("click", addItem);
document.getElementById("closeAddButton").addEventListener("click", requestCloseAddModal);
document.getElementById("addModal").addEventListener("input", sanitizeSevenDigitNumberInput);
document.getElementById("addModal").addEventListener("input", sanitizeAlphaNumericInput);
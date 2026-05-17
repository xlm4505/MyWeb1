const columns = [
    "ProdLn", "ItemCode", "ItemCodeDesc", "WHSE", "OnHand", "Qty PO", "StandardUnitCost", "LastSoldDate", "LastReceiptDate", "LastTotalUnitCost"
];

function createHeader(tableId) {
    const thead = document.querySelector(`#${tableId} thead tr`);
    thead.innerHTML = "";
    columns.forEach(col => {
        const th = document.createElement("th");
        th.textContent = col;
        thead.appendChild(th);
    });
}

async function searchItems() {
    const itemCode  = document.getElementById("search").value.trim();
    const wareHouse = document.getElementById("warehouse").value.trim();
    const minusOnly = document.getElementById("minusInventory").checked;

    const params = new URLSearchParams();
    if (itemCode)  params.append("itemCode", itemCode);
    if (wareHouse) params.append("wareHouse", wareHouse);
    if (minusOnly) params.append("minusOnly", "true");

    const response = await fetch(`/FOAInventory/Search?${params.toString()}`);
    if (!response.ok) {
        alert("Search failed.");
        return;
    }

    const data = await response.json();
    renderTable(data);
}

function renderTable(data) {
    const tbody = document.querySelector("#gridMain tbody");
    tbody.innerHTML = "";

    data.forEach(row => {
        const tr = document.createElement("tr");
        const values = [
            row.prodLn, row.itemCode, row.itemCodeDesc, row.whse,
            row.onHand, row.qtyPO, row.standardUnitCost,
            row.lastSoldDate, row.lastReceiptDate, row.lastTotalUnitCost
        ];
        values.forEach(cell => {
            const td = document.createElement("td");
            td.textContent = cell ?? "";
            tr.appendChild(td);
        });
        tbody.appendChild(tr);
    });
}

async function loadWareHouseList() {
    const response = await fetch("/FOAInventory/GetWareHouseList");
    if (!response.ok) return;

    const list = await response.json();
    const select = document.getElementById("warehouse");
    list.forEach(code => {
        const opt = document.createElement("option");
        opt.value = code;
        opt.textContent = code;
        select.appendChild(opt);
    });
}

document.addEventListener("DOMContentLoaded", () => {
    createHeader("gridMain");
    loadWareHouseList();

    document.getElementById("btnRefresh").addEventListener("click", () => {
        searchItems();
    });

    document.getElementById("btnExport").addEventListener("click", () => {
        const itemCode  = document.getElementById("search").value.trim();
        const wareHouse = document.getElementById("warehouse").value.trim();
        const minusOnly = document.getElementById("minusInventory").checked;

        const params = new URLSearchParams();
        if (itemCode)  params.append("itemCode", itemCode);
        if (wareHouse) params.append("wareHouse", wareHouse);
        if (minusOnly) params.append("minusOnly", "true");

        window.location.href = `/FOAInventory/ExportToExcel?${params.toString()}`;
    });
});

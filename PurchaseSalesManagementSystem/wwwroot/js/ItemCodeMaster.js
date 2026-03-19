const itemCodeMasterColumns = [
    "ItemCode",
    "ItemDesc",
    "ItemDesc2",
    "Category",
    "ProductLineDesc",
    "ProductType",
    "Inactive",
    "Weight(lb)",
    "Whse",
    "PrimaryVendor",
    "QtyDisc",
    "StdSalesPrice",
    "StdUnitCost",
    "LastCost",
    "AvgCost",
    "VenCost(USD)",
    "VenCost(JPY)",
    "OnHand",
    "OpenSO",
    "Available",
    "OpenPO",
    "(InShip)",
    "OnHand ",
    "OpenSO ",
    "Available ",
    "OpenPO ",
    "(InShip) ",
    "LastSold",
    "LastReceipt",
    "ExtendedDescriptionText",
    "DateCreated",
    "UserCreated",
    "DateUpdated",
    "UserUpdated",
    "List COP",
    "Standard",
    "Discount",
    "Class 4",
    "Class 5",
    "Contract",
    "Class 6"
];
const itemCodeMasterHeaderGroups = [
    { title: "", span: 1 },
    { title: "Product Information", span: 10 },
    { title: "Unit Price / Cost", span: 6 },
    { title: "Inventory (Regular Items)", span: 5 },
    { title: "Inventory (Excluded Items)", span: 5 },
    { title: "Last Transaction Date", span: 2 },
    { title: "", span: 1 },
    { title: "Database Access Information", span: 4 },
    { title: "Master Price List", span: 7 }
];
document.addEventListener("DOMContentLoaded", () => {
    const btnSearch = document.getElementById("btnSearch");
    const btnExport = document.getElementById("btnExport");

    loadHeader(itemCodeMasterColumns);

    btnSearch.addEventListener("click", loadItemCodeMasterData);

    btnExport.addEventListener("click", () => {
        window.location.href = `/ItemCodeMaster/ExportToExcel?${buildQueryString()}`;
    });

});

function buildQueryString() {
    const itemCode = document.getElementById("itemCode").value;
    const excludeInactiveItems = document.getElementById("excludeInactiveItems").checked;

    return `itemCode=${encodeURIComponent(itemCode)}&excludeInactiveItems=${excludeInactiveItems}`;
}

function loadItemCodeMasterData() {
    fetch(`/ItemCodeMaster/GetItemCodeMasterData?${buildQueryString()}`)
        .then(res => res.json())
        .then(data => {
            const tbody = document.querySelector("#gridMain tbody");
            const btnExport = document.getElementById("btnExport");

            loadHeader(itemCodeMasterColumns);
            tbody.innerHTML = "";

            if (!Array.isArray(data) || data.length === 0) {
                btnExport.disabled = true;
                return;
            }

            const columns = Object.keys(data[0]);

            data.forEach(row => {
                const tr = document.createElement("tr");

                columns.forEach(column => {
                    addCell(tr, formatValue(row[column]));
                });

                tbody.appendChild(tr);
            });

            btnExport.disabled = false;
        });
}
function loadHeader(columns) {
    const thead = document.querySelector("#gridMain thead");
    thead.innerHTML = "";

    const groupRow = document.createElement("tr");
    groupRow.classList.add("header-group-row");

    itemCodeMasterHeaderGroups.forEach(group => {
        const th = document.createElement("th");
        th.colSpan = group.span;
        th.textContent = group.title;
        groupRow.appendChild(th);
    });

    const detailRow = document.createElement("tr");
    detailRow.classList.add("header-detail-row");

    columns.forEach(column => {
        const th = document.createElement("th");
        th.textContent = column;
        detailRow.appendChild(th);
    });

    thead.appendChild(groupRow);
    thead.appendChild(detailRow);
}

function addCell(row, value) {
    const td = document.createElement("td");
    td.textContent = value ?? "";
    row.appendChild(td);
}

function formatValue(value) {
    if (value === null || value === undefined) {
        return "";
    }

    if (typeof value === "string") {
        const parsedDate = new Date(value);
        if (!Number.isNaN(parsedDate.getTime()) && /^\d{4}-\d{2}-\d{2}T/.test(value)) {
            const year = parsedDate.getFullYear();
            const month = String(parsedDate.getMonth() + 1).padStart(2, "0");
            const day = String(parsedDate.getDate()).padStart(2, "0");
            return `${year}-${month}-${day}`;
        }
    }

    return value;
}
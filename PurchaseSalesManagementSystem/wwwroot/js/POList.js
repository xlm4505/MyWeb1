// POList.js

const miscColumns = [
    "PoNo", "LnKey", "PODate", "Status", "Vendor", "VendorName", "ItemCode", "UDF_ITEMDESC",
    "QtyOrdered", "QtyRcpt", "QtyBalance", "QtyInvoiced", "UnitCost", "Amount", "RequiredDate"
];

const allVendorsColumns = [
    "VendorNo", "PO-Ln", "PONo", "LnKey", "PODate", "Status", "ItemCode", "ItemDesc",
    "Whse", "Ordered", "Received", "Balance", "Invoiced", "UnitCost", "StdUnitCost", "LastCost",
    "AvgCost", "VenCost(CM)", "RequiredDate", "PromiseDate", "SalesOrderNo"
];

document.addEventListener("DOMContentLoaded", () => {
    loadHeader(getColumnsByTarget(document.getElementById("selectA").value));

    document.getElementById("selectA").addEventListener("change", () => {
        const exportTarget = document.getElementById("selectA").value;

        loadHeader(getColumnsByTarget(exportTarget));
        document.querySelector("#gridMain tbody").innerHTML = "";
        document.getElementById("search").value = "";
        document.getElementById("btnExport").disabled = true;
    });

    document.getElementById("btnSearch").addEventListener("click", () => {
        const purchaseOrderNo = document.getElementById("search").value;
        const exportTarget = document.getElementById("selectA").value;

        fetch(`/POList/GetPOListData?purchaseOrderNo=${encodeURIComponent(purchaseOrderNo)}&exportTarget=${encodeURIComponent(exportTarget)}`)
            .then(res => res.json())
            .then(data => {
                const gridMain = document.querySelector("#gridMain tbody");
                gridMain.innerHTML = "";

                let rowCount = 0;

                data.forEach(v => {
                    const trMain = document.createElement("tr");

                    if (exportTarget === "All") {
                        addCell(trMain, v.vendorNo);
                        addCell(trMain, v.poLn ?? v.poln);
                        addCell(trMain, v.poNo ?? v.pONo);
                        addCell(trMain, v.lnKey);
                        addCell(trMain, formatDate(v.poDate));
                        addCell(trMain, v.status);
                        addCell(trMain, v.itemCode);
                        addCell(trMain, v.itemDesc);
                        addCell(trMain, v.whse);
                        addCell(trMain, v.ordered);
                        addCell(trMain, v.received);
                        addCell(trMain, v.balance);
                        addCell(trMain, v.invoiced);
                        addCell(trMain, v.unitCost);
                        addCell(trMain, v.stdUnitCost);
                        addCell(trMain, v.lastCost);
                        addCell(trMain, v.avgCost);
                        addCell(trMain, v.avgCostVenCostCM ?? v.venCostCM);
                        addCell(trMain, formatDate(v.requiredDate));
                        addCell(trMain, formatDate(v.promiseDate));
                        addCell(trMain, v.salesOrderNo);
                    } else {
                        addCell(trMain, v.poNo);
                        addCell(trMain, v.lnKey);
                        addCell(trMain, formatDate(v.poDate));
                        addCell(trMain, v.status);
                        addCell(trMain, v.vendor);
                        addCell(trMain, v.vendorName);
                        addCell(trMain, v.itemCode);
                        addCell(trMain, v.udF_ITEMDESC ?? v.udef_ITEMDESC ?? v.udf_ITEMDESC ?? v.itemDesc);
                        addCell(trMain, v.qtyOrdered);
                        addCell(trMain, v.qtyRcpt);
                        addCell(trMain, v.qtyBalance);
                        addCell(trMain, v.qtyInvoiced);
                        addCell(trMain, v.unitCost);
                        addCell(trMain, v.amount);
                        addCell(trMain, formatDate(v.requiredDate));
                    }
                    gridMain.appendChild(trMain);
                    rowCount++;
                });

                document.getElementById("btnExport").disabled = (rowCount === 0);
            });
    });

    document.getElementById("btnExport").addEventListener("click", () => {
        const purchaseOrderNo = document.getElementById("search").value;
        const exportTarget = document.getElementById("selectA").value;
        const url = `/POList/ExportToExcel?purchaseOrderNo=${encodeURIComponent(purchaseOrderNo)}&exportTarget=${encodeURIComponent(exportTarget)}`;

        window.location.href = url;
    });
});
function getColumnsByTarget(exportTarget) {
    return exportTarget === "All" ? allVendorsColumns : miscColumns;
}
function loadHeader(columns) {
    const thead = document.querySelector("#gridMain thead tr");
    thead.innerHTML = "";

    columns.forEach(col => {
        const th = document.createElement("th");
        th.textContent = col;
        thead.appendChild(th);
    });
}

function addCell(row, value) {
    const td = document.createElement("td");
    td.textContent = value ?? "";
    row.appendChild(td);
}
function formatDate(value) {
    if (!value) {
        return "";
    }

    const date = new Date(value);

    if (Number.isNaN(date.getTime())) {
        return value;
    }

    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const day = String(date.getDate()).padStart(2, "0");

    return `${year}-${month}-${day}`;
}
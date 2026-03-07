// POList.js

document.addEventListener("DOMContentLoaded", () => {
    const columns = [
        "PoNo", "LnKey", "PODate", "Status", "Vendor", "VendorName", "ItemCode", "ItemDesc",
        "QtyOrdered", "QtyRcpt", "QtyBalance", "QtyInvoiced", "UnitCost", "Amount", "RequiredDate"
    ];

    loadHeader(columns);

    document.getElementById("selectA").addEventListener("change", () => {
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

                    addCell(trMain, v.poNo);
                    addCell(trMain, v.lnKey);
                    addCell(trMain, v.status);
                    addCell(trMain, v.vendor);
                    addCell(trMain, v.vendorName);
                    addCell(trMain, v.itemCode);
                    addCell(trMain, v.itemDesc);
                    addCell(trMain, v.qtyOrdered);
                    addCell(trMain, v.qtyRcpt);
                    addCell(trMain, v.qtyBalance);
                    addCell(trMain, v.qtyInvoiced);
                    addCell(trMain, v.unitCost);
                    addCell(trMain, v.amount);

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
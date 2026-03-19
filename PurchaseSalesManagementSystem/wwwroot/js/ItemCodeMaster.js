document.addEventListener("DOMContentLoaded", () => {
    const btnSearch = document.getElementById("btnSearch");
    const btnExport = document.getElementById("btnExport");
    const itemCodeInput = document.getElementById("itemCode");

    btnSearch.addEventListener("click", loadItemCodeMasterData);
    itemCodeInput.addEventListener("keydown", event => {
        if (event.key === "Enter") {
            event.preventDefault();
            loadItemCodeMasterData();
        }
    });

    btnExport.addEventListener("click", () => {
        const itemCode = document.getElementById("itemCode").value;
        const excludeInactiveItems = document.getElementById("excludeInactiveItems").checked;
        const url = `/ItemCodeMaster/ExportToExcel?itemCode=${encodeURIComponent(itemCode)}&excludeInactiveItems=${excludeInactiveItems}`;

        window.location.href = url;
    });

    loadItemCodeMasterData();
});

function loadItemCodeMasterData() {
    const itemCode = document.getElementById("itemCode").value;
    const excludeInactiveItems = document.getElementById("excludeInactiveItems").checked;
    const url = `/ItemCodeMaster/GetItemCodeMasterData?itemCode=${encodeURIComponent(itemCode)}&excludeInactiveItems=${excludeInactiveItems}`;

    fetch(url)
        .then(res => res.json())
        .then(data => {
            const thead = document.querySelector("#gridMain thead tr");
            const tbody = document.querySelector("#gridMain tbody");
            const recordCount = document.getElementById("recordCount");
            const btnExport = document.getElementById("btnExport");

            thead.innerHTML = "";
            tbody.innerHTML = "";

            if (!Array.isArray(data) || data.length === 0) {
                recordCount.textContent = "0 records";
                btnExport.disabled = true;
                return;
            }

            const columns = Object.keys(data[0]);

            columns.forEach(column => {
                const th = document.createElement("th");
                th.textContent = column;
                thead.appendChild(th);
            });

            data.forEach(row => {
                const tr = document.createElement("tr");

                columns.forEach(column => {
                    addCell(tr, formatValue(row[column]));
                });

                tbody.appendChild(tr);
            });

            recordCount.textContent = `${data.length} records`;
            btnExport.disabled = false;
        });
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
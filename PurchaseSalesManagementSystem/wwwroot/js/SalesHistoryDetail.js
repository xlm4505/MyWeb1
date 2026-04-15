// 入力値→コードのマッピング
const customerMap = new Map(); // "QCode - QName" → QCode
const itemCodeMap = new Map(); // "ItemCode - ItemDesc" → ItemCode

document.addEventListener("DOMContentLoaded", function () {
    loadCustomers();
    loadItemCodes();
    loadItemDescs();
    setDefaultDates();
});

function loadCustomers() {
    fetch("/SalesHistoryDetail/GetCustomers")
        .then(res => res.json())
        .then(data => {
            const dl = document.getElementById("customerList");
            dl.innerHTML = "";
            customerMap.clear();

            data.forEach(c => {
                const display = c.qCode + " - " + c.qName;
                customerMap.set(display, c.qCode);
                const opt = document.createElement("option");
                opt.value = display;
                dl.appendChild(opt);
            });
        });
}

function loadItemCodes() {
    fetch("/SalesHistoryDetail/GetItemCodes")
        .then(res => res.json())
        .then(data => {
            const dl = document.getElementById("itemCodeList");
            dl.innerHTML = "";
            itemCodeMap.clear();

            data.forEach(item => {
                const display = item.itemCode + " - " + item.itemDesc;
                itemCodeMap.set(display, item.itemCode);
                const opt = document.createElement("option");
                opt.value = display;
                dl.appendChild(opt);
            });
        });
}

function loadItemDescs() {
    fetch("/SalesHistoryDetail/GetItemDescs")
        .then(res => res.json())
        .then(data => {
            const dl = document.getElementById("itemDescList");
            dl.innerHTML = "";

            data.forEach(item => {
                const opt = document.createElement("option");
                opt.value = item.itemDesc;
                dl.appendChild(opt);
            });
        });
}

function setDefaultDates() {
    const today = new Date().toISOString().slice(0, 10);
    document.getElementById("dateFrom").value = today;
    document.getElementById("dateTo").value = today;
}

async function doRun() {
    const customerInput = document.getElementById("customer").value.trim();
    const itemCodeInput = document.getElementById("itemCode").value.trim();
    const itemDesc      = document.getElementById("itemDesc").value.trim();
    const dateFrom      = document.getElementById("dateFrom").value;
    const dateTo        = document.getElementById("dateTo").value;

    // マップから正確なコードを取得。一致しない場合は空（*ALL扱い）
    const customer = customerMap.get(customerInput) ?? (customerInput === "" ? "" : customerInput);
    const itemCode = itemCodeMap.get(itemCodeInput) ?? (itemCodeInput === "" ? "" : itemCodeInput);

    const logList = document.getElementById("logList");
    logList.innerHTML = `<li class="list-group-item">Export Processing...</li>`;
    document.getElementById("checklist").style.display = "block";

    try {
        const params = new URLSearchParams({ customer, itemCode, itemDesc, dateFrom, dateTo });
        const res = await fetch("/SalesHistoryDetail/Run", { method: "POST", body: params });

        if (!res.ok) {
            logList.innerHTML = `<li class="list-group-item text-danger">Export Error</li>`;
            return;
        }

        const blob = await res.blob();
        const cd = res.headers.get("Content-Disposition") ?? "";
        const match = cd.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/);
        const filename = match
            ? match[1].replace(/['"]/g, "")
            : `Sales History Detail Report_${new Date().toISOString().slice(0, 10)}.xlsx`;

        const url = URL.createObjectURL(blob);
        const a = document.createElement("a");
        a.href = url;
        a.download = filename;
        a.click();
        URL.revokeObjectURL(url);

        logList.innerHTML = `<li class="list-group-item">Export Complete</li>`;
    } catch {
        logList.innerHTML = `<li class="list-group-item text-danger">Export Error</li>`;
    }
}

const itemCodeMap = new Map();
const vendorCodeMap = new Map();

document.addEventListener("DOMContentLoaded", () => {
    loadVendors();
    loadItemCodes();
    loadUsers();

    document.getElementById("btnRun").addEventListener("click", () => {

        // マップから正確なコードを取得。一致しない場合は空（*ALL扱い）
        const itemCodeInput = document.getElementById("itemCode").value.trim();
        const itemCode = itemCodeMap.get(itemCodeInput) ?? (itemCodeInput === "" ? "" : itemCodeInput);

        const vendorCodeInput = document.getElementById("vendorNo").value.trim();
        const vendorCode = vendorCodeMap.get(vendorCodeInput) ?? (vendorCodeInput === "" ? "" : vendorCodeInput);

        const dateFrom = document.getElementById("dateFrom").value;
        const dateTo = document.getElementById("dateTo").value;
        const poNo = document.getElementById("poNo").value;
        const invoiceNo = document.getElementById("invoiceNo").value;
        const receiptNo = document.getElementById("receiptNo").value;
        const userName = document.getElementById("userName").value;

        let url = `/PurchaseOrderReceiptHistory/ExportToExcel?dateFrom=${dateFrom}&dateTo=${dateTo}&vendorCode=${vendorCode}&poNo=${poNo}&invoiceNo=${invoiceNo}&receiptNo=${receiptNo}&itemCode=${itemCode}&userName=${userName}`;

        window.location.href = url;
    });
});

function loadVendors() {
    fetch("/PurchaseOrderReceiptHistory/GetVendors")
        .then(res => res.json())
        .then(data => {
            const dl = document.getElementById("vendorNoList");
            dl.innerHTML = "";
            vendorCodeMap.clear();

            data.forEach(vendor => {
                const display = vendor.vendorNo + " - " + vendor.vendorName;
                vendorCodeMap.set(display, vendor.vendorNo);
                const opt = document.createElement("option");
                opt.value = display;
                dl.appendChild(opt);
            });
        });
}

function loadItemCodes() {
    fetch("/PurchaseOrderReceiptHistory/GetItems")
        .then(res => res.json())
        .then(data => {
            const dl = document.getElementById("itemCodeList");
            dl.innerHTML = "";
            itemCodeMap.clear();

            data.forEach(item => {
                const display = item.itemCode + " - " + item.desc;
                itemCodeMap.set(display, item.itemCode);
                const opt = document.createElement("option");
                opt.value = display;
                dl.appendChild(opt);
            });
        });
}

function loadUsers() {
    fetch("/PurchaseOrderReceiptHistory/GetUsers")
        .then(res => res.json())
        .then(data => {
            const userSelect = document.getElementById("userName");
            userSelect.innerHTML = "";

            data.forEach(v => {
                const opt = document.createElement("option");
                opt.value = v.userCreatedKey;
                opt.textContent = v.userName;
                opt.dataset.name = v.userName;
                userSelect.appendChild(opt);
            });

            userSelect.selectedIndex = 0;
        });
}
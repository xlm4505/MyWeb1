document.addEventListener("DOMContentLoaded", () => {
    loadVendors();
    loadItemss();
    loadUsers();

    document.getElementById("vendorCode").addEventListener("change", function () {
        const selected = this.options[this.selectedIndex];
        document.getElementById("vendorName").value = selected.dataset.name || "";
    });
    document.getElementById("itemCode").addEventListener("change", function () {
        const selected = this.options[this.selectedIndex];
        document.getElementById("desc").value = selected.dataset.name || "";
    });


    document.getElementById("btnRun").addEventListener("click", () => {
        const dateFrom = document.getElementById("dateFrom").value;
        const dateTo = document.getElementById("dateTo").value;
        const vendorCode = document.getElementById("vendorCode").value;
        const poNo = document.getElementById("poNo").value;
        const invoiceNo = document.getElementById("invoiceNo").value;
        const receiptNo = document.getElementById("receiptNo").value;
        const itemCode = document.getElementById("itemCode").value;
        const userName = document.getElementById("userName").value;

        let url = `/PurchaseOrderReceiptHistory/ExportToExcel?dateFrom=${dateFrom}&dateTo=${dateTo}&vendorCode=${vendorCode}&poNo=${poNo}&invoiceNo=${invoiceNo}&receiptNo=${receiptNo}&itemCode=${itemCode}&userName=${userName}`;

        window.location.href = url;
    });
});

function loadVendors() {
    fetch("/PurchaseOrderReceiptHistory/GetVendors")
        .then(res => res.json())
        .then(data => {
            const vendorSelect = document.getElementById("vendorCode");
            vendorSelect.innerHTML = "";

            data.forEach(v => {
                const opt = document.createElement("option");
                opt.value = v.vendorNo;
                opt.textContent = v.vendorNo;
                opt.dataset.name = v.vendorName;
                vendorSelect.appendChild(opt);
            });

            vendorSelect.selectedIndex = 0;
            vendorSelect.dispatchEvent(new Event("change"));
        });
}

function loadItemss() {
    fetch("/PurchaseOrderReceiptHistory/GetItems")
        .then(res => res.json())
        .then(data => {
            const itemSelect = document.getElementById("itemCode");
            itemSelect.innerHTML = "";

            data.forEach(v => {
                const opt = document.createElement("option");
                opt.value = v.itemCode;
                opt.textContent = v.itemCode;
                opt.dataset.name = v.desc;
                itemSelect.appendChild(opt);
            });

            itemSelect.selectedIndex = 0;
            itemSelect.dispatchEvent(new Event("change"));
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
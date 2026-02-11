// PurchaseOrder.js
document.addEventListener("DOMContentLoaded", () => {
    loadVendors();

    document.getElementById("vendorCode").addEventListener("change", function () {
        const selected = this.options[this.selectedIndex];
        document.getElementById("vendorName").value = selected.dataset.name || "";
    });


    document.getElementById("btnRun").addEventListener("click", () => {
        const vendor = document.getElementById("vendorCode").value;
        const type = document.getElementById("productType").value;
        const vendorName = document.getElementById("vendorName").value;

        let url = `/PurchaseOrder/ExportToExcel?reportName=PurchaseOrder&productType=${type}&vendorName=${vendorName}`;

        // ★ ALL Vendors の場合は vendor を付けない
        if (vendor !== "00-0000000") {
            url += `&vendor=${vendor}`;
        }

        window.location.href = url;
    });
});

function loadVendors() {
    fetch("/PurchaseOrder/GetVendors")
        .then(res => res.json())
        .then(data => {
            const vendorSelect = document.getElementById("vendorCode");
            vendorSelect.innerHTML = "";

            //const allOpt = document.createElement("option");
            //allOpt.value = "00-0000000";
            //allOpt.textContent = "00-0000000";
            //allOpt.dataset.name = "ALL Vendors";
            //vendorSelect.appendChild(allOpt);

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


// PurchaseOrder.js
const vendorMap = new Map(); // "VendorCode - VendorName" -> { code, name }
let lastVendorDisplay = "";

document.addEventListener("DOMContentLoaded", () => {
    loadVendors();
    enableVendorFocusDropdown();

    document.getElementById("btnRun").addEventListener("click", () => {
        const vendorInput = document.getElementById("vendor").value.trim();
        const type = document.getElementById("productType").value;
        const mappedVendor = vendorMap.get(vendorInput);

        const vendor = mappedVendor
            ? mappedVendor.code
            : (vendorInput === "" ? "00-0000000" : vendorInput.split(" - ")[0].trim());

        const vendorName = mappedVendor
            ? mappedVendor.name
            : (vendorInput.includes(" - ")
                ? vendorInput.split(" - ").slice(1).join(" - ").trim()
                : "All vendors");

        let url = `/PurchaseOrder/ExportToExcel?reportName=PurchaseOrder&productType=${type}&vendorName=${encodeURIComponent(vendorName)}`;

        // ★ ALL Vendors の場合は vendor を付けない
        if (vendor !== "00-0000000") {
            url += `&vendor=${vendor}`;
        }

        window.location.href = url;
    });
});

function enableVendorFocusDropdown() {
    const vendorInput = document.getElementById("vendor");

    vendorInput.addEventListener("focus", () => {
        lastVendorDisplay = vendorInput.value;

        vendorInput.value = "";
        vendorInput.dispatchEvent(new Event("input", { bubbles: true }));
    });

    vendorInput.addEventListener("blur", () => {
        if (vendorInput.value.trim() === "") {
            vendorInput.value = lastVendorDisplay;
        }
    });
}

function loadVendors() {
    fetch("/PurchaseOrder/GetVendors")
        .then(res => res.json())
        .then(data => {
            const vendorList = document.getElementById("vendorList");
            const vendorInput = document.getElementById("vendor");
            vendorList.innerHTML = "";
            vendorMap.clear();

            const allVendorsDisplay = "00-0000000 - All vendors";
            vendorMap.set(allVendorsDisplay, { code: "00-0000000", name: "All vendors" });
            const allOpt = document.createElement("option");
            allOpt.value = allVendorsDisplay;
            vendorList.appendChild(allOpt);

            data
                .filter(v => v.vendorNo !== "00-0000000")
                .forEach(v => {
                    const opt = document.createElement("option");
                    const display = `${v.vendorNo} - ${v.vendorName}`;
                    vendorMap.set(display, { code: v.vendorNo, name: v.vendorName });
                    opt.value = display;
                    vendorList.appendChild(opt);
                });

            vendorInput.value = allVendorsDisplay;
            lastVendorDisplay = vendorInput.value;
        });
}
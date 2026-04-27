// POSeizo.js
const vendorMap = new Map(); // "VendorCode - VendorName" -> { code, name }
let lastVendorDisplay = "";

document.addEventListener("DOMContentLoaded", () => {
    document.getElementById("poDate").value = new Date().toISOString().slice(0, 10);

    loadVendors();
    enableVendorFocusDropdown();
    loadUsers();

    document.getElementById("btnRun").addEventListener("click", runExport);
});

function enableVendorFocusDropdown() {
    const vendorInput = document.getElementById("vendor");

    vendorInput.addEventListener("focus", () => {
        lastVendorDisplay = vendorInput.value;

        // datalist をプルダウンのように表示するため、いったん空文字で input イベントを発火
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
    fetch("/POSeizo/GetVendors")
        .then(res => res.json())
        .then(data => {
            const vendorList = document.getElementById("vendorList");
            const vendorInput = document.getElementById("vendor");
            vendorList.innerHTML = "";
            vendorMap.clear();

            data.forEach(v => {
                const display = `${v.vendorNo} - ${v.vendorName}`;
                vendorMap.set(display, { code: v.vendorNo, name: v.vendorName });
                const opt = document.createElement("option");
                opt.value = display;
                vendorList.appendChild(opt);
            });

            if (data.length > 1) {
                vendorInput.value = `${data[1].vendorNo} - ${data[1].vendorName}`;
                lastVendorDisplay = vendorInput.value;
            }
        });
}

function loadUsers() {
    fetch("/POSeizo/GetUser")
        .then(res => res.json())
        .then(data => {
            const userSelect = document.getElementById("userName");
            userSelect.innerHTML = "";

            data.forEach(u => {
                const opt = document.createElement("option");
                opt.value = u.userName;
                opt.textContent = u.userName;
                userSelect.appendChild(opt);
            });

            userSelect.selectedIndex = 0;
        });
}

async function downloadFromGetUrl(url) {
    const response = await fetch(url);
    if (!response.ok) {
        throw new Error("Failed to download the Excel file.");
    }

    const blob = await response.blob();
    const disposition = response.headers.get("content-disposition") || "";
    const fileNameMatch = disposition.match(/filename\*?=(?:UTF-8'')?"?([^";]+)"?/i);
    const decodedFileName = fileNameMatch ? decodeURIComponent(fileNameMatch[1]) : "PO_SeizoExport.xlsx";
    triggerDownload(blob, decodedFileName);
}

async function downloadFromPostUrl(url, payload) {
    const response = await fetch(url, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(payload)
    });

    if (!response.ok) {
        let errorMessage = "The print process failed.";
        try {
            const json = await response.json();
            if (json?.message) {
                errorMessage = json.message;
            }
        } catch {
            // no-op
        }
        throw new Error(errorMessage);
    }

    const blob = await response.blob();
    const disposition = response.headers.get("content-disposition") || "";
    const fileNameMatch = disposition.match(/filename\*?=(?:UTF-8'')?"?([^";]+)"?/i);
    const decodedFileName = fileNameMatch ? decodeURIComponent(fileNameMatch[1]) : "PurchaseOrder.pdf";
    triggerDownload(blob, decodedFileName);
}

function triggerDownload(blob, fileName) {
    const blobUrl = URL.createObjectURL(blob);
    const anchor = document.createElement("a");
    anchor.href = blobUrl;
    anchor.download = fileName;
    document.body.appendChild(anchor);
    anchor.click();
    anchor.remove();
    URL.revokeObjectURL(blobUrl);
}

async function runExport() {
    const vendorInput = document.getElementById("vendor").value.trim();
    const userName = document.getElementById("userName").value;
    const orderStatus = document.getElementById("orderStatus").value;
    const poEntryDate = document.getElementById("poDate").value;
    const shouldPrint = document.getElementById("printOption").checked;
    const mappedVendor = vendorMap.get(vendorInput);

    const vendor = mappedVendor
        ? mappedVendor.code
        : (vendorInput === "" ? "" : vendorInput.split(" - ")[0].trim());

    const vendorName = mappedVendor
        ? mappedVendor.name
        : (vendorInput.includes(" - ")
            ? vendorInput.split(" - ").slice(1).join(" - ").trim()
            : "");

    let actionUrl = "";
    let checkUrl = "";
    if (vendor === "08-0000250") {
        actionUrl = "/POSeizo/ExportToExcel_TKF";
        checkUrl = "/POSeizo/CheckData_TKF";
    } else {
        actionUrl = "/POSeizo/ExportToExcel_ALL";
        checkUrl = "/POSeizo/CheckData_ALL";
    }

    try {
        const checkResponse = await fetch(checkUrl, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                vendor: vendor,
                userName: userName,
                orderStatus: orderStatus,
                poEntryDate: poEntryDate
            })
        });

        const checkResult = await checkResponse.json();
        if (!checkResult.success) {
            alert(checkResult.message);
            return;
        }

        let excelUrl = `${actionUrl}?reportName=POSeizo`;
        if (vendor !== "00-0000000") {
            excelUrl += `&vendor=${vendor}`;
        }
        excelUrl += `&userName=${encodeURIComponent(userName)}`;
        excelUrl += `&orderStatus=${orderStatus}`;
        excelUrl += `&poEntryDate=${poEntryDate}`;
        excelUrl += `&vendorName=${encodeURIComponent(vendorName)}`;

        await downloadFromGetUrl(excelUrl);

        if (shouldPrint) {
            await downloadFromPostUrl("/POSeizo/RunPrint", {
                vendor: vendor,
                vendorName: vendorName,
                userName: userName,
                orderStatus: orderStatus,
                poEntryDate: poEntryDate
            });
        }
    } catch (error) {
        alert(error?.message || "A communication error has occurred.");
    }
}

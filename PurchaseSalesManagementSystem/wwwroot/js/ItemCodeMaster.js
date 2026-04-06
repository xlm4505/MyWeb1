// ItemMaster.js
document.addEventListener("DOMContentLoaded", () => {

    loadHeader();

    document.getElementById("btnSearch").addEventListener("click", () => {
        let rowCount = 0;

        const itemNo = document.getElementById("itemNo").value;
        const excludeInactive = document.getElementById("chkInactive").checked;

        fetch(`/ItemCodeMaster/GetItemCodeMaster?ItemNo=${itemNo}&ExcludeInactive=${excludeInactive}`)
            .then(res => res.json())
            .then(data => {
                const gridMain = document.querySelector("#gridMain tbody");
                gridMain.innerHTML = "";

                data.forEach(v => {
                    const tr = document.createElement("tr");

                    loadHeader.columns.forEach(col => {
                        const key = loadHeader.map[col]; 
                        let val = v[key];

                        // ★ 日付列はフォーマット
                        if (["dateCreated", "dateUpdated", "lastSold", "lastReceipt"].includes(key)) {
                            val = formatDate(val);
                        }

                        // ★ 0 の場合は空欄にする（AI〜AO列）
                        if (["listCOP", "standard", "discount", "class4", "class5", "contract", "class6"].includes(key)) {
                            if (val === 0) val = "";
                        }

                        const td = document.createElement("td");
                        td.textContent = val ?? "";
                        tr.appendChild(td);
                    });

                    gridMain.appendChild(tr);
                    rowCount++;
                });
            });
    });

    // Export ボタン
    document.getElementById("btnExport").addEventListener("click", () => {
        const itemNo = document.getElementById("itemNo").value;

        const excludeInactive = document.getElementById("chkInactive").checked;
        window.location.href = `/ItemCodeMaster/ExportToExcel?ItemNo=${itemNo}&ExcludeInactive=${excludeInactive}`;
    });
});


// ================================
// ヘッダー生成
// ================================
function loadHeader() {

    loadHeader.columns = [
        "ItemCode", "ItemDesc", "ItemDesc2", "Category", "ProductLineDesc", "ProductType",
        "Inactive", "Weight(lb)", "Whse", "PrimaryVendor", "QtyDisc", "StdSalesPrice", "StdUnitCost",
        "LastCost", "AvgCost", "VenCost(USD)", "VenCost(JPY)", "OnHand", "OpenSO",
        "Available", "OpenPO", "(InShip)", "OnHand ", "OpenSO ", "Available ", "OpenPO ", "(InShip) ",
        "LastSold", "LastReceipt", "ExtendedDescriptionText", "DateCreated", "UserCreated",
        "DateUpdated", "UserUpdated", "List COP", "Standard", "Discount",
        "Class 4", "Class 5", "Contract", "Class 6"
    ];

    const thead = document.querySelector("#gridMain thead tr");
    thead.innerHTML = "";

    loadHeader.columns.forEach(col => {
        const th = document.createElement("th");
        th.textContent = col;
        thead.appendChild(th);
    });
}


// ================================
// 表示名 → JSON キー名（小文字）
// ================================
loadHeader.map = {
    "ItemCode": "itemCode",
    "ItemDesc": "itemDesc",
    "ItemDesc2": "itemDesc2",
    "Category": "category",
    "ProductLineDesc": "productLineDesc",
    "ProductType": "productType",
    "Inactive": "inactive",

    "Weight(lb)": "weight",

    "Whse": "whse",
    "PrimaryVendor": "primaryVendor",
    "QtyDisc": "qtyDisc",

    "StdSalesPrice": "stdSalesPrice",
    "StdUnitCost": "stdUnitCost",
    "LastCost": "lastCost",
    "AvgCost": "avgCost",

    "VenCost(USD)": "venCost_USD_",
    "VenCost(JPY)": "venCost_JPY",

    "OnHand": "onHand",
    "OpenSO": "openSO",
    "Available": "available",
    "OpenPO": "openPO",
    "(InShip)": "inShip",

    "OnHand ": "onHand_",
    "OpenSO ": "openSO_",
    "Available ": "available_",
    "OpenPO ": "openPO_",
    "(InShip) ": "inShip_",

    "LastSold": "lastSold",
    "LastReceipt": "lastReceipt",
    "ExtendedDescriptionText": "extendedDescriptionText",
    "DateCreated": "dateCreated",
    "UserCreated": "userCreated",
    "DateUpdated": "dateUpdated",
    "UserUpdated": "userUpdated",

    "List COP": "listCOP",
    "Standard": "standard",
    "Discount": "discount",
    "Class 4": "class4",
    "Class 5": "class5",
    "Contract": "contract",
    "Class 6": "class6"
};


// ================================
// 日付フォーマット
// ================================
function formatDate(value) {
    if (!value) return "";
    const d = new Date(value);
    if (isNaN(d)) return value;
    return `${d.getFullYear()}/${d.getMonth() + 1}/${d.getDate()}`;
}

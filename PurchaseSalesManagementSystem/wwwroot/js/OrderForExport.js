// OrderForExport.js
document.addEventListener("DOMContentLoaded", () => {
    loadHeader();

    document.getElementById("btnSearch").addEventListener("click", () => {
        let rowCount = 0;
        fetch("/OrderForExport/GetOrderData?salesOrderNo=" + document.getElementById("salesOrderNo").value)
            .then(res => res.json())
            .then(data => {
                const gridMain = document.querySelector("#gridMain tbody");
                gridMain.innerHTML = "";

                data.forEach(v => {
                    const trMain = document.createElement("tr");

                    const td1 = document.createElement("td");
                    td1.textContent = v.salesOffice;
                    
                    const td2= document.createElement("td");
                    td2.textContent = v.salesOrderNo;

                    const td3 = document.createElement("td");
                    td3.textContent = v.orderDateText;

                    const td4 = document.createElement("td");
                    td4.textContent = v.orderType;

					const td5 = document.createElement("td");
                    td5.textContent = v.orderStatus;

					const td6 = document.createElement("td");
                    td6.textContent = v.customerPONo;

                    const td7 = document.createElement("td");
                    td7.textContent = v.customerNo;

					const td8 = document.createElement("td");
                    td8.textContent = v.billToName;

                    const td9 = document.createElement("td");
                    td9.textContent = v.shipToCity;

					const td10 = document.createElement("td");
                    td10.textContent = v.shipVia;

                    const td11 = document.createElement("td");
                    td11.textContent = v.headerComment;

                    const td12 = document.createElement("td");
                    td12.textContent = v.custPO_Ln;

					const td13 = document.createElement("td");
                    td13.textContent = v.itemCode;

                    const td14 = document.createElement("td");
                    td14.textContent = v.itemDescription;

                    const td15 = document.createElement("td");
                    td15.textContent = v.aliasItemNo;

                    const td16 = document.createElement("td");
                    td16.textContent = v.whs;

					const td17 = document.createElement("td");
                    td17.textContent = v.weight;

                    const td18 = document.createElement("td");
                    td18.textContent = v.ordded;

                    const td19 = document.createElement("td");
                    td19.textContent = v.shipped;

					const td20 = document.createElement("td");
                    td20.textContent = v.bo;

					const td21 = document.createElement("td");
					td21.textContent = v.unitPrice;

                    const td22 = document.createElement("td");
                    td22.textContent = v.extensionAmt;

                    const td23 = document.createElement("td");
                    td23.textContent = v.reqDateText;

                    const td24 = document.createElement("td");
					td24.textContent = v.pushOutText;

                    const td25 = document.createElement("td");
                    td25.textContent = v.promiseDateText;

                    const td26 = document.createElement("td");
                    td26.textContent = v.commitDateText;

					const td27 = document.createElement("td");
                    td27.textContent = v.deliveryDateText;

                    const td28 = document.createElement("td");
                    td28.textContent = v.commentText;

                    const td29 = document.createElement("td");
                    td29.textContent = v.unitCost;

					const td30 = document.createElement("td");
                    td30.textContent = v.purchaseOrderNo;

                    const td31 = document.createElement("td");
                    td31.textContent = v.udf_custpono;

					const td32 = document.createElement("td");
                    td32.textContent = v.internalNotes;

                    trMain.appendChild(td1);
                    trMain.appendChild(td2);
                    trMain.appendChild(td3);
					trMain.appendChild(td4);
					trMain.appendChild(td5);
                    trMain.appendChild(td6);
					trMain.appendChild(td7);
					trMain.appendChild(td8);
					trMain.appendChild(td9);
					trMain.appendChild(td10);
					trMain.appendChild(td11);
					trMain.appendChild(td12);
					trMain.appendChild(td13);
					trMain.appendChild(td14);
					trMain.appendChild(td15);
					trMain.appendChild(td16);
					trMain.appendChild(td17);
					trMain.appendChild(td18);
					trMain.appendChild(td19);
					trMain.appendChild(td20);
					trMain.appendChild(td21);
					trMain.appendChild(td22);
					trMain.appendChild(td23);
					trMain.appendChild(td24);
					trMain.appendChild(td25);
                    trMain.appendChild(td26);
					trMain.appendChild(td27);
					trMain.appendChild(td28);
					trMain.appendChild(td29);
					trMain.appendChild(td30);
					trMain.appendChild(td31);
					trMain.appendChild(td32);

                    gridMain.appendChild(trMain);
                    rowCount++;
                });
            });
    });
   
    document.getElementById("btnExport").addEventListener("click", () => {

        const salesOrderNo = document.getElementById("salesOrderNo").value;
        let url = "/OrderForExport/ExportToExcel?salesOrderNo=" + salesOrderNo;

        window.location.href = url;
    });
});

function loadHeader() {
    const columns = [
        "SalesOffice", "SalesOrderNo", "OrderDate", "OrderType", "OrderStatus", "CustomerPONo",
        "CustomerNo", "BillToName", "ShipToCity", "ShipVia", "HeaderComment", "CustPO_Ln", "ItemCode",
        "ItemDescription", "AliasItemNo", "Whs", "Weight", "#Ordded", "#Shipped", "#BO", "UnitPrice",
        "ExtensionAmt", "ReqDate", "PushOut", "PromiseDate", "CommitDate", "DeliveryDate", "CommentText",
        "UnitCost", "PurchaseOrderNo", "UDF_CUSTPONO", "InternalNotes"
    ];

    // ヘッダー生成
    function createHeader(tableId) {
        const thead = document.querySelector(`#${tableId} thead tr`);
        thead.innerHTML = "";
        columns.forEach(col => {
            const th = document.createElement("th");
            th.textContent = col;
            thead.appendChild(th);
        });
    }

    createHeader("gridMain");
}


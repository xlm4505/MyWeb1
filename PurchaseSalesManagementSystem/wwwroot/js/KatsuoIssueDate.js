// KatsuoIssueDate.js
document.addEventListener("DOMContentLoaded", () => {
    loadHeader();

    document.getElementById("searchForm").addEventListener("submit", (e) => {
        e.preventDefault();
        fetch("/KatsuoIssueDate/GetData?userName=" + encodeURIComponent(document.getElementById("userName").value))
            .then(res => res.json())
            .then(data => {
                const tbody = document.querySelector("#gridMain tbody");
                tbody.innerHTML = "";

                data.forEach(v => {
                    const tr = document.createElement("tr");

                    const td1 = document.createElement("td");
                    td1.textContent = v.userName;

                    const td2 = document.createElement("td");
                    td2.textContent = v.id;

                    const td3 = document.createElement("td");
                    td3.textContent = v.issueDateText;

                    tr.appendChild(td1);
                    tr.appendChild(td2);
                    tr.appendChild(td3);

                    tbody.appendChild(tr);
                });

                // 10行分の高さ + theadの高さをmax-heightに設定
                const container = document.getElementById("tableContainer");
                const theadHeight = document.querySelector("#gridMain thead").offsetHeight;
                const rows = document.querySelectorAll("#gridMain tbody tr");
                if (rows.length > 0) {
                    const rowHeight = rows[0].offsetHeight;
                    container.style.maxHeight = (theadHeight + rowHeight * 11) + "px";
                } else {
                    container.style.maxHeight = "";
                }
            });
    });
});

function loadHeader() {
    const columns = ["UserName", "ID", "IssueDate"];

    const thead = document.querySelector("#gridMain thead tr");
    thead.innerHTML = "";
    columns.forEach(col => {
        const th = document.createElement("th");
        th.textContent = col;
        thead.appendChild(th);
    });
}

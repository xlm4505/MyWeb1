// ファイル選択時に一覧を表示
document.getElementById("fileInput").addEventListener("change", function () {
    const fileListItems = document.getElementById("fileListItems");
    fileListItems.innerHTML = "";
    if (this.files.length === 0) {
        const li = document.createElement("li");
        li.textContent = "No File Selected";
        fileListItems.appendChild(li);
    } else {
        for (const file of this.files) {
            const li = document.createElement("li");
            li.textContent = file.name;
            fileListItems.appendChild(li);
        }
    }
});

// タブ切り替え時に画面クリア
document.addEventListener("DOMContentLoaded", () => {
    const tabEl = document.querySelectorAll('button[data-bs-toggle="tab"]');
    tabEl.forEach(tab => {
        tab.addEventListener('shown.bs.tab', function (event) {
            const targetId = event.target.getAttribute("data-bs-target");
            if (targetId === "#upload") {
                document.getElementById("fileInput").value = "";
                document.getElementById("fileListItems").innerHTML = "";
                document.getElementById("uploadMessage").textContent = "";
            }
            if (targetId === "#checklist") {
                document.getElementById("selectAction").selectedIndex = 0;
                document.getElementById("checklistArea").innerHTML = "";
                document.getElementById("messageArea").textContent = "";
            }
        });
    });
});

// チェックリスト生成処理
async function runChecklist() {
    const selectAction = document.getElementById("selectAction").value;
    const msgEl = document.getElementById("messageArea");

    msgEl.className = "mt-3 text-primary fw-bold";
    msgEl.textContent = "In processing...";

    try {
        const formData = new FormData();
        formData.append("selectAction", selectAction);

        const response = await fetch("/KatsuoUploadCheck/RunChecklist", {
            method: "POST",
            body: formData
        });

        if (!response.ok) {
            const errText = await response.text();
            msgEl.className = "mt-3 text-danger fw-bold";
            msgEl.textContent = "Error: " + errText;
            return;
        }

        const blob = await response.blob();
        const disposition = response.headers.get("Content-Disposition") ?? "";
        const match = disposition.match(/filename\*?=(?:UTF-8'')?["']?([^;"'\r\n]+)/i);
        const filename = match
            ? decodeURIComponent(match[1])
            : `Open PO Check List (${selectAction}).xlsx`;

        const url = URL.createObjectURL(blob);
        const a = document.createElement("a");
        a.href = url;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);

        msgEl.className = "mt-3 text-success fw-bold";
        msgEl.textContent = "Process complete!";
    } catch (err) {
        msgEl.className = "mt-3 text-danger fw-bold";
        msgEl.textContent = "Error: " + err.message;
    }
}

async function runUpload() {
    const fileInput = document.getElementById("fileInput");
    const msg = document.getElementById("uploadMessage");

    msg.className = "mt-3 fw-bold";
    msg.textContent = "";

    if (!fileInput.files.length) {
        msg.className = "mt-3 fw-bold text-danger";
        msg.textContent = "No file selected.";
        return;
    }

    const formData = new FormData();
    formData.append("excelFile", fileInput.files[0]);

    msg.className = "mt-3 fw-bold text-primary";
    msg.textContent = "Uploading...";

    try {
        const response = await fetch("/KatsuoUploadCheck/UploadExcel", {
            method: "POST",
            body: formData
        });

        if (!response.ok) {
            const errText = await response.text();
            msg.className = "mt-3 fw-bold text-danger";
            msg.textContent = "Error: " + errText;
            return;
        }

        msg.className = "mt-3 fw-bold text-success";
        msg.textContent = "Upload complete!";
    } catch (e) {
        msg.className = "mt-3 fw-bold text-danger";
        msg.textContent = "Error: " + e.message;
    }
}

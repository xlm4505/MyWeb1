document.addEventListener("DOMContentLoaded", function () {

    const selectTarget = document.getElementById("selectB");
    const selectAction = document.getElementById("selectA");
    const actionButton = document.getElementById("actionButton");
    const fileInput = document.getElementById("fileInput");
    const messageArea = document.getElementById("messageArea");
    const checklist = document.getElementById("checklist");
    const fileListItems = document.getElementById("fileListItems");

    // =========================
    // ファイル一覧表示
    // =========================
    fileInput.addEventListener("change", function () {

        fileListItems.innerHTML = "";

        if (this.files.length === 0) {
            const li = document.createElement("li");
            li.textContent = "No File Selected";
            fileListItems.appendChild(li);
            return;
        }

        for (const file of this.files) {
            const li = document.createElement("li");
            li.textContent = file.name;
            fileListItems.appendChild(li);
        }
    });

    // =========================
    // Target変更制御
    // =========================
    function updateActionState() {

        const target = selectTarget.value;

        if (target === "TK" || target === "CCL") {

            // Upload固定
            selectAction.value = "Upload";
            selectAction.disabled = true;
            actionButton.textContent = "Upload";

        } else if (target === "FJK") {

            // Check/Upload可能
            selectAction.disabled = false;

            if (!selectAction.value) {
                selectAction.value = "Check";
            }

            actionButton.textContent = selectAction.value;
        }
    }

    selectTarget.addEventListener("change", updateActionState);

    selectAction.addEventListener("change", function () {
        if (!selectAction.disabled) {
            actionButton.textContent = this.value;
        }
    });

    // 初期化（超重要）
    updateActionState();

    // =========================
    // 実行ボタン処理
    // =========================
    actionButton.addEventListener("click", function () {

        const target = selectTarget.value;
        const action = selectAction.value;

        if (!fileInput.files.length) {
            alert("Select upload File");
            return;
        }

        const formData = new FormData();
        formData.append("actionType", action);
        formData.append("target", target);

        for (let i = 0; i < fileInput.files.length; i++) {
            formData.append("files", fileInput.files[i]);
        }

        // ===== URL分岐 =====
        let url = "";

        if (target === "TK") {
            url = "/PurchaseReceipt/ProcessTK";
        }
        else if (target === "CCL") {
            url = "/PurchaseReceipt/ProcessCCL";
        }
        else if (target === "FJK") {
            url = action === "Check"
                ? "/PurchaseReceipt/CheckFJK"
                : "/PurchaseReceipt/ProcessFJK";
        }

        messageArea.textContent = action + " Start...";
        checklist.style.display = "none";

        fetch(url, {
            method: "POST",
            body: formData
        })
            .then(async response => {

                if (!response.ok) {
                    const msg = await response.text();
                    throw new Error(msg);
                }

                const isExcelDownload =
                    target === "TK" ||
                    (target === "FJK" && action === "Check");

                // TK / FJK Check は Excel を返す
                if (isExcelDownload) {
                    // ★ ファイル名をレスポンスヘッダから取得
                    const disposition = response.headers.get("Content-Disposition");
                    let filename = "download.xlsx";

                    if (disposition) {

                        // ★ filename*=UTF-8''xxxx を優先
                        const utf8FilenameMatch = disposition.match(/filename\*\=UTF-8''(.+)/);
                        if (utf8FilenameMatch && utf8FilenameMatch[1]) {
                            filename = decodeURIComponent(utf8FilenameMatch[1]);
                        } else {
                            // ★ 通常の filename="xxx"
                            const filenameMatch = disposition.match(/filename=\"?([^\";]+)\"?/);
                            if (filenameMatch && filenameMatch[1]) {
                                filename = filenameMatch[1];
                            }
                        }
                    }

                    const blob = await response.blob();

                    return { blob, filename, isExcelDownload };
                }

                return response.json();
            })
            .then(data => {

                // ===== TK / FJK Check Excelダウンロード =====
                if (data.isExcelDownload) {

                    const { blob, filename } = data;

                    const downloadUrl = window.URL.createObjectURL(blob);
                    const a = document.createElement("a");
                    a.href = downloadUrl;
                    a.download = filename;   // ★ サーバーのファイル名を使用
                    document.body.appendChild(a);
                    a.click();
                    a.remove();
                    window.URL.revokeObjectURL(downloadUrl);

                    messageArea.textContent = "Completed";
                    return;
                }

                // ===== JSON処理 =====
                messageArea.textContent = data.message;

                if (data.success) {
                    checklist.style.display = "block";
                }
            })
            .catch(err => {
                //alert("Error: " + err.message);
                //messageArea.textContent = "";
                //checklist.style.display = "none";
                const msg = err.message.replaceAll("<br>", "\n");

                alert("Error:\n" + msg);

                messageArea.textContent = "";
                checklist.style.display = "none";
            });
    });

});
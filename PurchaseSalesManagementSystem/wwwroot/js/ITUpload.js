async function runUpload() {
    const fileInput = document.getElementById("fileInput");
    const msg = document.getElementById("uploadMessage");

    const errorLogDiv = document.getElementById("errorLogDiv");
    const errorLog = document.getElementById("errorLog");

    errorLogDiv.style.display = "none";
    msg.className = "mt-3 fw-bold";
    msg.textContent = "";
    errorLog.innerHTML = '';

    if (!fileInput.files.length) {
        msg.className = "mt-3 fw-bold text-danger";
        msg.textContent = "No file selected.";
        return;
    }

    document.getElementById('uploadBtn').disabled = true;

    const formData = new FormData();
    formData.append("excelFile", fileInput.files[0]);

    msg.className = "mt-3 fw-bold text-primary";
    msg.textContent = "Uploading...";

    try {
        const response = await fetch("/ITUpload/UploadExcel", {
            method: "POST",
            body: formData
        });

        if (response.ok) {
            const data = await response.json();

            if (data.errorList.length > 0) {
                msg.className = "mt-3 fw-bold text-danger";
                msg.textContent = "Upload failed! (Number of files processed: " + data.successCount + ")";

                // エラーログ作成
                let htmlTxt = '';
                for (let errorMsg of data.errorList) {
                    htmlTxt += '<li class="list-group-item">' + errorMsg.replace(/\r?\n/g, '<br>') + '</li>';
                }
                errorLog.innerHTML = htmlTxt;
                errorLogDiv.style.display = "block";
            }
            else {
                msg.className = "mt-3 fw-bold text-success";
                msg.textContent = "Upload complete! (Number of files processed: " + data.successCount + ")";
            }

        }
        else {
            const errText = await response.text();
            msg.className = "mt-3 fw-bold text-danger";
            msg.textContent = "Error: " + errText;
        }
        
    } catch (e) {
        msg.className = "mt-3 fw-bold text-danger";
        msg.textContent = "Error: " + e.message;
    }

    document.getElementById('uploadBtn').disabled = false;
}
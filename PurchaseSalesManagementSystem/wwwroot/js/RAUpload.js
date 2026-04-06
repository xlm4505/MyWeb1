// OrderForExport.js
document.addEventListener("DOMContentLoaded", () => {
    document.getElementById("fileInput").addEventListener("change", function () {
        const fileListItems = document.getElementById("fileListItems");
        fileListItems.innerHTML = "";
        if (this.files.length === 0) {
            const li = document.createElement("li");
            li.textContent = "No File Selected";
            fileListItems.appendChild(li);
        } else {
			const file = this.files[0];
            const fileName = file.name;
            const extension = fileName.split('.').pop().toLowerCase();
            const allowedExtensions = ['xlsx', 'xls', 'xlsm'];
            if (allowedExtensions.includes(extension)) {
                document.getElementById('messageArea').textContent = '';
                for (const file of this.files) {
                    const li = document.createElement("li");
                    li.textContent = file.name;
                    fileListItems.appendChild(li);
                }
            } else {
                alert('Please select the Receiving Active file (.xlsm).');
            }
        }
    });

    document.getElementById("btnUpload").addEventListener("click", () => {

        const fileInput = document.getElementById('fileInput');
        if (fileInput.files.length > 1) {
            alert('Please select one Excel file.');
            return;
        }

        const file = fileInput.files[0];

        if (!file) {
            alert('Please select the Receiving Active file (.xlsm).');
            return;
        }

        const allowedExtensions = ['xlsx', 'xls', 'xlsm'];

        const extension = file.name.split('.').pop().toLowerCase();
        if (!allowedExtensions.includes(extension)) {
            alert('Please select the Receiving Active file (.xlsm).');
            return;
        }

        document.getElementById('messageArea').textContent = 'Processing on the Receiving Active file.';

        const formData = new FormData();
        formData.append('excelFile', file);

        fetch('/RAUpload/UploadExcel', {
            method: 'POST',
            body: formData
        })
            .then(response => {
                if (!response.ok) throw new Error('Server Error');
                return response.blob();
            })
            .then(blob => {
                // ダウンロードリンクを作成
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = 'RA_StockList_' + getCurrentDateMMddyyyy() + '.xlsx';
                document.body.appendChild(a);
                a.click();
                a.remove();
                window.URL.revokeObjectURL(url);
                document.getElementById('messageArea').textContent = 'RA_StockList file (Excel) created.';
            })
            .catch(error => {
                document.getElementById('messageArea').textContent = '"Process incomplete due to an error(s).';
            });
    });
    function getCurrentDateMMddyyyy() {
        const today = new Date();
        const month = String(today.getMonth() + 1).padStart(2, '0');
        const day = String(today.getDate()).padStart(2, '0');
        const year = today.getFullYear();
        return month + day + year;
    }
});


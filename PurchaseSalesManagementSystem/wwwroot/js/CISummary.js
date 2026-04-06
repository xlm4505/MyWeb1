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
            const allowedExtensions = ['xlsx', 'xls', 'xlsm','pdf'];
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
        const files = fileInput.files;

        if (!files[0]) {
            alert('No File Selected.');
            return;
        }

        const allowedExtensions = ['xlsx', 'xls', 'xlsm', 'pdf'];
        const formData = new FormData();
        for (var i = 0; i < files.length; i++) {
            var file = files[i];
            const extension = file.name.split('.').pop().toLowerCase();
            if (!allowedExtensions.includes(extension)) {
                alert('Please select the file (.xlsm or .pdf).');
                return;
            }
            if (extension == 'pdf') {
                formData.append("pdfFiles", file);
            } else {
				formData.append("excelFiles", file);
            }
        }

        document.getElementById('messageArea').textContent = 'CI Summary process started.';

        fetch('/CISummary/UploadFiles', {
            method: 'POST',
            body: formData
        })
            .then(response => {
                if (!response.ok) throw new Error('Server error');
                return response.blob();
            })
            .then(blob => {
                // ダウンロードリンクを作成
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = 'CISummary-' + getCurrentDateMMddyyyy() + '.zip';
                document.body.appendChild(a);
                a.click();
                a.remove();
                window.URL.revokeObjectURL(url);
                document.getElementById('messageArea').textContent = 'Process complete.';
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


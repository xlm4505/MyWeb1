document.addEventListener("DOMContentLoaded", () => {
    // ファイル選択時に一覧を表示
    document.getElementById("fileInput").addEventListener("change", function () {
        const fileListItems = document.getElementById("fileListItems");
        fileListItems.innerHTML = "";

        if (this.files.length === 0) {
            const li = document.createElement("li");
            li.textContent = "No File Selected";
            fileListItems.appendChild(li);
        } else {
            const allowedExtensions = ['xlsx', 'xls', 'xlsm'];
            const allowedPrefixes = ['Receiving_Active', 'CI_Summary'];
            const invalidExt = Array.from(this.files).find(f => !allowedExtensions.includes(f.name.split('.').pop().toLowerCase()));
            if (invalidExt) {
                alert('Please select Excel files only (.xlsx, .xls, .xlsm).');
                this.value = '';
                return;
            }
            const invalidName = Array.from(this.files).find(f => !allowedPrefixes.some(p => f.name.startsWith(p)));
            if (invalidName) {
                alert(`Invalid file name: "${invalidName.name}"\nFile name must start with "Receiving_Active" or "CI_Summary".`);
                this.value = '';
                return;
            }
            document.getElementById('messageArea').textContent = '';
            for (const file of this.files) {
                const li = document.createElement("li");
                li.textContent = file.name;
                fileListItems.appendChild(li);
            }
        }
    });

    // アップロードボタン
    document.getElementById("btnUpload").addEventListener("click", () => {
        const fileInput = document.getElementById('fileInput');
        const messageArea = document.getElementById('messageArea');

        if (fileInput.files.length === 0) {
            alert('Please select Excel files (.xlsx, .xls, .xlsm).');
            return;
        }

        const allowedExtensions = ['xlsx', 'xls', 'xlsm'];
        const allowedPrefixes = ['Receiving_Active', 'CI_Summary'];
        const invalidExt = Array.from(fileInput.files).find(f => !allowedExtensions.includes(f.name.split('.').pop().toLowerCase()));
        if (invalidExt) {
            alert('Please select Excel files only (.xlsx, .xls, .xlsm).');
            return;
        }
        const invalidName = Array.from(fileInput.files).find(f => !allowedPrefixes.some(p => f.name.startsWith(p)));
        if (invalidName) {
            alert(`Invalid file name: "${invalidName.name}"\nFile name must start with "Receiving_Active" or "CI_Summary".`);
            return;
        }

        messageArea.textContent = 'Uploading...';

        const formData = new FormData();
        for (const file of fileInput.files) {
            formData.append('excelFiles', file);
        }

        fetch('/ReceivingActive/UploadExcel', {
            method: 'POST',
            body: formData
        })
        .then(response => {
            const contentType = response.headers.get('Content-Type') || '';
            if (contentType.includes('application/json')) {
                return response.json().then(json => { throw new Error(json.error_msg || 'Server Error'); });
            }
            if (!response.ok) throw new Error('Server Error');
            const disposition = response.headers.get('Content-Disposition') || '';
            const match = disposition.match(/filename[^;=\n]*=([^;\n]*)/);
            const fileName = match ? match[1].replace(/['"]/g, '').trim() : 'download.xlsm';
            return response.blob().then(blob => ({ blob, fileName }));
        })
        .then(({ blob, fileName }) => {
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = fileName;
            document.body.appendChild(a);
            a.click();
            a.remove();
            window.URL.revokeObjectURL(url);
            messageArea.textContent = 'Download complete.';
        })
        .catch(error => {
            messageArea.textContent = 'Error: ' + error.message;
        });
    });
});

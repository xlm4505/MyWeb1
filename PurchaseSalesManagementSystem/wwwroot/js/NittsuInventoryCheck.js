document.addEventListener("DOMContentLoaded", () => {
    const file1 = document.getElementById("file1");
    const file2 = document.getElementById("file2");
    const fileListItems = document.getElementById("fileListItems");
    const messageArea = document.getElementById("messageArea");
    const compareButton = document.getElementById("btnCompare");

    file1.addEventListener("change", updateFileList);
    file2.addEventListener("change", updateFileList);
    compareButton.addEventListener("click", compareFiles);

    function updateFileList() {
        fileListItems.innerHTML = "";
        const f1 = file1.files[0];
        const f2 = file2.files[0];

        if (!f1 && !f2) {
            const li = document.createElement("li");
            li.textContent = "No File Selected";
            fileListItems.appendChild(li);
            return;
        }

        if (f1) {
            const li = document.createElement("li");
            li.textContent = "Mas Inventory Data: " + f1.name;
            fileListItems.appendChild(li);
        }

        if (f2) {
            const li = document.createElement("li");
            li.textContent = "Nittsu Inventory Data: " + f2.name;
            fileListItems.appendChild(li);
        }
    }

    async function compareFiles() {
        const f1 = file1.files[0];
        const f2 = file2.files[0];

        if (!f1 || !f2) {
            alert("Please select both Mas and Nittsu inventory data files!");
            return;
        }

        messageArea.textContent = "Compare Start...";

        const formData = new FormData();
        formData.append("masFile", f1);
        formData.append("nittsuFile", f2);

        try {
            const response = await fetch('/NittsuInventoryCheck/Compare', {
                method: 'POST',
                body: formData
            });

            const contentType = response.headers.get('Content-Type') ?? '';
            if (contentType.includes('application/json')) {
                const data = await response.json();
                throw new Error(data.error_msg ?? 'Server Error');
            }
            if (!response.ok) {
                throw new Error('Server Error');
            }

            const blob = await response.blob();
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = 'Compare.xlsx';
            document.body.appendChild(a);
            a.click();
            a.remove();
            window.URL.revokeObjectURL(url);
            messageArea.textContent = 'Compare Complete!';
        } catch (error) {
            messageArea.textContent = error.message;
        }
    }

    updateFileList();
});
const selectA = document.getElementById("selectA");
const targetDataRow = document.getElementById("targetDataRow");

selectA.addEventListener("change", function () {
    targetDataRow.style.display = this.value === "report" ? "none" : "flex";
});

document.addEventListener("DOMContentLoaded", async () => {
    await loadTargetYears();
});

async function loadTargetYears() {
    const targetYearSelect = document.getElementById("targetYear");
    targetYearSelect.innerHTML = "";

    try {
        const res = await fetch('/MonthlySalesSummary/GetTargetYears');
        if (!res.ok) throw new Error('Target year fetch failed');

        const years = await res.json();
        const currentYear = new Date().getFullYear();

        years.forEach(year => {
            const option = document.createElement('option');
            option.value = year;
            option.textContent = year;
            if (Number(year) === currentYear) {
                option.selected = true;
            }
            targetYearSelect.appendChild(option);
        });

        if (!targetYearSelect.value && years.length > 0) {
            targetYearSelect.value = years[0];
        }
    } catch {
        const fallbackYear = new Date().getFullYear();
        const option = document.createElement('option');
        option.value = fallbackYear;
        option.textContent = fallbackYear;
        option.selected = true;
        targetYearSelect.appendChild(option);
    }
}

function doSearch() {
    const exportTarget = document.getElementById("selectA").value;
    const targetYear = document.getElementById("targetYear").value;
    const targetData = document.getElementById("targetData").value;
    const params = new URLSearchParams({
        exportTarget,
        targetYear
    });

    if (exportTarget === "summary") {
        params.append("targetData", targetData);
    }

    window.location.href = `/MonthlySalesSummary/ExportToExcel?${params.toString()}`;
}

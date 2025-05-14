function exportToExcel(tableId, filename, sheetName) {
    const table = document.getElementById(tableId);
    const headers = Array.from(table.querySelectorAll("thead th")).map(th => th.innerText.trim());
    const excludeIndex = headers.findIndex(h => h.toLowerCase() === "actions");

    const data = [];
    const rows = table.querySelectorAll("tbody tr");

    const filteredHeaders = headers.filter((_, i) => i !== excludeIndex);
    data.push(filteredHeaders);

    rows.forEach(row => {
        const cells = Array.from(row.querySelectorAll("td"));
        if (cells.length < 2) return;
        const rowData = cells
            .filter((_, i) => i !== excludeIndex)
            .map(cell => cell.innerText.trim());
        data.push(rowData);
    });

    const worksheet = XLSX.utils.aoa_to_sheet(data);
    const workbook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(workbook, worksheet, sheetName);
    XLSX.writeFile(workbook, filename);
}
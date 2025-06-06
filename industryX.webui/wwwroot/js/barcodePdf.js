﻿async function downloadBarcodePdf(barcode, productName) {
    try {
        const { jsPDF } = window.jspdf;
        const doc = new jsPDF();

        const canvas = document.createElement("canvas");

        JsBarcode(canvas, barcode, {
            format: "CODE128",
            displayValue: true,
            fontSize: 16,
            height: 50,
            width: 2
        });

        const imgData = canvas.toDataURL("image/png");

        doc.setFontSize(16);
        doc.text(`Product Name: ${productName}`, 20, 20);
        doc.text(`Barcode: ${barcode}`, 20, 30);
        doc.addImage(imgData, "PNG", 20, 40, 160, 40);

        doc.save(`Barcode_${barcode}.pdf`);
    } catch (error) {
        console.error("Error generating barcode PDF:", error);
        alert("An error occurred while generating the barcode PDF.");
    }
}

function generateBarcode(prefix, targetInputId) {
    const random = Math.floor(Math.random() * 1000000)
        .toString()
        .padStart(5, "0");

    const barcode = `${prefix}-${random}`;
    document.getElementById(targetInputId).value = barcode;
}

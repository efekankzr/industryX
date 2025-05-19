function generateUrlFromName(sourceInputId, targetInputId) {
    const turkishMap = {
        'ç': 'c',
        'ğ': 'g',
        'ı': 'i',
        'ö': 'o',
        'ş': 's',
        'ü': 'u',
        'Ç': 'c',
        'Ğ': 'g',
        'İ': 'i',
        'Ö': 'o',
        'Ş': 's',
        'Ü': 'u'
    };

    const sourceInput = document.getElementById(sourceInputId);
    const targetInput = document.getElementById(targetInputId);

    if (!sourceInput || !targetInput) return;

    let text = sourceInput.value;

    // Türkçe karakterleri çevir
    text = text.replace(/[\u00C0-\u024F]/g, function (char) {
        return turkishMap[char] || char;
    });

    // Boşlukları "-" yap, tümünü küçük harfe çevir
    text = text
        .trim()
        .toLowerCase()
        .replace(/ /g, '-')
        .replace(/[^\w-]+/g, ''); // sadece harf, rakam, _ ve - kalsın

    targetInput.value = text;
}

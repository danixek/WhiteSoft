
async function loadCards() {
    const templateResponse = await fetch('/templates/productCard.html');
    const template = await templateResponse.text();

    const containers = [
        { id: 'pinnedContainer', api: '/api/orders/pinned' },
        { id: 'productsContainer', api: '/api/orders/eshop' }
    ];

    for (const c of containers) {
        const container = document.getElementById(c.id);
        if (!container) continue; // pokud kontejner na stránce není, přeskočí

        const response = await fetch(c.api);
        const products = await response.json();

        container.innerHTML = '';

        products.forEach(p => {
            let cardHtml = template
                .replace(/{{id}}/g, p.id)
                .replace(/{{name}}/g, p.name)
                .replace(/{{price}}/g, p.price.toFixed(2))
                .replace(/{{imageUrl}}/g, p.imageUrl);

            const div = document.createElement('div');
            div.classList.add('col-4', 'my-4'); // bootstrap grid
            div.innerHTML = cardHtml;
            container.appendChild(div);
        });
    }

    // Přidání listenerů pro tlačítka do košíku
    document.addEventListener('click', async (e) => {
        const btn = e.target.closest('.add-to-cart');
        if (!btn) return;

        const id = parseInt(btn.getAttribute('data-id'));
        await fetch('/api/orders/AddToCart', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ productId: id, quantity: 1 })
        });

        alert("Produkt přidán do košíku!");
        updateCartCount(); // aktualizace badge
    });

}

// spustí se automaticky při načtení stránky
loadCards();
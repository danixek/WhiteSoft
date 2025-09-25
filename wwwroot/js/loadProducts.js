
async function loadProducts() {
    const templateResponse = await fetch('/templates/productCard.html');
    const template = await templateResponse.text();

    const containers = [
        { id: 'pinnedContainer', api: '/api/orders/pinned' },
        { id: 'productsContainer', api: '/api/orders/eshop' }
    ];

    for (const c of containers) {
        const container = document.getElementById(c.id);
        if (!container) continue;

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
            div.classList.add('col-4', 'my-3', 'd-flex');
            div.innerHTML = cardHtml;
            container.appendChild(div);
        });
    }

    // add the listeners for the buttons to the cart
    document.addEventListener('click', async (e) => {
        const btn = e.target.closest('.add-to-cart');
        if (!btn) return;

        const id = parseInt(btn.getAttribute('data-id'));
        const q = parseInt(btn.getAttribute('data-quantity'));
        await fetch('/api/orders/AddToCart', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            body: JSON.stringify({ productId: id, quantity: q })
        });

        alert("Produkt přidán do košíku!");
        updateCartCount();
        loadCart();
    });

}

// it starts in the same time when the page is reloaded
loadProducts();
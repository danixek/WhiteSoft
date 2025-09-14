async function updateCartCount() {
    const response = await fetch('/api/orders/cart');
    const items = await response.json();

    let totalQty = 0;
    items.forEach(item => totalQty += item.quantity);

    const countElem = document.getElementById('cartCount');
    if (countElem) {
        countElem.textContent = totalQty;
    }
}

// zavoláme při načtení stránky
updateCartCount();
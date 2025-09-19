async function updateCartCount() {
    const response = await fetch('/api/orders/cart');
    const items = await response.json();

    let totalQty = 0;
    items.forEach(item => totalQty += item.details.quantity);

    const cartCount = document.getElementById('cartCount');
    if (cartCount) {
        cartCount.textContent = totalQty;
    }
}

updateCartCount();
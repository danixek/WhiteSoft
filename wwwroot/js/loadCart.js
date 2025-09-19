document.addEventListener('DOMContentLoaded', async () => {

    function getCartFromCookie() {
        // It load the cart from cookies
        const cartJson = document.cookie.split('; ').find(row => row.startsWith('Cart='));
        return cartJson ? JSON.parse(decodeURIComponent(cartJson.split('=')[1])) : [];
    }

    async function loadCart() {
        const container = document.getElementById('cartContainer')

        container.innerHTML = '';

        const cart = getCartFromCookie();

        if (!cart.length) {
            const cartEmpty = document.getElementById('cartEmpty')
            cartEmpty.innerHTML = "&nbsp;je prázdný";
            const formCartEmpty = document.querySelector('form');
            formCartEmpty.remove();
            return;
        }
        const customerName = document.getElementById('CustomerName').value;
        const customerEmail = document.getElementById('CustomerEmail').value;

        const styleTextLeft = 'text-align: left; padding-left: 20px';
        const styleTextRight = 'text-align: right; padding-right: 20px';

        // Cart table
        let total = 0;
        const response = await fetch('/api/orders/cart');
        const cartItems = await response.json();

        for (const item of cartItems) {
            const row = document.createElement('tr');
            row.dataset.productId = item.productId;
            item.details.lineTotal = item.details.price * item.details.quantity;
            total += item.details.lineTotal;

            row.innerHTML = `
            <td style="${styleTextLeft}">${item.details.name}</td>
            <td>
                <div class="quantity-control">
                <button type="button" class="decrease-btn">&lt;</button>
                <span class="quantity">${item.details.quantity}</span>
                <button type="button" class="increase-btn">&gt;</button>
            </div>
            <td style="${styleTextRight}">${item.details.price.toFixed(2)} Kč</td>
            <td style="${styleTextRight}">${(item.details.total || 0).toFixed(2)} Kč</td>
            <td>
                <button type="button" class="delete-btn text-danger fw-bold">x</button>
            </td>
        `;
            container.appendChild(row);
        }

        // Total cost
        const totalRow = document.createElement('tr');
        totalRow.innerHTML = `
        <td style="${styleTextLeft}">Celkem:</td>
        <td></td>
        <td></td>
        <td style="${styleTextRight}">${total.toFixed(2)} Kč</td>
        <td></td>`;
        container.appendChild(totalRow);

        // Send button form
        document.getElementById('submitCart').addEventListener('click', () => submitOrder(customerName, customerEmail));
        document.getElementById('submitCartNoPay').addEventListener('click', () => submitOrder(customerName, customerEmail));
    }

    async function submitOrder(customerName, customerEmail) {
        const cart = getCartFromCookie();
        if (!cart.length) return alert("Košík je prázdný");

        if (!customerName || !customerEmail) return alert("Vyplňte jméno a e-mail");
        const payload = {
            CustomerName: customerName,
            CustomerEmail: customerEmail,
            Cart: cart
        };

        try {
            const response = await fetch('/api/orders/checkout', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload),
                credentials: 'same-origin'
            });

            // Success sent
            if (response.ok) {
                // Redirect to main page
                window.location.href = '/';
            } else {
                // Error
                const errorText = await response.text();
                console.error(errorText);
                alert(errorText || "Při odesílání objednávky nastala chyba.");
            }
            alert("Objednávka byla odeslána!");
        } catch (err) {
            console.error(err);
            alert("Při odesílání objednávky nastala neočekávaná chyba.");
        }
    }

    function updateCartCookie(productId, newQuantity) {
        const cartJson = document.cookie
            .split('; ')
            .find(row => row.startsWith('Cart='));
        if (!cartJson) return;

        const cart = JSON.parse(decodeURIComponent(cartJson.split('=')[1]));

        // search the product and update the quantity
        const item = cart.find(p => p.ProductId == productId);
        if (item) item.Quantity = newQuantity;

        // update the cookie
        document.cookie = `Cart=${encodeURIComponent(JSON.stringify(cart))}; path=/; max-age=${7 * 24 * 60 * 60}`;
    }
    function deleteFromCart(productId) {
        const cartJson = document.cookie
            .split('; ')
            .find(row => row.startsWith('Cart='));
        if (!cartJson) return;

        let cart = JSON.parse(decodeURIComponent(cartJson.split('=')[1]));

        // Reject the product from cart and save changes to cookie
        cart = cart.filter(item => item.ProductId != productId);
        document.cookie = `Cart=${encodeURIComponent(JSON.stringify(cart))}; path=/; max-age=${7 * 24 * 60 * 60}`;
    }


    document.addEventListener('click', (e) => {
        const row = e.target.closest('tr');
        if (!row) return;

        const quantitySpan = row.querySelector('.quantity');
        let qty = parseInt(quantitySpan.textContent);
        const productId = row.dataset.productId;

        if (e.target.classList.contains('increase-btn')) {
            qty++;
            quantitySpan.textContent = qty;
            updateCartCookie(productId, qty);
            loadCart();
            updateCartCount();
        }
        if (e.target.classList.contains('decrease-btn')) {
            if (qty > 1) qty--; else deleteFromCart(productId);
            quantitySpan.textContent = qty;
            updateCartCookie(productId, qty);
            loadCart();
            updateCartCount();
        }
        if (e.target.classList.contains('delete-btn')) {
            deleteFromCart(productId);
            loadCart();
            updateCartCount();
        }
    });

    loadCart()
});

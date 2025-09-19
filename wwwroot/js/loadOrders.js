function renderPagination(totalPages) {
    const pagination = document.querySelector('#pagination');
    pagination.classList.add('text-center');
    pagination.innerHTML = '';

    for (let i = 1; i <= totalPages; i++) {
        const btn = document.createElement('button');
        btn.textContent = i;
        btn.className = 'btn btn-sm btn-secondary secondary mx-1';
        if (i === currentPage) btn.classList.add('active');
        btn.onclick = () => loadOrders(i);
        pagination.appendChild(btn);
    }
}

const pageSize = 30;
let currentPage = parseInt(localStorage.getItem('ordersPage')) || 1;
async function loadOrders(page = currentPage) {
    const response = await fetch(`/api/orders?page=${page}`);
    const data = await response.json();
    const orders = data.orders; 

    const tbody = document.querySelector('#ordersTable tbody');
    tbody.innerHTML = '';

    orders.forEach(order => {
        const tr = document.createElement('tr');
        const date = new Date(order.createdAt);

        const formattedDate = date.toLocaleString(undefined, {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit',
            hour12: false,
        });

        tr.innerHTML = `
                <td>${order.id}</td>
                <td>${order.customerName}</td>
                <td>${order.total.toFixed(2)} Kč</td>
                <td>
                    <button class="btn btn-sm btn-primary" onclick="loadOrderItems(${order.id})">
                        Položky
                    </button>
                </td>
                <td>${formattedDate}</td>
            `;
        tbody.appendChild(tr);
    });

    // update the actual page
    currentPage = page;
    renderPagination(data.totalPages);
}

loadOrders();

// every 10 seconds refresh
setInterval(loadOrders, 10000);

async function loadOrderItems(orderId) {
    const response = await fetch(`/api/orders/orderitems?orderId=${orderId}`);
    const items = await response.json();

    const tbody = document.querySelector('#orderItemsTableBody');
    tbody.innerHTML = '';

    items.forEach(item => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
                <td>${item.productName}</td>
                <td>${item.quantity}</td>
                <td>${item.price.toFixed(2)}</td>
                <td>${(item.price * item.quantity).toFixed(2)}</td>
            `;
        tbody.appendChild(tr);
    });

    // open modal
    const modal = new bootstrap.Modal(document.getElementById('orderItemsModal'));
    modal.show();
}
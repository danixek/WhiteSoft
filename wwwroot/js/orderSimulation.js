// The Cart and orders simulation
let carts = [
    [{ productId: 1, quantity: 2 }, { productId: 2, quantity: 1 }],
    [{ productId: 1, quantity: 2 }, { productId: 3, quantity: 2 }],
    [{ productId: 2, quantity: 1 }, { productId: 3, quantity: 1 }],
    [{ productId: 1, quantity: 1 }, { productId: 2, quantity: 2 }],
];

// Customer simulation
const customers = [
    { name: "Michal Novák", email: "michal.novak@email.com" },
    { name: "Daniel Černý", email: "daniel.cerny@email.com" },
    { name: "Anna Nováková", email: "anna.novakova@email.com" },
    { name: "Seznam a. s.", email: "info@seznam.cz" },
    { name: "Alza a. s.", email: "info@alza.cz" },
    { name: "Pojišťák.NET s. r. o.", email: "info@pojistak.net" },
    { name: "Air Bank a. s.", email: "info@airbank.cz" }
];

// Send the simulation orders to API
async function submitOrderSimulation() {
    // select random customer and his cart
    const customer = customers[Math.floor(Math.random() * customers.length)];
    const customerName = customer.name;
    const customerEmail = customer.email;

    const cart = carts[Math.floor(Math.random() * carts.length)];

    const orderData = {
        customerName: customerName,
        customerEmail: customerEmail,
        cart
    };

    const response = await fetch('/api/orders/checkout', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-Simulation': 'true'
        },
        body: JSON.stringify(orderData)
    });

    try {

        if (!response.ok) throw new Error(`Chyba`); 
        console.log("✅ Simulace odeslána", customerName);

    } catch (err) {
        console.error("❌ Chyba při odesílání simulace:", err);
    }
}
let simulationInterval = null;

document.getElementById("startSimulation").addEventListener("click", () => {
    // if interval is running, do not run it again
    if (simulationInterval) return;

    // sends order immediately
    submitOrderSimulation();

    // it starts interval every 10 seconds
    simulationInterval = setInterval(submitOrderSimulation, 10000);

    // customizable: it change the text of button to „Simulation is running“
    document.getElementById("startSimulation").textContent = "Simulace běží...";
});

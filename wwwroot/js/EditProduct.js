document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll("tbody tr").forEach(row => {
        row.addEventListener("click", function () {
            loadProduct(this.dataset.id, this);
        });
    });
});
    function loadProduct(id, row) {
        // fetch id from <tr> after clicking
        const cells = row.getElementsByTagName("td");

        let imageUrl;
        if (row.dataset.image != null && row.dataset.image !== "") {
            imageUrl = row.dataset.image;
        } else {
            imageUrl = "data:image/svg+xml;base64,...";
        }
        const name = cells[1].innerText.trim();
        const type = cells[2].innerText.trim();
        const maxCapacity = cells[3].innerText.trim();
        const price = parseFloat(cells[4].innerText.replace(/[^\d.,]/g, '').replace(',', '.').trim());
        const pinned = cells[5].innerText.trim() === "Yes";

        const quantityInput = document.getElementById('modalCapacity');
        const capacityLimit = document.getElementById('hasLimit');

        // Funkce pro nastavení stavu inputu podle checkboxu
        function updateQuantityState() {
            if (capacityLimit.checked) {
                quantityInput.disabled = true;
                quantityInput.required = false;
            } else {
                quantityInput.disabled = false;
                quantityInput.required = true;
                quantityInput.value = '';
            }
        }

        // Inicializace podle maxCapacity
        if (maxCapacity == null || maxCapacity === "∞") {
            capacityLimit.checked = true;  // nekonečné množství → input zakázán
        } else {
            capacityLimit.checked = false;   // omezené množství → input povolen
        }
        // Použij funkci pro nastavení stavu
        updateQuantityState();

        // Getting to the modal
        document.getElementById("modalImage").src = imageUrl;
        document.getElementById("modalId").value = id;
        document.getElementById("modalDeleteButton").value = id;
        document.getElementById("modalName").value = name;
        document.getElementById("modalType").value = type;
        document.getElementById("modalCapacity").value = maxCapacity;
        document.getElementById("modalPrice").value = price;
        document.getElementById("modalPinned").checked = pinned;

        // Posluchač pro změny checkboxu
        capacityLimit.addEventListener('change', updateQuantityState);

        // The delete button
        var deleteBtn = document.getElementById('modalDeleteForm');
        if (id == 0) deleteBtn.style.display = 'none';

        // open the modal
        const modal = new bootstrap.Modal(document.getElementById('productEditModal'));
        modal.show();
    }
    document.getElementById('addProductBtn').addEventListener('click', () => {
        // najde první tr, který má onclick
        const firstRow = document.querySelector('tbody tr');
        if (firstRow) {
            firstRow.click(); // simuluje kliknutí
        }
    });
    function saveProduct() {
        // Posluchač pro změny checkboxu
        const hasLimit = document.getElementById("hasLimit").checked

        const formData = new FormData();
        formData.append("Id", document.getElementById("modalId").value);
        formData.append("Name", document.getElementById("modalName").value);
        formData.append("Type", document.getElementById("modalType").value);

        if (!hasLimit) {
            formData.append("MaxCapacity", parseInt(document.getElementById("modalCapacity").value, 10));
        }
        formData.append("Price", parseFloat(document.getElementById("modalPrice").value.replace(',', '.')) || 0);
        formData.append("IsPinned", document.getElementById("modalPinned").checked);

        const fileInput = document.getElementById("image-file");
        if (fileInput.files.length > 0) {
            formData.append("productImage", fileInput.files[0]);
    }
        console.log(formData);
        fetch("/api/products/save", {
            method: "POST",
            body: formData
        })
            .then(r => {
                if (!r.ok) throw new Error("Chyba při ukládání");
                console.log("Saved OK");
                location.reload();
            })
            .catch (err => console.error(err));
    }

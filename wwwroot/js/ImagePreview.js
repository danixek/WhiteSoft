const fileInput = document.getElementById("image-file");
const previewImg = document.getElementById("previewImage");

fileInput.addEventListener("change", function () {
    const file = this.files[0];
    if (file) {
        previewImg.src = URL.createObjectURL(file);
    }
});
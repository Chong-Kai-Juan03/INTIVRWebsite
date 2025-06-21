// wwwroot/js/upload360.js
// Complete 360 image upload functionality (including preview operations and form submission)

function initUpload360() {
    // DOM elements
    const fileInput = document.getElementById('panoramaImage');
    const preview = document.getElementById('imagePreview');
    const zoomInBtn = document.querySelector('.zoom-in');
    const zoomOutBtn = document.querySelector('.zoom-out');
    const fullscreenBtn = document.querySelector('.fullscreen');
    const resetBtn = document.getElementById('resetBtn');
    const titleSelect = document.getElementById('title');
    const scenePreview = document.getElementById('scenePreview');
    const scenePreviewContainer = document.getElementById('scenePreviewContainer');
    const uploadForm = document.getElementById('uploadForm');

    // Image scaling variables
    let currentScale = 1;
    const minScale = 0.3;
    const maxScale = 3;
    const scaleStep = 0.2;

    // Load scenes when page loads
    loadScenes();

    async function loadScenes() {
        try {
            console.log("Loading scenes from Firebase...");
            const response = await fetch('/Home/GetScenes', {
                headers: {
                    'Accept': 'application/json'
                }
            });
            
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const scenes = await response.json();
            console.log("Scenes loaded:", scenes);

            // Clear existing options except the first one
            while (titleSelect.options.length > 1) {
                titleSelect.remove(1);
            }

            // Add new options
            scenes.forEach(scene => {
                console.log("Processing scene:", scene); // 调试日志
                if (scene.title) {
                    console.log("Adding option for:", scene.title);
                    const option = new Option(scene.title, scene.title);
                    option.dataset.imageUrl = scene.imageUrl;
                    titleSelect.add(option);
                }

            });

            if (scenes.length === 0) {
                console.warn("No scenes found in Firebase");
            }
        } catch (error) {
            console.error('Error loading scenes:', error);
            alert('Failed to load scenes. Please try again later.');
        }
    }

    // Handle title selection change
    titleSelect.addEventListener('change', function () {
        if (this.value) {
            const selectedOption = this.options[this.selectedIndex];
            scenePreview.src = selectedOption.dataset.imageUrl;
            scenePreviewContainer.style.display = 'block';
        } else {
            scenePreviewContainer.style.display = 'none';
        }
    });

    // Image preview when file selected
    fileInput.addEventListener('change', function (e) {
        const file = this.files[0];

        if (!file) {
            preview.style.display = 'none';
            return;
        }

        if (!file.type.match('image.*')) {
            alert('Please select an image file.');
            this.value = '';
            preview.style.display = 'none';
            return;
        }

        const reader = new FileReader();
        reader.onload = function (e) {
            preview.src = e.target.result;
            preview.style.display = 'block';
            currentScale = 1;
            updatePreviewTransform();
        };
        reader.readAsDataURL(file);
    });


    // Zoom in functionality
    zoomInBtn.addEventListener('click', function () {
        currentScale = Math.min(currentScale + scaleStep, maxScale);
        updatePreviewTransform();
    });

    // Zoom out functionality
    zoomOutBtn.addEventListener('click', function () {
        currentScale = Math.max(currentScale - scaleStep, minScale);
        updatePreviewTransform();
    });

    // Fullscreen functionality
    fullscreenBtn.addEventListener('click', function () {
        if (preview.style.display === 'block') {
            if (preview.requestFullscreen) {
                preview.requestFullscreen();
            } else if (preview.webkitRequestFullscreen) {
                preview.webkitRequestFullscreen();
            } else if (preview.msRequestFullscreen) {
                preview.msRequestFullscreen();
            }
        }
    });

    // Reset functionality
    resetBtn.addEventListener('click', function () {
        fileInput.value = '';
        preview.src = '#';
        preview.style.display = 'none';
        titleSelect.value = '';
        scenePreviewContainer.style.display = 'none';
        currentScale = 1;
        updatePreviewTransform();
        document.getElementById('imageUrlDisplay').innerHTML = '';
    });

    function updatePreviewTransform() {
        preview.style.transform = `scale(${currentScale})`;
    }

    // Form submission handler
    uploadForm.addEventListener('submit', async function (e) {
        e.preventDefault();

        const submitBtn = document.getElementById('confirmUpload');
        const submitText = document.getElementById('submitText');
        const spinner = document.getElementById('spinner');
        const title = titleSelect.value;
        const file = fileInput.files[0];

        // Validation
        if (!title) {
            alert('Please select a scene');
            return;
        }

        if (!file) {
            alert('Please select an image to upload');
            return;
        }

        // Disable button and show loading state
        submitBtn.disabled = true;
        submitText.textContent = 'Uploading...';
        spinner.style.display = 'inline-block';

        try {
            const formData = new FormData();
            formData.append('panoramaImage', file);
            formData.append('title', title);

            const response = await fetch('/Home/Upload360', {
                method: 'POST',
                body: formData
            });

            const result = await response.json();

            if (result.success) {
                // Update the scene preview with new image
                scenePreview.src = result.url;

                // Show success message
                document.getElementById('imageUrlDisplay').innerHTML = `
                    <div class="alert alert-success mt-3">
                        <div class="d-flex justify-content-between align-items-center">
                            <div>
                                <strong>Upload successful!</strong>
                                <div class="mt-2">
                                    <a href="${result.url}" target="_blank" class="text-break">${result.url}</a>
                                </div>
                            </div>
                            <button class="btn btn-sm btn-outline-secondary" onclick="copyToClipboard('${result.url}')">
                                Copy Link
                            </button>
                        </div>
                    </div>
                `;

                // Reload scenes to get updated list
                loadScenes();
            } else {
                throw new Error(result.message || 'Upload failed');
            }
        } catch (error) {
            console.error('Upload error:', error);
            alert(`Upload failed: ${error.message}`);
        } finally {
            submitBtn.disabled = false;
            submitText.textContent = 'Confirm Upload';
            spinner.style.display = 'none';
        }
    });
}

// Copy to clipboard function
function copyToClipboard(text) {
    navigator.clipboard.writeText(text).then(() => {
        const alert = document.createElement('div');
        alert.className = 'alert alert-info position-fixed top-0 end-0 m-3';
        alert.style.zIndex = '1000';
        alert.textContent = 'Link copied!';
        document.body.appendChild(alert);

        setTimeout(() => {
            alert.remove();
        }, 2000);
    }).catch(err => {
        console.error('Failed to copy:', err);
    });
}

// Initialize
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initUpload360);
} else {
    initUpload360();
}
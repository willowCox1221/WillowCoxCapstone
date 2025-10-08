const codeReader = new ZXing.BrowserMultiFormatReader();
const videoElement = document.getElementById('camera');
const resultElement = document.getElementById('result');
const startButton = document.getElementById('startButton');
const stopButton = document.getElementById('stopButton');

let scanning = false;
let currentDeviceId = null;

startButton.addEventListener('click', async () => {
    if (scanning) {
        resultElement.textContent = "⚠️ Scanner already running!";
        return;
    }

    try {
        console.log("🎥 Listing video input devices...");
        const videoInputDevices = await codeReader.listVideoInputDevices();

        if (videoInputDevices.length === 0) {
            resultElement.textContent = '❌ No camera found.';
            return;
        }

        // Prefer the back camera on mobile
        const backCamera = videoInputDevices.find(device =>
            device.label.toLowerCase().includes('back')
        ) || videoInputDevices[0];

        currentDeviceId = backCamera.deviceId;
        scanning = true;
        resultElement.textContent = '📷 Scanning in progress...';

        console.log("📸 Using camera:", backCamera.label || "Unnamed camera");

        await codeReader.decodeFromVideoDevice(currentDeviceId, videoElement, (result, err) => {
            if (result) {
                resultElement.textContent = `✅ Scanned Code: ${result.text}`;
                console.log("🎯 Scan result:", result.text);

                // 🛑 Stop the camera right after successful scan
                codeReader.reset();
                scanning = false;

                if (videoElement.srcObject) {
                    const tracks = videoElement.srcObject.getTracks();
                    tracks.forEach(track => track.stop());
                    videoElement.srcObject = null;
                }

                resultElement.textContent += "\nCamera stopped automatically.";
            } else if (err && !(err instanceof ZXing.NotFoundException)) {
                console.error(err);
                resultElement.textContent = `Error: ${err}`;
            }
        });

    } catch (err) {
        console.error("Camera start error:", err);
        resultElement.textContent = '⚠️ Camera access denied or error occurred.';
    }
});

stopButton.addEventListener('click', () => {
    if (!scanning) {
        resultElement.textContent = "🛑 Scanner is not running.";
        return;
    }

    codeReader.reset();
    scanning = false;

    if (videoElement.srcObject) {
        const tracks = videoElement.srcObject.getTracks();
        tracks.forEach(track => track.stop());
        videoElement.srcObject = null;
    }

    resultElement.textContent = "✅ Scanning stopped manually.";
});

// console.log("✅ CameraScanner.js loaded");

// const startButton = document.getElementById('startButton');
// const resultElement = document.getElementById('result');

// startButton.addEventListener('click', () => {
//     console.log("🎯 Start button clicked");
//     resultElement.textContent = "Start button works!";
// });
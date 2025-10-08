const codeReader = new ZXing.BrowserMultiFormatReader();
const videoElement = document.getElementById('camera');
const resultElement = document.getElementById('result');
const startButton = document.getElementById('startButton');
const stopButton = document.getElementById('stopButton');

// Keep track of whether the scanner is running
let scanning = false;

startButton.addEventListener('click', async () => {
    if (scanning) {
        resultElement.textContent = "⚠️ Scanner already running!";
        return;
    }

    try {
        const videoInputDevices = await codeReader.listVideoInputDevices();

        if (videoInputDevices.length === 0) {
            resultElement.textContent = '❌ No camera found.';
            return;
        }

        // Prefer the back camera on mobile devices
        const backCamera = videoInputDevices.find(device =>
            device.label.toLowerCase().includes('back')
        ) || videoInputDevices[0];

        scanning = true;
        resultElement.textContent = '📷 Scanning in progress...';

        await codeReader.decodeFromVideoDevice(backCamera.deviceId, videoElement, (result, err) => {
            if (result) {
                resultElement.textContent = `✅ Scanned Code: ${result.text}`;
            } else if (err && !(err instanceof ZXing.NotFoundException)) {
                console.error(err);
                resultElement.textContent = `Error: ${err}`;
            }
        });
    } catch (err) {
        console.error(err);
        resultElement.textContent = '⚠️ Camera access denied or error occurred.';
    }
});

stopButton.addEventListener('click', async () => {
    if (!scanning) {
        resultElement.textContent = "🛑 Scanner is not running.";
        return;
    }

    codeReader.reset(); // Stop the camera
    scanning = false;
    resultElement.textContent = "✅ Scanning stopped.";
});
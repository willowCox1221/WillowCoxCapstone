const codeReader = new ZXing.BrowserMultiFormatReader();
const videoElement = document.getElementById('video');
const resultElement = document.getElementById('result');
const startButton = document.getElementById('startButton');

startButton.addEventListener('click', async () => {
    try {
        const videoInputDevices = await codeReader.listVideoInputDevices();

    if (videoInputDevices.length === 0) {
        resultElement.textContent = 'No camera found.';
        return;
    }

    const firstDeviceId = videoInputDevices[0].deviceId;

    await codeReader.decodeFromVideoDevice(firstDeviceId, 'video', (result, err) => {
        if (result) {
        resultElement.textContent = `✅ Scanned Code: ${result.text}`;
    } else if (err && !(err instanceof ZXing.NotFoundException)) {
        console.error(err);
        resultElement.textContent = `Error: ${err}`;
    }
    });

    resultElement.textContent = 'Scanning in progress...';
} catch (err) {
    console.error(err);
    resultElement.textContent = 'Camera access denied or error occurred.';
}
});
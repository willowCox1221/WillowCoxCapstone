window.addEventListener("DOMContentLoaded", () => {
    const startButton = document.getElementById("startButton");
    const stopButton = document.getElementById("stopButton");
    const video = document.getElementById("camera");
    const result = document.getElementById("result");

    let codeReader = new ZXing.BrowserMultiFormatReader();
    let scanning = false;

    startButton.addEventListener("click", async () => {
        if (scanning) {
            result.textContent = "Scanner already running!";
            return;
        }

        try {
            result.textContent = "Starting camera...";

            // ✅ The method is now part of the codeReader instance
            const devices = await codeReader.listVideoInputDevices();

            if (devices.length === 0) {
                result.textContent = "No camera found.";
                return;
            }

            const backCamera =
                devices.find(d => d.label.toLowerCase().includes("back")) || devices[0];

            scanning = true;

            await codeReader.decodeFromVideoDevice(
                backCamera.deviceId,
                video,
                (scanResult, err) => {
                    if (scanResult) {
                        result.textContent = `✅ Scanned Code: ${scanResult.text}`;

                        // 🛑 Stop camera automatically after success
                        codeReader.reset();
                        scanning = false;
                    } else if (err && !(err instanceof ZXing.NotFoundException)) {
                        console.error(err);
                        result.textContent = `Error: ${err}`;
                    }
                }
            );
        } catch (err) {
            console.error("Error starting scanner:", err);
            result.textContent = "⚠️ Camera access denied or error occurred.";
        }
    });

    stopButton.addEventListener("click", () => {
        if (!scanning) {
            result.textContent = "Scanner not running.";
            return;
        }

        codeReader.reset();
        scanning = false;
        result.textContent = "🛑 Scanning stopped.";
    });
});
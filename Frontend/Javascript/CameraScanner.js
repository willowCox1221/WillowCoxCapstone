    (async () => {
    // Elements
    const startButton = document.getElementById("startButton");
    const stopButton = document.getElementById("stopButton");
    const video = document.getElementById("camera");
    const resultEl = document.getElementById("result");

    if (!startButton || !stopButton || !video || !resultEl) {
        console.error("CameraScanner: missing DOM elements (start/stop/camera/result).");
        return;
    }

    // API and language
    const API_URL = "http://localhost:5000/preset";
    const lang = localStorage.getItem("language") || "en";

    // Default English fallbacks
    const FALLBACK = {
        scan_waiting: "Waiting for scan...",
        scan_starting: "Starting camera...",
        scan_stopping: "Scanning stopped.",
        scan_no_camera: "No camera found.",
        scan_running: "Scanner already running!",
        scan_not_running: "Scanner not running.",
        scan_error: "⚠️ Camera access denied or error occurred.",
        scan_success: "✅ Scanned Code:"
    };

    // Load translations from backend; if fails, use fallback
    let translations = {};
    try {
        console.log(`CameraScanner: loading translations for '${lang}' from ${API_URL}`);
        const r = await fetch(`${API_URL}?lang=${lang}`);
        if (!r.ok) throw new Error(`HTTP ${r.status}`);
        translations = await r.json();
        console.log("CameraScanner: translations loaded", translations);
    } catch (err) {
        console.warn("CameraScanner: failed to load translations, using fallback. Error:", err);
        translations = FALLBACK;
    }

    // Ensure any missing keys use fallback
    for (const k in FALLBACK) {
        if (!translations[k]) translations[k] = FALLBACK[k];
    }

    // Show initial waiting text (translated)
    resultEl.textContent = translations.scan_waiting;

    const codeReader = new ZXing.BrowserMultiFormatReader();
    let scanning = false;
    let currentDecodeSubscription = null;

    startButton.addEventListener("click", async () => {
        if (scanning) {
        resultEl.textContent = translations.scan_running;
        return;
        }

        // set "starting" message
        resultEl.textContent = translations.scan_starting;

        try {
        const devices = await codeReader.listVideoInputDevices();

        if (!devices || devices.length === 0) {
            resultEl.textContent = translations.scan_no_camera;
            return;
        }

        const backCamera = devices.find(d => (d.label || "").toLowerCase().includes("back")) || devices[0];

        scanning = true;

        // decodeFromVideoDevice returns a Promise that resolves when decoding is bound
        // we pass the video element id (or element) and a callback for results
        codeReader.decodeFromVideoDevice(backCamera.deviceId, video, (scanResult, error) => {
            if (scanResult) {
            // success
            const text = scanResult.getText ? scanResult.getText() : String(scanResult);
            resultEl.textContent = `${translations.scan_success} ${text}`;
            // stop scanning after first successful scan
            try {
                codeReader.reset();
            } catch (e) { /* ignore */ }
            scanning = false;
            }
            // ignore NotFoundException — it's normal while scanning
            if (error && !(error instanceof ZXing.NotFoundException)) {
            console.error("CameraScanner: decode error:", error);
            }
        });

        // set a user-facing message while scanning
        resultEl.textContent = translations.scan_starting;
        } catch (err) {
        console.error("Error starting scanner:", err);
        resultEl.textContent = translations.scan_error;
        scanning = false;
        }
    });

    stopButton.addEventListener("click", () => {
        if (!scanning) {
        resultEl.textContent = translations.scan_not_running;
        return;
        }

        try {
        codeReader.reset();
        } catch (e) {
        console.warn("CameraScanner: reset error", e);
        }
        scanning = false;
        resultEl.textContent = translations.scan_stopping;
    });

})();
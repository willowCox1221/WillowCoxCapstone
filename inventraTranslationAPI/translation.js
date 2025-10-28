import express from "express";
import fetch from "node-fetch";
import cors from "cors";

const app = express();
app.use(express.json());
app.use(cors());

// POST /translate endpoint
app.post("/translate", async (req, res) => {
    try {
        const { text, target } = req.body;

        if (!text || !target) {
        return res.status(400).json({ error: "Missing text or target language." });
        }

        // LibreTranslate API endpoint
        const response = await fetch("https://libretranslate.com/translate", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            q: text,
            source: "en",
            target,
            format: "text"
        }),
        });

        const data = await response.json();
        res.json({ translatedText: data.translatedText });
    } catch (error) {
        console.error("Translation error:", error);
        res.status(500).json({ error: "Translation failed." });
    }
});

const PORT = 5000;
app.listen(PORT, () => console.log(`✅ Translation API running on port ${PORT}`));
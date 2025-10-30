import express from "express";
import fetch from "node-fetch";
import cors from "cors";

const app = express();
app.use(express.json());
app.use(cors());

// 🌐 Automatic translation using LibreTranslate
app.post("/translate", async (req, res) => {
    try {
        const { text, target } = req.body;

        if (!text || !target) {
            return res.status(400).json({ error: "Missing text or target language." });
        }

        const response = await fetch("https://libretranslate.com/translate", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                q: text,
                source: "en",
                target,
                format: "text",
            }),
        });

        const data = await response.json();
        res.json({ translatedText: data.translatedText });
    } catch (error) {
        console.error("Translation error:", error);
        res.status(500).json({ error: "Translation failed." });
    }
});

// 💾 Preset translations for UI text
const translations = {
    en: {
        // Navbar
        nav_home: "Home",
        nav_scan: "Scan Product",
        nav_inventory: "Inventory",
        nav_login: "Login",
        nav_signup: "Sign Up",

        // 🏠 Home Page
        welcome_title: "Welcome to Inventra",
        welcome_text: "Search your products or explore new items in your inventory.",
        search_placeholder: "Search products...",
        search_button: "Search",

        // Login Page
        login_title: "Login to Inventra",
        username_label: "Username",
        password_label: "Password",
        username_placeholder: "Enter your username",
        password_placeholder: "Enter your password",
        login_button: "Login",

        // Sign Up Page
        signup_title: "Sign Up with Inventra",
        email_label: "Email",
        email_placeholder: "Enter your email",
        username_label: "Username",
        username_placeholder: "Enter your username",
        password_label: "Password",
        password_placeholder: "Enter your password",
        signup_button: "Sign Up",

        // Scan Page
        scan_header: "Scan a Product",
        scan_waiting: "Waiting for scan...",
        scan_start: "Start Scanning",
        scan_stop: "Stop Scanning",
        scan_starting: "Starting camera...",
        scan_stopping: "Scanning stopped.",
        scan_running: "Scanning already running!",
        // Inventory Page
    },
    es: {
        nav_home: "Inicio",
        nav_scan: "Escanear Producto",
        nav_inventory: "Inventario",
        nav_login: "Iniciar Sesión",
        nav_signup: "Registrarse",

        // 🏠 Home Page
        welcome_title: "Bienvenido a Inventra",
        welcome_text: "Busca tus productos o explora nuevos artículos en tu inventario.",
        search_placeholder: "Buscar productos...",
        search_button: "Buscar",

        login_title: "Inicia sesión en Inventra",
        username_label: "Nombre de usuario",
        password_label: "Contraseña",
        username_placeholder: "Introduce tu nombre de usuario",
        password_placeholder: "Introduce tu contraseña",
        login_button: "Entrar",

        signup_title: "Regístrate en Inventra",
        email_label: "Correo electrónico",
        email_placeholder: "Introduce tu correo electrónico",
        username_label: "Nombre de usuario",
        username_placeholder: "Introduce tu nombre de usuario",
        password_label: "Contraseña",
        password_placeholder: "Introduce tu contraseña",
        signup_button: "Registrarse",

        // Scan Page
        scan_header: "Escanea un Producto",
        scan_waiting: "Esperando escaneo...",
        scan_start: "Comenzar Escaneo",
        scan_stop: "Detener Escaneo",
        scan_starting: "Iniciando cámara...",
        scan_stopping: "Escaneo detenido.",
        scan_running: "¡El escaneo ya está en curso!",
        // Inventory Page
    },
    fr: {
        nav_home: "Accueil",
        nav_scan: "Scanner le produit",
        nav_inventory: "Inventaire",
        nav_login: "Connexion",
        nav_signup: "S'inscrire",

        // 🏠 Home Page
        welcome_title: "Bienvenue sur Inventra",
        welcome_text: "Recherchez vos produits ou explorez de nouveaux articles dans votre inventaire.",
        search_placeholder: "Rechercher des produits...",
        search_button: "Rechercher",

        login_title: "Connectez-vous à Inventra",
        username_label: "Nom d'utilisateur",
        password_label: "Mot de passe",
        username_placeholder: "Entrez votre nom d'utilisateur",
        password_placeholder: "Entrez votre mot de passe",
        login_button: "Connexion",

        signup_title: "Inscrivez-vous à Inventra",
        email_label: "Email",
        email_placeholder: "Entrez votre adresse email",
        username_label: "Nom d'utilisateur",
        username_placeholder: "Entrez votre nom d'utilisateur",
        password_label: "Mot de passe",
        password_placeholder: "Entrez votre mot de passe",
        signup_button: "S'inscrire",

        // Scan Page
        scan_header: "Scanner un produit",
        scan_waiting: "En attente du scan...",
        scan_start: "Démarrer le scan",
        scan_stop: "Arrêter le scan",
        scan_starting: "Démarrage de la caméra...",
        scan_stopping: "Scan arrêté.",
        scan_running: "Le scan est déjà en cours!",
        // Inventory Page
    },
};

// 🌍 Hardcoded UI translation endpoint
app.get("/preset", (req, res) => {
    const lang = req.query.lang || "en";
    res.json(translations[lang] || translations.en);
});

// 🚀 Start the API
const PORT = 5000;
app.listen(PORT, () => console.log(`✅ Translation API running on port ${PORT}`));
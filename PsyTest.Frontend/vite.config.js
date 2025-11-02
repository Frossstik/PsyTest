//import { defineConfig } from "vite";
//import react from "@vitejs/plugin-react";
//import tailwindcss from "@tailwindcss/vite";

//export default defineConfig({
//    plugins: [
//        react(),
//        tailwindcss(),
//    ],
//    server: {
//        port: process.env.VITE_PORT ? Number(process.env.VITE_PORT) : 5173,
//    },
//});

import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import tailwindcss from "@tailwindcss/vite";

const FRONTEND_PORT = process.env.VITE_PORT
    ? Number(process.env.VITE_PORT)
    : 5173;

export default defineConfig({
    plugins: [
        react(),
        tailwindcss(),
    ],
    server: {
        port: FRONTEND_PORT,
        strictPort: true, // не прыгать на другой порт (Aspire этого не любит)

        proxy: {
            // первый backend сервис (например основной API)
            "/api": {
                target: "https://localhost:7089/",
                changeOrigin: true,
                secure: false,
            },

            // второй backend сервис (например Identity/Auth)
            "/auth": {
                target: "https://localhost:7182/",
                changeOrigin: true,
                secure: false,
            },
        }, 
        allowedHosts: ["2d277cc6eda2.ngrok-free.app"],
    },
});

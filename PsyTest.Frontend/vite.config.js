import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite'

export default defineConfig({
    plugins: [react(), tailwindcss()],
    server: {
        port: process.env.VITE_PORT ? Number(process.env.VITE_PORT) : 54710,

    },
});

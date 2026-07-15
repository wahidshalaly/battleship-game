import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  // Fixed port (fails fast if taken) so it always matches the API's CORS allow-list.
  server: { port: 5173, strictPort: true },
})

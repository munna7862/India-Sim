import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import path from 'path';

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  base: './', // relative paths for GitHub Pages and offline desktop packaging
  server: {
    port: 3000,
    open: true,
  },
  build: {
    outDir: 'dist-web',
    sourcemap: true,
  },
});

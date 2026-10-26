import axios from 'axios';

// Use Vite's same-origin development proxy to avoid HTTPS certificate and CORS
// problems. Set VITE_API_BASE_URL only when connecting to a separately hosted API.
const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || '/api',
  headers: { 'Content-Type': 'application/json' },
  timeout: 15000,
});

export default api;

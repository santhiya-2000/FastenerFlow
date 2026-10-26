import axios from 'axios';

const api = axios.create({
  baseURL: 'https://localhost:5001/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Ignore SSL certificate errors for development
api.interceptors.request.use(config => {
  return config;
});

export default api;

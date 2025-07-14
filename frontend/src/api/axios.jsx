import axios from 'axios';

const api = axios.create({
  baseURL: 'https://www.conversordeimagenes.somee.com/api',  // apunta al backend desplegado
  headers: {
    'Content-Type': 'application/json'
  }
});

api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

export default api;

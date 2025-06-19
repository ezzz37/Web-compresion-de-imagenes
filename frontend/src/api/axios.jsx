import axios from 'axios';

const api = axios.create({
  baseURL: 'https://www.conversordelimagenes.somee.com/api', //url de produccion de mi API
  headers: {
    'Content-Type': 'application/json',
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

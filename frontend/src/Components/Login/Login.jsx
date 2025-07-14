import React, { useState } from 'react';
import { FaUser } from 'react-icons/fa';
import { useNavigate } from 'react-router-dom';
import api from '../../api/axios';
import './Login.css';

const Login = () => {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');

    // Limpia cualquier token previo por seguridad
    localStorage.removeItem('token');
    localStorage.removeItem('tokenExpiration');

    try {
      // Envia la request SIN el header Authorization, aunque haya un token viejo
      const response = await api.post(
        '/auth/login',
        { username, password },
        { headers: { Authorization: '' } }
      );

      const { token, expiration } = response.data;

      if (token) {
        localStorage.setItem('token', token);
        localStorage.setItem('tokenExpiration', expiration || '');
        navigate('/dashboard');
      } else {
        setError('Respuesta inválida del servidor.');
        localStorage.removeItem('token');
        localStorage.removeItem('tokenExpiration');
      }
    } catch (err) {
      console.error(err);
      localStorage.removeItem('token');
      localStorage.removeItem('tokenExpiration');
      if (err.response?.status === 401) {
        setError('Usuario o contraseña invalidos.');
      } else {
        setError('Error al conectar con el servidor.');
      }
    }
  };

  return (
    <div className="login-page">
      <div className="login-card">
        <div className="login-header">
          <div className="icon-circle">
            <FaUser size={32} color="#FF7B00" />
          </div>
        </div>
        <div className="login-body">
          <h2 className="login-title">Iniciar Sesión</h2>
          {error && <p className="login-error">{error}</p>}
          <form className="login-form" onSubmit={handleSubmit}>
            <label htmlFor="username">Nombre de Usuario</label>
            <input
              id="username"
              type="text"
              placeholder="Ingresa tu nombre"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              required
            />

            <label htmlFor="password">Contraseña</label>
            <input
              id="password"
              type="password"
              placeholder="Ingresa tu contraseña"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />

            <button type="submit" className="btn-login">
              Ingresar
            </button>
          </form>
          <a href="#" className="forgot-link">¿Olvidaste tu contraseña?</a>
        </div>
      </div>
    </div>
  );
};

export default Login;

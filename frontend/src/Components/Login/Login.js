import React, { useState } from 'react';
import { FaUser } from 'react-icons/fa';
import './Login.css';

const Login = ({ onSubmit }) => {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');

  const handleSubmit = e => {
    e.preventDefault();

    if (onSubmit) {
      onSubmit({ username, password });
    }
  };

  return (
    <div className="login-page">
      <div className="login-card">
        {/* Encabezado degradado + icono */}
        <div className="login-header">
          <div className="icon-circle">
            <FaUser size={32} color="#FF7B00" />
          </div>
        </div>

        {/* Cuerpo del card */}
        <div className="login-body">
          <h2 className="login-title">Iniciar Sesion</h2>
          <form className="login-form" onSubmit={handleSubmit}>
            <label htmlFor="username">Nombre de Usuario</label>
            <input
              id="username"
              type="text"
              placeholder="Ingresa tu nombre"
              value={username}
              onChange={e => setUsername(e.target.value)}
              required
            />

            <label htmlFor="password">Contraseña</label>
            <input
              id="password"
              type="password"
              placeholder="Ingresa tu contraseña"
              value={password}
              onChange={e => setPassword(e.target.value)}
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

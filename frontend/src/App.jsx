import React, { useState, useEffect } from "react";
import { Routes, Route, Navigate, useNavigate } from "react-router-dom";
import "./App.css";
import Login from "./Components/Login/Login";
import Dashboard from "./Components/Dashboard/Dashboard";
import api from "./api/axios";

function App() {
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const navigate = useNavigate();

  useEffect(() => {
    const token = localStorage.getItem("token");
    const expiration = localStorage.getItem("tokenExpiration");
    if (token && expiration) {
      const expireDate = new Date(expiration);
      if (Date.now() < expireDate.getTime()) {
        setIsLoggedIn(true);
      } else {
        localStorage.removeItem("token");
        localStorage.removeItem("tokenExpiration");
        setIsLoggedIn(false);
      }
    }
  }, []);

  // Manejar login
  const handleLogin = async ({ username, password }) => {
    try {
      const response = await api.post("/Auth/login", { username, password });
      const { token, expiration } = response.data;
      localStorage.setItem("token", token);
      localStorage.setItem("tokenExpiration", expiration);
      setIsLoggedIn(true);
      navigate("/dashboard");
    } catch (err) {
      console.error(err);
      alert(
        err.response?.status === 401
          ? "Usuario o contraseña invalidos."
          : "Error al conectar con el servidor."
      );
    }
  };

  // Manejar logout
  const handleLogout = () => {
    localStorage.removeItem("token");
    localStorage.removeItem("tokenExpiration");
    setIsLoggedIn(false);
    navigate("/login");
  };

  // Rutas protegidas
  const ProtectedRoute = ({ children }) =>
    isLoggedIn ? children : <Navigate to="/login" replace />;

  return (
    <Routes>
      <Route path="/login" element={<Login onSubmit={handleLogin} />} />

      <Route
        path="/"
        element={
          <ProtectedRoute>
            <Dashboard onLogout={handleLogout} />
          </ProtectedRoute>
        }
      />
      <Route
        path="/"
        element={
          isLoggedIn
            ? <Navigate to="/dashboard" replace />
            : <Navigate to="/login" replace />
        }
      />

      {/* Cualquier ruta no definida */}
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}

export default App;

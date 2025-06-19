// src/components/Compare/CompareModal.jsx
import React, { useEffect, useState } from 'react';
import api from '../../api/axios';
import './CompareModal.css';

export default function CompareModal({ isOpen, onClose }) {
  //–– 1) lista original de imágenes ––
  const [originalImages, setOriginalImages] = useState([]);
  const [loadingOriginals, setLoadingOriginals] = useState(false);
  const [errorOriginals, setErrorOriginals] = useState('');

  //–– 2) lista de imágenes procesadas ––
  const [processedImages, setProcessedImages] = useState([]);
  const [loadingProcessed, setLoadingProcessed] = useState(false);
  const [errorProcessed, setErrorProcessed] = useState('');

  //–– 3) estados de comparación ––
  const [selectedOriginalId, setSelectedOriginalId] = useState('');
  const [selectedProcessedId, setSelectedProcessedId] = useState('');
  const [comparisonData, setComparisonData] = useState(null);
  const [loadingCompare, setLoadingCompare] = useState(false);
  const [errorCompare, setErrorCompare] = useState('');

  // cada vez que isOpen pase a true, recargamos ambas listas
  useEffect(() => {
    if (!isOpen) return;

    // reset
    setOriginalImages([]);
    setLoadingOriginals(true);
    setErrorOriginals('');
    setProcessedImages([]);
    setLoadingProcessed(true);
    setErrorProcessed('');
    setSelectedOriginalId('');
    setSelectedProcessedId('');
    setComparisonData(null);
    setLoadingCompare(false);
    setErrorCompare('');

    // 1) fetch originales
    api.get('/Imagenes')
      .then(res => {
        const arr = res.data.map(img => ({
          id: img.idImagen,
          nombre: img.nombreArchivo || `ID ${img.idImagen}`
        }));
        setOriginalImages(arr);
      })
      .catch(err => {
        console.error('Error cargando imágenes originales:', err);
        setErrorOriginals('No se pudo cargar las imágenes originales.');
      })
      .finally(() => setLoadingOriginals(false));

    // 2) fetch procesadas
    api.get('/ImagenesProcesadas')
      .then(res => {
        const arr = res.data.map(img => ({
          id: img.idImagenProcesada,
          nombre: `ID ${img.idImagenProcesada} (orig: ${img.idImagenOriginal})`
        }));
        setProcessedImages(arr);
      })
      .catch(err => {
        console.error('Error cargando imágenes procesadas:', err);
        setErrorProcessed('No se pudo cargar las imágenes procesadas.');
      })
      .finally(() => setLoadingProcessed(false));
  }, [isOpen]);

  const handleCompare = () => {
    setErrorCompare('');
    setComparisonData(null);

    if (!selectedOriginalId || !selectedProcessedId) {
      setErrorCompare('Debes seleccionar ambas imágenes.');
      return;
    }
    if (Number(selectedOriginalId) === Number(selectedProcessedId)) {
      setErrorCompare('No puedes comparar la misma imagen.');
      return;
    }

    setLoadingCompare(true);
    const token = localStorage.getItem('token');
    api.post(
      '/Comparaciones/comparar',
      {
        IdImagenOriginal: Number(selectedOriginalId),
        IdImagenProcesada: Number(selectedProcessedId),
      },
      {
        headers: {
          Authorization: `Bearer ${token}`
        }
      }
    )
      .then(res => {
        const data = res.data;
        setComparisonData({
          mse: data.mse,
          psnr: data.psnr,
          imagenDiferenciasBase64: data.imagenDiferenciasBase64,
          fechaComparacion: data.fechaComparacion
        });
      })
      .catch(err => {
        console.error('Error al comparar imágenes:', err);
        setErrorCompare('Error al cargar los datos de comparación.');
      })
      .finally(() => setLoadingCompare(false));
  };

  if (!isOpen) return null;

  return (
    <div className="compare-modal-overlay">
      <div className="compare-modal-container">
        <h2>Comparar Imágenes</h2>

        {/* Original */}
        <div className="compare-modal-field">
          <label>Imagen Original:</label>
          {loadingOriginals ? (
            <p>Cargando imágenes originales…</p>
          ) : errorOriginals ? (
            <p className="compare-modal-error">{errorOriginals}</p>
          ) : (
            <select
              value={selectedOriginalId}
              onChange={e => setSelectedOriginalId(e.target.value)}
            >
              <option value="">-- Selecciona --</option>
              {originalImages.map(img => (
                <option key={img.id} value={img.id}>
                  {img.nombre}
                </option>
              ))}
            </select>
          )}
        </div>

        {/* Procesada */}
        <div className="compare-modal-field">
          <label>Imagen Procesada:</label>
          {loadingProcessed ? (
            <p>Cargando imágenes procesadas…</p>
          ) : errorProcessed ? (
            <p className="compare-modal-error">{errorProcessed}</p>
          ) : (
            <select
              value={selectedProcessedId}
              onChange={e => setSelectedProcessedId(e.target.value)}
            >
              <option value="">-- Selecciona --</option>
              {processedImages.map(img => (
                <option key={img.id} value={img.id}>
                  {img.nombre}
                </option>
              ))}
            </select>
          )}
        </div>

        {/* Botón comparar */}
        <button
          className="compare-modal-button"
          onClick={handleCompare}
          disabled={loadingCompare}
        >
          {loadingCompare ? 'Cargando…' : 'Mostrar Comparación'}
        </button>
        {errorCompare && <p className="compare-modal-error">{errorCompare}</p>}

        {/* Resultados */}
        {comparisonData && (
          <div className="compare-modal-results">
            <h3>Resultado de la Comparación</h3>
            <table className="compare-modal-table">
              <tbody>
                <tr>
                  <td><strong>MSE</strong></td>
                  <td>{comparisonData.mse}</td>
                </tr>
                <tr>
                  <td><strong>PSNR</strong></td>
                  <td>{comparisonData.psnr}</td>
                </tr>
                <tr>
                  <td><strong>Fecha</strong></td>
                  <td>
                    {new Date(comparisonData.fechaComparacion)
                      .toLocaleString()}
                  </td>
                </tr>
                <tr>
                  <td><strong>Imagen Diferencias</strong></td>
                  <td>
                    <img
                      src={`data:image/png;base64,${comparisonData.imagenDiferenciasBase64}`}
                      alt="Diferencias"
                      style={{ maxWidth: '100%', height: 'auto' }}
                    />
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        )}

        {/* Cerrar */}
        <button
          className="compare-modal-close"
          onClick={onClose}
        >
          Cerrar
        </button>
      </div>
    </div>
  );
}

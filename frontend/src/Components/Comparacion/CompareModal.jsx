import React, { useEffect, useState } from 'react';
import './CompareModal.css';

export default function CompareModal({ isOpen, onClose }) {
  //–– 1) lista original de imagenes ––
  const [originalImages, setOriginalImages] = useState([]);
  const [loadingOriginals, setLoadingOriginals] = useState(false);
  const [errorOriginals, setErrorOriginals] = useState(null);

  //–– 2) lista de img procesadas ––
  const [processedImages, setProcessedImages] = useState([]);
  const [loadingProcessed, setLoadingProcessed] = useState(false);
  const [errorProcessed, setErrorProcessed] = useState(null);

  //–– 3) estado para la comparacion puntual ––
  const [selectedOriginalId, setSelectedOriginalId] = useState('');
  const [selectedProcessedId, setSelectedProcessedId] = useState('');
  const [comparisonData, setComparisonData] = useState(null);
  const [loadingCompare, setLoadingCompare] = useState(false);
  const [errorCompare, setErrorCompare] = useState(null);

  // Cada vez que isOpen cambie a true, recargamos solo originales y procesadas
  useEffect(() => {
    if (!isOpen) return;

    setOriginalImages([]);
    setErrorOriginals(null);
    setLoadingOriginals(false);
    setProcessedImages([]);
    setErrorProcessed(null);
    setLoadingProcessed(false);
    setSelectedOriginalId('');
    setSelectedProcessedId('');
    setComparisonData(null);
    setErrorCompare(null);
    setLoadingCompare(false);

    //–– Cargar lista de imagenes Originales ––
    setLoadingOriginals(true);
    fetch('http://localhost:5288/api/Imagenes')
      .then(async (res) => {
        if (!res.ok) {
          const text = await res.text();
          console.error(`GET /api/Imagenes → ${res.status} : ${text}`);
          throw new Error('No se pudo cargar las imágenes originales.');
        }
        return res.json();
      })
      .then((data) => {
        const arr = data.map((img) => ({
          id: img.idImagen,
          nombre: img.nombreArchivo || `ID ${img.idImagen}`,
        }));
        setOriginalImages(arr);
      })
      .catch((err) => {
        console.error('Error en fetch(Imagenes):', err);
        setErrorOriginals('No se pudo cargar las imagenes originales.');
      })
      .finally(() => {
        setLoadingOriginals(false);
      });

    //–– Cargar lista de Imágenes Procesadas ––
    setLoadingProcessed(true);
    fetch('http://localhost:5288/api/ImagenesProcesadas')
      .then(async (res) => {
        if (!res.ok) {
          const text = await res.text();
          console.error(`GET /api/ImagenesProcesadas → ${res.status} : ${text}`);
          throw new Error('No se pudo cargar las imagenes procesadas.');
        }
        return res.json();
      })
      .then((data) => {
        const arr = data.map((img) => ({
          id: img.idImagenProcesada,
          nombre: `ID ${img.idImagenProcesada} (orig: ${img.idImagenOriginal})`,
        }));
        setProcessedImages(arr);
      })
      .catch((err) => {
        console.error('Error en fetch(ImagenesProcesadas):', err);
        setErrorProcessed('No se pudo cargar las imagenes procesadas.');
      })
      .finally(() => {
        setLoadingProcessed(false);
      });
  }, [isOpen]);

  const handleCompare = () => {
    setErrorCompare(null);
    setComparisonData(null);

    if (!selectedOriginalId || !selectedProcessedId) {
      setErrorCompare('Debes seleccionar ambas imagenes.');
      return;
    }
    if (selectedOriginalId === selectedProcessedId) {
      setErrorCompare('No puedes comparar la misma imagen consigo misma.');
      return;
    }

    setLoadingCompare(true);
    fetch('http://localhost:5288/api/Comparaciones/comparar', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        IdImagenOriginal: Number(selectedOriginalId),
        IdImagenProcesada: Number(selectedProcessedId),
      }),
    })
      .then(async (res) => {
        if (!res.ok) {
          const text = await res.text();
          console.error(`POST /api/Comparaciones/comparar → ${res.status} : ${text}`);
          throw new Error('No se pudo procesar la comparacion.');
        }
        return res.json();
      })
      .then((data) => {
        setComparisonData({
          idComparacion: data.idComparacion,
          mse: data.mse,
          psnr: data.psnr,
          imagenDiferenciasBase64: data.imagenDiferenciasBase64,
          fechaComparacion: data.fechaComparacion,
        });
      })
      .catch((err) => {
        console.error('Error en POST Comparar:', err);
        setErrorCompare('Error al cargar los datos de comparacion.');
      })
      .finally(() => {
        setLoadingCompare(false);
      });
  };

  if (!isOpen) {
    return null;
  }

  return (
    <div className="compare-modal-overlay">
      <div className="compare-modal-container">
        <h2>Comparar Imágenes</h2>

        {/* Selección de Imagen Original */}
        <div className="compare-modal-field">
          <label>Imagen Original:</label>
          {loadingOriginals ? (
            <p>Cargando imágenes originales…</p>
          ) : errorOriginals ? (
            <p className="compare-modal-error">{errorOriginals}</p>
          ) : (
            <select
              value={selectedOriginalId}
              onChange={(e) => setSelectedOriginalId(e.target.value)}
            >
              <option value="">-- Selecciona --</option>
              {originalImages.map((img) => (
                <option key={img.id} value={img.id}>
                  {img.nombre}
                </option>
              ))}
            </select>
          )}
        </div>

        {/* Seleccion de Imagen Procesada */}
        <div className="compare-modal-field" style={{ marginTop: '1rem' }}>
          <label>Imagen Procesada:</label>
          {loadingProcessed ? (
            <p>Cargando imágenes procesadas…</p>
          ) : errorProcessed ? (
            <p className="compare-modal-error">{errorProcessed}</p>
          ) : (
            <select
              value={selectedProcessedId}
              onChange={(e) => setSelectedProcessedId(e.target.value)}
            >
              <option value="">-- Selecciona --</option>
              {processedImages.map((img) => (
                <option key={img.id} value={img.id}>
                  {img.nombre}
                </option>
              ))}
            </select>
          )}
        </div>

        {/* Botón “Mostrar Comparación” */}
        <div style={{ marginTop: '1.25rem' }}>
          <button
            className="compare-modal-button"
            onClick={handleCompare}
            disabled={loadingCompare}
          >
            {loadingCompare ? 'Cargando…' : 'Mostrar Comparacion'}
          </button>
          {errorCompare && (
            <p className="compare-modal-error" style={{ marginTop: '0.5rem' }}>
              {errorCompare}
            </p>
          )}
        </div>

        {/* Resultado puntual (MSE / PSNR / Imagen de diferencias) */}
        {comparisonData && (
          <div className="compare-modal-results" style={{ marginTop: '1.5rem' }}>
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
                  <td>{new Date(comparisonData.fechaComparacion).toLocaleString()}</td>
                </tr>
                <tr>
                  <td><strong>Imagen Diferencias</strong></td>
                  <td>
                    {comparisonData.imagenDiferenciasBase64 ? (
                      <img
                        src={`data:image/png;base64,${comparisonData.imagenDiferenciasBase64}`}
                        alt="Diferencias"
                        style={{ maxWidth: '100%', height: 'auto' }}
                      />
                    ) : '–'}
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        )}

        {/* Boton “Cerrar” */}
        <button
          className="compare-modal-close"
          onClick={onClose}
          style={{ marginTop: '1.5rem' }}
        >
          Cerrar
        </button>
      </div>
    </div>
  );
}

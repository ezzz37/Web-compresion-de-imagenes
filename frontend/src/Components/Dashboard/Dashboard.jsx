import React, { useState } from "react";
import ImageGallery from "../ImageGallery/ImageGallery";
import CompareModal from "../Comparacion/CompareModal";
import "./Dashboard.css";

export default function Dashboard({ onLogout }) {
  const [resolution, setResolution] = useState(500);
  const [colorDepthIndex, setColorDepthIndex] = useState(1);
  const [colorDepth, setColorDepth] = useState(8);
  const [compression, setCompression] = useState(0.8);
  const [selectedFile, setSelectedFile] = useState(null);
  const [originalImagePreviewUrl, setOriginalImagePreviewUrl] = useState(null);
  const [uploadedImage, setUploadedImage] = useState(null);
  const [processedImage, setProcessedImage] = useState(null);
  const [imagePreviewUrl, setImagePreviewUrl] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [showGallery, setShowGallery] = useState(false);
  const [isCompareOpen, setIsCompareOpen] = useState(false);

  const DEPTHS = [1, 8, 24];

  const handleFileChange = (e) => {
    const file = e.target.files[0];
    if (file) {
      setSelectedFile(file);
      const reader = new FileReader();
      reader.onloadend = () => setOriginalImagePreviewUrl(reader.result);
      reader.readAsDataURL(file);
    }
  };

  const handleUpload = async () => {
    if (!selectedFile) {
      alert("Por favor selecciona un archivo primero");
      return;
    }
    setLoading(true);
    setError(null);

    try {
      const formData = new FormData();
      formData.append("Archivo", selectedFile);
      formData.append("Nombre", selectedFile.name);

      const res = await fetch(
        "http://conversordeimagenes.somee.com/api/Imagenes/upload",
        { method: "POST", body: formData }
      );
      if (!res.ok) throw new Error("Error al subir la imagen");

      const data = await res.json();
      setUploadedImage(data);
      setProcessedImage(null);
      setImagePreviewUrl(null);
      alert("Imagen subida correctamente");
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const handleProcess = async () => {
    if (!uploadedImage) {
      alert("Primero subi una imagen");
      return;
    }
    setLoading(true);
    setError(null);

    const payload = {
      AnchoResolucion: resolution,
      AltoResolucion: resolution,
      ProfundidadBits: colorDepth,
      IdAlgoritmoCompresion: 1,
      Algoritmo: "JPEG",
      NivelCompresion: Math.round(compression * 100),
    };

    try {
      const res = await fetch(
        `http://conversordeimagenes.somee.com/api/ImagenesProcesadas/procesar/${uploadedImage.idImagen}`,
        {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify(payload),
        }
      );
      if (!res.ok) throw new Error(await res.text());
      const data = await res.json();

      if (!data.DatosProcesadosBase64 || data.DatosProcesadosBase64.trim() === "") {
        setError("La imagen fue procesada, pero el servidor no retornó los datos de la imagen (DatosProcesadosBase64).");
        setProcessedImage(null);
        setImagePreviewUrl(null);
        return;
      }

      const mime = data.algoritmo?.toLowerCase() === "png" ? "png" : "jpeg";
      setImagePreviewUrl(`data:image/${mime};base64,${data.DatosProcesadosBase64}`);
      setProcessedImage(data);
    } catch (err) {
      setError(err.message);
      setProcessedImage(null);
      setImagePreviewUrl(null);
    } finally {
      setLoading(false);
    }
  };

  // Si quieres poder ver de nuevo una imagen procesada desde la galeria o el historial:
  const fetchProcessedImageById = async (id) => {
    setLoading(true);
    setError(null);
    try {
      const res = await fetch(`http://conversordeimagenes.somee.com/api/ImagenesProcesadas/${id}`);
      if (!res.ok) throw new Error("No se pudo obtener imagen procesada");
      const data = await res.json();
      if (!data.DatosProcesadosBase64 || data.DatosProcesadosBase64.trim() === "") {
        setError("La imagen seleccionada no tiene datos de imagen procesada.");
        setProcessedImage(null);
        setImagePreviewUrl(null);
        return;
      }
      const mime = data.algoritmo?.toLowerCase() === "png" ? "png" : "jpeg";
      setImagePreviewUrl(`data:image/${mime};base64,${data.DatosProcesadosBase64}`);
      setProcessedImage(data);
    } catch (err) {
      setError(err.message);
      setProcessedImage(null);
      setImagePreviewUrl(null);
    } finally {
      setLoading(false);
    }
  };

  const handleDownload = () => {
    if (!imagePreviewUrl) return;
    const link = document.createElement("a");
    link.href = imagePreviewUrl;
    link.download = "imagen-digitalizada.jpg";
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  const handleReset = () => {
    setResolution(500);
    setColorDepthIndex(1);
    setColorDepth(8);
    setCompression(0.8);
    setSelectedFile(null);
    setOriginalImagePreviewUrl(null);
    setUploadedImage(null);
    setProcessedImage(null);
    setImagePreviewUrl(null);
    setError(null);
  };

  const toggleGallery = () => setShowGallery(!showGallery);

  const handleImageSelect = (image) => {
    setShowGallery(false);
    setUploadedImage(image);
    setSelectedFile(null);
    setProcessedImage(null);
    setImagePreviewUrl(null);
    if (image.datosImagenBase64) {
      setOriginalImagePreviewUrl(`data:image/png;base64,${image.datosImagenBase64}`);
    } else {
      setOriginalImagePreviewUrl(null);
    }
    // Si seleccionás una imagen procesada de la galeria, cargarla:
    if (image.idImagenProcesada) {
      fetchProcessedImageById(image.idImagenProcesada);
    }
  };

  const handleColorDepthChange = (e) => {
    const idx = Number(e.target.value);
    setColorDepthIndex(idx);
    setColorDepth(DEPTHS[idx]);
  };

  const setDepthPreset = (targetDepth) => {
    const idx = DEPTHS.indexOf(targetDepth);
    if (idx !== -1) {
      setColorDepthIndex(idx);
      setColorDepth(targetDepth);
    }
  };

  const handleOpenCompare = () => setIsCompareOpen(true);
  const handleCloseCompare = () => setIsCompareOpen(false);

  return (
    <div className="App">
      <header className="app-header">
        <div>
          <h1>Digitalizador de Imagenes</h1>
          <p>Convierte imágenes analogicas a formato digital con diferentes parametros</p>
        </div>
        <button className="btn-logout" onClick={onLogout}>
          Cerrar Sesión
        </button>
      </header>

      <section className="image-panels">
        <div className="panel">
          <h2>Imagen Original</h2>
          <div className="image-drop-area">
            <input
              type="file"
              accept="image/*"
              onChange={handleFileChange}
              style={{
                position: "absolute",
                width: "100%",
                height: "100%",
                opacity: 0,
                cursor: "pointer",
                zIndex: 2,
              }}
            />
            {!selectedFile && !originalImagePreviewUrl && (
              <span
                style={{
                  position: "absolute",
                  pointerEvents: "none",
                  userSelect: "none",
                  color: "rgba(255 255 255 / 0.7)",
                  fontSize: "1rem",
                  top: "50%",
                  left: "50%",
                  transform: "translate(-50%, -50%)",
                  textAlign: "center",
                  zIndex: 1,
                }}
              >
                Arrastra una imagen o hace clic para seleccionar
              </span>
            )}
            {originalImagePreviewUrl && (
              <img
                src={originalImagePreviewUrl}
                alt="Vista previa"
                style={{
                  maxWidth: "100%",
                  maxHeight: "100%",
                  objectFit: "contain",
                  zIndex: 0,
                }}
              />
            )}
          </div>

          <div style={{ display: "flex", gap: "10px", marginTop: "1rem" }}>
            <button
              className="btn-primary"
              onClick={handleUpload}
              disabled={loading || !selectedFile}
            >
              {loading ? "Subiendo..." : "Cargar Imagen"}
            </button>
            <button onClick={toggleGallery}>Ver Imagenes Cargadas</button>
          </div>
        </div>

        <div className="panel">
          <h2>Imagen Digitalizada</h2>
          <div className="image-drop-area digitalized">
            {loading && <p>Cargando...</p>}
            {!loading && imagePreviewUrl && (
              <>
                <img
                  src={imagePreviewUrl}
                  alt="Imagen digitalizada"
                  style={{ maxWidth: "100%", maxHeight: "100%", objectFit: "contain" }}
                />
              </>
            )}
            {!loading && !imagePreviewUrl && <p>No hay imagen procesada</p>}
          </div>

          <div
            style={{
              display: "flex",
              justifyContent: "space-between",
              gap: "10px",
              marginTop: "1rem",
            }}
          >
            <button
              onClick={handleDownload}
              disabled={!imagePreviewUrl || loading}
              style={{
                flexGrow: 1,
                backgroundColor: imagePreviewUrl ? "#4caf50" : "#d29b9b",
                color: imagePreviewUrl ? "white" : "#4a2c2c",
                fontWeight: "bold",
                border: "none",
                borderRadius: "4px",
                cursor: imagePreviewUrl ? "pointer" : "not-allowed",
                padding: "0.5rem 1rem",
              }}
            >
              Descargar Imagen
            </button>

            <button
              onClick={handleReset}
              style={{
                backgroundColor: "white",
                color: "#333",
                fontWeight: "bold",
                border: "none",
                borderRadius: "4px",
                cursor: "pointer",
                padding: "0.5rem 1rem",
              }}
            >
              Reiniciar
            </button>
          </div>
        </div>
      </section>

      <section className="params">
        <h2>Parametros de Digitalizacion</h2>

        <div className="digitalization-row">
          <div className="param-group">
            <div className="param-label">
              <h3>Muestreo (Resolución)</h3>
              <span>
                Resolucion: {resolution}×{resolution}
              </span>
            </div>
            <input
              type="range"
              min="50"
              max="1000"
              value={resolution}
              onChange={(e) => setResolution(Number(e.target.value))}
            />
            <div className="presets">
              <button onClick={() => setResolution(100)}>100×100</button>
              <button onClick={() => setResolution(500)}>500×500</button>
              <button onClick={() => setResolution(1000)}>1000×1000</button>
            </div>
          </div>

          <div className="param-group">
            <div className="param-label">
              <h3>Profundidad de Color</h3>
              <span>Bits por canal: {colorDepth}</span>
            </div>
            <input
              type="range"
              min="0"
              max="2"
              step="1"
              value={colorDepthIndex}
              onChange={handleColorDepthChange}
            />
            <div className="presets">
              <button onClick={() => setDepthPreset(1)}>1 bit (2 colores)</button>
              <button onClick={() => setDepthPreset(8)}>8 bits (256 colores)</button>
              <button onClick={() => setDepthPreset(24)}>24 bits (16.7 M colores)</button>
            </div>
          </div>
        </div>

        <div className="param-group">
          <div className="param-label">
            <h3>Compresión</h3>
            <span>Nivel de Compresión: {compression}</span>
          </div>
          <input
            type="range"
            min="0"
            max="1"
            step="0.01"
            value={compression}
            onChange={(e) => setCompression(Number(e.target.value))}
          />
          <div className="labels-range">
            <span>Alta Compresión</span>
            <span>Sin Compresión</span>
          </div>
        </div>

        <div style={{ display: "flex", gap: "10px", marginTop: "1rem" }}>
          <button
            className="btn-primary"
            onClick={handleProcess}
            disabled={loading || !uploadedImage}
          >
            {loading ? "Procesando..." : "Procesar Imagen"}
          </button>

          <button
            className="btn-secondary"
            onClick={handleOpenCompare}
            disabled={loading}
            style={{
              backgroundColor: "#1976d2",
              color: "white",
              border: "none",
              borderRadius: "4px",
              cursor: loading ? "not-allowed" : "pointer",
              padding: "0.5rem 1rem",
            }}
          >
            Comparar Imagenes
          </button>
        </div>

        {error && (
          <p style={{ color: "red", marginTop: "1rem", textAlign: "center" }}>
            {error}
          </p>
        )}
      </section>

      {showGallery && (
        <ImageGallery onClose={toggleGallery} onSelect={handleImageSelect} />
      )}

      {isCompareOpen && (
        <CompareModal isOpen={isCompareOpen} onClose={handleCloseCompare} />
      )}
    </div>
  );
}

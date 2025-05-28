import React, { useState } from "react";
import "./App.css";

function App() {
  const [resolution, setResolution] = useState(500);
  const [colorDepth, setColorDepth] = useState(8);
  const [compression, setCompression] = useState(0.8);

  const [selectedFile, setSelectedFile] = useState(null);
  const [uploadedImage, setUploadedImage] = useState(null);
  const [processedImage, setProcessedImage] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  // Carga archivo local y lo guarda en estado
  const handleFileChange = (e) => {
    setSelectedFile(e.target.files[0]);
  };

  // Subir imagen al backend (POST /api/Imagenes/upload)
  const handleUpload = async () => {
    if (!selectedFile) {
      alert("Por favor seleccioná un archivo primero");
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const formData = new FormData();
      formData.append("Archivo", selectedFile);
      formData.append("Nombre", selectedFile.name);

      // Cambiado a http y puerto 5288 según backend corriendo en HTTP
      const res = await fetch("http://localhost:5288/api/Imagenes/upload", {
        method: "POST",
        body: formData,
      });

      if (!res.ok) {
        throw new Error("Error al subir la imagen");
      }

      const data = await res.json();
      setUploadedImage(data);
      setProcessedImage(null);
      alert("Imagen subida correctamente");
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  // Procesar imagen en backend (POST /api/ImagenesProcesadas/procesar/{idImagen})
  const handleProcess = async () => {
    if (!uploadedImage) {
      alert("Primero subí una imagen");
      return;
    }

    setLoading(true);
    setError(null);

    const idAlgoritmoCompresion = 1; // Ejemplo JPEG

    const payload = {
      AnchoResolucion: resolution,
      AltoResolucion: resolution,
      ProfundidadBits: colorDepth,
      IdAlgoritmoCompresion: idAlgoritmoCompresion,
      Algoritmo: "JPEG",
    };

    try {
      const res = await fetch(
        `http://localhost:5288/api/ImagenesProcesadas/procesar/${uploadedImage.idImagen}`,
        {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify(payload),
        }
      );

      if (!res.ok) {
        const text = await res.text();
        throw new Error(`Error al procesar imagen: ${text}`);
      }

      const data = await res.json();
      setProcessedImage(data);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const [imagePreviewUrl, setImagePreviewUrl] = useState(null);

  // Cargar imagen procesada para mostrar preview
  const fetchProcessedImageData = async () => {
    if (!processedImage) return;

    setLoading(true);
    setError(null);

    try {
      const res = await fetch(
        `http://localhost:5288/api/ImagenesProcesadas/${processedImage.idImagenProcesada}`
      );
      if (!res.ok) throw new Error("No se pudo obtener imagen procesada");

      const data = await res.json();

      if (!data.DatosProcesados) throw new Error("Imagen procesada sin datos");

      let base64String;

      if (typeof data.DatosProcesados === "string") {
        base64String = data.DatosProcesados;
      } else if (Array.isArray(data.DatosProcesados)) {
        const bytes = Uint8Array.from(data.DatosProcesados);
        base64String = btoa(
          bytes.reduce((data, byte) => data + String.fromCharCode(byte), "")
        );
      } else {
        throw new Error("Formato datos procesados no reconocido");
      }

      setImagePreviewUrl(`data:image/jpeg;base64,${base64String}`);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  React.useEffect(() => {
    if (processedImage) fetchProcessedImageData();
    else setImagePreviewUrl(null);
  }, [processedImage]);

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
    setColorDepth(8);
    setCompression(0.8);
    setSelectedFile(null);
    setUploadedImage(null);
    setProcessedImage(null);
    setImagePreviewUrl(null);
    setError(null);
  };

  return (
    <div className="App">
      <header>
        <h1>Digitalizador de Imagenes</h1>
        <p>
          Convierte imagenes analogicas a formato digital con diferentes parametros
        </p>
      </header>

      <section className="image-panels">
        <div className="panel">
          <h2>Imagen Original</h2>
          <div className="image-drop-area">
            <input
              type="file"
              accept="image/*"
              onChange={handleFileChange}
              style={{ width: "100%", height: "100%", opacity: 0, cursor: "pointer" }}
            />
            {!selectedFile && (
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
                }}
              >
                Arrastra una imagen o haz clic para seleccionar
              </span>
            )}
            {selectedFile && <p>{selectedFile.name}</p>}
          </div>
          <button
            className="btn-primary"
            onClick={handleUpload}
            disabled={loading || !selectedFile}
          >
            {loading ? "Subiendo..." : "Cargar Imagen"}
          </button>
        </div>

        <div className="panel">
          <h2>Imagen Digitalizada</h2>
          <div className="image-drop-area digitalized">
            {loading && <p>Cargando...</p>}
            {!loading && imagePreviewUrl && (
              <img
                src={imagePreviewUrl}
                alt="Imagen digitalizada"
                style={{ maxWidth: "100%", maxHeight: "100%" }}
              />
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
        <h2>Parámetros de Digitalización</h2>

        <div className="digitalization-row">
          <div className="param-group">
            <div className="param-label">
              <h3>Muestreo (Resolución)</h3>
              <span>
                Resolución: {resolution}x{resolution}
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
              <button onClick={() => setResolution(100)}>100x100</button>
              <button onClick={() => setResolution(500)}>500x500</button>
              <button onClick={() => setResolution(1000)}>1000x1000</button>
            </div>
          </div>

          <div className="param-group">
            <div className="param-label">
              <h3>Profundidad de Color</h3>
              <span>Bits por canal: {colorDepth}</span>
            </div>
            <input
              type="range"
              min="1"
              max="24"
              value={colorDepth}
              onChange={(e) => setColorDepth(Number(e.target.value))}
            />
            <div className="presets">
              <button onClick={() => setColorDepth(1)}>1 bit (2 colores)</button>
              <button onClick={() => setColorDepth(8)}>8 bits (256 colores)</button>
              <button onClick={() => setColorDepth(24)}>
                24 bits (16.7M colores)
              </button>
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

        <button
          className="btn-primary"
          onClick={handleProcess}
          disabled={loading || !uploadedImage}
        >
          {loading ? "Procesando..." : "Procesar Imagen"}
        </button>

        {error && (
          <p style={{ color: "red", marginTop: "1rem", textAlign: "center" }}>
            {error}
          </p>
        )}
      </section>
    </div>
  );
}

export default App;

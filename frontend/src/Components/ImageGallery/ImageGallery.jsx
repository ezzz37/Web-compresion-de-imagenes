import React, { useState, useEffect } from "react";
import "./ImageGallery.css";

const ImageGallery = ({ onClose, onSelect }) => {
  const [images, setImages] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const fetchImages = async () => {
    try {
      const res = await fetch("http://conversordeimagenes.somee.com/api/Imagenes");
      if (!res.ok) throw new Error("Error al obtener las imagenes");
      const data = await res.json();

      // Eliminar duplicados por nombre
      const seen = new Set();
      const uniqueImages = data.filter((img) => {
        if (seen.has(img.nombre)) return false;
        seen.add(img.nombre);
        return true;
      });

      setImages(uniqueImages);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchImages();
  }, []);

  return (
    <div className="image-gallery-modal">
      <div className="modal-content">
        <div className="modal-header">
          <h2>Imagenes Cargadas</h2>
          <button onClick={onClose} className="close-btn">Cerrar</button>
        </div>

        <div className="modal-body">
          {loading ? (
            <p>Cargando imágenes...</p>
          ) : error ? (
            <p style={{ color: "red" }}>Error: {error}</p>
          ) : (
            <div className="gallery-grid">
              {images.length > 0 ? (
                images.map((image) => (
                  <button
                    key={image.idImagen}
                    className="image-item"
                    onClick={() => onSelect(image)}
                  >
                    <img
                      src={`data:image/png;base64,${image.datosImagenBase64}`}
                      alt={image.nombre}
                      style={{
                        width: "80px",
                        height: "80px",
                        objectFit: "cover",
                        borderRadius: "6px",
                        marginBottom: "0.5rem",
                      }}
                    />
                    <div style={{ fontSize: "0.75rem", fontWeight: "bold" }}>
                      {image.nombre}
                    </div>
                  </button>
                ))
              ) : (
                <p>No hay imagenes cargadas</p>
              )}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default ImageGallery;

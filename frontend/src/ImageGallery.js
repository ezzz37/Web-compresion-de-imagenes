import React, { useState, useEffect } from "react";
import "./ImageGallery.css";

const ImageGallery = ({ onClose, onSelect }) => {
  const [images, setImages] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const fetchImages = async () => {
    try {
      const res = await fetch("http://localhost:5288/api/Imagenes");
      if (!res.ok) throw new Error("Error al obtener las imágenes");
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
          <h2>Imágenes Cargadas</h2>
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
                    {image.nombre}
                  </button>
                ))
              ) : (
                <p>No hay imágenes cargadas</p>
              )}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default ImageGallery;

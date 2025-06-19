import React, { useState } from 'react';
import CompareModal from './CompareModal';
import './ImageProcessor.css';

export default function ImageProcessor() {
  const [isCompareOpen, setIsCompareOpen] = useState(false);

  const handleOpenCompare = () => {
    setIsCompareOpen(true);
  };
  const handleCloseCompare = () => {
    setIsCompareOpen(false);
  };

  return (
    <div className="image-processor-container">
      {/* Aqui irian tus controles de resolución, color, compresion, etc. */}
      {/* … */}
      <div className="buttons-row">
        {/* Tu botón original para procesar imagen */}
        <button className="btn-procesar-imagen">
          Procesar Imagen
        </button>

        {/* Nuevo boton para abrir el modal de comparaciones */}
        <button
          className="btn-comparar-imagen"
          onClick={handleOpenCompare}
        >
          Comparar Imágenes
        </button>
      </div>

      {/* El modal solo se renderiza si isCompareOpen === true */}
      <CompareModal isOpen={isCompareOpen} onClose={handleCloseCompare} />
    </div>
  );
}

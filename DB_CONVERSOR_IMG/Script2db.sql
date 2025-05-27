USE master;
GO

-- 1) Si existe, poner SINGLE_USER y eliminar la BD
IF EXISTS (SELECT 1 FROM sys.databases WHERE name = 'DigitalizacionImagenesBD')
BEGIN
    ALTER DATABASE [DigitalizacionImagenesBD] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [DigitalizacionImagenesBD];
END
GO

-- 2) Crear la base de datos
CREATE DATABASE [DigitalizacionImagenesBD];
GO

-- 3) Conectar a la nueva BD
USE [DigitalizacionImagenesBD];
GO

-- 4) Algoritmos de compresión
CREATE TABLE dbo.AlgoritmosCompresion (
    IdAlgoritmoCompresion INT         IDENTITY(1,1) PRIMARY KEY,
    NombreAlgoritmo       NVARCHAR(50) NOT NULL UNIQUE
);
GO

-- 5) Semilla de algoritmos (sólo inserta los que falten)
INSERT INTO dbo.AlgoritmosCompresion (NombreAlgoritmo)
SELECT v.Nombre
FROM (VALUES('JPEG'),('PNG'),('RLE')) AS v(Nombre)
LEFT JOIN dbo.AlgoritmosCompresion a
    ON a.NombreAlgoritmo = v.Nombre
WHERE a.NombreAlgoritmo IS NULL;
GO

-- 6) Imágenes originales
CREATE TABLE dbo.Imagenes (
    IdImagen      INT            IDENTITY(1,1) PRIMARY KEY,
    Nombre        NVARCHAR(255)  NOT NULL,
    DatosImagen   VARBINARY(MAX) NOT NULL,
    AnchoOriginal INT            NOT NULL,
    AltoOriginal  INT            NOT NULL,
    FechaCarga    DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

-- 7) Imágenes procesadas
CREATE TABLE dbo.ImagenesProcesadas (
    IdImagenProcesada     INT            IDENTITY(1,1) PRIMARY KEY,
    IdImagenOriginal      INT            NOT NULL,
    AnchoResolucion       INT            NOT NULL,
    AltoResolucion        INT            NOT NULL,
    ProfundidadBits       TINYINT        NOT NULL,
    IdAlgoritmoCompresion INT            NULL,
    RatioCompresion       FLOAT          NULL,
    DatosProcesados       VARBINARY(MAX) NOT NULL,
    FechaProcesamiento    DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_ImgProc_ImgOriginal
      FOREIGN KEY (IdImagenOriginal)
      REFERENCES dbo.Imagenes(IdImagen)
      ON DELETE CASCADE,

    CONSTRAINT FK_ImgProc_AlgComp
      FOREIGN KEY (IdAlgoritmoCompresion)
      REFERENCES dbo.AlgoritmosCompresion(IdAlgoritmoCompresion)
);
GO

-- 7.1) Validar ProfundidadBits
ALTER TABLE dbo.ImagenesProcesadas
  ADD CONSTRAINT CHK_ProfundidadBits
    CHECK (ProfundidadBits IN (1,8,24));
GO

-- 8) Comparaciones
CREATE TABLE dbo.Comparaciones (
    IdComparacion       INT            IDENTITY(1,1) PRIMARY KEY,
    IdImagenOriginal    INT            NOT NULL,
    IdImagenProcesada   INT            NOT NULL,
    MSE                  FLOAT         NULL,
    PSNR                 FLOAT         NULL,
    ImagenDiferencias    VARBINARY(MAX) NULL,
    FechaComparacion     DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Comp_ImgOriginal
      FOREIGN KEY (IdImagenOriginal)
      REFERENCES dbo.Imagenes(IdImagen)
      ON DELETE NO ACTION,

    CONSTRAINT FK_Comp_ImgProcesada
      FOREIGN KEY (IdImagenProcesada)
      REFERENCES dbo.ImagenesProcesadas(IdImagenProcesada)
      ON DELETE CASCADE
);
GO

-- 9) Índices para búsquedas rápidas
CREATE INDEX IX_ImgProc_IdImagenOriginal
  ON dbo.ImagenesProcesadas(IdImagenOriginal);
CREATE INDEX IX_ImgProc_IdAlgCompresion
  ON dbo.ImagenesProcesadas(IdAlgoritmoCompresion);
CREATE INDEX IX_Comp_IdImagenProcesada
  ON dbo.Comparaciones(IdImagenProcesada);
GO

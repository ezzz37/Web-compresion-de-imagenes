USE master;
GO

--------------------------------------------------------------------------------
-- 1) Si existe, poner SINGLE_USER y eliminar la BD
--------------------------------------------------------------------------------
IF EXISTS (SELECT 1 FROM sys.databases WHERE name = 'DigitalizacionImagenesBD')
BEGIN
    ALTER DATABASE [DigitalizacionImagenesBD] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [DigitalizacionImagenesBD];
END
GO

--------------------------------------------------------------------------------
-- 2) Crear la base de datos
--------------------------------------------------------------------------------
CREATE DATABASE [DigitalizacionImagenesBD];
GO

--------------------------------------------------------------------------------
-- 3) Conectar a la nueva BD
--------------------------------------------------------------------------------
USE [DigitalizacionImagenesBD];
GO

--------------------------------------------------------------------------------
-- 4) Crear tabla AlgoritmosCompresion
--------------------------------------------------------------------------------
CREATE TABLE dbo.AlgoritmosCompresion (
    IdAlgoritmoCompresion INT         IDENTITY(1,1) PRIMARY KEY,
    NombreAlgoritmo       NVARCHAR(50) NOT NULL UNIQUE
);
GO

--------------------------------------------------------------------------------
-- 5) Semilla de algoritmos (inserta JPEG, PNG, RLE si no existen)
--------------------------------------------------------------------------------
INSERT INTO dbo.AlgoritmosCompresion (NombreAlgoritmo)
SELECT v.Nombre
FROM (VALUES('JPEG'),('PNG'),('RLE')) AS v(Nombre)
LEFT JOIN dbo.AlgoritmosCompresion a
    ON a.NombreAlgoritmo = v.Nombre
WHERE a.NombreAlgoritmo IS NULL;
GO

--------------------------------------------------------------------------------
-- 6) Crear tabla Imagenes (imágenes originales)
--------------------------------------------------------------------------------
CREATE TABLE dbo.Imagenes (
    IdImagen      INT            IDENTITY(1,1) PRIMARY KEY,
    Nombre        NVARCHAR(255)  NOT NULL,
    DatosImagen   VARBINARY(MAX) NOT NULL,
    AnchoOriginal INT            NOT NULL,
    AltoOriginal  INT            NOT NULL,
    FechaCarga    DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

--------------------------------------------------------------------------------
-- 7) Crear tabla ImagenesProcesadas (imágenes procesadas)
--------------------------------------------------------------------------------
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

--------------------------------------------------------------------------------
-- 7.1) Agregar restricción para ProfundidadBits
--------------------------------------------------------------------------------
ALTER TABLE dbo.ImagenesProcesadas
  ADD CONSTRAINT CHK_ProfundidadBits
    CHECK (ProfundidadBits IN (1,8,24));
GO

--------------------------------------------------------------------------------
-- 8) Crear tabla Comparaciones (comparativas entre original y procesada)
--------------------------------------------------------------------------------
CREATE TABLE dbo.Comparaciones (
    IdComparacion       INT            IDENTITY(1,1) PRIMARY KEY,
    IdImagenOriginal    INT            NOT NULL,
    IdImagenProcesada   INT            NOT NULL,
    MSE                 FLOAT          NULL,
    PSNR                FLOAT          NULL,
    ImagenDiferencias   VARBINARY(MAX) NULL,
    FechaComparacion    DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME(),

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

--------------------------------------------------------------------------------
-- 9) Índices para búsquedas rápidas
--------------------------------------------------------------------------------
CREATE INDEX IX_ImgProc_IdImagenOriginal
    ON dbo.ImagenesProcesadas(IdImagenOriginal);
GO
CREATE INDEX IX_ImgProc_IdAlgCompresion
    ON dbo.ImagenesProcesadas(IdAlgoritmoCompresion);
GO
CREATE INDEX IX_Comp_IdImagenProcesada
    ON dbo.Comparaciones(IdImagenProcesada);
GO

--------------------------------------------------------------------------------
-- 10) Crear Master Key, Certificate y Symmetric Key para AES-256
--------------------------------------------------------------------------------

-- 10.a) Crear Master Key (reemplazar 'TuPasswordMaestroSegura!' por contraseña segura)
CREATE MASTER KEY ENCRYPTION BY PASSWORD = 'TuPasswordMaestroSegura!';
GO

-- 10.b) Crear un certificado para proteger la Symmetric Key
CREATE CERTIFICATE CertUsuarios
    WITH SUBJECT = 'Certificado para proteger la llave simétrica de Usuarios';
GO

-- 10.c) Crear la Symmetric Key que usará AES_256
CREATE SYMMETRIC KEY KeyUsuarios
    WITH ALGORITHM = AES_256
    ENCRYPTION BY CERTIFICATE CertUsuarios;
GO

--------------------------------------------------------------------------------
-- 11) Crear tabla Usuarios (solo username y contraseña cifrada)
--------------------------------------------------------------------------------
CREATE TABLE dbo.Usuarios (
    IdUsuario    INT            IDENTITY(1,1) PRIMARY KEY,
    Username     NVARCHAR(50)   NOT NULL,
    PasswordEnc  VARBINARY(MAX) NOT NULL
);
GO

--------------------------------------------------------------------------------
-- 11.1) Índice único en Username
--------------------------------------------------------------------------------
CREATE UNIQUE INDEX UX_Usuarios_Username
    ON dbo.Usuarios (Username);
GO

--------------------------------------------------------------------------------
-- 12) Insertar usuario principal con credenciales '1','1'
--     (la contraseña '1' se cifra con AES-256 usando KeyUsuarios)
--------------------------------------------------------------------------------

-- 12.a) Abrir la llave simétrica para poder cifrar
OPEN SYMMETRIC KEY KeyUsuarios
    DECRYPTION BY CERTIFICATE CertUsuarios;
GO

-- 12.b) Insertar el usuario con Username='1' y Password='1' (cifrado)
INSERT INTO dbo.Usuarios (Username, PasswordEnc)
VALUES (
    '1',
    EncryptByKey(
        KEY_GUID('KeyUsuarios'),
        CONVERT(VARBINARY(MAX), '1')
    )
);
GO

-- 12.c) Cerrar la llave simétrica
CLOSE SYMMETRIC KEY KeyUsuarios;
GO

--------------------------------------------------------------------------------
-- 13) Crear el Stored Procedure para validar el usuario (sp_ValidarUsuario)
--------------------------------------------------------------------------------
CREATE PROCEDURE dbo.sp_ValidarUsuario
(
    @Username      NVARCHAR(50),
    @PasswordInput NVARCHAR(MAX)
)
AS
BEGIN
    SET NOCOUNT ON;

    -- 1) Abrir la llave simétrica para permitir desencriptar PasswordEnc
    OPEN SYMMETRIC KEY KeyUsuarios 
        DECRYPTION BY CERTIFICATE CertUsuarios;

    -- 2) Seleccionar el IdUsuario si el username existe y la contraseña coincide
    SELECT 
        IdUsuario 
    FROM dbo.Usuarios
    WHERE 
        Username = @Username
        AND CONVERT(NVARCHAR(MAX), DecryptByKey(PasswordEnc)) = @PasswordInput;

    -- 3) Cerrar la llave simétrica
    CLOSE SYMMETRIC KEY KeyUsuarios;
END
GO

--------------------------------------------------------------------------------
-- 14) (Opcional) Pruebas rápidas del SP en SQL Server
--     Puedes ejecutar estos bloques en SSMS para comprobar que funciona.
--------------------------------------------------------------------------------

/*
-- 14.1) Validar con credenciales correctas ('1','1')
EXEC dbo.sp_ValidarUsuario
     @Username = '1',
     @PasswordInput = '1';
-- → Debería devolver IdUsuario = 1

-- 14.2) Validar con contraseña incorrecta
EXEC dbo.sp_ValidarUsuario
     @Username = '1',
     @PasswordInput = 'otra_clave';
-- → No devuelve filas

-- 14.3) Validar con usuario inexistente
EXEC dbo.sp_ValidarUsuario
     @Username = 'usuarioX',
     @PasswordInput = '1';
-- → No devuelve filas
*/
--------------------------------------------------------------------------------

-- FIN DEL SCRIPT



# WEB-COMPRESIÓN-DE-IMÁGENES

![Último commit: hoy](https://img.shields.io/badge/last%20commit-hoy-brightgreen) ![C# 45.4%](https://img.shields.io/badge/C%23-45.4%25-blue) ![Lenguajes: 6](https://img.shields.io/badge/lenguajes-6-lightgrey)

**Construido con las herramientas y tecnologías:**

![JSON](https://img.shields.io/badge/JSON-informational) ![Markdown](https://img.shields.io/badge/Markdown-informational) ![npm](https://img.shields.io/badge/npm-critical) ![JavaScript](https://img.shields.io/badge/JavaScript-yellow) ![GNU Bash](https://img.shields.io/badge/GNU%20Bash-success) ![React](https://img.shields.io/badge/React-61DAFB) ![NuGet](https://img.shields.io/badge/NuGet-00AFF0) ![Axios](https://img.shields.io/badge/Axios-5A29E4)

---

## Contenido

* [Visión general](#visión-general)
* [Comenzando](#comenzando)

  * [Prerrequisitos](#prerrequisitos)
  * [Instalación](#instalación)
  * [Uso](#uso)
  * [Pruebas](#pruebas)

---

# Link del Deploy
Db: deploy en somee
Backend: deploy en el IIS de somee
Frontend: en vercel

CREDENCIALES POR DEFECTO
-------------------------
Usuario: 1
Clave: 1
-------------------------

Link: https://web-compresion-de-imagenes.vercel.app/

# Integrantes:

Pretti Ezequiel 33427
Jesus Vergara 33319
cuervo emiliano 33340
iannotta Sebastián 33751
Fiuza Pedro 33142


# Propuesta:

3. Digitalización de Imágenes
Un conversor que tome imágenes en formato analógico (simuladas) y las convierta en datos digitales
mediante muestreo y cuantización de color.
Funcionalidades:
Carga de imágenes en alta resolución.
Aplicación de muestreo con distintos niveles de resolución (ej: 100x100, 500x500, 1000x1000).
Reducción de la profundidad de bits por canal (ej: 1 bit, 8 bits, 24 bits).
Comparación de la imagen original vs. la digitalizada.
Aplicación de compresión para reducir el tamaño del archivo.

## Visión general

Web-compresión-de-imágenes es una herramienta todo en uno diseñada para facilitar la compresión, procesamiento y comparación de imágenes en una aplicación web. Combina un frontend en React con un backend potente en .NET, soportando flujos de trabajo completos desde la interacción del usuario hasta la gestión de datos.

### ¿Por qué Web-compresión-de-imágenes?

Este proyecto busca agilizar el desarrollo y las pruebas de sistemas de procesamiento de imágenes. Sus características principales incluyen:

* 🗂️ **Inicio orquestado:** Scripts para lanzar simultáneamente los servicios de frontend y backend, asegurando un desarrollo sincronizado.
* 🎨 **Frontend en React:** Interfaz modular y testeable con enrutamiento, autenticación de usuarios y componentes de galería de imágenes.
* 🔍 **Comparación de imágenes:** Herramientas para visualizar y analizar imágenes originales frente a las procesadas.
* 🔒 **Backend seguro:** Modelos de datos robustos, autenticación de usuarios y almacenamiento de imágenes con métricas detalladas.
* 🛠️ **Servicios modulares:** Funcionalidades clave de procesamiento de imágenes como redimensionado, compresión y evaluación de calidad.
* 📊 **Gestión de datos:** Esquemas de base de datos completos para imágenes, comparaciones y datos de usuarios.

---

## Comenzando

### Prerrequisitos

* **Lenguaje de programación:** C#
* **Gestor de paquetes:** npm, NuGet

### Instalación

1. Clona el repositorio:

   ```bash
   git clone https://github.com/ezzz37/Web-compresion-de-imagenes
   ```

2. Navega al directorio del proyecto:

   ```bash
   cd Web-compresion-de-imagenes
   ```

3. Instala las dependencias:

   * Con npm:

     ```bash
     npm install
     ```

   * Con NuGet:

     ```bash
     dotnet restore
     ```

### Uso

* Para ejecutar la aplicación con npm:

  ```bash
  npm start
  ```

* Para ejecutar la aplicación con .NET:

  ```bash
  dotnet run
  ```

### Pruebas

Web-compresión-de-imágenes utiliza el framework de pruebas `{test_framework}`. Ejecuta la suite de pruebas con:

* Con npm:

  ```bash
  npm test
  ```

* Con .NET:

  ```bash
  dotnet test
  ```


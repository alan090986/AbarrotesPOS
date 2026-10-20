# 🛒 AbarrotesPOS - Sistema de Punto de Venta y Gestión de Inventario

![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![WPF](https://img.shields.io/badge/UI-WPF-blue)
![SQLite](https://img.shields.io/badge/Database-SQLite-green)
![License](https://img.shields.io/badge/License-MIT-yellow)

**AbarrotesPOS** es un sistema de escritorio (Desktop) de código abierto, diseñado específicamente para la gestión operativa y punto de venta de tiendas de abarrotes. Funciona **100% offline**, garantizando la operación incluso sin conexión a internet, y está optimizado para ser utilizado con lectores de código de barras.

## ✨ Características Principales

*   **🛒 Punto de Venta (POS) Ágil:** Interfaz optimizada para escaneo rápido con lectores de código de barras (Plug & Play).
*   **📦 Control de Inventario:** Entradas, salidas, ajustes y alertas de stock mínimo en tiempo real.
*   **💵 Corte de Caja (Auditoría):** Apertura y cierre de turnos con desglose exacto de billetes y monedas, cálculo automático de efectivo esperado y registro de faltantes/sobrantes.
*   **🔐 Gestión de Roles:** Acceso diferenciado para Administradores y Cajeros (con autorización de supervisor para cierres de caja).
*   **📊 Dashboard y Reportes:** Gráficas interactivas de ventas, productos más vendidos e indicadores clave (KPIs).
*   **💾 Respaldos Automáticos:** Generación automática de copias de seguridad de la base de datos para evitar pérdida de información.
*   **🎨 UI/UX Moderna:** Interfaz limpia y responsiva inspirada en plataformas SaaS empresariales.

## 🛠️ Tecnologías Utilizadas

*   **Lenguaje:** C# 12 / .NET 8
*   **Interfaz Gráfica:** WPF (Windows Presentation Foundation) con arquitectura MVVM (Model-View-ViewModel).
*   **Base de Datos:** SQLite (Local, sin servidor) + Entity Framework Core 8.
*   **Librerías UI:** MaterialDesignInXAML (Iconos y componentes), LiveCharts2 (Gráficas).
*   **MVVM Toolkit:** CommunityToolkit.Mvvm (Source Generators).
*   **Empaquetado:** Inno Setup (Instalador).

## 📸 Capturas de Pantalla

*(Reemplaza estos enlaces con las rutas de tus imágenes subidas a GitHub)*

| Dashboard Principal | Punto de Venta |
| :---: | :---: |
| ![Dashboard](RUTA_A_TU_IMAGEN_DASHBOARD) | ![POS](RUTA_A_TU_IMAGEN_POS) |

| Inventario | Corte de Caja |
| :---: | :---: |
| ![Inventario](RUTA_A_TU_IMAGEN_INVENTARIO) | ![Caja](RUTA_A_TU_IMAGEN_CAJA) |

## 🚀 Instalación y Uso (Para Usuarios Finales)

Si solo deseas utilizar el sistema en tu tienda:
1. Descarga el archivo `Instalador_AbarrotesPOS.exe` desde la sección de [Releases](../../releases).
2. Ejecuta el instalador y sigue los pasos (Siguiente > Siguiente > Instalar).
3. Abre el programa. En el primer inicio, aparecerá un asistente para configurar los datos de tu tienda y crear tu contraseña de Administrador.

## 💻 Desarrollo y Compilación (Para Programadores)

Si deseas clonar el código y modificarlo:

### Requisitos previos
* Visual Studio 2022 o VS Code con el SDK de .NET 8.
* Inno Setup 6 (si deseas generar el instalador).

### Pasos
1. Clona el repositorio:
   ```bash
   git clone https://github.com/TU_USUARIO/TiendaAbarrotes.git
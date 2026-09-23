# Guía de Implementación de DevPod en NovaLeave

Esta guía describe cómo implementar y utilizar [DevPod](https://devpod.sh/) en **NovaLeave** para que los equipos de desarrollo en **Windows** y **macOS** compartan exactamente el mismo entorno de programación reproducible, aislado y sin diferencias de plataforma.

---

## 1. ¿Por qué DevPod en NovaLeave?

NovaLeave es una solución basada en **.NET 10**, **C#**, y base de datos **SQL Server 2022** (con opción SQLite). Al desarrollar entre diferentes sistemas operativos suelen presentarse fricciones habituales:

- **Diferencias de plataforma:** Saltos de línea (`CRLF` vs `LF`) que rompen scripts bash (`run-local.sh`, `run-tests.sh`) en Windows.
- **Versiones de SDK:** Inconsistencias entre versiones locales del .NET SDK, herramientas globales como `dotnet-ef` o runtimes.
- **Arquitectura en macOS (Apple Silicon):** Imágenes x86 como SQL Server requieren configuración específica de emulación.
- **Costos y dependencia de proveedores:** A diferencia de GitHub Codespaces o Gitpod, DevPod es **100% open-source**, no tiene vendor lock-in y permite ejecutar contenedores en local (Docker/Podman/OrbStack) o en cualquier nube/VM remota por SSH.
- **Libertad de IDE:** Soporta VS Code, Cursor, JetBrains Rider y navegador web sin cambiar la configuración del contenedor.

---

## 2. Estructura de Configuración (.devcontainer)

DevPod utiliza el estándar abierto de **Development Containers** (`devcontainer.json`). La configuración recomendada para NovaLeave se organiza así:

```text
NovaLeave/
├── .devcontainer/
│   ├── devcontainer.json
│   └── docker-compose.devcontainer.yml
├── docker-compose.yml
├── NovaLeave.sln
└── src/
```

### A. `.devcontainer/docker-compose.devcontainer.yml`

Define el servicio de desarrollo (.NET 10) y el servicio de base de datos SQL Server:

```yaml
version: '3.8'

services:
  app:
    image: mcr.microsoft.com/devcontainers/dotnet:1-10.0
    volumes:
      - ..:/workspace:cached
    network_mode: service:sqlserver
    command: sleep infinity

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Passw0rd
      - MSSQL_PID=Express
    volumes:
      - sqlserver_data:/var/opt/mssql

volumes:
  sqlserver_data:
```

### B. `.devcontainer/devcontainer.json`

Configura las extensiones de desarrollo, puertos y tareas de inicialización automática:

```json
{
  "name": "NovaLeave Dev Environment",
  "dockerComposeFile": "docker-compose.devcontainer.yml",
  "service": "app",
  "workspaceFolder": "/workspace",

  "customizations": {
    "vscode": {
      "extensions": [
        "ms-dotnettools.csdevkit",
        "ms-dotnettools.csharp",
        "ms-mssql.mssql",
        "editorconfig.editorconfig"
      ],
      "settings": {
        "dotnet.defaultSolution": "NovaLeave.sln"
      }
    }
  },

  "forwardPorts": [5211, 1433],

  "postCreateCommand": "dotnet restore && dotnet tool install --global dotnet-ef || true && chmod +x *.sh"
}
```

---

## 3. Requisitos Previos por Sistema Operativo

### En macOS (Intel o Apple Silicon M1/M2/M3/M4)
1. Instalar un runtime de contenedores: **Docker Desktop** u **OrbStack**.
2. **Importante para Apple Silicon:** 
   - En Docker Desktop: Activar *Settings > General > Use Rosetta for x86/amd64 emulation*. Esto permite que SQL Server 2022 (imagen x86_64) funcione con rendimiento óptimo bajo emulación.
3. Instalar DevPod:
   ```bash
   brew install devpod
   ```

### En Windows (10/11)
1. Instalar **Docker Desktop** con backend **WSL 2** activado.
2. Instalar DevPod mediante `winget` o descargando el instalador oficial:
   ```powershell
   winget install Loft.DevPod
   ```

---

## 4. Puesta en Marcha (Paso a Paso)

### Opción A: Interfaz Gráfica (DevPod Desktop)
1. Abrir la aplicación **DevPod**.
2. En **Providers**, verificar que `docker` esté seleccionado y activo.
3. En **Settings > Default IDE**, seleccionar su editor de preferencia (**VS Code**, **Cursor** o **JetBrains Rider**).
4. Seleccionar **Create Workspace**:
   - Ingresar la ruta de la carpeta local del proyecto o la URL del repositorio Git.
5. Hacer clic en **Create**. DevPod construirá los contenedores, ejecutará las migraciones/restore y abrirá automáticamente el IDE conectado al contenedor.

### Opción B: Terminal (DevPod CLI)
Desde la raíz del proyecto NovaLeave:

```bash
# Iniciar workspace en Docker local con VS Code
devpod up . --ide vscode

# O iniciar con JetBrains Rider
devpod up . --ide rider
```

---

## 5. Flujo de Trabajo Dentro del Contenedor

Una vez dentro del entorno DevPod, la terminal está en Linux con todas las herramientas preconfiguradas:

- **Compilar la solución:**
  ```bash
  dotnet build
  ```

- **Ejecutar pruebas unitarias e integración:**
  ```bash
  dotnet test
  # O usar los scripts existentes:
  ./run-tests.sh
  ```

- **Ejecutar la aplicación web:**
  ```bash
  dotnet run --project src/NovaLeave.Web
  ```
  La aplicación quedará accesible en `http://localhost:5211` en el navegador del host.

- **Manejo de migraciones con Entity Framework Core:**
  ```bash
  dotnet ef database update --project src/NovaLeave.Infrastructure --startup-project src/NovaLeave.Web
  ```

---

## 6. Resolución de Problemas Frecuentes

| Problema | Causa | Solución |
| :--- | :--- | :--- |
| **Error en macOS al iniciar SQL Server** | Falta de emulación amd64 en Apple Silicon. | Activar emulación Rosetta en Docker Desktop / OrbStack. |
| **Conflicto de puerto 1433** | Hay otra instancia de SQL Server corriendo en la máquina host. | Detener el servicio local o mapear otro puerto en `forwardPorts`. |
| **Scripts `.sh` con error `\r: command not found`** | Archivos guardados con terminación de línea `CRLF` (Windows). | Ejecutar `dos2unix run-local.sh` o configurar `.gitattributes` con `* text=auto eol=lf`. |

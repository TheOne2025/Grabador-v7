# 🖱️ Macro Recorder Pro - Instrucciones de Instalación

## 📋 Requisitos del Sistema

- **Sistema Operativo**: Windows 10/11 (64-bit)
- **Framework**: .NET 6.0 o superior
- **Visual Studio**: 2022 (Community, Professional o Enterprise)
- **RAM**: Mínimo 4GB, recomendado 8GB
- **Espacio en disco**: 500MB libres

## 🚀 Instalación Paso a Paso

### 1. Preparar el Proyecto

1. **Crear nuevo proyecto en Visual Studio 2022**:
   - Abrir Visual Studio 2022
   - Archivo → Nuevo → Proyecto
   - Seleccionar "Aplicación de Windows Forms (.NET)"
   - Nombre: `MacroRecorderPro`
   - Framework: `.NET 6.0`

2. **Reemplazar archivos generados**:
   - Eliminar `Form1.cs` y `Form1.Designer.cs` generados
   - Reemplazar `Program.cs` con el código proporcionado
   - Reemplazar el archivo `.csproj` con el proporcionado

### 2. Instalar Paquetes NuGet

Abrir la **Consola del Administrador de Paquetes** (Herramientas → Administrador de paquetes NuGet → Consola) y ejecutar:

```powershell
Install-Package FontAwesome.Sharp -Version 6.3.0
Install-Package Newtonsoft.Json -Version 13.0.3
Install-Package System.Drawing.Common -Version 7.0.0
Install-Package NAudio -Version 2.2.1
Install-Package MouseKeyHook -Version 5.6.0
```

### 3. Crear Estructura de Carpetas

En el **Explorador de Soluciones**, crear las siguientes carpetas y archivos:

```
MacroRecorderPro/
├── Controls/
│   ├── CustomControls.cs
│   ├── RecordingPanel.cs
│   ├── MacrosPanel.cs
│   ├── HotkeysPanel.cs
│   ├── SettingsPanel.cs
│   ├── AnalyticsPanel.cs
│   └── FilesPanel.cs
├── Models/
│   └── MacroModels.cs
├── Services/
│   ├── MacroRecordingService.cs
│   └── AdditionalServices.cs
├── MainForm.cs
└── Program.cs
```

### 4. Agregar Archivos de Código

1. **Clic derecho en el proyecto** → Agregar → Carpeta → Nombre: `Controls`
2. **Clic derecho en Controls** → Agregar → Clase → Nombre: `CustomControls.cs`
3. Repetir para todas las carpetas y archivos listados arriba
4. **Copiar y pegar** el código correspondiente en cada archivo

### 5. Configurar MainForm como Formulario Principal

1. **Clic derecho en MainForm.cs** → Ver diseñador
2. **Clic derecho en el proyecto** → Propiedades
3. En **Aplicación** → **Formulario de inicio**: Seleccionar `MainForm`

### 6. Recursos Adicionales (Opcional)

Para mejorar la apariencia, puedes agregar:

1. **Icono de aplicación**:
   - Agregar archivo `icon.ico` al proyecto
   - En propiedades del proyecto → Aplicación → Icono: seleccionar el archivo

2. **Información de ensamblado**:
   - Las propiedades ya están configuradas en el archivo `.csproj`

## 🔧 Configuración Adicional

### Permisos de Windows

La aplicación necesita permisos para:
- **Hooks globales de teclado y ratón**: Para capturar eventos
- **Acceso a archivos**: Para guardar macros
- **Registro de Windows**: Para configurar inicio automático (opcional)

### Compilación

1. **Configurar para Release**:
   - Compilación → Administrador de configuración → Configuración activa: Release

2. **Compilar proyecto**:
   - Compilación → Compilar solución (Ctrl+Shift+B)

3. **Ejecutar**:
   - Depurar → Iniciar sin depurar (Ctrl+F5)

## ⚠️ Solución de Problemas Comunes

### Error: "No se puede cargar FontAwesome.Sharp"
```bash
# Reinstalar el paquete
Uninstall-Package FontAwesome.Sharp
Install-Package FontAwesome.Sharp -Version 6.3.0
```

### Error: "MouseKeyHook no encontrado"
```bash
# Instalar dependencias adicionales
Install-Package System.Windows.Forms
Install-Package Microsoft.Win32.Registry
```

### Error: "No se puede crear hooks globales"
- **Ejecutar como administrador**: Clic derecho en Visual Studio → "Ejecutar como administrador"
- **Antivirus**: Agregar excepción para la carpeta del proyecto

### Error de compilación con .NET
- **Verificar versión**: Proyecto → Propiedades → Aplicación → Marco de destino: `.NET 6.0`
- **Limpiar y recompilar**: Compilación → Limpiar solución → Recompilar solución

## 🎯 Funcionalidades Implementadas

✅ **Interfaz completa estilo DriverBooster**
✅ **Grabación de movimientos de ratón**
✅ **Grabación de clics de ratón**
✅ **Grabación de pulsaciones de teclado**
✅ **Reproducción de macros**
✅ **Gestión de archivos de macro**
✅ **Configuración avanzada**
✅ **Estadísticas de uso**
✅ **Hotkeys globales**
✅ **Exportación/Importación**

## 📦 Crear Instalador (Opcional)

Para crear un instalador profesional:

1. **Instalar extensión**: Visual Studio Installer Projects
2. **Agregar proyecto**: Agregar → Nuevo proyecto → Setup Project
3. **Configurar**: Agregar salida del proyecto principal
4. **Compilar**: Generar archivo `.msi`

## 🔄 Actualizaciones Futuras

El proyecto está preparado para:
- ✨ **Funciones Pro**: Licenciamiento implementado
- 🔒 **Encriptación de macros**
- 🌐 **Múltiples idiomas**
- 📱 **Sincronización en la nube**
- 🎨 **Temas personalizables**

## 📞 Soporte

Si encuentras problemas:
1. **Verificar requisitos del sistema**
2. **Reinstalar paquetes NuGet**
3. **Ejecutar como administrador**
4. **Comprobar permisos de antivirus**

¡Ya tienes todo listo para compilar y usar tu Macro Recorder Pro! 🎉
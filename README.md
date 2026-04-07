# CUbePuzzle - Servidor Dedicado

## 📋 Descripción

**CUbePuzzle** es un proyecto de Unity que implementa un sistema de juego multiplayer basado en **servidor dedicado**. El servidor gestiona la lógica del juego, la sincronización de estado y la comunicación con los clientes conectados.

## 🏗️ Arquitectura - Servidor Dedicado

Este proyecto utiliza una arquitectura de **servidor dedicado** donde:

- **Servidor Central**: Gestiona la lógica del juego, validación de acciones y sincronización de estado
- **Clientes**: Se conectan al servidor dedicado mediante polling y reciben actualizaciones del estado del juego
- **Comunicación**: Los clientes envían comandos al servidor y reciben respuestas con el estado actualizado
- **Validación**: El servidor valida todas las acciones para garantizar la integridad del juego

## 📁 Estructura de Scripts - Carpeta CUbePuzzle

```
Assets/CUbePuzzle/
├── Scripts/
│   ├── Manager/          # Gestores principales del juego
│   ├── Network/          # Lógica de comunicación con el servidor
│   ├── Player/           # Lógica y control del jugador
│   ├── Puzzle/           # Lógica del puzzle (vacío)
│   └── UI/               # Interfaces de usuario (vacío)
├── Scenes/               # Escenas del proyecto
├── Prefabs/              # Prefabs reutilizables
└── Assets/               # Recursos visuales
```

### Descripción de Carpetas

#### 🎮 **Manager/** - Gestores Principales
Contiene los scripts que coordinan los diferentes sistemas del juego:

- **GameManager.cs**: Gestor principal del juego. Coordina el flujo general y estados del juego
- **LevelManager.cs**: Gestiona los niveles del puzzle, su progresión y condiciones de victoria
- **PlayerManager.cs**: Administra los datos y estado de los jugadores
- **SpawnManager.cs**: Controla el spawn/aparición de elementos en el juego
- **ConetionManager.cs**: Gestiona la conexión con el servidor dedicado
- **MenuClientSelector.cs**: Menu para seleccionar cliente/servidor

#### 🌐 **Network/** - Comunicación de Red
Implementa la comunicación cliente-servidor mediante polling:

- **PollingClient.cs**: Cliente que realiza polling al servidor dedicado para obtener actualizaciones del estado del juego

#### 👤 **Player/** - Sistema de Jugador
Contiene toda la lógica relacionada con el control y datos del jugador:

- **PlayerController.cs**: Controla los inputs y comportamiento del jugador
- **PlayerClickMover.cs**: Maneja el movimiento del jugador mediante clicks
- **PlayerData.cs**: Estructura de datos que almacena información del jugador (posición, estado, etc.)
- **SelectedPlayer.cs**: Gestiona el jugador seleccionado actualmente

#### 🧩 **Puzzle/** - Lógica del Puzzle
Carpeta reservada para scripts relacionados con la mecánica del puzzle (actualmente vacía).

#### 🎨 **UI/** - Interfaces de Usuario
Carpeta reservada para scripts de interfaz de usuario (actualmente vacía).

---

## 🚀 Inicio Rápido

1. **Menu de Selector**: Utiliza `MenuClientSelector.cs` para seleccionar entre cliente y servidor
2. **Conexión**: `ConetionManager.cs` establece y mantiene la conexión al servidor dedicado
3. **Comunicación**: `PollingClient.cs` sincroniza el estado mediante polling periódico
4. **Control del Jugador**: `PlayerController.cs` gestiona los inputs del jugador local

## 🔗 Flujo de Datos

```
PlayerController (Input)
    ↓
ConetionManager (Envía al Servidor)
    ↓
Servidor Dedicado (Valida y Procesa)
    ↓
PollingClient (Recibe Actualizaciones)
    ↓
GameManager/LevelManager (Actualiza Estado)
    ↓
PlayerManager/SpawnManager (Actualiza Entidades)
```

---

**Nota**: Este proyecto está diseñado para ejecutarse con un servidor dedicado que valida y sincroniza el estado del juego en tiempo real.

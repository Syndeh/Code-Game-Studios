# Systems Index: Ajedrez táctico (rolchess)

> **Status**: Draft
> **Created**: 2026-11-19
> **Last Updated**: 2026-11-19
> **Source Concept**: design/gdd/game-concept.md

---

## Overview

Rolchess es un duelo táctico 1 contra 1 sobre un tablero de 24 × 15. Su alcance mecánico
gira alrededor de un solo momento: el turno en que el jugador mueve hasta 12 piezas más el
Rey, asigna ataques y ve caer todos los golpes juntos (daño sumado por objetivo, escudo
restado una vez). Todo lo demás existe para alimentar ese turno (tablero, catálogo,
castillo, reclutamiento, Modificador), para mostrarlo (ficha, vista previa, render, audio),
para medirlo (métricas y analítica) o para llevarlo a otras personas (servidor, cuentas,
lobby, torneos). Los sistemas sirven a los cuatro pilares del concepto: **1 Planificar y
cosechar** (vista previa, daño fijo), **2 La fuerza está en combinar** (resolución
simultánea, cobertura), **3 Siempre hay vuelta, con límites** (Modificador, reclutamiento) y
**4 Duelo corto** (presupuesto de duración de la Máquina de turno, timer, métricas). El
índice cubre 41 sistemas: 19 de MVP, 6 de Vertical Slice (juntos forman la Fase 1
publicable), 11 de Alpha (Fase 2, partida en 2a y 2b) y 5 de Full Vision (Fases 3 a 5). El
número de cada sistema es también su lugar en el orden de diseño.

### Frontera del núcleo puro de C#

Las reglas viven en una sola librería C# sin Unity, usada por cliente, servidor, tests y
repeticiones (TD-SYSTEM-BOUNDARY). **Dentro del núcleo:** 1 Librería (contrato),
2 Catálogo (datos estáticos), 3 Máquina de turno, 4 Tablero, 5 Castillo, 6 Movimiento,
7 Alcance y asignación, 8 Cobertura, 9 Resolución, 10 Reclutamiento, 11 Modificador y
26 Registro de partida. **Fuera del núcleo:** todo lo demás. Reglas duras para cada GDD del
núcleo:

- El núcleo no depende de Localización (14), Ajustes (20), el reloj del Timer (12), las
  Métricas (19, 32) ni Unity. Errores y resultados salen como códigos o enums; la
  presentación los traduce.
- El Timer (12) no entra al núcleo: el reloj no es determinista. Solo emite el comando
  `TurnTimeout` (el cliente en partida local, el servidor online), así una repetición
  reproduce el timeout sin reloj.
- El modo de timer es configuración de la partida, que se pasa al crear el estado. Ajustes
  solo recuerda la última elección.
- Las estadísticas efectivas no viven en el Catálogo: el Catálogo es estático, los bonos
  del turno (Grito de guerra) viven en el estado de la partida y una función pura
  `EffectiveStats(state, pieceId)` los combina.
- Las Métricas (19) observan los eventos que el núcleo ya emite; el núcleo no sabe que
  existen.
- El Rival IA (25) usa la librería como cualquier jugador: emite los mismos comandos y no
  lee nada que un jugador no vea.

### Decisiones de esta sesión (2026-11-19)

Ya pasadas al concepto (`design/gdd/game-concept.md`).

- **Deshacer:** pila que cubre todos los movimientos del turno, en orden inverso. Dueño: la
  Máquina de turno (3). La tirada del Modificador no se deshace; falta decidir si el
  reclutamiento y la asignación de ataques entran en la pila.
- **Rey:** empieza en el centro de su castillo; sus propios muros no lo bloquean; no hay
  regla de entrar ni salir. Se mueve con su alcance de movimiento y no sube a los muros. Sin
  GDD propio: sus reglas se reparten en Catálogo (2), Máquina de turno (3, fin de partida),
  Castillo (5) y Movimiento (6).
- **Catapulta:** 14 de daño en el punto elegido y 10 en el área alrededor (reemplaza "la
  mitad hacia abajo"); el ataque de su ficha pasa a 14.
- **Variante sin castillos:** descartada.
- **Timer:** cuatro modos, solo en partidas locales: Variable 90 → 60 (60 + 30 y luego
  40 + 20 desde el minuto 10), Fijo 90, Fijo 60, Sin límite. Online usa el Variable.
- **Duración de partida (pilar 4):** la Máquina de turno (3) es dueña del presupuesto de
  duración; su GDD decide si hace falta un cierre de partida por tiempo dentro de
  `EvaluateMatchEnd`. Los GDD de Resolución (9), Reclutamiento (10) y Modificador (11)
  declaran cuánto suma cada uno a la duración.
- **Compuerta del arte:** no se produce arte final hasta que 3 partidas seguidas duren
  15 min o menos, con piezas provisorias. Se mide con las Métricas locales (19).
- **Calibración:** con partidas humanas, sin simulador. Después de las primeras 20 partidas
  online se escribe una propuesta de cambios (Modificador, umbral 2 de 3, duración,
  Distracción) con los datos de la Analítica online (32) y el Registro de partida (26).
- **Métricas:** en la Fase 1 cada partida local se guarda en un archivo que se exporta a mano
  (nada se envía); en la Fase 2a el envío es automático, con consentimiento, y hay un
  tablero de análisis. Cubren balance de partida, uso y retención, y rendimiento técnico.
- **Tutorial:** dos partes. El tutorial básico de 3 pasos (qué hacer en el turno; atacar
  con piezas comunes; piezas especiales) con la vista de estadísticas de todas las piezas, y
  aparte una partida tutorial completa contra un Rival IA básico.
- **Menú principal:** Juego local, Online, Ranking, Tienda, Repeticiones, Tutorial, Ajustes.
  Las entradas de fases futuras se ven bloqueadas. Tutorial ofrece el básico y la partida
  contra la IA.
- **Idioma:** español latino e inglés, se cambia desde Ajustes.
- **Fases renumeradas:** 1 web local · 2 online, cuentas y ranking (2a y 2b) · 3 tienda y
  skins · 4 torneos · 5 móvil · 6 PC y consolas (solo si el móvil tiene éxito).
- **Plazos aceptados (PR-SCOPE):** Fase 1 en 14–22 meses (~18); Fases 1 y 2 en 24–34 (~29).
  Confianza baja; se recalibran con horas reales después de 2–3 sprints. El Rival IA básico
  y las dos piezas de métricas suman alcance que esa estimación no contaba.

### Sistemas uno por uno

**Núcleo de reglas (MVP)**

- **1 Librería de reglas (contrato).** Fija qué promete el núcleo: determinismo, solo
  enteros, orden de resolución estable por ID de pieza, sin `float`, `DateTime`,
  `System.Random` ni iterar colecciones sin ordenar; azar solo del Modificador, registrado.
  La estructura del código va a `/create-architecture` y un ADR (compilar igual en Unity
  WebGL/IL2CPP y en .NET para Cloud Run). Es el cuello de botella de todo el juego.
- **2 Catálogo de piezas.** Datos estáticos por tipo: movimiento, alcance, ataque, escudo,
  vida, tope de piezas vivas. Externos al código. Incluye al Rey (vida 30).
- **3 Máquina de turno y fin de partida.** Pipeline de fases con resultados tipados
  (Modificador → mover → asignar → resolver), sin conocer a ningún sistema por nombre. Dueño
  de la pila de deshacer, de `EvaluateMatchEnd(state)` y del **presupuesto de duración de la
  partida** (pilar 4); termina el turno cuando no quedan acciones o llega `TurnTimeout`.
- **4 Tablero y despliegue inicial.** 24 × 15, castillos en sectores al azar (azar
  registrado), ejército de 4 soldados, 4 arqueros, 4 escuderos y el Rey en el centro del
  castillo.
- **5 Castillo y muros.** 16 tramos de 30 de vida sin escudo; un tramo roto abre un hueco y
  ese castillo deja de reclutar. Arqueros sobre el muro (+2 de escudo, daño compartido con el
  tramo). Los muros propios no bloquean al Rey; el Rey no sube a ellos.
- **6 Movimiento.** Hasta 12 piezas más el Rey, en cruz, atravesando aliados; el jinete pasa
  por encima de enemigos sin terminar sobre ellos. Lee las estadísticas efectivas (Grito de
  guerra) y el límite del turno (Arremetida) desde el estado, no desde el Modificador.
- **7 Alcance y asignación de ataques.** Alcance circular (dx² + dy² ≤ alcance² + 1), la
  catapulta no alcanza las 8 vecinas, flechas de ataque por pieza. Entrega a la Resolución la
  lista de asignaciones.
- **8 Cobertura del escudero.** En la fase de ataque, en lugar de atacar, el escudero cubre a
  un aliado vecino: todos los ataques enemigos a ese aliado los recibe él hasta su siguiente
  turno.
- **9 Resolución simultánea del daño.** La fórmula central del pilar 2: daño sumado por
  objetivo, escudo restado una vez, área de catapulta (14/10) y explosión del demoledor,
  redirección por cobertura, daño a muros, muertes, en orden determinista.
- **10 Reclutamiento y límites.** Obligatorio cuando sale la cara; aparecen del lado del
  castillo que mira al rival; topes de llamadas (Llamada al servicio 8, Cuerno de guerra 4
  por tipo) y de piezas vivas. Expone la consulta "¿cara disponible? ¿llamadas restantes?"
  que lee el Modificador.
- **11 Modificador (dado adaptativo).** Cinco caras con probabilidades que dependen del
  estado (piezas vivas, muros rotos, caras agotadas). Se engancha a la fase de la Máquina de
  turno; escribe bonos en el estado. Online, la tirada la decide solo el servidor. Su GDD
  mide el impacto de la Distracción en la duración y en el umbral 2 de 3.

**Interfaz, presentación, medición y base del cliente (MVP)**

- **12 Timer de turno.** Cuatro modos locales; emite `TurnTimeout`. Fuera del núcleo.
- **13 Entrada y selección.** Mouse y táctil: seleccionar pieza, mover, trazar flechas,
  deshacer. Mover 12 piezas en 60 s en pantalla táctil es un riesgo marcado por el TD.
- **14 Localización.** Tablas de textos desde el primer texto en pantalla; español latino e
  inglés.
- **15 Ficha de pieza y HUD.** Ícono de tipo, ataque, escudo, vida y color del bando (capa
  del sistema intocable por las skins); timers y estado del turno.
- **16 Vista previa de ataques.** Muestra qué daño recibe cada objetivo y qué muere antes de
  resolver, incluida la redirección por cobertura y el área. Es la herramienta del pilar 1 y
  enseña la regla del escudo sin texto.
- **17 Interfaz del Modificador.** Nombre de la cara, elecciones en el centro del tablero y
  panel "?" con las probabilidades de los dos jugadores (pilar 3b).
- **18 Render, animación y feedback de combate.** Campo de Batalla reducido (1–2
  orientaciones, animación por código) y el golpe combinado que se siente distinto (pilar 2).
  **Define el formato de skin:** toda pieza se dibuja leyendo una skin como datos; la skin
  básica es la primera. Así el Kit de skins (38) no obliga a rehacer el render. Sin
  sobrediseñar antes de pasar la compuerta de duración.
- **19 Métricas locales de partida.** Define el **esquema de eventos** del juego y lo guarda,
  por partida local, en un archivo que se exporta a mano (nada se envía). Balance: duración
  de partida y de turno, caras del Modificador, piezas vivas por turno (remontadas, umbral 2
  de 3), piezas usadas, ataques combinados, uso del deshacer y del timer. Uso: pasos del
  tutorial completados o abandonados. Técnico: fps, tiempos de carga, errores. Es lo que mide
  la compuerta de 15 min desde los primeros playtests del MVP.

**Fase 1 publicable (Vertical Slice)**

- **20 Ajustes.** Idioma, volumen, última elección de timer.
- **21 Menú principal y flujo de pantallas.** Las siete entradas, inicio de partida local con
  elección de timer, fin de partida y revancha.
- **22 Audio y SFX.** Golpes, acento que crece con la cantidad de atacantes, dado, música.
- **23 Accesibilidad.** Daltonismo (todo color con significado lleva forma o ícono), escala
  de texto, tamaños táctiles.
- **24 Tutorial básico de 3 pasos.** Turno, piezas comunes, piezas especiales, y la vista de
  estadísticas de todas las piezas.
- **25 Rival IA básico (partida tutorial).** Un rival simple para jugar una partida
  completa de práctica desde el menú Tutorial. Juega con las mismas reglas y comandos que una
  persona y solo ve lo que vería una persona. Es lo que tiene en la Fase 1 quien juega solo.

**Fase 2a: online mínimo publicable (Alpha)**

- **26 Registro de partida y serialización.** Log canónico de comandos con la versión de
  reglas grabada en cada partida. Lo comparten el Servidor de partida (28), las Repeticiones
  (35) y la Analítica online (32); evita formatos distintos y que los replays viejos se
  rompan en silencio al cambiar las reglas.
- **27 Backend online y presencia.** Firebase Realtime Database: sesión, estados (Listo, En
  espera, Jugando), reconexión, filtro del chat en el servidor.
- **28 Servidor autoritativo de partida.** Cloud Run con la librería C#: valida comandos,
  tira el dado, emite el `TurnTimeout`. Los comandos del turno se acumulan en el cliente
  (con su pila de deshacer) y se confirman al terminar el turno.
- **29 Cuentas (Google).** Identidad del jugador desde el primer día del online.
- **30 Partida rápida e invitación.** Emparejar contra un jugador en Listo o por link de
  invitación.
- **31 Emotes y frases predefinidas.** La comunicación de la 2a, sin moderación.
- **32 Analítica online y tablero.** Envío automático de los eventos de las Métricas locales
  (19) y del Registro de partida (26), con aviso de privacidad y consentimiento (público
  desde 16 años). Suma retención (sesiones, partidas por día, abandono, retorno a 1, 7 y 30
  días), rendimiento online (desconexiones, latencia) y un **tablero de análisis** para leer
  los datos sin consultas a mano. Es la fuente de la propuesta de cambios después de las
  primeras 20 partidas online.

**Fase 2b: la parte social (Alpha)**

- **33 Lobby con estados y desafíos.** Lista de jugadores con su estado, botón Listo, desafío
  con aviso de 15 s (empieza sin esperar si el desafiado está en Listo); al terminar, vuelta
  al lobby en En espera.
- **34 Chat global con filtro.** Solo en los menús Online y Torneos; filtro de palabras en
  el servidor; sin reportes ni silenciar por ahora.
- **35 Repeticiones.** Últimas 10 partidas online por cuenta, reproducibles desde el menú.
- **36 Estadísticas y ranking.** Resultados de las partidas por cuenta y tabla de ranking.

**Fases 3 a 5 (Full Vision)**

- **37 Skins y colección (Fase 3).** Dueño de las compras (entitlements) validadas en el
  servidor, nunca en el cliente.
- **38 Kit de creación de skins con validador (Fase 3).** Plantilla y herramienta para que
  cualquiera con conocimientos artísticos básicos cree un pack. El validador hace cumplir
  los límites del juego antes de aceptarlo: silueta y arma reconocibles por tipo (también en
  grises), ficha del sistema intocable, color del bando reservado, cantidad de cuadros y
  tamaño de texturas dentro del presupuesto.
- **39 Tienda con preview animado (Fase 3).** Muestra todas las piezas animadas con la skin
  elegida. Abstrae al proveedor de pago desde el principio: Stripe en web; Google Play, App
  Store y Steam exigen el suyo.
- **40 Torneos (Fase 4).** Jueves, viernes y sábados a las 21 y 23 (hora del servidor);
  inscripción hasta 5 min antes; 1 min para ponerse Listo o se pierde la llave; si nadie está
  Listo, la llave queda libre. Necesita jobs programados en el backend.
- **41 Adaptación móvil (Fase 5).** Zoom o dos niveles de estadísticas, controles táctiles,
  compras de cada tienda.

### Análisis de relaciones

El juego tiene un **núcleo denso y una periferia ancha**. Once sistemas de reglas (1–11) se
leen entre sí todo el tiempo: cada turno toca el tablero, el catálogo, el castillo, el
movimiento, la asignación, la cobertura y la resolución, y el Modificador decide qué recursos
entran. Ahí están los cuatro ciclos que hubo que cortar (ver más abajo) y los cuellos de
botella (1, 2, 3, 9). La periferia (presentación, medición, online, producto) depende del
núcleo pero casi no entre sí, salvo el online, que es una cadena: backend (27) → cuentas
(29) → emparejamiento (30) → lobby (33) → torneos (40). Las métricas cierran el circuito:
miden lo que el núcleo produce y alimentan los cambios de balance. La consecuencia práctica:
**un cambio de regla toca el núcleo y se propaga a la vista previa, el render, los
tutoriales, la IA, las métricas y el servidor**, así que las reglas tienen que estar
cerradas y medidas (duración de partida) antes de producir arte y antes de escribir el
servidor.

---

## Systems Enumeration

| # | System Name | Category | Priority | Status | Design Doc | Depends On |
|---|-------------|----------|----------|--------|------------|------------|
| 1 | Librería de reglas (contrato) (inferred) | Core | MVP | Designed | design/gdd/libreria-de-reglas.md | — |
| 2 | Catálogo de piezas | Core | MVP | Not Started | — | — |
| 3 | Máquina de turno y fin de partida | Core | MVP | Not Started | — | Librería de reglas |
| 4 | Tablero y despliegue inicial | Core | MVP | Not Started | — | Librería de reglas, Catálogo de piezas |
| 5 | Castillo y muros | Gameplay | MVP | Not Started | — | Tablero, Catálogo de piezas |
| 6 | Movimiento | Gameplay | MVP | Not Started | — | Tablero, Catálogo de piezas, Máquina de turno, Castillo y muros |
| 7 | Alcance y asignación de ataques | Gameplay | MVP | Not Started | — | Tablero, Catálogo de piezas, Máquina de turno |
| 8 | Cobertura del escudero | Gameplay | MVP | Not Started | — | Alcance y asignación, Tablero |
| 9 | Resolución simultánea del daño | Gameplay | MVP | Not Started | — | Alcance y asignación, Cobertura del escudero, Castillo y muros, Catálogo de piezas |
| 10 | Reclutamiento y límites | Gameplay | MVP | Not Started | — | Castillo y muros, Tablero, Catálogo de piezas |
| 11 | Modificador (dado adaptativo) | Gameplay | MVP | Not Started | — | Máquina de turno, Reclutamiento, Castillo y muros |
| 12 | Timer de turno | Core | MVP | Not Started | — | Máquina de turno |
| 13 | Entrada y selección (inferred) | Core | MVP | Not Started | — | — |
| 14 | Localización (inferred) | Meta | MVP | Not Started | — | — |
| 15 | Ficha de pieza y HUD | UI | MVP | Not Started | — | Catálogo de piezas, Máquina de turno, Timer, Modificador |
| 16 | Vista previa de ataques | UI | MVP | Not Started | — | Alcance y asignación, Resolución, Cobertura del escudero |
| 17 | Interfaz del Modificador | UI | MVP | Not Started | — | Modificador, Reclutamiento |
| 18 | Render, animación y feedback de combate | UI | MVP | Not Started | — | Tablero, Catálogo de piezas, Movimiento, Resolución |
| 19 | Métricas locales de partida | Meta | MVP | Not Started | — | Máquina de turno, Resolución, Reclutamiento, Modificador, Timer |
| 20 | Ajustes (inferred) | Persistence | Vertical Slice | Not Started | — | Localización |
| 21 | Menú principal y flujo de pantallas (inferred) | UI | Vertical Slice | Not Started | — | Máquina de turno, Ajustes, Localización |
| 22 | Audio y SFX | Audio | Vertical Slice | Not Started | — | Resolución, Modificador, Render |
| 23 | Accesibilidad (inferred) | Meta | Vertical Slice | Not Started | — | Ficha y HUD, Render, Ajustes |
| 24 | Tutorial básico de 3 pasos | Meta | Vertical Slice | Not Started | — | Movimiento, Alcance y asignación, Resolución, Reclutamiento, Modificador, Ficha y HUD, Vista previa, Interfaz del Modificador, Menú principal |
| 25 | Rival IA básico (partida tutorial) | Gameplay | Vertical Slice | Not Started | — | Librería de reglas, Máquina de turno, Movimiento, Alcance y asignación, Cobertura del escudero, Resolución, Reclutamiento, Modificador, Menú principal |
| 26 | Registro de partida y serialización (inferred) | Persistence | Alpha | Not Started | — | Librería de reglas |
| 27 | Backend online y presencia | Core | Alpha | Not Started | — | — |
| 28 | Servidor autoritativo de partida | Core | Alpha | Not Started | — | Librería de reglas, Máquina de turno, Timer, Modificador, Registro de partida, Backend online |
| 29 | Cuentas (Google) | Persistence | Alpha | Not Started | — | Backend online |
| 30 | Partida rápida e invitación | Meta | Alpha | Not Started | — | Backend online, Cuentas, Servidor de partida |
| 31 | Emotes y frases predefinidas | UI | Alpha | Not Started | — | Backend online, Localización |
| 32 | Analítica online y tablero | Meta | Alpha | Not Started | — | Métricas locales, Registro de partida, Backend online, Cuentas |
| 33 | Lobby con estados y desafíos | Meta | Alpha | Not Started | — | Partida rápida, Backend online, Cuentas |
| 34 | Chat global con filtro | Meta | Alpha | Not Started | — | Lobby, Localización |
| 35 | Repeticiones | Persistence | Alpha | Not Started | — | Registro de partida, Cuentas, Render, Menú principal |
| 36 | Estadísticas y ranking | Progression | Alpha | Not Started | — | Servidor de partida, Cuentas |
| 37 | Skins y colección | Economy | Full Vision | Not Started | — | Cuentas, Render, Catálogo de piezas |
| 38 | Kit de creación de skins con validador | Economy | Full Vision | Not Started | — | Render, Skins y colección |
| 39 | Tienda con preview animado | Economy | Full Vision | Not Started | — | Skins y colección, Cuentas, Render |
| 40 | Torneos | Meta | Full Vision | Not Started | — | Cuentas, Lobby, Chat global, Servidor de partida |
| 41 | Adaptación móvil | Core | Full Vision | Not Started | — | Entrada y selección, Ficha y HUD, Servidor de partida, Tienda |

---

## Categories

| Category | Description | Systems in this game |
|----------|-------------|-----------------|
| **Core** | Foundation systems everything depends on | 1, 2, 3, 4, 12, 13, 27, 28, 41 |
| **Gameplay** | The systems that make the game fun | 5, 6, 7, 8, 9, 10, 11, 25 |
| **Progression** | How the player grows over time (en habilidad y ranking, nunca en poder) | 36 |
| **Economy** | Resource creation and consumption (solo cosméticos) | 37, 38, 39 |
| **Persistence** | Save state and continuity | 20, 26, 29, 35 |
| **UI** | Player-facing information displays | 15, 16, 17, 18, 21, 31 |
| **Audio** | Sound and music systems | 22 |
| **Meta** | Systems outside the core game loop (incluye métricas y analítica) | 14, 19, 23, 24, 30, 32, 33, 34, 40 |

Sin Narrative: el juego no tiene historia.

---

## Priority Tiers

| Tier | Definition | Target Milestone | Design Urgency |
|------|------------|------------------|----------------|
| **MVP** | Required for the core loop to function. Without these, you can't test "is this fun?" | Fase 1: el duelo local jugable y medible (1–19) | Design FIRST |
| **Vertical Slice** | Required for one complete, polished area. Demonstrates the full experience. | Fase 1 publicable: ajustes, menú, audio, accesibilidad, tutorial básico, partida tutorial contra IA (20–25) | Design SECOND |
| **Alpha** | All features present in rough form. Complete mechanical scope, placeholder content OK. | Fase 2: **2a** online mínimo publicable con analítica (26–32) · **2b** parte social (33–36) | Design THIRD |
| **Full Vision** | Polish, edge cases, nice-to-haves, and content-complete features. | Fase 3 tienda y skins (37–39) · Fase 4 torneos (40) · Fase 5 móvil (41) | Design as needed |

> **Creative Director Note** (MVP): CD-SYSTEMS dio CONCERNS el 2026-11-19. Resuelto así:
> el pilar 4 tiene dueño (la Máquina de turno, 3, con el presupuesto de duración) y la
> compuerta del arte se alineó a 15 min; la calibración del Modificador y de la duración se
> hace con partidas humanas, con una propuesta de cambios después de 20 partidas online (sin
> simulador, por decisión del jugador); la numeración de la enumeración y del orden de diseño
> quedó unificada. Las Métricas locales (19), sumadas después del gate, dan cómo medir la
> compuerta y el umbral 2 de 3 con esas partidas humanas.

> **Creative Director Note** (Vertical Slice): la Fase 1 no tenía rival para quien juega
> solo. Resuelto con el Rival IA básico (25) para una partida tutorial completa, separada del
> tutorial básico de 3 pasos (24).

---

## Dependency Map

### Foundation Layer (no dependencies)

1. Librería de reglas (contrato) — define qué promete todo el núcleo; nada se diseña antes.
2. Catálogo de piezas — datos estáticos que leen casi todos los sistemas de reglas.
3. Entrada y selección — interacción del cliente, independiente de las reglas.
4. Localización — tablas de textos; todo texto en pantalla sale de acá.
5. Backend online y presencia — base de todo el online (Fase 2a); no depende de las reglas.

### Core Layer (depends on foundation)

1. Máquina de turno y fin de partida — depends on: Librería de reglas
2. Tablero y despliegue inicial — depends on: Librería de reglas, Catálogo
3. Timer de turno — depends on: Máquina de turno (fuera del núcleo; emite `TurnTimeout`)
4. Ajustes — depends on: Localización
5. Registro de partida y serialización — depends on: Librería de reglas

### Feature Layer (depends on core)

Reglas del duelo (núcleo):

1. Castillo y muros — depends on: Tablero, Catálogo
2. Movimiento — depends on: Tablero, Catálogo, Máquina de turno, Castillo
3. Alcance y asignación de ataques — depends on: Tablero, Catálogo, Máquina de turno
4. Cobertura del escudero — depends on: Alcance y asignación, Tablero
5. Resolución simultánea del daño — depends on: Alcance y asignación, Cobertura, Castillo, Catálogo
6. Reclutamiento y límites — depends on: Castillo, Tablero, Catálogo
7. Modificador — depends on: Máquina de turno, Reclutamiento, Castillo

Medición:

8. Métricas locales de partida — depends on: Máquina de turno, Resolución, Reclutamiento, Modificador, Timer
9. Analítica online y tablero — depends on: Métricas locales, Registro de partida, Backend online, Cuentas

Online y producto:

10. Cuentas (Google) — depends on: Backend online
11. Servidor autoritativo de partida — depends on: Librería, Máquina de turno, Timer, Modificador, Registro de partida, Backend online
12. Partida rápida e invitación — depends on: Backend online, Cuentas, Servidor de partida
13. Lobby con estados y desafíos — depends on: Partida rápida, Backend online, Cuentas
14. Chat global con filtro — depends on: Lobby, Localización
15. Repeticiones — depends on: Registro de partida, Cuentas, Render, Menú principal
16. Estadísticas y ranking — depends on: Servidor de partida, Cuentas
17. Skins y colección — depends on: Cuentas, Render, Catálogo
18. Kit de creación de skins con validador — depends on: Render, Skins
19. Tienda con preview animado — depends on: Skins, Cuentas, Render
20. Torneos — depends on: Cuentas, Lobby, Chat global, Servidor de partida

### Presentation Layer (depends on features)

1. Ficha de pieza y HUD — depends on: Catálogo, Máquina de turno, Timer, Modificador
2. Vista previa de ataques — depends on: Alcance y asignación, Resolución, Cobertura
3. Interfaz del Modificador — depends on: Modificador, Reclutamiento
4. Render, animación y feedback de combate — depends on: Tablero, Catálogo, Movimiento, Resolución
5. Menú principal y flujo de pantallas — depends on: Máquina de turno, Ajustes, Localización
6. Audio y SFX — depends on: Resolución, Modificador, Render
7. Emotes y frases predefinidas — depends on: Backend online, Localización

### Polish Layer (depends on everything)

1. Accesibilidad — depends on: Ficha y HUD, Render, Ajustes
2. Tutorial básico de 3 pasos — depends on: Movimiento, Alcance y asignación, Resolución, Reclutamiento, Modificador, Ficha y HUD, Vista previa, Interfaz del Modificador, Menú principal
3. Rival IA básico (partida tutorial) — depends on: Librería, Máquina de turno, Movimiento, Alcance y asignación, Cobertura, Resolución, Reclutamiento, Modificador, Menú principal
4. Adaptación móvil — depends on: Entrada y selección, Ficha y HUD, Servidor de partida, Tienda

**Cuellos de botella** (muchos dependen de ellos; alto riesgo): 1 Librería, 2 Catálogo,
3 Máquina de turno, 9 Resolución, 27 Backend online, 28 Servidor de partida.
**Hojas** (bajo riesgo, se pueden diseñar tarde): 22, 23, 24, 25, 31, 32, 34, 36, 38, 39,
40, 41.

---

## Recommended Design Order

El orden coincide con la numeración de la enumeración. Esfuerzo: S = 1 sesión, M = 2–3
sesiones, L = 4 o más (una sesión es una conversación de diseño que produce un GDD completo).

| Order | System | Priority | Layer | Agent(s) | Est. Effort |
|-------|--------|----------|-------|----------|-------------|
| 1 | Librería de reglas (contrato) | MVP | Foundation | systems-designer, lead-programmer | M |
| 2 | Catálogo de piezas | MVP | Foundation | systems-designer | S |
| 3 | Máquina de turno y fin de partida | MVP | Core | game-designer, systems-designer | M |
| 4 | Tablero y despliegue inicial | MVP | Core | game-designer | S |
| 5 | Castillo y muros | MVP | Feature | game-designer, systems-designer | M |
| 6 | Movimiento | MVP | Feature | game-designer | M |
| 7 | Alcance y asignación de ataques | MVP | Feature | game-designer, systems-designer | M |
| 8 | Cobertura del escudero | MVP | Feature | game-designer | S |
| 9 | Resolución simultánea del daño | MVP | Feature | systems-designer, game-designer | L |
| 10 | Reclutamiento y límites | MVP | Feature | game-designer, economy-designer | M |
| 11 | Modificador (dado adaptativo) | MVP | Feature | systems-designer, game-designer | L |
| 12 | Timer de turno | MVP | Core | game-designer | S |
| 13 | Entrada y selección | MVP | Foundation | ux-designer | M |
| 14 | Localización | MVP | Foundation | localization-lead | S |
| 15 | Ficha de pieza y HUD | MVP | Presentation | ux-designer, art-director | M |
| 16 | Vista previa de ataques | MVP | Presentation | ux-designer, game-designer | M |
| 17 | Interfaz del Modificador | MVP | Presentation | ux-designer | M |
| 18 | Render, animación y feedback de combate | MVP | Presentation | art-director, technical-artist | L |
| 19 | Métricas locales de partida | MVP | Feature | analytics-engineer, systems-designer | M |
| 20 | Ajustes | Vertical Slice | Core | ux-designer | S |
| 21 | Menú principal y flujo de pantallas | Vertical Slice | Presentation | ux-designer | S |
| 22 | Audio y SFX | Vertical Slice | Presentation | audio-director, sound-designer | M |
| 23 | Accesibilidad | Vertical Slice | Polish | accessibility-specialist | S |
| 24 | Tutorial básico de 3 pasos | Vertical Slice | Polish | game-designer, ux-designer | M |
| 25 | Rival IA básico (partida tutorial) | Vertical Slice | Polish | ai-programmer, game-designer | M |
| 26 | Registro de partida y serialización | Alpha (2a) | Core | systems-designer, network-programmer | M |
| 27 | Backend online y presencia | Alpha (2a) | Foundation | network-programmer | L |
| 28 | Servidor autoritativo de partida | Alpha (2a) | Feature | network-programmer, security-engineer | L |
| 29 | Cuentas (Google) | Alpha (2a) | Feature | network-programmer, security-engineer | M |
| 30 | Partida rápida e invitación | Alpha (2a) | Feature | game-designer, network-programmer | M |
| 31 | Emotes y frases predefinidas | Alpha (2a) | Presentation | ux-designer, writer | S |
| 32 | Analítica online y tablero | Alpha (2a) | Feature | analytics-engineer, security-engineer | L |
| 33 | Lobby con estados y desafíos | Alpha (2b) | Feature | ux-designer, network-programmer | M |
| 34 | Chat global con filtro | Alpha (2b) | Feature | network-programmer, community-manager | M |
| 35 | Repeticiones | Alpha (2b) | Feature | gameplay-programmer, ux-designer | M |
| 36 | Estadísticas y ranking | Alpha (2b) | Feature | systems-designer, analytics-engineer | M |
| 37 | Skins y colección | Full Vision (F3) | Feature | economy-designer, art-director | M |
| 38 | Kit de creación de skins con validador | Full Vision (F3) | Feature | technical-artist, tools-programmer, art-director | L |
| 39 | Tienda con preview animado | Full Vision (F3) | Feature | economy-designer, ux-designer | L |
| 40 | Torneos | Full Vision (F4) | Feature | game-designer, network-programmer | L |
| 41 | Adaptación móvil | Full Vision (F5) | Polish | ux-designer, unity-specialist | L |

Reclutamiento va antes que el Modificador por decisión del 2026-11-19 (el concepto decía lo
contrario): el Modificador lee los límites de reclutamiento ya cerrados.

---

## Circular Dependencies

Los cuatro ciclos encontrados se cortaron con contratos; ninguno queda abierto.

- **Modificador ↔ Reclutamiento:** el Modificador dispara el reclutamiento y sus
  probabilidades dependen de las llamadas restantes. Corte: Reclutamiento expone una consulta
  (cara disponible, llamadas restantes) que el Modificador solo lee. Reclutamiento se diseña
  primero.
- **Modificador ↔ Máquina de turno:** la Máquina tiene la fase "Modificador" y la
  Distracción hace perder el turno. Corte: la Máquina define un pipeline de fases con
  resultados tipados (seguir, perder el turno, cambiar el límite de movimientos), sin
  conocer al Modificador por nombre.
- **Modificador → Movimiento y Resolución** (Grito de guerra +2 de ataque y +2 de
  movimiento). Corte: los bonos viven en el estado de la partida; Movimiento y Resolución
  leen `EffectiveStats(state, pieceId)`. El Catálogo queda estático (corrección del TD a la
  primera propuesta, que ponía los bonos en el Catálogo).
- **Máquina de turno ↔ Resolución** (fin de partida por muerte del Rey). Corte: la Máquina es
  dueña de `EvaluateMatchEnd(state)` y lo consulta después de resolver, sin conocer cómo se
  calcula el daño.

---

## High-Risk Systems

| System | Risk Type | Risk Description | Mitigation |
|--------|-----------|-----------------|------------|
| Duración de partida (dueño: Máquina de turno, 3) | Design | La primera partida de la v3 duró 33 min contra la meta de 10–15; romper el pilar 4 obliga a cambiar reglas y todo lo que depende de ellas | Presupuesto de duración en el GDD del 3 (con posible cierre por tiempo); 9, 10 y 11 declaran su aporte; medido con las Métricas locales (19); **compuerta del arte:** no se produce arte final hasta que 3 partidas seguidas duren 15 min o menos |
| Modificador | Design | La remontada puede volverse injusta o el dado puede sentirse decisivo; la Distracción va contra los pilares 1 y 3 | Probabilidades consultables; umbral "el que tiene más piezas gana 2 de cada 3" a fijar en su GDD; calibración con partidas humanas y propuesta de cambios después de 20 partidas online |
| Resolución simultánea del daño | Design / Technical | Es la fórmula central; área, cobertura y muros interactúan; un orden no determinista rompe online y repeticiones | Orden estable por ID; tests de casos borde; vista previa como verificación visual |
| Librería de reglas | Technical | Tiene que compilar igual en Unity WebGL/IL2CPP y en .NET para Cloud Run | ADR antes de escribir código; target `netstandard2.1`; prohibiciones del contrato |
| Render y skins (18, 37, 38, 39) | Scope | 105–168 animaciones por pack en 8 direcciones; el arte no estaba contado en la primera estimación | Render del MVP reducido; formato de skin como datos desde el MVP; Kit con validador para que la comunidad aporte packs |
| Rival IA básico | Scope | Suma alcance a la Fase 1, no contado en la estimación del productor | IA simple solo para la partida tutorial; recalibrar plazos después de 2–3 sprints |
| Analítica online y tablero | Legal / Scope | Datos de jugadores desde 16 años: consentimiento, política de privacidad, retención de datos; un tablero propio es trabajo continuo | Fase 1 sin envío (archivo local); en la 2a, consentimiento explícito y un servicio de analítica existente en lugar de un tablero hecho a mano, a decidir en su GDD y un ADR |
| Entrada y selección | Design | Mover 12 o 13 piezas en 60–90 s en pantalla táctil puede ser pesado | Probar en un teléfono real antes de cerrar las reglas |
| Servidor de partida y backend (27, 28) | Technical / Scope | Timer autoritativo, reconexión, dos integraciones de Firebase; un corte a mitad de la Fase 2 dejaría un online impublicable | Fase 2 partida en 2a y 2b, cada una publicable |
| Tienda | Technical | Stripe solo sirve en web; Google Play, App Store y Steam exigen su propio cobro | Abstracción del proveedor de pago y compras validadas en el servidor desde el primer GDD |
| Chat global | Scope / Legal | Moderación con público desde 16 años | Emotes en la 2a; chat con filtro en el servidor recién en la 2b |

---

## Review Notes

> **TD-SYSTEM-BOUNDARY: CONCERNS** (2026-11-19). Aplicados: frontera del núcleo puro con el
> Timer afuera, Catálogo estático con `EffectiveStats`, servidor partido en Backend online
> (27) y Servidor de partida (28). El Registro de partida (26) se sumó después, cuando las
> cuentas pasaron a la Fase 2. Queda para los GDD: límite de la pila de deshacer, azar del
> dado registrado y decidido por el servidor, filtro del chat en el servidor, compras
> validadas en el servidor.

> **PR-SCOPE: OPTIMISTIC** (2026-11-19). Aplicados: Fase 2 partida en 2a y 2b, emotes en la
> 2a y chat con filtro en la 2b, compuerta del arte por duración de partida, plazos nuevos
> aceptados (Fase 1 ~18 meses, Fases 1 y 2 ~29). Para decidir en sus GDD (`game-designer`):
> render del MVP con una sola orientación y tweens; diferir el demoledor o el área de la
> catapulta; deshacer de un solo paso. Recalibrar plazos después de 2–3 sprints con horas
> reales. El Rival IA (25), las Métricas locales (19) y la Analítica online (32) se sumaron
> después de este gate y no están en su estimación.

> **CD-SYSTEMS: CONCERNS** (2026-11-19). Ver las Creative Director Notes bajo Priority Tiers.

---

## Progress Tracker

| Metric | Count |
|--------|-------|
| Total systems identified | 41 |
| Design docs started | 1 |
| Design docs reviewed | 0 |
| Design docs approved | 0 |
| MVP systems designed | 1/19 |
| Vertical Slice systems designed | 0/6 |

---

## Next Steps

- [x] Actualizar `design/gdd/game-concept.md` con las decisiones de esta sesión
- [ ] Prototipo v4 con todos los cambios de reglas de esta sesión
- [ ] Diseñar los sistemas del MVP en orden (`/design-system librería-de-reglas` primero)
- [ ] Correr `/design-review` en cada GDD terminado
- [ ] Medir la duración de partida con piezas provisorias: 3 partidas seguidas de 15 min o menos antes de producir arte
- [ ] Después de las primeras 20 partidas online: propuesta de cambios del Modificador y la duración
- [ ] Correr `/gate-check technical-setup` cuando los sistemas del MVP estén diseñados
- [ ] Validar los sistemas de mayor riesgo con `/vertical-slice` antes de entrar en producción

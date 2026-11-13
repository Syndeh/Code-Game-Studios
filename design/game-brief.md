# Game Brief: Ajedrez táctico (título provisional)

## In your words

# Proyecto: Videojuego de Estrategia por Turnos

## Concepto del juego
- Mezcla entre **ajedrez y juego de estrategia por turnos**
- Cada turno se divide en dos pasos: **mover piezas** → **decidir y ejecutar ataque**
- Los jugadores se alternan en turnos
- **Objetivo:** eliminar al Rey enemigo
- Hay varios tipos de piezas (diseño detallado a definir)

## Modalidad online
- **2 jugadores** por partida
- Sistema de **salas**: dos jugadores entran a enfrentarse
- Partidas **online en tiempo real por turnos**

## Sistema de skins
- Skins para todo el ejército, personalizables por el jugador
- Tres formas de obtenerlas:
  1. **Pack completo por tipo de pieza** (compra directa)
  2. **Caja de skins aleatoria** (sin repetidos, pago)
  3. **Logros dentro del juego** (gratis por progresión)
- Posibilidad de que la comunidad diseñe packs de skins

## Torneos automatizados
- Torneos en días fijos (ej: jueves y sábados) en dos horarios
- Inscripción automática o manual
- Brackets generados automáticamente
- Recompensas: skins u otros items del juego

## Plataformas objetivo
- **Prioridad:** Web (navegador)
- **Futuro:** Mobile (App Store, Google Play)
- Consolas descartadas por ahora

## Stack tecnológico decidido
| Capa | Tecnología |
|---|---|
| Motor | **Godot Engine** |
| Autenticación | **Firebase Auth** (Google, Apple, email) |
| Multijugador / Salas | **Firebase Realtime Database** |
| Inventario / Tienda | **Firebase + Stripe** |
| Torneos automatizados | **Firebase Functions** |

## Plan de desarrollo por fases
1. **Fase 1** → Juego local funcional (sin online)
2. **Fase 2** → Online básico (salas, 2 jugadores)
3. **Fase 3** → Cuentas y login social
4. **Fase 4** → Tienda y sistema de skins
5. **Fase 5** → Torneos automatizados
6. **Fase 6** → Port a mobile

## Próximos pasos
- [ ] El desarrollador comparte el diseño detallado del juego (piezas, movimientos, combate)
- [ ] Definir arquitectura técnica completa basada en el diseño
- [ ] Arrancar con Fase 1: prototipo local en Godot

## Notas
- Proyecto desarrollado en solitario con asistencia de Claude
- Claude puede ayudar con: Godot/GDScript, Firebase, Stripe, lógica de torneos, arquitectura y debugging

Revisión con Claude (2026-11-12):
- Movimiento por turno: definido en archivos de reglas aún no cargados.
- Combate: las piezas tienen puntos de vida y puntos de defensa; la pieza cae al llegar a 0.
- Victoria: el Rey tiene vida; la partida termina cuando cae.
- Online: Cloud Functions valida cada jugada en el servidor.
- Skins: se quita la caja aleatoria; quedan packs de compra directa, logros y packs de la comunidad.
- Pagos: se deciden en la Fase 4.
- Motor: Unity (C#) en vez de Godot. Backend Firebase: SDK de JavaScript vía puente jslib en web, SDK de Unity en móvil.

Reglas del cuaderno (2026-11-13): las reglas completas están en `design/reglas-cuaderno.md`
(transcripción del PDF en `design/references/reglas-cuaderno.pdf`). Decisiones sobre sus pendientes:
- Jugadores: 2 ahora; el modo de 3 jugadores, más adelante.
- Estadísticas: la tabla grande de la página 1, con el demoledor en ataque 30.
- Cara 5 del dado: se guarda una sola carga para usarla después.
- Tablero: el tamaño se ajusta al probar.
- Terrenos: el primer prototipo va sin terrenos; se prueban más adelante.
- Ejército inicial: sin cambios; las piezas especiales solo llegan reclutando con el dado.

**Pitch:** Ajedrez con vida, escudo y dado: tiras, mueves tu ejército, combinas ataques y derribas al Rey rival en duelos online 1 contra 1.

**Genre:** estrategia táctica por turnos sobre tablero

**Controls:** ratón/táctil: seleccionar pieza → casilla destino → objetivos del ataque *(guessed)*

**Question:** ¿el turno "dado → mover hasta 12 → atacar sumando fuerzas" da decisiones interesantes frente a otra persona?

**Hooks:** ataques combinados que suman fuerza contra el escudo; un dado que da recursos (refuerzos, bonus, retirada) en vez de decidir golpes; un castillo con torres que el demoledor puede derribar; el Rey es una pieza con vida, no un jaque mate.

**Intent:** needs 2 players (you said)

## Core loop
- Tiras el dado (refuerzo, bonus, retirada o turno perdido) → mueves hasta 12 piezas más el Rey → eliges ataques, solos o combinados → el rival responde
- Las partidas y los logros dan skins → personalizas tu ejército → vuelves a jugar y a los torneos

## Goal & fail state
- Ganas cuando la vida del Rey enemigo llega a 0; pierdes cuando cae el tuyo.

## Features
- [ ] Tablero cuadriculado con dos castillos 5×5 y 7 tipos de pieza (estadísticas en `design/reglas-cuaderno.md`)
- [ ] Turno: dado → mover hasta 12 piezas más el Rey → atacar
- [ ] Combate: ataques combinados, escudo y luego vida; alcances por tipo; escudero que cubre a un aliado de las flechas
- [ ] Dado de 6 caras con sus efectos (turno perdido, bonus acumulables, reclutar, retirada al castillo, movimiento libre)
- [ ] Castillo: el Rey entra y sale, los arqueros suben a las torres, el demoledor lo derriba
- [ ] Partida local de 2 jugadores (Fase 1)
- [ ] Salas online de 2 jugadores con validación en servidor (Fase 2)

## Your systems
- piezas y combate (vida, escudo, alcance) · in the slice
- dado y habilidades · in the slice
- castillo, torres y reclutamiento · in the slice
- grupos y terrenos (bosque, río, desierto) · later
- modo de 3 jugadores · later
- salas online · later
- cuentas y login social · later
- skins (packs, logros, comunidad) · later
- tienda y pagos · later
- torneos automatizados · later

## Avoid
- cajas aleatorias de pago

## Out of scope
- Terrenos y bonus de grupo hasta después del primer prototipo
- Modo de 3 jugadores, más adelante
- Online, cuentas, tienda, skins y torneos hasta su fase (2 a 5)
- Móvil hasta la Fase 6; PC (Steam) y consolas hasta la Fase 7

## Build order
1. Fase 1: juego local funcional (tablero sin terrenos, piezas, dado, mover → atacar, castillo, el Rey cae); el tamaño del tablero se ajusta jugando
2. Fase 2: online básico (salas, 2 jugadores, Cloud Functions)
3. Fase 3: cuentas y login social
4. Fase 4: tienda y skins (decidir pagos)
5. Fase 5: torneos automatizados (jueves y sábados, dos horarios, brackets, premios)
6. Fase 6: port a móvil
7. Fase 7: PC (Steam) y consolas

---
**Who it's for / what they feel:** jugadores de ajedrez y de táctica que quieren duelos cortos y personalizar su ejército *(guessed)*

# Entrada y selección

> **Status**: In Design (esqueleto + decisiones; resto sin escribir)
> **Author**: Martin Arin + Claude
> **Last Updated**: 2026-11-25
> **Last Verified**: 2026-11-25
> **Implements Pillar**: Pilar 1 (Planificar y cosechar), Pilar 4 (Duelo corto)

## Summary

[To be designed]

> **Quick reference** — Layer: `Foundation` · Priority: `MVP` · Key deps: `Librería de reglas (#1)` (provisional: Movimiento #6, Alcance #7, Cobertura #8, Reclutamiento #10)

## Overview

Entrada y selección traduce toques y clics del jugador en los comandos cerrados de la Librería (`Mover`, `Deshacer`, `TerminarMovimiento`, `AsignarAtaque`, `QuitarAtaque`, `Cubrir`, `Reclutar`, `Resolver`, `Rendirse`) y usa sus consultas puras (`MovimientosLegales`, `ObjetivosValidos`) para resaltar casillas. No decide reglas: solo propone comandos y muestra los códigos de rechazo. Mismo modelo con mouse y con dedo, sin información que aparezca solo al pasar el mouse (art bible, Controles).

## Decisiones tomadas (2026-11-25, jugador)

- **Mover:** tocar la pieza, tocar el destino. Sin arrastrar. Se resaltan los destinos legales; la pieza sigue seleccionada si le quedan movimientos.
- **Atacar:** tocar al atacante, tocar el objetivo. Tocar de nuevo el mismo objetivo (o la flecha) la quita (`QuitarAtaque`).
- **Deshacer:** solo botón, un paso por pulsación, desactivado con la pila vacía (lo pide la Máquina de turno, UI Requirements). Sin atajo de teclado ni "mantener para varios".

## Hechos de los GDD vecinos que este sistema debe respetar

- Comandos y códigos: Librería regla 3 y tabla de validación (filas 10–15); `ComandoViejo` obliga a llevar `contadorEsperado`.
- Pila de deshacer hasta 256, cerrada al terminar el movimiento (`FaseIncorrecta`).
- Tablero 24×15, casilla 68 px a 1080p, ~26 pt en móvil (bajo los ~44 pt táctiles): el tamaño táctil se decide en #41/#23.
- Sin cuadrícula por defecto; al elegir pieza se marcan alcanzables y alcance (Tablero, UI Requirements).
- Riesgo del TD: 12–13 piezas en 60–90 s en táctil (índice, riesgos de diseño).

## Player Fantasy

[To be designed]

## Detailed Design

### Core Rules

[To be designed]

### States and Transitions

[To be designed]

### Interactions with Other Systems

[To be designed]

## Formulas

[To be designed]

## Edge Cases

[To be designed]

## Dependencies

[To be designed]

## Tuning Knobs

[To be designed]

## Visual/Audio Requirements

[To be designed]

## Game Feel

[To be designed]

## UI Requirements

[To be designed]

## Cross-References

[To be designed]

## Acceptance Criteria

[To be designed]

## Open Questions

- **NOT SOURCEABLE**: `docs/engine-reference/unity/modules/` no cubre UI Toolkit/UGUI sobre el tablero ni WebGL táctil; solo `input.md` (Input System, `EnhancedTouch`). Factibilidad no verificada contra el motor fijado. No existe `scripting.md`/`core.md` equivalente.
- Contrato provisional con #6, #7, #8, #10 (sin GDD): ¿una pieza se mueve más de una vez por turno? ¿Cubrir se elige igual que atacar?
- Probar en teléfono real antes de cerrar las reglas (riesgo del índice).

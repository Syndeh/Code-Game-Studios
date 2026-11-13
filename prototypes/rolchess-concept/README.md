<!-- PROTOTYPE - NOT FOR PRODUCTION -->
<!-- Question: Does moving up to 12 pieces and combining attacks keep turns brisk? -->
<!-- Date: 2026-11-13 -->

# rolchess-concept

**Status:** concluded — PROCEED (2026-11-13)

## Hypothesis

Si cada turno el jugador puede mover hasta 12 piezas y combinar ataques, el turno sigue
siendo ágil. Se cumple si el **turno promedio dura menos de 90 s** y una **partida local
completa termina en menos de 40 min** sin que nadie diga que se hace pesada.

Riesgo que prueba: turnos demasiado largos.

## Cómo correrlo

1. En Unity Hub, **Add → Add project from disk** y elige `prototypes/rolchess-concept/`
   (editor 6000.3.28f1).
2. Abre cualquier escena (o la vacía) y pulsa **Play**. El prototipo se crea solo.
3. Partida en la misma PC: los dos jugadores se turnan con el ratón.

En pantalla: turno actual, promedio por jugador, promedio general, turno más largo y
tiempo de partida. Cada turno también queda en la consola (`[Rolchess] Turno ...`).

## Qué incluye

Tablero 24×24, dos castillos 5×5 (solo el Rey propio entra), ejército inicial
(4 soldados, 4 arqueros, 4 escuderos, Rey), dado con caras 1/2/3/4/6, mover hasta 12
piezas más el Rey, estadísticas de la tabla grande (demoledor 30), ataques combinados
(suma − escudo → vida), gana quien mata al Rey.

**Fuera:** torres, entrar/salir/destruir castillo, cara 5, escudero que cubre de flechas,
área del demoledor, colocación inicial libre, menús, arte, sonido, online.

**Supuestos del prototipo** (no están en el cuaderno): el movimiento es ortogonal y no
atraviesa piezas; los ataques no necesitan línea de visión; el bonus del dado (+1) dura
todo el turno y solo se usa uno por turno.

## Findings

**PROCEED.** Turnos ágiles ("bastante dinámico"); promedio ~1:14 sin el turno ausente, con
datos poco confiables (partida con interrupciones). Las remontadas por reclutamiento funcionan.
Nuevo riesgo: el dado manda demasiado. Error del prototipo: los reclutas aparecían del lado
izquierdo para ambos jugadores (favoreció a J2). Cambios de reglas, interfaz necesaria y las
19 notas: ver [REPORT.md](REPORT.md).

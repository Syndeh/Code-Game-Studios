<!-- PROTOTYPE - NOT FOR PRODUCTION -->
<!-- Question: with the v1 fixes, do turns stay brisk and do combined attacks become real decisions? -->
<!-- Date: 2026-11-17 -->

# Concept Prototype Report: Ajedrez táctico (rolchess) — v2

> **Date**: 2026-11-17 (partidas del 2026-11-16)
> **Prototype Path**: Engine (Unity 6.3 LTS, 6000.3.28f1)
> **Concept File**: design/game-brief.md (rules: design/reglas-cuaderno.md)
> **Previous**: prototypes/rolchess-concept/REPORT.md (v1, PROCEED con reservas)

---

## Hypothesis

Con los arreglos de la v1 (ataques planificados con flechas, atravesar aliados, alcance en
zona, muro con vida, timer de turno), el turno sigue siendo ágil y el ataque combinado se
vuelve una decisión real. Se cumple si, en una partida de corrido y simétrica:
- el turno promedio queda por debajo del timer elegido y la partida dura menos de 40 min;
- en la mitad de los turnos o más se combina un ataque de 2 o más piezas sobre un objetivo;
- nadie dice que la ganó "el dado".

---

## Riskiest Assumption Tested

**El dado manda demasiado** (el riesgo que dejó abierto la v1), junto con los criterios de
salida del CD-PLAYTEST de la v1. Resultado: **sigue abierto**. El ritmo y el ataque combinado
quedan probados; el dado todavía inclina partidas.

---

## Approach

Proyecto Unity 6.3 descartable nuevo (la v1 quedó intacta), IMGUI, con pantalla de opciones:
tablero cuadrado/hexagonal (hex oculto desde el 2026-11-16), castillos sí/no y posición por
sectores, timer 60/90 s o sin límite. Aplica las 19 notas de la v1; el detalle está en
`README.md`.

**Path chosen:** Engine — lo que se mide (ritmo, legibilidad de los ataques combinados) depende
de jugar en pantalla.

**Shortcuts taken (intentional):** sin arte, sonido ni online; valores hardcodeados; fuera la
cara 5 original (retirada), el escudero que cubre de flechas, las torres y la tirada de quién
empieza. Supuestos propios: movimiento en cruz en el cuadrado, solo se atacan muros enemigos.

---

## Result

Tres partidas de corrido el 2026-11-16, tablero cuadrado con castillos, timer de 90 s, **dos
personas distintas** jugando (no un solo jugador con los dos lados).

| # | Config | Duración | Turno prom. (J1 / J2) | Más largo | Perdidos | Ataque combinado | Ganó |
|---|---|---|---|---|---|---|---|
| 1 | castillos fijos | 13:42 | 0:47 (0:57 / 0:38) | 1:26 | 2 de 19 | no medido | Azul |
| 2 | castillos en 3 y 9 | 13:35 | 0:50 (0:43 / 0:57) | 1:20 | 4 de 20 | 13 de 16 (81%) | Rojo |
| 3 | castillos en 4 y 6 | 14:03 | 1:05 (0:57 / 1:16) | 1:25 | 5 de 17 | 11 de 12 (91%) | Azul |

"Perdidos" cuenta sobre tiradas (turnos jugados más perdidos); el ataque combinado, sobre
turnos jugados. Total de las tres: **11 de 56 tiradas perdidas (20%)**.

Lo que dijo el jugador (2026-11-17):
- **Ataques combinados:** "la decisión de ataques combinados estuvo muy bien".
- **El dado:** "a veces sí tornaba la partida para un lado, es una cuestión de azar".
- **Mejor momento (igual que en la v1):** que se pueda remontar una partida según la suerte.
- **Peor momento:** caras del dado que no sirven en el momento en que salen. Si ibas perdiendo
  y te tocaba saltar el turno, o salía un bonus de ataque y no tenías tropas para usarlo, "esa
  opción no tiene sentido".
- **Propuesta del jugador:** que las probabilidades del dado dependan de cómo va la partida.
  Ejemplo: con pocas piezas, más chances de reclutar.
- **Sin castillos:** no se probó. La opción se mantiene para otra tanda.

---

## Exit Criteria (CD-PLAYTEST de la v1)

| Criterio | Resultado |
|---|---|
| 1. Partida simétrica y de corrido, con timer | ✅ Tres partidas, ninguna cortada por timer, reclutas del lado del rival en los dos bandos |
| 2. Un rival externo | ✅ Jugaron dos personas distintas |
| 3. Ataques combinados desde varios frentes funcionando | ✅ 81% y 91% de los turnos; el jugador lo valora como decisión. Reserva: ese contador marcaba todo turno con ataque; el contador "decisivo" (más exigente) se agregó después y no tiene partidas medidas |
| 4. Turnos perdidos bien por debajo del 22% | ❌ 20% total, 29% en la partida 3 (la cara 1 sigue haciendo perder el turno, a pedido) |

Hipótesis de la v2: duración ✅ (~14 min, turno 0:47–1:05 bajo el timer de 90 s), ataque
combinado ✅, "nadie dice que la ganó el dado" ❌ (el jugador dice que a veces la inclina).

---

## Metrics

| Metric | Value |
|--------|-------|
| Path used | Engine |
| Prototype duration | varias sesiones de construcción (2026-11-13 a 2026-11-16) + 3 partidas |
| Playtesters | 2 personas distintas (incluye al menos uno externo) |
| Feel assessment | Ágil: partidas de ~14 min, ningún turno por encima de 1:26 |

- **Core tuning values discovered:**
  - Con tablero 24 de ancho, castillos y timer de 90 s la partida dura ~14 min: muy por debajo
    de los 40 min de la v1. Hay margen para decidir qué es un "duelo corto".
  - Timer de 90 s: nunca se agotó. 60 s probablemente alcance para jugadores con experiencia.
  - Cara 1 (perder turno) con probabilidad 1/6 da ~20% de tiradas perdidas: demasiado.
  - Dos demoledores (30 + 30) matan a un Rey de 30 de vida de una vez (partida 1): a vigilar.
- **Assumptions confirmed:**
  - Planificar los ataques con flechas y resolverlos juntos hace que combinar sea natural.
  - Las reglas nuevas de la v1 (atravesar aliados, alcance en zona, muro con vida, escudo una
    vez por objetivo) no frenan el ritmo.
  - La remontada sigue siendo el mejor momento del juego.
- **Assumptions disproved / to redesign:**
  - Un dado con caras fijas e iguales para los dos no sirve tal cual: hay caras que en ciertos
    estados de la partida no tienen efecto útil, y perder el turno castiga más al que va
    perdiendo.
- **Emergent mechanic (propuesta):** dado que se adapta al estado de la partida (más chances
  de reclutar con pocas piezas). Convierte la remontada en algo del diseño y no solo de la
  suerte.

---

## Recommendation: PROCEED

El núcleo (mover muchas piezas, planificar y combinar ataques, reclutar) está probado: es ágil
y el jugador y un rival externo lo disfrutan. Lo que falla es el dado, y lo que hay que
cambiar son sus reglas (probabilidades y caras), no el núcleo. Eso se diseña y se calcula en
papel antes de construir otra versión.

**Reservas:**
- El dado es el riesgo principal y el criterio 4 no se cumplió.
- El contador de ataque combinado decisivo no tiene partidas medidas.
- No se jugó sin castillos.
- Muestra chica: tres partidas, una sola configuración de tablero.

**Next steps** (rigor `full`; aún no existe `design/gdd/game-concept.md`):
1. `/brainstorm` — formalizar el concepto en `design/gdd/game-concept.md` con las reglas de la
   v1 y la v2; decidir qué significa "duelo corto"; dejar el rediseño del dado como primer
   problema abierto.
2. `/design-review design/gdd/game-concept.md`
3. `/map-systems`
4. `/design-system dado` — probabilidades según el estado de la partida, caras que siempre
   tengan sentido, qué hacer con la cara 1; con fórmulas y valores esperados.
5. Otra tanda de prueba (puede ser sobre la v2): dado nuevo, contador decisivo, variante sin
   castillos.

---

## Lessons Learned

- **What assumptions were broken by actually building this?** Que bastaba con cambiar las
  caras del dado: el problema es que una cara fija vale distinto según cómo va la partida.
- **What surprised us?** Lo rápido que quedaron las partidas (~14 min, contra ~44 en la v1) y
  que el ataque combinado apareciera en casi todos los turnos.
- **What would we test differently next time?** Medir con el contador decisivo desde la
  primera partida, anotar qué cara salió en cada turno y el estado de la partida en ese
  momento, y alternar configuraciones (con y sin castillos) en la misma tanda.

---

> *Prototype code location: `prototypes/rolchess-v2-concept/`*
> *This code is throwaway. Never refactor into production.*

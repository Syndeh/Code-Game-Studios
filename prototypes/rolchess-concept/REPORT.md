<!-- PROTOTYPE - NOT FOR PRODUCTION -->
<!-- Question: Does moving up to 12 pieces and combining attacks keep turns brisk? -->
<!-- Date: 2026-11-13 -->

# Concept Prototype Report: Ajedrez táctico (rolchess)

> **Date**: 2026-11-13
> **Prototype Path**: Engine (Unity 6.3 LTS, 6000.3.28f1)
> **Concept File**: design/game-brief.md (rules: design/reglas-cuaderno.md)

---

## Hypothesis

Si cada turno el jugador puede mover hasta 12 piezas y combinar ataques, el turno sigue
siendo ágil — lo sabremos si el **turno promedio dura menos de 90 s** y una **partida local
completa termina en menos de 40 min** sin que nadie diga que se hace pesada.

---

## Riskiest Assumption Tested

**Turnos demasiado largos** (elegido por el jugador por sobre "el dado decide demasiado" y
"el combate no da decisiones"). Resultado: no se sostuvo como riesgo — el jugador describe el
juego como "bastante dinámico". Pero apareció con fuerza otro riesgo que no era el elegido:
**el dado manda demasiado**.

---

## Approach

Un proyecto Unity 6.3 descartable, todo en un script (`Assets/Scripts/RolchessPrototype.cs`)
dibujado con IMGUI: se crea solo al pulsar Play, sin escena, sprites ni configuración de input.
Hecho en una sesión; compiló a la primera en batchmode.

**Path chosen:** Engine
**Reason for path:** la duración del turno depende de cómo se juega en pantalla (seleccionar
piezas, ver alcances, cálculo automático de daño), que el papel no puede medir.

**Shortcuts taken (intentional):**
- Cuadrados de colores con letras y vida; sin arte, menús, sonido ni online
- Valores hardcodeados (tabla grande del cuaderno, demoledor 30)
- Fuera: torres, entrar/salir/destruir castillo, cara 5 del dado, escudero que cubre,
  área del demoledor, colocación inicial libre
- Supuestos no escritos en el cuaderno: movimiento ortogonal sin atravesar piezas, ataque
  sin línea de visión, bonus del dado dura el turno

---

## Result

Una partida hotseat completa (2 jugadores, ganó J2), jugada con interrupciones.

- **Ritmo:** "El tema del tiempo por turnos me parece que estuvo bien. El juego se ve bastante
  dinámico."
- **Mejor momento — la remontada:** un lado parecía perdido, le tocó reclutar con el dado y la
  partida se dio vuelta. "Se puede dar vuelta a una partida y eso está bueno."
- **Peor momento — reclutamiento desbalanceado:** los reclutas de ambos jugadores aparecían del
  lado izquierdo del castillo: atrás para J1 (azul), en el frente para J2 (rojo). **Error del
  prototipo** que favoreció a J2, el ganador. La partida quedó sesgada.
- **El dado manda:** perder el turno con un 1 desbalancea; 8 de 36 turnos se perdieron (22%).
- **Sorpresa (mala):** las piezas no atraviesan a sus compañeros; una línea de escuderos dejaba
  trabados a los soldados de atrás. "Eso no me gustó."

### Las 19 notas de la partida

1. No se puede deshacer un movimiento.
2. No se entiende quién ataca a quién.
3. No deja combinar ataques desde varios frentes (atacantes desalineados no cuentan como en
   alcance).
4. Los reclutados aparecen detrás del castillo; deberían aparecer del lado que mira al rival.
5. Castillos en extremos opuestos del tablero.
6. El Rey se siente encerrado en el castillo.
7. Las piezas propias se pueden atravesar ("se corren"); las enemigas no.
8. Ataques planificados: se asigna quién ataca a quién (con flecha) y se resuelven todos al final.
9. El dado tiene que verse grande, con su efecto, y con la lista de qué hace cada cara.
10. El escudo se resta una sola vez por objetivo (5 + 5 contra escudo 4 = 6 de daño, no 2).
11. El castillo es un muro solo por fuera, con vida; cualquier pieza en alcance puede atacarlo.
12. La explosión del demoledor daña también a los aliados.
13. Tiempo por turno: bien, dinámico.
14. Timer de turno con cuenta regresiva (~40 s o más, según experiencia); al llegar a 0 termina.
15. Fin de turno automático cuando no quedan acciones.
16. Probar variante sin castillos, con el Rey atrás.
17. Probar tablero hexagonal.
18. Alcance 1 = todas las direcciones (soldado, escudero, jinete, Rey).
19. Ver ataque, escudo y vida en cada pieza sin hacer clic.

---

## Metrics

| Metric | Value |
|--------|-------|
| Path used | Engine |
| Iterations to playable | 1 (compiló y se jugó sin correcciones) |
| Prototype duration | 1 sesión de construcción + 1 partida |
| Playtesters | 1 interno (jugando los dos lados) / 0 externos |
| Feel assessment | "Bastante dinámico". Panel: partida 89:55, promedio J1 1:17 (17 turnos), J2 3:33 (19), general 2:28, turno más largo 45:32, 8 turnos perdidos. Sin el turno de 45:32 (ausencia): promedio ~1:14 en 35 turnos, partida ~44 min |
| Hypothesis verdict | PARTIALLY CONFIRMED — turno ~1:14 bajo la meta de 1:30, pero partida ~44 min sobre la meta de 40; el jugador la vio confirmada; datos poco confiables (interrupciones). Ajustado tras CD-PLAYTEST |

---

## Recommendation: PROCEED

El jugador lo dice así: "el juego es tal cual me lo imaginaba, un poco mejor en algunas cosas,
un poco peor, pero se puede ajustar en otras, y así que vale mucho la pena seguir." El riesgo
elegido (turnos largos) no apareció: con la corrección del turno ausente, el promedio queda
debajo de la meta, y la sensación fue de juego dinámico. La remontada muestra que el loop de
dado + refuerzos genera tensión. Los problemas encontrados son de reglas puntuales y de
interfaz, no del núcleo. Reservas: la medición no fue limpia, la partida estuvo sesgada por el
error de reclutamiento, y el peso del dado quedó como el nuevo riesgo principal.

### Creative Director Review (CD-PLAYTEST): CONCERNS — aceptado con reservas

PROCEED se mantiene (decisión del jugador, 2026-11-13), con estas reservas:
- La hipótesis se cumplió a medias: la duración de la partida es parte de la promesa de
  "duelos cortos", no un detalle.
- El dado se aleja del hook del brief ("da recursos en vez de decidir golpes"): 22% de turnos
  perdidos y la sensación de que "el dado manda".
- Los ataques combinados, el principal diferenciador, casi no se probaron (notas 2 y 3).
- Evidencia débil: 1 jugador interno jugando los dos lados, con interrupciones, en hotseat; el
  error de reclutamiento pudo causar en parte la remontada.

**Criterios de salida para el próximo prototipo:**
1. Una partida simétrica y de corrido, con timer de turno.
2. Un rival externo.
3. Ataques combinados desde varios frentes funcionando.
4. Caras del dado ajustadas para que los turnos perdidos queden bien por debajo del 22%.

Además: escribir los cambios de reglas en el concepto antes de `/design-system`, y decidir
qué significa "duelo corto" (¿menos de 40 min o menos?), porque define el timer y el tablero.

---

## If Proceeding

- **Core tuning values discovered:**
  - Turno de ~1:15 en promedio con 12 movimientos → un timer de ~40–60 s es exigente pero
    viable para jugadores con experiencia (nota 14); calibrar por nivel.
  - 22% de turnos perdidos con la cara 1 se siente excesivo.
- **Assumptions confirmed:**
  - Mover hasta 12 piezas por turno no vuelve lento el juego.
  - El dado y el reclutamiento permiten remontadas.
- **Assumptions disproved (cambian las reglas del cuaderno):**
  - Alcance 1 es en todas las direcciones, no solo horizontal/vertical.
  - Las piezas propias se atraviesan; las enemigas bloquean.
  - El castillo es un muro perimetral con vida que cualquier pieza puede atacar, no un bloque
    que solo el demoledor destruye de un golpe.
  - La explosión del demoledor también daña aliados.
  - Ataques: se planifican todos (con flechas) y se resuelven juntos; el escudo se resta una
    vez por objetivo.
- **Emergent mechanics:** formaciones (escuderos al frente, soldados que avanzan entre ellos)
  aparecen como algo deseado en cuanto las piezas propias se atraviesan.
- **A probar en otro prototipo:** tablero hexagonal, variante sin castillos, rebalanceo de las
  caras del dado, timer con fin automático.
- **Interfaz mínima necesaria:** dado grande y explicado, estadísticas visibles en cada pieza,
  flechas de ataque, deshacer movimiento, reclutas del lado del frente.

**Next steps** (rigor `full`; aún no existe `design/gdd/game-concept.md`):
1. `/brainstorm` — formalizar el concepto en `design/gdd/game-concept.md` con estas reglas nuevas
2. `/design-review design/gdd/game-concept.md`
3. `/map-systems`
4. `/design-system [mecánica]` — empezando por combate y dado (usar estos hallazgos en Tuning
   Knobs y Formulas)

---

## Lessons Learned

- **What assumptions were broken by actually building this?**
  El cuaderno no definía cómo se mueve una pieza (en cruz o libre, si atraviesa) ni cómo se
  combinan ataques en el tiempo; el prototipo tuvo que suponerlo y varias suposiciones
  resultaron equivocadas (notas 3, 7, 10, 18).
- **What surprised us that didn't show up in the brainstorm?**
  Que el riesgo real no fuera la duración del turno sino el peso del dado, y que el
  reclutamiento sea la fuente de las mejores remontadas.
- **What would we test differently next time?**
  Jugar una partida de corrido para medir, con timer de turno integrado. Revisar que el
  prototipo sea simétrico entre jugadores antes de probarlo (el error de reclutamiento sesgó
  el resultado). Mostrar todas las estadísticas en pantalla desde el inicio.

---

> *Prototype code location: `prototypes/rolchess-concept/`*
> *This code is throwaway. Never refactor into production.*

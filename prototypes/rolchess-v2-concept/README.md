<!-- PROTOTYPE - NOT FOR PRODUCTION -->
<!-- Question: with the v1 fixes, do turns stay brisk and do combined attacks become real decisions? -->
<!-- Date: 2026-11-13 -->

# rolchess-v2-concept

**Status:** in progress

## Hypothesis

Con los arreglos de la v1 (ataques planificados con flechas, atravesar aliados, alcance en
zona, muro con vida, timer de turno), el turno sigue siendo ágil y el ataque combinado se
vuelve una decisión real. Se cumple si, en una **partida de corrido y simétrica**:
- el turno promedio queda por debajo del timer elegido y la partida dura **menos de 40 min**;
- en la mitad de los turnos o más se combina un ataque de 2 o más piezas sobre un objetivo;
- nadie dice que la ganó "el dado".

Comparar: cuadrado vs hexagonal, con vs sin castillos (criterios del CD-PLAYTEST de la v1).

## Cómo correrlo

1. Unity Hub → **Add → Add project from disk** → `prototypes/rolchess-v2-concept/`
   (editor 6000.3.28f1).
2. Pulsa **Play** en cualquier escena. Aparece la pantalla de opciones: tablero, castillos,
   tamaño y timer. **Empezar**.
3. Partida en la misma PC, turnándose con el ratón.

## Qué cambió desde la v1 (notas de la partida 1)

| Nota | En la v2 |
|---|---|
| 1 deshacer | Botón "Deshacer último movimiento" |
| 2, 8, 10 ataques | Se asignan con flechas y se resuelven todos juntos; daño sumado por objetivo, escudo una vez; vista previa del daño |
| 3, 18 alcance | Zona: cualquier casilla a esa distancia o menos (catapulta 2–6) |
| 4 reclutas | Aparecen del lado que mira al rival (y el error de asimetría de la v1 desaparece) |
| 5 castillos | En los extremos del tablero |
| 6 Rey | Regla del cuaderno: sale apareciendo al lado / entra donde elijas; ese turno no se mueve más |
| 7 atravesar | Se atraviesan aliados, nunca enemigos |
| 9 dado | Dado grande en el centro al tirarlo, efecto en el panel y lista de caras siempre visible |
| 11 muro | Solo el borde; 16 tramos de 30 de vida, sin escudo; cualquier pieza enemiga en alcance lo ataca; romper uno abre un hueco |
| 12 demoledor | Explota: daña todas las casillas vecinas (aliados y muros incluidos) y muere; la vista previa marca MUERE sobre él |
| 14 timer | 40 / 60 / 90 s o sin límite; al llegar a 0 se resuelven los ataques asignados y termina el turno |
| 15 auto | Pasa solo a ataque cuando no quedan movimientos; termina solo cuando no quedan ataques o todos están asignados (2 s de aviso) |
| 16 sin castillos | Opción: Rey atrás, reclutas a su alrededor |
| 17 hexagonal | Opción de tablero hexagonal: casillas con lado plano arriba, en columnas verticales, y fichas hexagonales |
| 19 estadísticas | Cada ficha muestra la letra grande y, en columna, V vida, A ataque, D defensa |

Sin cambios a pedido: la cara 1 del dado sigue haciendo perder el turno.

**Supuestos del prototipo** (no los dijo el jugador): en el cuadrado se mueve en cruz (en
hex, a las 6 vecinas); el castillo cuenta como destruido para reclutar con 4 tramos rotos;
solo se atacan muros enemigos (la explosión del demoledor sí daña los propios).

**Fuera:** cara 5 del dado, escudero que cubre de flechas, torres, tirada para ver quién empieza.

## Findings

Pendiente: se completa después de jugar.

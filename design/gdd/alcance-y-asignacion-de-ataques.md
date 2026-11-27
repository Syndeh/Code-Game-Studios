# Alcance y asignación de ataques

> **Status**: Designed (pendiente de `/design-review` en una sesión nueva)
> **Author**: Martin Arin + Claude
> **Last Updated**: 2026-11-25
> **Last Verified**: 2026-11-25
> **Implements Pillar**: Pilar 1 (Planificar y cosechar), Pilar 2 (La fuerza está en combinar); Pilar 4 (Duelo corto) como restricción
> **Creative Director Review (CD-GDD-ALIGN)**: CONCERNS (accepted) 2026-11-25 — pilar 4 (regla 14: la fase se abre sin ataques útiles), pilar 1 (reglas 6 y 7: fuego amigo sin confirmación; lo cubre la Vista previa, #16) y pilar 3 (regla 7: sacrificar demoledores puede mejorar el Modificador)

## Summary

Alcance y asignación de ataques decide a qué casillas puede pegar cada pieza y guarda, durante la fase de atacar, qué ataque hará cada una. El jugador traza hasta un ataque por pieza, los cambia las veces que quiera y, al resolver, todos salen juntos. Existe para que combinar golpes sobre un objetivo sea una decisión visible y corregible antes de que algo ocurra.

> **Quick reference** — Layer: `Feature` · Priority: `MVP` · Key deps: `Librería de reglas (#1), Catálogo de piezas (#2), Máquina de turno (#3), Tablero (#4), Castillo y muros (#5)`

## Overview

Este sistema responde dos preguntas de la fase `Atacar`: **a dónde llega cada pieza** y **qué ataque tiene asignado**. El alcance es circular sobre la grilla de 24 × 15, sin línea de visión: ni las piezas ni los muros tapan un ataque. Con él calcula `ObjetivosValidos(pieza)`, valida `AsignarAtaque` y `QuitarAtaque`, y guarda como mucho un ataque por pieza. Al resolver, entrega a Resolución (#9) la lista de asignaciones en orden de ID. No calcula daño, no mata a nadie y no mueve nada.

El jugador lo usa de forma activa en casi todos los turnos. Elige una pieza, ve dónde puede pegar, elige el objetivo y aparece una flecha; repite hasta armar los golpes combinados que quería y resuelve. Asignar no compromete nada: un ataque nuevo reemplaza al anterior y se puede quitar.

Hay tres maneras de atacar: el **ataque directo** de casi todas las piezas, el **disparo de área** de la catapulta, que elige una casilla cualquiera de su alcance, y la **explosión** del demoledor, que no elige objetivo. Sin este sistema, el gancho del juego (sumar el daño de varias piezas contra un escudo que se resta una vez) no tendría dónde decidirse.

*Alternativa descartada:* resolver cada ataque al elegirlo. Elimina la combinación simultánea y la vista previa, que son el centro de los pilares 1 y 2.

## Player Fantasy

**La mesa de guerra.** Sos el general sobre el mapa: tocás una pieza y ves hasta dónde llega, trazás una flecha, sumás otra y otra más sobre el mismo blanco, y ves cómo tres golpes que solos rebotaban pasan a romper el escudo.

**Momento ancla:** el soldado rival tiene escudo 3 y vida 25. Trazás la flecha de un soldado tuyo y falta mucho; sumás dos soldados y un arquero, las flechas se funden en una sola punta y aparece MUERE (8 + 8 + 8 + 6 = 30, menos 3 de escudo = 27). Todavía no pasó nada. Sacás una flecha, la mandás a otro lado, la devolvés. Recién entonces resolvés.

**Pilares que sirve:**

- Pilar 1, *"Al mover armás la estrategia; al atacar ejecutás el plan y ves sus frutos."*
- Pilar 2, *"Ninguna pieza gana sola: el poder sale de juntar varias sobre un mismo objetivo."*

**Qué le exige al resto del diseño:** un resaltado de alcance legible sin contar casillas (pilar 1), flechas que convergen (pilar 2), corregir sin costo, y una vista previa que se actualice con cada flecha (#16). El riesgo propio de esta fantasía es el fuego amigo: un general puede dañar lo suyo, pero nunca **por sorpresa**.

*Encuadres considerados:* "el trazo del plan" (solo las flechas) y "lo que ve mi alcance" (solo el resaltado). El jugador eligió la mezcla el 2026-11-25.

## Detailed Design

### Core Rules

1. **Quién puede atacar.** Toda pieza viva del jugador del turno, una vez por turno, en la fase `Atacar`. Haber movido no lo impide; tampoco haber sido reclutada ese turno (Reclutamiento, #10, reglas 9 y 10). El Rey ataca como cualquier pieza.
2. **Objetivo = casilla.** El `objetivo` de `AsignarAtaque(pieza, objetivo)` es siempre una casilla `(columna, fila)`, nunca un ID de pieza. Así, un solo dato cubre una pieza, un tramo, un arquero sobre un tramo y una casilla vacía. Lo que haya en la casilla recibe el golpe según Resolución (#9) y Castillo (#5, regla 8).
3. **Alcance circular.** Una casilla está en alcance si cumple `en_alcance` (F1). Se usa `alcanceMin` y `alcanceMax` de `EffectiveStats`, de modo que un arquero sobre un tramo propio en pie alcanza 1–6. El Grito de guerra no cambia el alcance. La propia casilla nunca está en alcance.
4. **Sin línea de visión.** Nada bloquea un ataque: ni piezas ni tramos de ningún bando (Castillo, regla 3).
5. **Modos de ataque.** El modo sale del Catálogo, no del jugador:

   | Modo | Quién | Objetivo válido |
   |---|---|---|
   | **Directo** | Pieza con `danoArea` 0 que no es demoledor (soldado, arquero, escudero, jinete, Rey) | Casilla en alcance con una pieza **enemiga** o un tramo **enemigo en pie** |
   | **Área** | Pieza con `danoArea` mayor que 0 (hoy la catapulta) | **Cualquier** casilla del tablero en alcance: vacía, hueco, pieza propia o enemiga, tramo propio o enemigo |
   | **Explosión** | El demoledor (ID de tipo 5) | Solo **su propia casilla**. No necesita enemigo cerca |

   El modo se evalúa en este orden: explosión, área, directo. Un `danoArea` distinto de 0 en el demoledor no cambia su modo.
6. **Área: decisión del 2026-11-25.** La catapulta puede apuntar también a un tramo **propio** en pie y a un hueco. Es una excepción al ataque directo, que nunca puede tomar lo propio. La regla 4 de Castillo aclara que su prohibición vale solo para el modo directo (2026-11-25).
7. **Explosión: decisión del 2026-11-25.** Asignar la explosión es la orden de estallar al resolver. Su zona (las 8 vecinas) y su daño son de Resolución (#9). Como el objetivo es su casilla, **`alcanceMin` y `alcanceMax` del demoledor no se leen**. Con enemigos, aliados o nada alrededor, vale igual; puede sacrificarse solo.
8. **Ataque directo sobre hueco.** Un tramo roto no es objetivo. Si hay una pieza enemiga parada en ese hueco, la pieza sí lo es.
9. **Una acción por pieza.** Cada pieza tiene como mucho **un** ataque asignado. `AsignarAtaque` sobre una pieza que ya tenía uno **lo reemplaza**, en un solo comando (decisión del 2026-11-25). Un escudero ataca **o** cubre: asignarle un ataque **borra la cobertura** que tenía, y `Cubrir` borra su ataque (Cobertura, #8, regla 5). Un comando rechazado no cambia nada: si el ataque nuevo no vale, queda el anterior.
10. **Quitar.** `QuitarAtaque(pieza)` borra el ataque asignado. Sin ataque, se rechaza con `SinAtaqueAsignado`. No toca coberturas: una cobertura se quita con `QuitarCobertura` (#8, regla 8). No usa la pila de deshacer.
11. **Sin efecto hasta resolver.** Asignar, reemplazar y quitar no cambian piezas, tramos ni estadísticas. Durante `Atacar` nada de lo que define la validez cambia, así que una asignación aceptada **sigue siendo válida** hasta resolver y Resolución no la revalida.
12. **Códigos de rechazo (fila 14 de la Librería), en este orden.** Se devuelve el primero que falla:

    | Orden | Código | Cuándo | ¿Lo ve el jugador? |
    |---|---|---|---|
    | 1 | `ObjetivoFueraDelTablero` | Columna fuera de 0–23 o fila fuera de 0–14 (cualquier int32) | Interno |
    | 2 | `ObjetivoInvalido` | Demoledor con una casilla que no es la suya | Interno |
    | 3 | `FueraDeAlcance` | Directo o área, y la casilla no cumple F1 (incluye la propia y la zona ciega) | Sí |
    | 4 | `ObjetivoPropio` | Directo, y la casilla tiene una pieza propia o un tramo propio en pie | Sí |
    | 5 | `SinObjetivo` | Directo, y la casilla no tiene pieza ni tramo en pie | Sí |
    | — | `SinAtaqueAsignado` | `QuitarAtaque` sobre una pieza sin ataque | Sí |

    Los códigos de turno, pieza y fase (`NoEsTuTurno`, `PiezaInexistente`, `NoEsTuPieza`, `FaseIncorrecta`) son de la Librería y se evalúan antes.
13. **Consultas puras.** Valen para cualquier pieza viva, de los dos bandos y en cualquier fase (todo es público). Devuelven casillas **ordenadas por fila y después columna**, sin repetir. Con un ID inexistente devuelven vacío.
    - `CasillasEnAlcance(pieza)`: las del tablero que cumplen F1, tengan o no objetivo. Para el demoledor, vacía.
    - `ObjetivosValidos(pieza)`: las que `AsignarAtaque` aceptaría si el dueño estuviera en `Atacar`. Coinciden exactamente: no hay dos reglas.
14. **Acción posible.** Para el salto de fase (Máquina, regla 10), una pieza tiene acción si `ObjetivosValidos` no está vacía, y un escudero también si `AliadosCubribles` (#8) no lo está. Con una catapulta o un demoledor propios vivos, `Atacar` **nunca se salta** (decisión del 2026-11-25).
15. **Eventos.** `AtaqueAsignado(pieza, objetivo, modo, objetivoAnterior)` (anterior vacío si no había) y `AtaqueQuitado(pieza, objetivo)`. Al desplazar una cobertura, primero va el evento de #8 (`CoberturaQuitada`, motivo `Desplazada`) y después `AtaqueAsignado`.
16. **Entrega a Resolución.** Una lista `(pieza, objetivo, modo)` por ID de pieza ascendente. El orden fija eventos, nunca resultados (Librería, regla 15). Al cerrar el turno la lista se vacía (Máquina, regla 11).
17. **Lo que el jugador no puede hacer.** Atacar dos veces con una pieza, atacar directo lo propio, apuntar directo al suelo, elegir el modo, o asignar ataques en el turno rival.

### States and Transitions

| Estado de la pieza (en `Atacar`) | Qué significa |
|---|---|
| **Libre** | Sin ataque ni cobertura |
| **Con ataque** | Un objetivo asignado |
| **Cubriendo** | Solo escudero (dueño: #8) |

| De → a | Disparador | Efecto |
|---|---|---|
| Libre → Con ataque | `AsignarAtaque` válido | Guarda el objetivo |
| Con ataque → Con ataque | `AsignarAtaque` válido (también con el mismo objetivo) | Reemplaza |
| Cubriendo → Con ataque | `AsignarAtaque` válido | Borra la cobertura y guarda el objetivo |
| Con ataque → Libre | `QuitarAtaque` | Borra |
| Libre → Cubriendo | `Cubrir` válido (#8) | Guarda la cobertura |
| Con ataque → Cubriendo | `Cubrir` válido (#8) | Borra el ataque y guarda la cobertura |
| Con ataque → Libre | Cierre del turno (`Resolver`, `TurnTimeout` de atacar) | Se entrega la lista y se vacía |
| cualquiera → el mismo | Comando rechazado | Nada cambia |

No hay ataques asignados fuera de `Atacar`. Si la partida termina por `Rendirse` o `CerrarPartida` durante `Atacar`, no se resuelven, y el estado Terminado los conserva (Librería, regla 11).

### Interactions with Other Systems

| Sistema | Qué entra | Qué sale | Dueño de la interfaz |
|---|---|---|---|
| #1 Librería | Comandos, orden de validación, forma canónica | Códigos, eventos y consultas; invariantes de la lista | Librería (contrato); este GDD (reglas) |
| #2 Catálogo | `alcanceMin`, `alcanceMax`, `danoArea`, IDs 2 (escudero) y 5 (demoledor) | — | Catálogo |
| #3 Máquina | Abre y cierra `Atacar` | `ObjetivosValidos` para el salto | Máquina (fase); este GDD (consulta) |
| #4 Tablero | `dentro`, `ocupante`, `hayTramo`, dimensiones | — | Tablero |
| #5 Castillo | Dueño y vida del tramo; bono del arquero vía `EffectiveStats` | — | Castillo |
| #8 Cobertura | Cobertura actual del escudero | Exclusión ataque/cobertura | Compartida |
| #9 Resolución | — | Lista `(pieza, objetivo, modo)` | Este GDD; confirmado por #9 |
| #10 Reclutamiento | Un recluta puede atacar ese turno | — | #10 |
| #13 Entrada, #15 HUD, #16 Vista previa, #18 Render, #23 Accesibilidad, #24 Tutorial | — | Consultas, códigos, eventos | Cliente, solo lectura |
| #19 Métricas | — | Eventos de asignación | Métricas |
| #25 Rival IA | — | `ObjetivosValidos` | IA, solo lectura |

## Formulas

### F1. `en_alcance`

The `en_alcance` formula is defined as:

`d2 = dx² + dy²`
`en_alcance = d2 ≥ 1 y d2 ≤ alcance_max² + 1 y (alcance_min ≤ 1 o d2 > (alcance_min − 1)² + 1)`

**Variables:**

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| dx | int | −23 a 23 | calculated | Columna del objetivo menos la del atacante |
| dy | int | −14 a 14 | calculated | Fila del objetivo menos la del atacante |
| alcance_min | int | 1–1.000 | data file (Catálogo, vía `EffectiveStats`) | Alcance mínimo |
| alcance_max | int | 1–1.010 | data file (Catálogo, más el bono de muro, #5 F5) | Alcance máximo efectivo |
| 1 (el «+ 1») | int | 1 | constant | Redondeo del círculo: con alcance 1 incluye las diagonales |

**Output Range:** verdadero o falso. `d2` va de 0 a 725 en el tablero; `1.010² + 1` = 1.020.101, sin desborde.
**Example:** catapulta 2–6 contra una casilla a (1, 6): d2 = 37 ≤ 37 y 37 > 2 → en alcance. A (1, 1): d2 = 2, no es > 2 → zona ciega. A (2, 6): 40 > 37 → fuera. Soldado a (1, 1): 2 ≤ 2 → en alcance.

**Por qué así.** El «+ 1» viene de los prototipos: da un círculo reconocible y hace que alcance 1 sea el contorno de 8. La cota interior es **el círculo de alcance `min − 1`**, de modo que la zona ciega tiene la misma forma que el alcance.

**Bordes revisados.** `alcance_min = alcance_max = m` nunca da un conjunto vacío en un tablero sin bordes: la casilla a `(m, 0)` cumple siempre. `alcance_max ≥ 27` cubre todo el tablero (27² + 1 = 730 ≥ 725; con 26, 677 no alcanza): más allá, el valor es inerte. Cerca de un borde el conjunto puede quedar vacío con un mínimo grande: `ObjetivosValidos` vacía, sin error. La relación es simétrica entre dos piezas con las mismas cifras.

### F2. `casillas_en_alcance` (oráculo de tests de balance)

The `casillas_en_alcance` formula is defined as:

`casillas_en_alcance = cantidad de casillas del tablero que cumplen en_alcance`

**Variables:**

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| casilla | (int, int) | dentro del tablero | calculated | Posición del atacante |
| alcance_min, alcance_max | int | ver F1 | data file | Alcance efectivo |

**Output Range:** 0 a 359 (360 casillas menos la propia).
**Example:** valores vigentes, en el centro `(12, 7)` y en la esquina `(0, 0)`:

| Alcance | Quién | Centro | Esquina |
|---|---|---|---|
| 1–1 | Soldado, jinete, Rey | 8 | 3 |
| 1–2 | Escudero | 20 | 7 |
| 1–4 | Arquero | 56 | 18 |
| 1–6 | Arquero sobre el muro | 120 | 36 |
| 2–6 | Catapulta | 112 | 33 |

Subir un punto de alcance al arquero en el muro agranda su zona de 120 a 160 (1–7): el alcance crece con el **cuadrado**, y por eso es la perilla de balance más sensible de este GDD. El conteo en esquina del arquero en el muro es solo geométrico.

### F3. `asignaciones_max`

The `asignaciones_max` formula is defined as:

`asignaciones_max = piezas_vivas`

**Variables:**

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| piezas_vivas | int | 1–61 | calculated (estado; tope `tope_vivas_por_jugador` del Catálogo) | Piezas vivas del jugador del turno |

**Output Range:** 1 a 61.
**Example:** con el ejército inicial (13 piezas), hasta 13 ataques. Con 61, hasta 61, como el fixture de AC-24 de la Librería.

**Lazos de retroalimentación:** más piezas dan más ataques, que matan más piezas (lazo positivo buscado). Lo amortiguan el escudo, los topes de piezas vivas y la remontada del Modificador (#11), no este sistema.

**Aporte a la duración (pilar 4):** 0 turnos propios. El costo es tiempo dentro de la etapa de atacar, ya acotada por el Timer (30 s, 20 en el tramo final). La regla 14 puede abrir una fase sin ataques útiles; el techo es la etapa completa.

## Edge Cases

### Objetivos y geometría

- **If la casilla está fuera del tablero** (cualquier int32): `ObjetivoFueraDelTablero`, antes de toda cuenta, así F1 nunca recibe valores grandes.
- **If la pieza apunta a su propia casilla:** `FueraDeAlcance`, salvo el demoledor, que explota.
- **If el demoledor apunta a un enemigo vecino:** `ObjetivoInvalido`. La explosión ya lo alcanza; hay una sola forma de escribirla.
- **If un arquero enemigo está sobre su tramo en pie:** una sola casilla objetivo; el golpe va a los dos (Castillo, regla 8).
- **If una pieza propia apunta al tramo propio donde hay un arquero propio:** directo, `ObjetivoPropio`; área, se acepta.
- **If una pieza enemiga está en un hueco de tu muro:** objetivo directo válido (regla 8).
- **If el objetivo directo es un hueco vacío:** `SinObjetivo`. La catapulta sí puede.
- **If una pieza está en un borde o esquina:** solo cuentan las casillas del tablero; el área que cae fuera la descarta Resolución (#9).
- **If un enemigo está pegado a la catapulta:** zona ciega, `FueraDeAlcance`. Puede apuntar a una casilla a distancia 2 y alcanzarlo con el área: es legal.

### Asignación

- **If se reasigna el mismo objetivo:** se acepta y emite `AtaqueAsignado` con `objetivoAnterior` igual al nuevo. Para quitar, el cliente usa `QuitarAtaque` (#13).
- **If el reemplazo es inválido:** se rechaza y queda el ataque anterior (y la cobertura, si la había).
- **If `QuitarAtaque` llega a un escudero que cubre:** `SinAtaqueAsignado`. La cobertura se quita con `QuitarCobertura` (#8).
- **If varias piezas apuntan a la misma casilla:** se aceptan todas; sumar es de Resolución.
- **If un ataque propio (área o explosión) mata a otra pieza propia con ataque asignado:** esa pieza golpea igual (Librería, regla 15).
- **If el ataque puede matar al propio Rey** (área o explosión): se acepta. La vista previa lo muestra; no hay confirmación extra en el núcleo. Si muere solo él, gana el rival (Máquina, regla 13).
- **If llega `TurnTimeout` de atacar a mitad de un plan:** se resuelve lo asignado hasta ese momento; una flecha sin confirmar en el cliente no existe.
- **If se agota el reloj de partida:** los ataques asignados no se resuelven (Máquina, regla 14).
- **If sale Distracción:** se pasa a `Atacar` desde las posiciones actuales, con las mismas reglas.

### Salto de fase y carga de estados

- **If no hay enemigo al alcance de nadie y hay una catapulta o un demoledor propio:** la fase se abre igual (regla 14) y la cierra `Resolver` o el timer.
- **If la catapulta no tiene ninguna casilla del tablero en alcance** (solo con un mínimo enorme en el Catálogo): no cuenta como acción.
- **If se carga un estado con un ataque que `AsignarAtaque` rechazaría, o con un escudero del jugador del turno que ataca y cubre a la vez:** `EstadoInvalido` (Librería, regla 11).

### Estrategias degeneradas revisadas

- *Disparar al tramo propio para abrirse paso:* legal; cuesta muro permanente y acerca el corte de reclutamiento. No suma a `vidaQuitada`.
- *Explotar demoledores propios para bajar la cuenta de piezas y mejorar las probabilidades del Modificador:* legal; cuesta una pieza de 30 de ataque. A vigilar (Open Questions).
- *Alargar el turno con una fase vacía:* acotado por la etapa y por el reloj del propio jugador, que es quien lo paga. En Sin límite no hay tope.

## Dependencies

**Depende de:**

| Sistema | Tipo | Interfaz | ¿El otro lo nombra? |
|---|---|---|---|
| #1 Librería | Dura | Comandos, orden de validación, eventos, determinismo, forma canónica | Sí |
| #2 Catálogo | Dura | `alcanceMin`, `alcanceMax`, `danoArea`, IDs de tipo | Sí |
| #3 Máquina de turno | Dura | Fase `Atacar`, cierre, salto | Sí |
| #4 Tablero | Dura | `dentro`, `ocupante`, `hayTramo`, dimensiones | Sí |
| #5 Castillo y muros | Dura | Tramo enemigo en pie, dueño del tramo, bono del arquero (F5) | Sí |
| #8 Cobertura | Blanda | Exclusión ataque/cobertura del escudero | Sí |
| #11 Modificador | Blanda | Grito: +2 de ataque salvo en el modo Explosión | Sí |
| #10 Reclutamiento | Blanda | Un recluta puede atacar ese turno | Sí |

**Dependen de este sistema:**

| Sistema | Tipo | Qué toma |
|---|---|---|
| #3 Máquina | Dura | `ObjetivosValidos` para el salto (ciclo ya cortado: la Máquina no lo conoce por nombre) |
| #8 Cobertura | Dura | La exclusión y la acción única por pieza |
| #9 Resolución | Dura | Lista `(pieza, objetivo, modo)` |
| #13 Entrada, #16 Vista previa | Dura | Consultas, códigos, flechas |
| #15 HUD, #18 Render, #19 Métricas, #23 Accesibilidad | Blanda | Eventos y alcance a la vista |
| #14 Localización | Blanda | Textos de los códigos visibles; `FueraDeAlcance` es uno solo para mover y atacar |
| #24 Tutorial | Dura | El paso de atacar |
| #25 Rival IA | Dura | `ObjetivosValidos` |

**Supuestos provisionales:** ninguno sobre #9: el formato de la lista y la zona de explosión los confirmó Resolución al cerrar la tanda 3 (2026-11-25).

## Tuning Knobs

| Perilla | Valor | Rango seguro | Demasiado alto | Demasiado bajo | Interactúa con |
|---|---|---|---|---|---|
| `alcanceMax` por tipo (**dueño: Catálogo, #2**) | 1 / 4 / 2 / 6 / 1 / 1 / 1 | 1–6, sin medir; inerte desde 27 | La zona crece con el cuadrado: más objetivos y tablero ilegible | Menos combinaciones posibles (pilar 2) | `alcanceMin`; el del demoledor **es inerte** (regla 7) |
| `alcanceMin` por tipo (**dueño: Catálogo, #2**) | 1; catapulta 2 | 1–3, sin medir | Zona ciega tan grande que la catapulta no sirve cerca | Con 1 la catapulta no tiene contrajuego cuerpo a cuerpo | `alcanceMax` |
| `bono_alcance_muro` (**dueño: Castillo, #5**) | 2 | 1–3 (lo fija #5) | El muro domina el centro | Subir no paga | `alcanceMax` del arquero |
| «+ 1» de F1 | 1 | Constante del contrato; no se ajusta | — | Con 0, alcance 1 pierde las diagonales | Cambiarla sube la versión de reglas |
| Ataques por pieza y turno | 1 | Constante del contrato; no se ajusta | Rompe el presupuesto de turno (pilar 4) | — | Cambiarla sube la versión de reglas |

Este GDD no agrega perillas propias: todo número ajustable es de #2 o #5.

## Visual/Audio Requirements

Principios del art bible: *El plan se lee antes que el detalle*, *El combo se ve como un solo golpe* y *El ritmo del turno manda sobre la animación*. Secciones 3 (*Gramática de formas*) y 4 (*Flechas de ataque*, *Daltonismo*).

| Evento | Visual | Audio (dirección; lo define #22) |
|---|---|---|
| Pieza elegida | `CasillasEnAlcance` con corchetes bermellón al 70%; los objetivos directos válidos, más marcados | Toque corto |
| `AtaqueAsignado` directo | Arco `#E55A40` al 100% con borde Tinta de 2 px; se funde con los que van a la misma casilla (×1 / ×1,5 / ×2) | Trazo corto que sube con la cantidad de atacantes |
| `AtaqueAsignado` de área | Arco más **anillo doble** en el punto elegido | Tensión de cuerda |
| `AtaqueAsignado` de explosión | Marca sobre el demoledor y sus 8 vecinas, sin flecha. La define Render (#18, regla 12); falta sumarla al art bible | Mecha |
| Reemplazo | La flecha vieja desaparece en el mismo cuadro en que entra la nueva | Solo el sonido de asignar |
| `AtaqueQuitado` | La flecha se borra | Toque seco |
| Lo propio dentro de área o explosión | Señal de **forma** (no solo color) sobre cada pieza y tramo propio: contorno discontinuo (Render, #18, regla 12; Vista previa, #16). Falta sumarla al art bible | Aviso discreto, una vez por asignación |
| Rechazo visible | Sacudida breve de la pieza y texto corto | Error suave |

Restricciones: solo flechas del jugador de turno y sin color de bando; transiciones ≤ 0,25 s; todo se distingue en grises. El art bible dice que el anillo exterior de la catapulta hace "la mitad" del daño, y hoy es 6 de 10. La columna de audio es una propuesta: no pasó por el lente de `audio-director`.

📌 **Asset Spec** — Visual/Audio requirements are defined. After the art bible is approved, run `/asset-spec system:alcance-y-asignacion-de-ataques` to produce per-asset visual descriptions, dimensions, and generation prompts from this section.

## Game Feel

- **Respuesta:** `AsignarAtaque` y `QuitarAtaque` entran en `comando_presupuesto_ms` (1 ms p95); la flecha aparece en el mismo cuadro en que se acepta el comando.
- **Vista previa encadenada:** después de cada asignación se recalcula dentro de `vista_previa_presupuesto_ms` (5 ms p95).
- **Corregir no cuesta:** reemplazar es un solo gesto. No hay confirmación para asignar ni para quitar.
- **Peso del combo (pilar 2):** cada atacante que se suma al mismo blanco se nota en la punta fundida y en el sonido antes de resolver.
- **Sin animación que bloquee:** se puede trazar la flecha siguiente sin esperar a que termine la anterior.
- **Verificación:** capturas retenidas y playtest con la pregunta "¿algo propio recibió daño sin que lo esperaras?".

## UI Requirements

- **Alcance al elegir una pieza**, propia o rival, en cualquier fase, sin depender de pasar el mouse.
- **Flechas** de todas las asignaciones del turno, siempre visibles en `Atacar`.
- **Pendientes:** las piezas propias con objetivo y sin ataque, marcadas de forma tenue. Para catapulta y demoledor, que siempre tienen objetivo, solo si algún objetivo tocaría algo enemigo.
- **Reemplazo del escudero:** al asignarle un ataque, la cobertura desaparece a la vista.
- **Demoledor:** la explosión se asigna tocando el demoledor otra vez; el gesto final lo fija Entrada (#13).
- **Fuego amigo:** la vista previa (#16) marca todo lo propio que recibe daño y, de forma destacada, si el Rey propio muere. Es lo que separa una jugada osada de una sorpresa (pilar 1).
- **Rechazos:** texto por código desde Localización (#14); `ObjetivoFueraDelTablero` y `ObjetivoInvalido` no deben salir por la interfaz.
- **Accesibilidad (#23):** objetivo, zona ciega y aviso de lo propio, distinguibles por forma.

📌 **UX Flag — Alcance y asignación de ataques**: This system has UI requirements. In Phase 4 (Pre-Production), run `/ux-design` to create a UX spec for each screen or HUD element this system contributes to **before** writing epics. Stories that reference UI should cite `design/ux/[screen].md`, not the GDD directly.

## Cross-References

| Qué | Documento | Dónde | Relación |
|---|---|---|---|
| `AsignarAtaque`, `QuitarAtaque`, fila 14, consultas, eventos | `design/gdd/libreria-de-reglas.md` | Reglas 3, 4, 8, 10, 11, 15 | Completa |
| Fase `Atacar`, salto, cierre, reloj | `design/gdd/maquina-de-turno.md` | Reglas 9 a 14 | Respeta |
| `alcanceMin`, `alcanceMax`, `danoArea`, IDs | `design/gdd/catalogo-de-piezas.md` | Core Rules, AC-33 | Lee; da el sentido geométrico |
| Grilla, `ocupante`, contorno | `design/gdd/tablero-y-despliegue-inicial.md` | Reglas 1 a 3 y 11 | Lee |
| Sin línea de visión, tramo como objetivo, bono | `design/gdd/castillo-y-muros.md` | Reglas 3, 4, 7, 8; F5; AC-4, AC-5 | Respeta |
| `FueraDeAlcance` | `design/gdd/movimiento.md` (en curso) | Códigos de rechazo | Mismo código, otro comando |
| Gestos de atacar | `design/gdd/entrada-y-seleccion.md` (en curso) | Decisiones tomadas | Respeta; agrega el demoledor |
| Alcance, ataques, catapulta, demoledor | `design/gdd/game-concept.md` | Reglas vigentes | Detalla |
| Flechas y marcas | `design/art/art-bible.md` | Secciones 3 y 4 | Aplica |

## Acceptance Criteria

Salvo indicación: fase `Atacar` del jugador 0 (azul), tablero con solo las piezas nombradas y los Reyes lejos, cifras vigentes, casillas `(columna, fila)`. Tipos: **Logic** (prueba en EditMode), **Integration** y **Visual** (captura retenida). Los que dependían de #8 o #9 ya no son PROVISIONAL (cierres de las tandas 2 y 3).

**Reglas**

- **AC-1 (regla 1)** — **GIVEN** un soldado azul en `(10, 7)` que ya movió y un rojo en `(11, 7)`, **WHEN** azul asigna el ataque, **THEN** se acepta. **Y** con el Rey azul en `(10, 6)` apuntando al mismo rojo, se acepta. *(Logic)*
- **AC-2 (regla 2)** — **GIVEN** un arquero rojo sobre su tramo en pie `(19, 7)` y un soldado azul en `(18, 7)`, **WHEN** se piden sus `ObjetivosValidos`, **THEN** `(19, 7)` aparece una sola vez. *(Logic)*
- **AC-3 (regla 3, muro)** — **GIVEN** un arquero azul sobre su tramo `(4, 7)` en pie, **WHEN** apunta a un rojo en `(10, 7)`, **THEN** se acepta; **Y** con ese tramo roto, `FueraDeAlcance`; **Y** con Grito de guerra y el arquero en `(5, 7)`, el rojo en `(10, 7)` sigue `FueraDeAlcance`. *(Integration #5)*
- **AC-4 (regla 4)** — **GIVEN** el turno rojo, un arquero rojo en `(6, 7)`, el tramo azul `(4, 7)` en pie y el Rey azul en `(2, 7)`, **WHEN** se piden `ObjetivosValidos`, **THEN** incluye `(2, 7)`. *(Integration #5; es el AC-4 de Castillo)*
- **AC-5 (regla 5, directo)** — **GIVEN** un soldado azul en `(10, 7)`, **WHEN** apunta a un soldado rojo en `(11, 8)`, **THEN** se acepta; a un azul en `(9, 7)`, `ObjetivoPropio`; a `(10, 6)` vacía, `SinObjetivo`; a un rojo en `(12, 7)`, `FueraDeAlcance`. *(Logic)*
- **AC-6 (regla 6, área)** — **GIVEN** una catapulta azul en `(8, 7)`, **WHEN** apunta a `(10, 7)` vacía, con pieza azul y con pieza roja, y a su tramo `(4, 7)` en pie y roto, **THEN** los cinco casos se aceptan. *(Logic)*
- **AC-7 (regla 5, área por dato)** — **GIVEN** un Catálogo con `danoArea` 8 en el arquero, **WHEN** apunta a una casilla vacía en alcance, **THEN** se acepta; con `danoArea` 0, `SinObjetivo`. *(Logic)*
- **AC-8 (regla 7, explosión)** — **GIVEN** un demoledor azul en `(10, 7)` sin nadie cerca, **WHEN** apunta a `(10, 7)`, **THEN** se acepta con modo explosión y `ObjetivosValidos` es exactamente `[(10, 7)]`; a un rojo en `(11, 7)`, `ObjetivoInvalido`. **Y** con `alcanceMax` 5 o `danoArea` 8 en su fila, el resultado es igual. *(Logic)*
- **AC-9 (regla 8)** — **GIVEN** el tramo azul `(4, 7)` roto y un soldado azul en `(5, 7)`, **WHEN** apunta a `(4, 7)` vacía, **THEN** `SinObjetivo`; con un soldado rojo en el hueco, se acepta. **Y** con el tramo en pie, `ObjetivoPropio`. *(Integration #5)*
- **AC-10 (regla 9, reemplazo)** — **GIVEN** un soldado con ataque a A, **WHEN** asigna B (válido), **THEN** queda un solo ataque, a B, y el evento trae `objetivoAnterior` A. **Y WHEN** después apunta fuera de alcance, **THEN** se rechaza, sigue B y la huella no cambia. *(Logic)*
- **AC-11 (regla 9, escudero)** — **GIVEN** un escudero que cubre a un aliado, **WHEN** asigna un ataque válido, **THEN** la cobertura ya no está y el ataque sí; con un ataque inválido, la cobertura sigue. **Y WHEN** un escudero con ataque cubre, **THEN** el ataque ya no está. *(Integration #8)*
- **AC-12 (regla 10)** — **GIVEN** un soldado con ataque, **WHEN** llega `QuitarAtaque`, **THEN** queda sin ataque y emite `AtaqueQuitado`; otra vez, `SinAtaqueAsignado`. **Y** con un escudero que cubre y no ataca, `SinAtaqueAsignado` y su cobertura sigue. *(Logic)*
- **AC-13 (regla 11)** — **GIVEN** 5 ataques asignados a piezas que no cubren, **WHEN** se compara el estado con el de antes, **THEN** solo difieren la lista de ataques y `contadorComandos`. *(Logic)*
- **AC-14 (regla 11, vigencia)** — **GIVEN** un fixture con una secuencia fija de 200 comandos de `Atacar` sin `Resolver`, **WHEN** se revisa cada ataque guardado tras cada comando, **THEN** su objetivo está en `ObjetivosValidos` de su pieza. *(Logic)*
- **AC-15 (regla 12, orden)** — **GIVEN** un demoledor que apunta a `(24, 0)`, **THEN** `ObjetivoFueraDelTablero`; un soldado que apunta a un aliado fuera de alcance, `FueraDeAlcance`; **WHEN** el objetivo lleva el int32 mínimo o máximo, **THEN** `ObjetivoFueraDelTablero` y nunca `ErrorInterno`. *(Logic)*
- **AC-16 (regla 13, coincidencia)** — **GIVEN** un fixture con los 7 tipos, muros y huecos, **WHEN** se prueba `AsignarAtaque` de cada pieza del jugador del turno en las 360 casillas, cada vez sobre una copia, **THEN** se acepta si y solo si la casilla está en `ObjetivosValidos`. *(Logic)*
- **AC-17 (regla 13, consultas)** — **GIVEN** la fase `Mover`, **WHEN** se consultan una pieza propia y una rival, **THEN** devuelven listas ordenadas por fila y columna, sin repetir; un ID inexistente, vacío; `CasillasEnAlcance` de un demoledor, vacía; la huella no cambia. *(Logic)*
- **AC-18 (regla 14)** — **GIVEN** azul con solo el Rey y una catapulta y ningún enemigo al alcance, **WHEN** se abre `Atacar`, **THEN** no hay `FaseSaltada`; igual con un demoledor; con un soldado en su lugar, `FaseSaltada(Atacar)`. *(Integration #3)*
- **AC-19 (regla 16)** — **GIVEN** ataques asignados en el orden de piezas 9, 2 y 5, **WHEN** llega `Resolver`, **THEN** Resolución recibe 2, 5 y 9, y en el turno siguiente no queda ninguno. *(Integration #9)*
- **AC-20 (regla 15)** — **GIVEN** una pieza sin ataque, **WHEN** se acepta su asignación, **THEN** el evento trae pieza, objetivo, modo y `objetivoAnterior` vacío; al desplazar una cobertura, el evento de #8 sale antes. *(Logic / Integration #8)*
- **AC-21 (regla 17)** — **GIVEN** el turno azul, **WHEN** rojo envía `AsignarAtaque` con una pieza suya, **THEN** `NoEsTuTurno`; azul en `Mover`, `FaseIncorrecta`. *(Logic)*

**Fórmulas**

- **AC-22 (F1, bordes)** — **GIVEN** alcance 2–6, **WHEN** se evalúan `(dx, dy)` = (0,0), (1,0), (1,1), (2,0), (6,1), (4,4), (6,2), (5,4), **THEN** falso, falso, falso, verdadero, verdadero, verdadero, falso, falso. **Y** con 1–1: (0,0) falso, (1,1) verdadero, (2,0) falso. **Y** con 3–3: (2,1) falso, (2,2) verdadero, (3,1) verdadero, (3,2) falso. *(Logic)*
- **AC-23 (F1, extremos)** — **GIVEN** mínimo 1, **WHEN** se mide de `(0, 0)` a `(23, 14)` con `alcance_max` 26, 27 y 1.010, **THEN** falso, verdadero y verdadero, sin desborde. *(Logic)*
- **AC-24 (F2)** — **GIVEN** un tablero sin bordes ajenos, **WHEN** se cuentan las casillas en `(12, 7)` y `(0, 0)` para 1–1, 1–2, 1–4, 1–6 y 2–6, **THEN** 8/3, 20/7, 56/18, 120/36 y 112/33. *(Logic)*
- **AC-25 (F3)** — **GIVEN** un fixture con 61 piezas azules, todas con objetivo válido, **WHEN** se asigna un ataque a cada una, **THEN** hay 61 ataques y reasignar cualquiera mantiene 61. *(Logic)*

**Casos borde, rendimiento y presentación**

- **AC-26 (estado inválido)** — **GIVEN** estados con un ataque fuera de alcance, con un escudero del turno que ataca y cubre, y con ataques en la fase `Mover`, **WHEN** se cargan, **THEN** los tres dan `EstadoInvalido`. *(Logic)*
- **AC-27 (timeout)** — **GIVEN** 2 ataques asignados, **WHEN** llega `TurnTimeout` de atacar, **THEN** se entregan esos 2; con `CerrarPartida(RelojAgotado)`, ninguno. *(Integration #3 y #9)*
- **AC-28 (Rey propio)** — **GIVEN** una catapulta azul con su Rey a 3 casillas en línea recta, **WHEN** apunta a la casilla del Rey, **THEN** se acepta y la vista previa incluye el daño al Rey propio. *(Integration #9)*
- **AC-29 (rendimiento)** — **GIVEN** el fixture de AC-24 de la Librería, **WHEN** se aplica `AsignarAtaque` 100 veces con el método de AC-25 de la Librería, **THEN** el p95 es ≤ 1 ms. *(Integration)*
- **AC-30 (visual)** — **GIVEN** 1, 2 y 3 atacantes a una casilla, un disparo de área con una pieza propia dentro y una explosión, **WHEN** se captura en color, protan, deutan, tritan y grises, **THEN** se distinguen alcance, flecha, anillo, explosión y aviso de lo propio en las cinco versiones. *(Visual)*

## Open Questions

| Pregunta | Dueño | Cuándo |
|---|---|---|
| ¿Se juega algún fuego amigo por sorpresa, sobre todo contra el Rey propio? (CD-GDD-ALIGN, aceptado) Lo recoge la Vista previa (#16, reglas 12 y 14; Open Questions 2). | Vista previa (#16), playtest | En su GDD y en el MVP |
| Zona y daño de la explosión, ¿alcanza a tramos propios como dice el concepto?, y muerte del demoledor | Resolución (#9) | En su GDD |
| ¿Resulta rentable el fuego amigo deliberado (explotar demoledores para mejorar el Modificador, disparar al muro propio)? | Modificador (#11), playtest | MVP |
| ¿Cuántos turnos abren `Atacar` sin ataques útiles por la regla 14, y cuánto tiempo cuestan? La medida existe (Métricas, #19, regla 14). | Métricas (#19), playtest | MVP |
| ¿Alcanzan 30 s (20 en el tramo final) para asignar 13 ataques o más en pantalla táctil? | Entrada (#13), playtest en teléfono | Antes de cerrar las reglas |
| ¿Mostrar el alcance desde un destino hipotético mientras se mueve (pilar 1)? Lo recoge la Vista previa (#16, Open Questions 4). | Entrada (#13), Vista previa (#16) | En sus GDD |
| **Capa de amenaza del turno siguiente del rival** (pedido del Tablero, #4, Player Fantasy y Open Questions, 2026-11-27): lo que el rival puede golpear en su próximo turno, movimiento en cruz + alcance en círculo, calculado **antes de su tirada**, con la franja base (sin bono, que contiene la de Distracción) y la franja del Grito aparte. El cálculo es de este GDD; la muestran Ficha y HUD (#15) y Vista previa (#16); el gesto que la activa es de Entrada (#13) | Alcance (#7), con #13, #15 y #16 | MVP |

**Factibilidad:** regla pura de C#, sin superficie de motor. La parte leída de `docs/engine-reference/unity/modules/core.md` (C# 9, .NET Standard 2.1) no limita este diseño; no se revisó `breaking-changes.md`. No hay ADR todavía.

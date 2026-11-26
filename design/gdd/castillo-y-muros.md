# Castillo y muros

> **Status**: Designed (pendiente de `/design-review` en una sesión nueva)
> **Author**: Martin Arin + Claude
> **Last Updated**: 2026-11-25
> **Last Verified**: 2026-11-25
> **Implements Pillar**: Pilar 1 (daño fijo y visible), Pilar 2 (combinar golpes contra el muro), Pilar 4 (como restricción: el muro no puede alargar la partida). **Tensión con el Pilar 3** (ver Open Questions).
> **Creative Director Review (CD-GDD-ALIGN)**: REVISED 2026-11-25 — CONCERNS sobre los pilares 3 (corte de reclutamiento), 2 (demoledor) y 4 (sin aporte declarado a la duración). Los dos primeros quedan aceptados y documentados en Open Questions; el tercero se corrigió con el encabezado y una pregunta nueva. La revisión se hizo sin releer el GDD completo del disco; la relectura posterior (mismo día) corrigió 9 errores. **El veredicto queda pendiente de `/design-review`.**

## Summary

Castillo y muros define el muro de 16 tramos de cada castillo: cuánta vida tiene cada tramo, cómo se rompe, a quién bloquea y qué hace perder. El jugador lo vive como el objetivo que se abre combinando golpes, o como la muralla donde sube un arquero (+2 de escudo y de alcance); con 2 tramos rotos, ese castillo deja de reclutar. Existe para darle al duelo un objetivo intermedio entre el ejército y el Rey sin alargar la partida (pilares 2 y 4).

> **Quick reference** — Layer: `Feature` · Priority: `MVP` · Key deps: `Librería de reglas (#1), Catálogo de piezas (#2), Tablero y despliegue inicial (#4), Máquina de turno (#3, blanda)`

## Overview

Castillo y muros define qué es el muro de cada castillo y qué pasa con él durante la partida. El Tablero lo coloca al empezar: un perímetro de 16 tramos alrededor de un bloque de 5 × 5, cada tramo con 30 de vida y sin escudo, ocupando una casilla. A partir de ahí este sistema es el dueño de todo lo que le ocurre. Los tramos son objetivos: cualquier pieza enemiga con un tramo en alcance puede atacarlo, y el área de la catapulta y la explosión del demoledor también lo golpean, sea propio o rival. Un tramo que llega a 0 de vida se rompe y deja un hueco por el que se pasa. Con 2 tramos rotos, ese castillo deja de reclutar. Ese es el único efecto de estado del muro, y Reclutamiento y Modificador lo leen. Además, un arquero puede subir a un tramo libre de su propio muro (uno por tramo). Arriba gana +2 de escudo y +2 de alcance (1–6) y ataca desde ahí, pero comparte el daño con el tramo: el golpe a esa casilla lo reciben el arquero (menos su escudo) y el muro completo. El Rey no se bloquea con sus propios muros ni puede subir a ellos.

El jugador lo vive de dos maneras. Como **defensor** decide si sube un arquero al muro (más alcance y escudo a cambio de exponer el tramo) y cuánto vale proteger la franja de muro que sostiene su reclutamiento. Como **atacante** decide cuándo gastar un golpe combinado en el muro en vez de en piezas o en el Rey. Sin este sistema el castillo sería decorado: no habría objetivo intermedio entre el ejército y el Rey, ni razón para combinar golpes contra algo que no se mueve (pilar 2), ni un costo concreto por perder la defensa (el reclutamiento).

*Fuera de este sistema:* la colocación inicial de los tramos y la geometría del bloque (Tablero, #4); cómo se calculan los recorridos (Movimiento, #6), con las reglas de bloqueo y huecos de este GDD; el cálculo del daño sumado, el escudo y la muerte de piezas (Resolución, #9); cuántas piezas se pueden reclutar y desde dónde (Reclutamiento, #10); las probabilidades del dado (Modificador, #11).

## Player Fantasy

**El general que abre la brecha y sostiene la muralla.** El castillo no es el fondo del tablero: es la mitad que tenés que sostener y la mitad del rival que podés abrir. Cuando atacás, sentís que el plan sale: varias piezas que solas no harían nada juntan el golpe que tumba un tramo y abre el camino. Cuando defendés, sentís que la muralla es tuya: subís un arquero, ves más lejos y sabés exactamente cuánto te cuesta tenerlo ahí.

**Momento ancla:** la fase de asignar. Trazás las flechas de cuatro piezas sobre un mismo tramo, la vista previa lo muestra en 0 y, al resolver, ves el hueco abierto y el contador del rival pasar a "1 de 2".

**Pilares que sirve:**

- Pilar 2, *"Ninguna pieza gana sola: el poder sale de juntar varias sobre un mismo objetivo."*
- Pilar 1, *"Al mover armás la estrategia; al atacar ejecutás el plan y ves sus frutos."*

**Qué le exige al resto del diseño:**

- Vista previa (#16) con el daño y la rotura del tramo antes de resolver.
- Estado del muro legible sin texto: tramos rotos de cada castillo (0, 1 o 2) siempre visibles para ambos jugadores.
- Costo del arquero en el muro predecible: el mismo daño fijo, sin azar (anti-pilar *"NOT información oculta al resolver"*).
- El muro es un medio (abrir hueco, cortar reclutamiento), no un fin: el Rey sigue siendo lo que termina la partida. El concepto descartó "el castillo con muros" como gancho porque es soporte, no el centro.

**Lo que este encuadre no promete** (queda en Open Questions):

- Que valga la pena atacar el muro antes que al ejército o al Rey: depende del balance y se mide en el MVP.
- Que cortar el reclutamiento del rival no apague su remontada (tensión con el pilar 3, *"El que va perdiendo tiene con qué remontar"*). El Modificador ya baja a 0% las caras de reclutar con 2 tramos rotos.

## Detailed Design

### Core Rules

1. **El tramo.** Cada castillo tiene 16 tramos, uno por casilla de muro (radio = `RADIO_MURO`, regla 5 del Tablero), identificados por su casilla. Cada uno guarda su `vida`, un entero de 0 a `vida_tramo`, y su escudo es 0. Un tramo no es una pieza: no se mueve, no ataca, no tiene ID de pieza y no cuenta como pieza viva (ni para el tope del Catálogo ni para las probabilidades del Modificador). Sí cuenta en `vidaQuitada`.
2. **Estados.** *En pie* (`vida > 0`) y *Roto* (`vida = 0`). El estado se deriva de la vida, no se guarda aparte. La presentación distingue además "dañado" (`vida × 2 < vida_tramo`, regla del art bible), que no tiene efecto de reglas.
3. **Bloqueo.** Un tramo en pie ocupa su casilla y la bloquea para el movimiento de todas las piezas, jinete incluido. Tiene dos excepciones: el Rey del dueño del castillo lo atraviesa sin poder terminar sobre él, y un arquero del dueño puede pasar por él o terminar sobre él si no hay otro arquero (regla 6; Movimiento, #6, regla 5). Los tramos del castillo rival bloquean también al Rey. Un tramo **no bloquea ataques**: el alcance es circular y no hay línea de visión. Este GDD define quién bloquea; Movimiento (#6) lo aplica al armar recorridos.
4. **Daño al tramo.** Un tramo en pie es objetivo válido de `AsignarAtaque` de una pieza **enemiga** que lo tenga en alcance (#7). El daño de toda la resolución sobre su casilla se suma y se aplica como `vida = max(0, vida − bruto)`, sin restar escudo. El área de la catapulta y la explosión del demoledor lo alcanzan, sea propio o rival (fuego amigo). **No se puede asignar un ataque directo a un tramo propio**, ni a un tramo roto (la casilla es un hueco). Vale solo para el modo directo: la catapulta puede apuntar a cualquiera de esas casillas (Alcance, #7, regla 6).
5. **Rotura y hueco.** Cuando la resolución deja un tramo en 0, queda **Roto** hasta el final de la partida (decisión 2026-11-25: no se repara). Una casilla de tramo roto se comporta como una casilla libre común: no bloquea el movimiento de nadie, de ningún bando, y las piezas pueden pararse en ella. Sigue contando como casilla de muro para la geometría (Tablero, Edge Cases).
6. **Arquero sobre el muro.**
   - *Subir:* `Mover(arquero, destino)` con destino un tramo propio, en pie y sin arquero, llegando desde una casilla del anillo propio, desde otro tramo propio vecino o desde un hueco vecino del mismo muro. Cuesta un paso, como cualquier otro, y cuenta en el límite de piezas movidas. La Librería no necesita un comando nuevo.
   - *Caminar y bajar:* `Mover` a otro tramo propio libre vecino (un paso) o a una casilla en cruz libre del anillo o a un hueco vecino del mismo muro (una casilla libre común). Nunca directo de un tramo al interior (3 × 3), ni del interior a un tramo: el arquero no atraviesa el muro (decisión 2026-11-25). Por un hueco ya abierto llega al interior como cualquier pieza. Sin un hueco, el interior solo lo ocupa el Rey.
   - Un arquero por tramo. Ninguna otra pieza pisa un tramo.
   - *Ejemplo:* el arquero azul de `(5, 5)` sube al tramo `(4, 5)` con un paso.
7. **Bono de posición.** `enMuro(pieza)` es verdadero si la pieza es un arquero y su casilla tiene un tramo **propio con vida mayor que 0**. No se guarda: es una consulta pura. Mientras sea verdadero, `EffectiveStats` suma **+2 de escudo y +2 de alcance máximo** (1–4 pasa a 1–6); el ataque no cambia. Es igual para los dos jugadores. El bono se evalúa con el estado **al empezar la resolución**: el daño de esa resolución usa el escudo de entonces, aunque el tramo se rompa en ella. *PROVISIONAL hasta Resolución (#9).*
8. **Daño compartido.** Todo daño que cae sobre la casilla de un tramo ocupado por un arquero va a **los dos receptores a la vez**: a la pieza (menos su escudo efectivo, con el bono) y al tramo (completo). El atacante no puede elegir uno solo. Si el arquero está cubierto por un escudero, el escudero absorbe la parte de la pieza (Cobertura, #8); la parte del tramo no se redirige, porque el tramo no es una pieza (decisión 2026-11-25).
9. **Tramo roto bajo un arquero.** Si la resolución rompe el tramo y el arquero sobrevive, queda en esa casilla, ahora hueco, como arquero común: pierde el bono. Si muere en la misma resolución, la casilla queda libre.
10. **Cortar el reclutamiento.** `tramosRotos(jugador)` cuenta los tramos con vida 0 del castillo de ese jugador. `puedeReclutar(jugador)` es verdadero si `tramosRotos < TRAMOS_ROTOS_CORTE` (2). Es permanente. Lo leen Reclutamiento (#10) y la Librería, que pone en 0 las caras de reclutar de la tabla efectiva del Modificador (#11). El corte rige desde la tabla del siguiente turno del dueño: los tramos se rompen al resolver, y reclutar es de la fase de mover.
11. **Rey.** El Rey del dueño empieza en el centro, atraviesa los tramos propios en pie sin terminar sobre ellos y puede terminar sobre un hueco propio o rival, como cualquier casilla libre.
12. **Fin de partida.** Romper muros nunca termina la partida: solo la muerte del Rey (Máquina de turno, #3).
13. **Información pública y orden.** La vida de cada tramo y los tramos rotos de cada castillo son visibles para los dos jugadores. Los tramos se recorren siempre por casilla (fila, columna).
14. **Constantes y config.** `TRAMOS_ROTOS_CORTE` (2), `BONO_ESCUDO_MURO` (2) y `BONO_ALCANCE_MURO` (2) son constantes nombradas del contrato, una sola definición versionada, como las del Tablero: cambiarlas sube la versión de reglas. Solo `vida_tramo` (hoy 30) viaja por la config de `CrearPartida`, validada por la Librería, y entra a la huella de la partida (decisión 2026-11-25; cierra la Open Question del Tablero sobre el esquema de config).

### States and Transitions

| Entidad | Estado | Cómo se deriva |
|---|---|---|
| Tramo | **En pie** | `vida > 0` |
| Tramo | **Roto** | `vida = 0`. Terminal |
| Castillo | **Activo** | `tramosRotos = 0` |
| Castillo | **Debilitado** | `tramosRotos = 1`: aviso en la interfaz, sin efecto de reglas |
| Castillo | **Sin reclutamiento** | `tramosRotos ≥ 2`. Terminal |
| Arquero | **Común** | No está sobre un tramo propio en pie |
| Arquero | **En el muro** | `enMuro = verdadero` |

| De → a | Disparador | Efecto |
|---|---|---|
| Tramo En pie → Roto | `Resolver` lo deja en 0 | La casilla pasa a ser libre. Se suma 1 a `tramosRotos`. El arquero encima, si queda vivo, pasa a Común |
| Tramo Roto → En pie | **No existe** | Sin reparación |
| Castillo Activo → Debilitado | 1.er tramo roto | Aviso en la interfaz |
| Castillo Debilitado → Sin reclutamiento | 2.º tramo roto | `puedeReclutar` pasa a falso; rige en la tabla del siguiente turno del dueño |
| Arquero Común → En el muro | `Mover` a un tramo propio en pie y libre | Suma el bono desde ese momento |
| Arquero En el muro → Común | `Mover` fuera del tramo, o el tramo se rompe | Pierde el bono |

### Interactions with Other Systems

| Sistema | Qué entra | Qué sale | Dueño de la interfaz |
|---|---|---|---|
| #1 Librería | Campo `vida_tramo` en la config (validado); estado con 16 tramos por castillo | Consultas `enMuro`, `tramosRotos`, `puedeReclutar`; el bono en `EffectiveStats` | Librería (contrato); este GDD (reglas) |
| #2 Catálogo | `escudo` y `alcanceMax` del arquero, ID del tipo arquero | — | Catálogo |
| #3 Máquina de turno | — | `vidaQuitada` suma la vida realmente perdida por tramos rivales | Máquina de turno |
| #4 Tablero | Casillas de muro, interior y anillo; tramos colocados | `vida_tramo` para colocar la vida inicial | Tablero (geometría); este GDD (vida) |
| #6 Movimiento | Mover hacia o desde un tramo | Quién bloquea (regla 3), huecos transitables | Este GDD; Movimiento lo aplica |
| #7 Alcance | — | Tramo enemigo en pie como objetivo válido; alcance efectivo del arquero | #7 |
| #8 Cobertura | — | Parte del tramo no redirigible | #8 |
| #9 Resolución | Daño por casilla | Daño a pieza y tramo a la vez; vida nueva, tramos rotos, muertes | #9 *(PROVISIONAL)* |
| #10 Reclutamiento | — | `puedeReclutar` (también define la cara *Agotada*, #10, F3) | Este GDD |
| #11 Modificador | — | `tramosRotos` / `puedeReclutar` para poner en 0 las caras de reclutar | #11 |
| #15, #16, #18, #25 | — | Vida de cada tramo, contador, `enMuro`; vista previa del daño y de ROTO | Cliente y IA, solo lectura |

**Suposiciones provisionales:** #9 todavía no tiene GDD. Las reglas 7 y 8 fijan el contrato que deberá respetar o reabrir.

## Formulas

El muro define reglas numéricas (vida del tramo, bonos del arquero, umbral de corte). Los valores de las piezas (ataque, escudo, vida, alcance) son del Catálogo (#2); el daño bruto de una resolución es de Resolución (#9). Las fórmulas F3 y F4 son **oráculos para tests de balance**: no entran al núcleo ni a la vista previa, que suma el daño real con F1.

### F1. Vida del tramo tras resolver

The `vida_tramo_tras_resolver` formula is defined as:

`vida_nueva = max(0, vida − bruto)`

**Variables:**

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `vida` | int | 0 – `vida_tramo` | calculated (estado) | Vida del tramo antes de resolver |
| `bruto` | int | 0 – 24.999.999 | calculated (Resolución, #9) | Suma de todo el daño sobre la casilla del tramo en esta resolución (regla 15 de la Librería) |
| `vida_tramo` | int | 1 – 1.000 | data file (config de `CrearPartida`) | Vida inicial y máxima del tramo. Hoy 30. Validada por la Librería; 0 o menos es `ConfigInvalida` |

**Output Range:** 0 a `vida_tramo`; nunca negativo. Un tramo roto (`vida = 0`) no cambia.
**Example:** vida 30 y bruto 32 → 0 (roto). Vida 30 y bruto 8 → 22. Vida 22 y bruto 0 → 22.

### F2. Vida realmente quitada al tramo

The `vida_quitada_tramo` formula is defined as:

`quitada = vida − vida_nueva = min(vida, bruto)`

**Variables:** `vida`, `bruto` y `vida_nueva` como en F1.

**Output Range:** 0 a `vida_tramo`. Se suma a `vidaQuitada` del atacante solo si el tramo es rival: el fuego amigo no cuenta (Máquina de turno, regla 15).
**Example:** vida 5 y bruto 32 → 5, no 32.

### F3. Piezas para romper un tramo (oráculo de tests de balance)

The `piezas_para_romper` formula is defined as:

`piezas_para_romper(a) = ceil(vida_tramo / ataque_a) = (vida_tramo + ataque_a − 1) / ataque_a`, con división entera hacia abajo

**Variables:**

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `vida_tramo` | int | 1 – 1.000 | data file (config) | Como en F1 |
| `ataque_a` | int | 1 – 1.000 | data file (Catálogo, F2 del Catálogo) | Ataque del tipo atacante `a`, con bonos de turno si se evalúa con Grito de guerra |

**Output Range:** 1 a 1.000. Mínimo 1 (`ataque_a` ≥ `vida_tramo`). `ataque_a = 0` daría división por cero, y el Catálogo lo excluye con F2 (`ataque ≥ 1`); no entra al núcleo, así que la comprobación desactivada de IL2CPP no importa.
**Example:** soldado: (30 + 7) / 8 = 4.

| Atacante | Ataque | Piezas (sin bonos) | Con Grito de guerra (+2) |
|---|---|---|---|
| Soldado | 8 | 4 | 3 |
| Arquero | 6 | 5 | 4 |
| Escudero | 4 | 8 | 5 |
| Catapulta | 10 | 3 | 3 |
| Jinete | 8 | 4 | 3 |
| Demoledor | 30 | **1** | 1 |
| Rey | 8 | 4 | 3 |

Ignora el área de la catapulta (6 en las vecinas) y la explosión del demoledor sobre varios tramos: cada pieza se cuenta contra un solo tramo.

**Excepción aprobada (2026-11-25): el demoledor como pieza de asedio.** Con `vida_tramo` = 30 y `ataque_demoledor` = 30 el demoledor rompe cada tramo de un golpe. Pegado a un tramo recto desde afuera explota sobre 3 tramos (2 desde una esquina); como con 2 tramos rotos el castillo deja de reclutar, **una sola pieza de 10 de vida puede cortar el reclutamiento rival** y abrir una brecha. Es un borde exacto: con `vida_tramo` = 31 pasaría a necesitar 2 golpes. El jugador lo aprobó como intencional, a cambio de que el demoledor muere al explotar, se mueve 6 casillas por turno y el rival puede matarlo antes de que llegue (vida 10, escudo 1). *PROVISIONAL hasta Resolución (#9), que confirma la explosión sobre muros; se mide en el MVP (Open Questions).*

### F4. Piezas para cortar el reclutamiento (oráculo de tests de balance)

The `piezas_para_cortar` formula is defined as:

`piezas_para_cortar(a) = TRAMOS_ROTOS_CORTE × piezas_para_romper(a)`

**Variables:**

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `TRAMOS_ROTOS_CORTE` | int | 1 – 16 | constant (contrato) | Tramos rotos que cortan el reclutamiento. Hoy 2 |
| `piezas_para_romper(a)` | int | 1 – 1.000 | calculated (F3) | Piezas del tipo `a` para romper un tramo |

**Output Range:** 1 a 16.000. Es una cota inferior sin área ni explosión.
**Example:** soldados: 2 × 4 = 8. Para comparar, el Rey cae con 5 soldados (`piezasParaMatar`, Catálogo): cortar el reclutamiento cuesta más que matar al Rey, así que los muros no son un atajo para ganar. Su valor es proteger al Rey: con el Rey en el centro y el muro entero, desde afuera solo lo alcanzan el arquero y la catapulta (la distancia mínima es 3); el escudero (alcance 2) y las piezas de alcance 1 necesitan un hueco o que el Rey se acerque al muro.

### F5. Estadísticas del arquero sobre el muro

The `estadisticas_en_muro` formula is defined as:

`escudo_efectivo = escudo + (enMuro ? BONO_ESCUDO_MURO : 0)`
`alcance_max_efectivo = alcance_max + (enMuro ? BONO_ALCANCE_MURO : 0)`

**Variables:**

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `escudo` | int | 0 – 1.000 | data file (Catálogo) | Escudo del arquero. Hoy 2 |
| `alcance_max` | int | 1 – 1.000 | data file (Catálogo) | Alcance máximo del arquero. Hoy 4 |
| `enMuro` | bool | verdadero o falso | calculated (estado) | Regla 7: arquero sobre un tramo propio con vida > 0 |
| `BONO_ESCUDO_MURO` | int | 0 – 10 | constant (contrato) | Hoy 2. Tope 10 por la regla de bonos de la Librería |
| `BONO_ALCANCE_MURO` | int | 0 – 10 | constant (contrato) | Hoy 2 |

**Output Range:** escudo efectivo 2 a 12 (bono de 0 a 10; con el vigente, 4); con bono 0, `enMuro` no cambia nada. Se suma a los bonos de turno que combina `EffectiveStats`.
**Example:** arquero (escudo 2, alcance 4) en el muro → escudo 4 y alcance 1–6; fuera del muro → 2 y 1–4. El ataque no cambia.

### F6. Daño en la casilla de un tramo con arquero

The `dano_casilla_tramo_con_arquero` formula is defined as:

`neto_pieza = max(0, bruto − escudo_efectivo)` y `neto_tramo = bruto`, y a este último se le aplica F1.

**Variables:**

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `bruto` | int | 0 – 24.999.999 | calculated (Resolución, #9) | Daño total sobre la casilla en la resolución |
| `escudo_efectivo` | int | 0 – 1.010 | calculated (F5) | Escudo del arquero con el bono, evaluado al empezar la resolución |

**Output Range:** `neto_pieza` de 0 a `bruto`; `neto_tramo` igual a `bruto`, acotado por F1. La parte de la pieza la puede absorber un escudero que la cubre (Cobertura, #8); la del tramo no.
**Example:** 4 soldados juntos, `bruto` 32: la pieza recibe 28 (muere con 15 de vida) y el tramo 32 (roto). Un soldado, `bruto` 8: pieza 4, tramo 8. Área de catapulta, `bruto` 6: pieza 2, tramo 6.

### F7. Tramos rotos y corte de reclutamiento

The `tramos_rotos` formula is defined as:

`tramos_rotos(j) = número de tramos del castillo de j con vida = 0` y `puede_reclutar(j) = tramos_rotos(j) < TRAMOS_ROTOS_CORTE`

**Variables:**

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `vida_i` | int | 0 – `vida_tramo` | calculated (estado) | Vida de cada uno de los 16 tramos del castillo de `j` |
| `TRAMOS_ROTOS_CORTE` | int | 1 – 16 | constant (contrato) | Como en F4 |

**Output Range:** `tramos_rotos` de 0 a 16; `puede_reclutar` es booleano y, al ser permanente, una vez falso no vuelve.
**Example:** 1 tramo roto → verdadero. 2 tramos rotos → falso.

### Límites de variables

| Variable | Mínimo | Máximo | 0 / fuera de rango |
|---|---|---|---|
| `vida_tramo` | 1: cualquier golpe rompe | 1.000: sin desborde | 0 → `ConfigInvalida` (todos los tramos nacerían rotos) |
| `TRAMOS_ROTOS_CORTE` | 1 | 16 | 0 → nunca se recluta desde el turno 1; 17 o más → el corte no ocurre y la regla del Modificador queda inerte |
| `BONO_*_MURO` | 0 | 10 | 0 → subir al muro no aporta nada y la decisión del defensor desaparece |
| `bruto` | 0 → sin efecto | 24.999.999 → se acota a 0 de vida | — |

### Lazos de retroalimentación

- **Positivo (nombrado, no eliminado):** romper el muro quita reclutamiento, deja al rival con menos piezas y le da ventaja de piezas al atacante. Es la tensión con el pilar 3. Lo amortiguan el costo (60 de vida de tramos, F4) y el reparto de la probabilidad liberada hacia Grito de guerra y Arremetida (Librería, `reparto_liberado`). Se mide en el MVP.
- **Negativo:** cada golpe al muro es un golpe que no va al ejército ni al Rey (costo de oportunidad).

### Arrastre a otros GDD

Estos cambios los provocó este GDD y **se aplicaron el 2026-11-25** (ver Dependencies, *Bidireccionalidad*): Librería (campo `vida_tramo` en la config y su validación, 1 a 1.000), Máquina de turno (la cota de `vidaQuitada` pasa de `+ 480` a `+ 16 × vida_tramo`), Tablero (cierra su Open Question del esquema de config y su PROVISIONAL de la vida inicial del tramo).

## Edge Cases

### Rotura y daño

- **If un golpe deja el tramo exactamente en 0** (`bruto = vida`): queda Roto (F1).
- **If el daño supera la vida del tramo:** `vida_nueva = 0` y a `vidaQuitada` solo suma lo que el tramo tenía (F2); el excedente se pierde y la vista previa muestra ROTO sin mostrar el exceso.
- **If dos o más tramos se rompen en la misma resolución:** se actualizan juntos (recorrido por casilla, fila y columna), `tramosRotos` suma todos y el corte rige igual desde la tabla del siguiente turno del dueño. No hay orden "primero uno, después otro".
- **If un tramo roto vuelve a recibir daño** (área sobre un hueco): sin efecto, y no suma a `vidaQuitada`.
- **If el daño al tramo es fuego amigo** (área o explosión propias): el tramo pierde vida y se rompe igual, pero `vidaQuitada` no suma. El corte de reclutamiento no distingue quién rompió el tramo.
- **If varios ataques apuntan al mismo tramo y se resuelven juntos:** se suman en un solo `bruto` (daño sumado por objetivo).

### Arquero y muro

- **If el tramo se rompe y el arquero sobrevive:** queda en el hueco como arquero común y pierde el bono.
- **If el tramo se rompe y el arquero muere en la misma resolución:** la casilla queda libre; el daño a pieza y tramo se calcula con el estado al empezar la resolución (F6).
- **If el arquero baja del tramo y otro arquero propio sube en el mismo turno:** es legal; el tramo queda libre apenas se completa el `Mover` del primero (uno por tramo, en orden de comandos).
- **If se usa `Deshacer` sobre un `Mover` que subió o bajó:** el arquero vuelve a su casilla y `enMuro` se recalcula solo (consulta pura, sin estado que corregir).
- **If un arquero enemigo intenta pisar un tramo en pie del rival:** `Mover` rechazado; solo el dueño sube. Si ese tramo está roto, es una casilla libre común y no da bono.
- **If un arquero propio entra a un hueco de su propio muro:** casilla común; sin bono (`enMuro` exige un tramo con vida mayor que 0).
- **If el Modificador da Grito de guerra al arquero que está en el muro:** suma +2 de ataque y de movimiento; el alcance sigue en 1–6, porque el Grito no da alcance.
- **If el arquero en el muro está cubierto por un escudero:** regla 8 (el escudero absorbe la parte de la pieza; el tramo recibe su golpe completo).
- **If un arquero camina por el muro:** cada paso de tramo en tramo cuesta 1 de movimiento; no puede pasar a un tramo ocupado por otro arquero (uno por tramo).

### Movimiento y huecos

- **If se rompe un tramo de esquina:** el hueco se puede pisar pero **por sí solo no abre paso al interior**: el movimiento es en cruz y las otras dos vecinas de una esquina son tramos. Si además se rompe un tramo vecino, sí lo abre (por ejemplo afuera → `(0, 5)` → `(0, 6)` → interior). Sí cuenta para el corte de reclutamiento. Es intencional (Movimiento, #6).
- **If se rompe un tramo del lado pegado al borde del tablero** (columna 0 del azul, 23 del rojo): el hueco por sí solo queda sin acceso desde afuera (ese lado no tiene anillo): se llega desde el interior, desde un hueco vecino o, si es un arquero propio, desde un tramo vecino.
- **If un jinete llega a un tramo en pie:** lo frena (regla 3); pasa por encima de piezas enemigas, nunca de tramos.
- **If el Rey rival intenta atravesar un tramo en pie:** bloqueado; solo el Rey del dueño atraviesa los propios.
- **If los 16 tramos están rotos:** no hay regla de "castillo destruido": el interior queda abierto al paso común y el castillo no recluta.
- **If una pieza enemiga entra al castillo por un hueco:** nada especial; sale por cualquier hueco. Como no hay reparación, nadie queda atrapado.

### Reclutamiento, Modificador, config y fin de partida

- **If los tramos se rompen en el turno del rival** (al resolver): la tabla del dueño se calcula al empezar su turno, así que ya incluye el corte. Nunca sale una cara de reclutar sin poder reclutar por causa del muro (los topes son de #10).
- **If los tramos se rompen por fuego amigo en el propio turno:** el corte rige desde su siguiente turno; en el actual ya no queda fase de reclutar, que es de mover.
- **If los dos castillos tienen 2 tramos rotos:** ambas tablas pierden las caras de reclutar; la partida sigue hasta Rey muerto, reloj agotado, rendición o tablas.
- **If un jugador corta su propio reclutamiento a propósito** (fuego amigo): permitido. Gana más probabilidad de Grito de guerra y Arremetida (Librería, `reparto_liberado`) a cambio de perder el reclutamiento para siempre y exponer su castillo. *Posible estrategia degenerada; se mide (Open Questions).*
- **If la config trae `vida_tramo` 0 o mayor que 1.000:** `ConfigInvalida`; no se crea la partida.
- **If un registro de partida trae otra `vida_tramo` que la config actual:** la huella difiere y el registro no se mezcla con la config actual; qué hace la Librería con la versión grabada lo decide ella.
- **If el Rey muere en la misma resolución que rompe tramos:** la partida termina y el estado final conserva los tramos como quedaron.

## Dependencies

**Depende de:**

| Sistema | Tipo | Qué usa |
|---|---|---|
| #1 Librería de reglas | Dura | Contrato: comandos cerrados (`Mover`, `AsignarAtaque`), enteros y orden fijo, estado con 16 tramos por castillo, `EffectiveStats` (el bono de la regla 7 entra ahí), config de `CrearPartida` con el campo `vida_tramo` |
| #2 Catálogo de piezas | Dura | `escudo` y `alcanceMax` del arquero y el ID del tipo arquero. **No lee su `vida`**: esa es de Resolución |
| #4 Tablero y despliegue | Dura | Grilla, geometría del castillo (muro, interior, anillo), tramos colocados y las consultas `hayTramo`, `ocupante`, `distanciaCruz` |
| #3 Máquina de turno | Blanda | Estructura del turno que fija el momento del corte: reclutar es de la fase de mover, la tabla del Modificador se calcula al abrir el turno; `Deshacer` cubre los `Mover` de subir y bajar |

**Dependen de este sistema:**

| Sistema | Tipo | Qué usa |
|---|---|---|
| #1 Librería | Dura | Estado de tramos, `vida_tramo`, consultas `enMuro`, `tramosRotos` y `puedeReclutar`, y el bono en `EffectiveStats` |
| #4 Tablero | Blanda, solo un dato | `vida_tramo` para colocar la vida inicial de los 16 tramos (llega por la config) |
| #6 Movimiento | Dura | Quién bloquea el paso, huecos transitables, subir y bajar del muro (reglas 3 y 6) |
| #7 Alcance y asignación | Dura | Tramo enemigo en pie como objetivo válido; ataque **directo** a tramo propio o roto rechazado (el área se acepta); alcance efectivo del arquero (F5) |
| #8 Cobertura | Blanda | La parte del tramo no se redirige (regla 8) |
| #9 Resolución | Dura | Daño a pieza y tramo a la vez (F1, F2, F6), vida nueva, rotura y la explosión del demoledor sobre muros |
| #10 Reclutamiento | Dura | `puedeReclutar` |
| #11 Modificador | Dura | `tramosRotos` / `puedeReclutar` para poner en 0 las caras de reclutar |
| #15 Ficha y HUD, #16 Vista previa, #18 Render | Blanda | Vida de cada tramo, contador de tramos rotos, `enMuro`, estados En pie, dañado y Roto, daño previsto al tramo |
| #25 Rival IA | Blanda | Mismas consultas que un jugador; el estado del muro es público |
| #26 Registro | Blanda | Guarda `vida_tramo` dentro de la config de cada partida |

**Bidireccionalidad: estado de los documentos vecinos** (al 2026-11-25):

- **Librería:** hecho el 2026-11-25: campo `vida_tramo` (1 a 1.000) en la regla 2, su validación, su lugar en el estado y la huella (regla 11), una fila para este sistema, la cota de `vidaQuitada` con `16 × vida_tramo`, la fila de `vida_tramo` en la tabla de dominios, AC-44 y AC-46, y el AC-12b con el bono ya definido. **Pendiente en la Librería:** un nuevo `/design-review`, porque el contrato cambió.
- **Máquina de turno:** hecho el 2026-11-25: fila blanda de #5 en Dependencies y la cota de `vidaQuitada` de la regla 15 con `16 × vida_tramo`.
- **Catálogo:** hecho el 2026-11-25: las dos filas de #5 dicen `escudo`, `alcanceMax` e ID del tipo, y I1 recoge la excepción del demoledor contra el tramo.
- **Tablero:** hecho el 2026-11-25: cerrada su Open Question del esquema de config y levantado su PROVISIONAL de la vida inicial del tramo.
- **systems-index:** hecho el 2026-11-25: fila 5 con la Librería y la Máquina de turno, y las aristas de #7, #8, #15, #16, #18 y #25 hacia #5.
- **art-bible:** pendiente (no se tocó): las dos discrepancias de §6 de Visual/Audio.

**Suposiciones provisionales:** #9 no tiene GDD; el tipo de cada arista de esas filas es el que este sistema necesita de ellos y se confirma cuando los diseñen.

## Tuning Knobs

**Dónde viven.** `vida_tramo` es un dato de la config de `CrearPartida`: se cambia sin tocar código y entra a la huella de la partida. `TRAMOS_ROTOS_CORTE`, `BONO_ESCUDO_MURO` y `BONO_ALCANCE_MURO` son **constantes nombradas del contrato**, con una sola definición versionada: cambiar una es tocar esa definición, subir la versión de reglas y actualizar los tests (decisión 2026-11-25). Los rangos seguros salen de las fórmulas con los valores vigentes del Catálogo y **no están medidos**: se calibran en el MVP.

| Perilla | Vigente | Rango seguro | Si es demasiado alto | Si es demasiado bajo |
|---|---|---|---|---|
| `vida_tramo` (config; válido 1–1.000) | 30 | **18 a 40**, sin medir | Más de 40: un soldado necesita 6 golpes y cortar el reclutamiento cuesta 12 soldados o más; el muro casi no se rompe en 15–20 min, el Rey queda fuera del alcance de las piezas de alcance 1 o 2 y la partida se estanca (pilar 4) | Menos de 18: cortar el reclutamiento (`2 × vida_tramo`) cuesta menos que matar al Rey (35). Menos de 13: una catapulta con Grito de guerra (12) rompe un tramo sola (pilar 2). Menos de 9: un soldado también |
| `TRAMOS_ROTOS_CORTE` | 2 | **2 a 3** | 4 o más: cortar cuesta 16 soldados o más; el corte casi nunca ocurre y la regla del Modificador queda inerte. 17: apagado | 1 (valor de la v4): una sola brecha corta el reclutamiento; el jugador lo subió a 2 en la v5. 0: nadie recluta desde el turno 1 |
| `BONO_ESCUDO_MURO` | 2 | **1 a 4** | Con 6 un soldado solo no le saca nada al arquero (escudo efectivo 8 contra ataque 8); con 8 tampoco la catapulta (10). Subir al muro vuelve inmune a quien no se coordina contra el pilar 2 | 0: subir al muro no aporta escudo; con el bono de alcance también en 0, la decisión del defensor desaparece |
| `BONO_ALCANCE_MURO` | 2 | **1 a 3**, sin medir | Con 2 el arquero en su muro llega a la columna 10, el borde de la zona en disputa del Tablero (10 a 13). Con 3 o más se mete más adentro y ventajea al que defiende | 0: el arquero arriba no alcanza más lejos; solo queda el escudo |

**Perillas que interactúan:**

- **`vida_tramo` y `TRAMOS_ROTOS_CORTE`** se miran juntos: su producto (hoy 60) es el costo mínimo de cortar el reclutamiento. Conviene que sea mayor que escudo + vida del Rey (35), así los muros no son un atajo para ganar.
- **`vida_tramo` y el `ataque` del demoledor (30):** el borde exacto de la excepción aprobada. Con `vida_tramo` mayor que 30 el demoledor necesita 2 golpes y la excepción deja de existir. Cambiar uno exige revisar el otro.
- **`vida_tramo` y los ataques del Catálogo, y el +2 del Grito de guerra:** mueven la tabla de F3. Cualquier cambio de ataque o del Grito se revisa contra esa tabla.
- **`BONO_ESCUDO_MURO` y el escudo y ataque del Catálogo:** fijan cuánto le saca un golpe al arquero en el muro. El umbral de inmunidad contra un soldado es `escudo + bono ≥ ataque`.
- **`BONO_ALCANCE_MURO` y `COLUMNAS` o el margen del castillo (Tablero):** juntos fijan hasta qué columna llega el arquero desde su muro y cuánto se solapa la zona en disputa.
- **`TRAMOS_ROTOS_CORTE` y `RADIO_MURO` (Tablero):** con `RADIO_MURO` 1 el muro tiene 8 tramos y 2 rotos son el 25%; con 2 (16 tramos), el 12,5%.

**Perillas que son de otro GDD (no se duplican acá):**

- `RADIO_MURO`, `COLUMNAS`, `FILAS` y el margen del castillo: Tablero (#4).
- Ataque, escudo, vida, alcance y movimiento de cada pieza: Catálogo (#2).
- El +2 de ataque y movimiento del Grito de guerra y las probabilidades de las caras: Modificador (#11).
- Los topes de llamadas y de piezas vivas: Reclutamiento (#10) y Catálogo (#2).

## Visual/Audio Requirements

Fuente: `design/art/art-bible.md` (§3 *Castillo y muros*, §4 *ROTO* y daltonismo, §6 *Estilo del castillo* y *El terreno cuenta la batalla*). Todo vive en la capa del sistema salvo el dibujo del muro, que es del mundo.

| Evento | Feedback visual | Feedback sonoro (dirección; la especificación es de #22) |
|---|---|---|
| **Vista previa del daño al tramo** (asignar) | El daño previsto sobre el tramo como número. Si alcanza su vida, la placa **ROTO**: Tinta con texto `#F5F1E8`, grieta en zigzag, la palabra ROTO y el tramo dibujado como hueco | Señal distinta de ROTO al confirmar la vista previa (pedido del art bible al director de audio) |
| **Golpe combinado sobre un tramo** | Los trazos de los atacantes convergen en el tramo; la punta fundida crece con la cantidad (1 atacante, ×1,5 con 2, ×2 con 3 o más); el efecto sobre el tramo crece igual | Golpe a piedra, distinto del golpe a una pieza; el acento crece con los atacantes (#22) |
| **Cambio de estado del tramo** | **Entero:** borde recto. **Mellado** (`vida × 2 < vida_tramo`): melladuras en el borde superior. **Roto:** hueco con escombro bajo y borde quebrado. Transición de ≤ 0,25 s | Derrumbe corto al romperse |
| **Escombros alrededor del muro** | Cuantos más tramos dañados, más escombros; se lee de lejos qué castillo viene perdiendo. **Persisten**: son estado | — |
| **Contador de tramos rotos** (0, 1, 2) | En el HUD (#15), siempre visible para los dos jugadores, con el número y un ícono de grieta; en 1 un aviso, en 2 un aviso de una sola vez ("sin reclutamiento"). Nunca solo color | Señal de una sola vez al pasar a 2 |
| **Arquero sube al muro** | La figura se apoya sobre el tramo y un parapeto marca el +2 de escudo; la silueta queda entera. La ficha muestra escudo y alcance efectivos (4 y 1–6); el alcance se dibuja con corchetes en las esquinas, bermellón al 70% | Apoyo sutil (#22) |
| **Hueco transitable** | Se mueve por él como por una casilla libre: puntos blancos al 60%. Se distingue del tramo en pie por forma, no por color | — |

**Restricciones de estilo y rendimiento:**

- **Jerarquía de lectura:** el muro está por encima del terreno y por debajo de las piezas; contorno fino y más claro que el de las figuras. Color Piedra de muro `#8A867C` (saturación ≈ 10%); nunca el color de bando, reservado a la capa del sistema.
- **Forma antes que color:** ROTO se lee por la forma y por el texto. El par Escudo acero contra Piedra de muro (tritanopia) se separa por el ícono de escudo y por la forma de bloque del muro.
- **Ritmo:** todos los golpes de una resolución se animan a la vez; la resolución completa no pasa de ~2 s (propuesta del art bible, a validar en prototipo).
- **Packs de skins:** un castillo por pack respeta 16 tramos de una casilla y los tres estados (entero, mellado, roto) más el interior. El umbral de "mellado" es el de la regla 2, igual para todos los packs.
- **Sin sangre ni texto de bando en el muro.**

**Principios del art bible que aplican:** *Regla visual (orden de lectura)*, *Principio 1: El plan se lee antes que el detalle*, *Principio 2: El combo se ve como un solo golpe*, *Principio 3: El ritmo del turno manda sobre la animación*, y de §4 los colores semánticos y el respaldo de daltonismo.

**Para corregir en el art bible** (no en este GDD):

- §6 dice "interior de 5 × 5 de empedrado", pero el bloque es de 5 × 5 con 9 casillas de interior (Tablero, regla 5).
- §6 dice que las marcas del terreno "se desvanecen a los 3 turnos". El hueco de un tramo roto es estado y no se desvanece nunca (el tramo no se repara); esa regla de 3 turnos es solo para marcas como el cráter y la quemadura.

## Game Feel

- **Subir y bajar del muro es un paso más:** mismo `Mover`, mismo presupuesto de la Librería (1 ms p95 por comando), sin menú ni confirmación extra. La ficha muestra escudo y alcance efectivos en el mismo cuadro en que se envía el `Mover`.
- **Anticipación y cosecha (pilar 1):** el daño previsto al tramo y la placa ROTO aparecen en la vista previa. Al resolver, el resultado coincide exactamente con lo previsto: sin azar y sin variación de daño ni de animación que cambie lo que se lee.
- **Peso del golpe (pilar 2):** el efecto sobre el tramo crece con la cantidad de atacantes; la rotura se siente como un solo golpe combinado, no como varios sueltos.
- **Ritmo (pilar 4):** la rotura se anima dentro de la ventana de resolución (~2 s) y a la vez que los demás golpes. Sin cámara lenta ni pausa extra. La transición de mellado a roto dura ≤ 0,25 s.
- **Aviso de corte:** una sola vez y no modal: no frena el turno ni pide un clic.
- **Hueco:** apenas se rompe el tramo se puede pasar por él con el siguiente comando; ninguna animación bloquea el movimiento.
- **Verificación:** capturas retenidas (AC-31) y playtest de la Player Fantasy.

## UI Requirements

Sin pantalla propia: vive en el tablero y en el HUD (#15).

- **Contador de tramos rotos por castillo** (0, 1, 2 o más): número más ícono de grieta, siempre visible para los dos jugadores, uno por castillo. En 1, aviso; en 2, aviso de una sola vez ("sin reclutamiento").
- **Vida de cada tramo:** el estado (entero, mellado, roto) se ve siempre por la forma; el número exacto, al elegir o pasar sobre el tramo, y siempre en la vista previa junto al daño previsto. En pantalla táctil, al tocarlo.
- **Arquero:** al elegirlo se marcan los tramos propios libres a los que puede subir (puntos blancos al 60%) y su alcance efectivo (corchetes). En el muro, la ficha muestra escudo 4 y alcance 1–6, con el parapeto en la figura.
- **Vista previa (#16):** el daño al tramo y la placa ROTO, igual que con las piezas.
- **Panel "?" del Modificador (#17):** cuando un castillo no recluta, las caras de reclutar se muestran en 0% **con la causa** ("muros rotos"), no solo el número.
- **Tutorial (#24):** debe enseñar subir al muro y romper un tramo en el paso de piezas especiales.
- **Accesibilidad (#23):** el contador va con número e ícono, el hueco se distingue por forma, y el tramo tiene el tamaño táctil de una casilla.

## Cross-References

| Este GDD dice | Documento | Elemento exacto | Naturaleza |
|---|---|---|---|
| Campo `vida_tramo` en la config (1–1.000), con validación y huella | `libreria-de-reglas.md` | Regla 2 (config) y regla 11 (huella) | **Agrega** (hecho el 2026-11-25) |
| Bono de posición en `EffectiveStats`; consultas `enMuro`, `tramosRotos`, `puedeReclutar` | `libreria-de-reglas.md` | AC-12b ("se agrega cuando su GDD lo defina") | **Cierra** y agrega las consultas (hecho el 2026-11-25) |
| Subir al muro con `Mover`, sin comando nuevo | `libreria-de-reglas.md` | Regla 3 (lista cerrada) | Compatible, sin cambio |
| Corte con 2 tramos rotos y reparto de la probabilidad liberada | `libreria-de-reglas.md` | Tabla efectiva del Modificador y `reparto_liberado` | Consistente: "2 muros rotos" pasa a ser `TRAMOS_ROTOS_CORTE` |
| Cota de `vidaQuitada` `+ 480` | `maquina-de-turno.md` | Regla 15 | **Cambia** a `+ 16 × vida_tramo`; agrega fila blanda en Dependencies (hecho el 2026-11-25) |
| Esquema de config de la vida del tramo | `tablero-y-despliegue-inicial.md` | Open Questions; regla 5 e Interactions (PROVISIONAL) | **Cierra** (hecho el 2026-11-25) |
| Quién pasa por el hueco | `tablero-y-despliegue-inicial.md` | Edge Cases ("un tramo de muro se rompe") | **Cierra** (reglas 3 y 5) |
| Arquero: lee `escudo`, `alcanceMax` e ID; los +2 y +2 son de este GDD | `catalogo-de-piezas.md` | Regla 5 y Dependencies, fila #5 | **Corrige** "`vida` y `escudo`" (hecho el 2026-11-25) |
| Demoledor (30) contra tramo (30) | `catalogo-de-piezas.md` | I1 (excepciones) y Open Questions (el demoledor mata solo) | **Agrega** la excepción (hecho el 2026-11-25) |
| Fila 5 del índice y aristas #1, #3, #7, #8, #15, #16, #18, #25 | `systems-index.md` | Tabla de sistemas y Dependency Map | **Agrega** (hecho el 2026-11-25) |
| Reglas del castillo, arqueros en el muro, Rey, demoledor y pilares | `game-concept.md` | "Reglas vigentes" y Game Pillars | Fuente; sin cambio |
| `arquero`, `radio_muro`, `piezas_para_matar` | `entities.yaml` | Entradas existentes | Valores nuevos a registrar en la Fase 5 |
| Estados del tramo, ROTO, escombros | `art-bible.md` | §3, §4, §6 | Fuente; dos discrepancias a corregir (ver Visual/Audio) |

## Acceptance Criteria

Los criterios usan las coordenadas del despliegue: el azul tiene el tramo `(4, 7)` con el anillo en `(5, 7)` y el interior en `(3, 7)`, y la esquina es `(4, 5)`. Tipos: **Logic** (prueba unitaria en Edit Mode), **Integration** (con otro sistema) y **Visual** (captura retenida). Los *Integration* que dependen de #9 quedan **PROVISIONAL** hasta que esos GDD existan.

**Reglas (Core Rules)**

- **AC-1 (regla 1, el tramo)**: **GIVEN** el despliegue inicial con `vida_tramo` 30, **WHEN** se lee el estado, **THEN** cada castillo tiene 16 tramos con vida 30 y escudo 0, ningún tramo tiene ID de pieza y cada jugador tiene 13 piezas vivas para el tope y para el Modificador. *(Logic)*
- **AC-2 (regla 2, estados)**: **GIVEN** un tramo con vida 30, **WHEN** recibe bruto 15, 16 y 30 (tres fixtures), **THEN** queda con vida 15 (en pie, no dañado), 14 (en pie, dañado) y 0 (Roto). *(Logic)*
- **AC-3 (regla 3 y regla 11, bloqueo y Rey)**: **GIVEN** el tramo azul `(4, 7)` en pie y un soldado azul en `(5, 7)` con 4 de movimiento, **WHEN** intenta `Mover` a `(4, 7)` o a `(3, 7)`, **THEN** ambos se rechazan (igual para un jinete). **Y GIVEN** el Rey azul en `(3, 7)`, **THEN** `Mover` a `(5, 7)` atravesando el tramo se acepta y `Mover` a `(4, 7)` se rechaza. **Y GIVEN** el Rey rojo en `(5, 7)`, **THEN** atravesar `(4, 7)` se rechaza. *(Integration con #6)*
- **AC-4 (regla 3, sin línea de visión)**: **GIVEN** un arquero rojo en `(6, 7)` (alcance máximo 4), el Rey azul en `(2, 7)` y el tramo `(4, 7)` en pie, **WHEN** se piden `ObjetivosValidos`, **THEN** incluye `(2, 7)`. *(Integration con #7)*
- **AC-5 (regla 4, objetivos y daño)**: **GIVEN** el tramo azul `(4, 7)` en pie y un soldado rojo en `(5, 7)`, **WHEN** se llama `AsignarAtaque` hacia `(4, 7)`, **THEN** se acepta. **Y GIVEN** un soldado azul con el mismo tramo propio, **THEN** `ObjetivoPropio`; con un tramo roto vacío, `SinObjetivo`. **Y GIVEN** una catapulta azul en alcance, **THEN** los dos se aceptan. **Y WHEN** 4 soldados rojos (bruto 32) se resuelven sobre `(4, 7)`, **THEN** su vida queda en 0. *(Integration con #7 y #9, PROVISIONAL)*
- **AC-6 (regla 4, área y fuego amigo)**: **GIVEN** una catapulta azul con centro en una casilla libre junto al tramo propio `(4, 7)` (6 de área sobre el tramo), **WHEN** se resuelve, **THEN** el tramo pasa de 30 a 24 y `vidaQuitada` del azul no cambia. **Y GIVEN** una catapulta azul con centro en el tramo rival `(19, 7)` (10), **THEN** el tramo queda en 20 y `vidaQuitada` del azul suma 10. *(Integration con #9, PROVISIONAL)*
- **AC-7 (regla 5, rotura y hueco)**: **GIVEN** el tramo `(4, 7)` en vida 0, **WHEN** un soldado azul se mueve de `(5, 7)` a `(4, 7)` y luego a `(3, 7)`, **THEN** ambos pasos se aceptan, `hayTramo(4, 7)` es falso y el Tablero sigue clasificando la casilla como muro. **Y WHEN** pasan 10 turnos sin daño, **THEN** la vida sigue en 0. *(Integration con #6 y #4)*
- **AC-8 (regla 6, subir, caminar y bajar)**: **GIVEN** un arquero azul en `(5, 5)` y el tramo `(4, 5)` libre, **WHEN** hace `Mover` a `(4, 5)`, **THEN** se acepta y gasta 1 de movimiento. **Y WHEN** va a `(4, 6)` (tramo propio libre) se acepta; **Y** a `(5, 6)` (anillo) se acepta; **Y** a `(3, 6)` (interior) se rechaza. **Y GIVEN** el tramo `(4, 7)` roto y el arquero en `(4, 6)`, **THEN** `Mover` de `(4, 6)` a `(4, 7)` y de vuelta se aceptan. **Y GIVEN** un segundo arquero que apunta a un tramo ocupado, un soldado azul que apunta a un tramo propio libre o un arquero rojo que apunta a un tramo azul en pie, **THEN** los tres se rechazan. *(Integration con #6)*
- **AC-9 (regla 7, bono de posición)**: **GIVEN** un arquero azul sobre `(4, 5)` con el tramo en pie, **WHEN** se llama `EffectiveStats`, **THEN** escudo 4, alcance máximo 6, ataque 6 y movimiento 4. **Y GIVEN** el mismo arquero en `(5, 5)` o sobre un tramo roto, **THEN** escudo 2 y alcance 4. **Y GIVEN** Grito de guerra activo con el arquero en el muro, **THEN** ataque 8, movimiento 6, escudo 4 y alcance 6. *(Logic)*
- **AC-10 (regla 7, el bono rige al empezar la resolución)**: **GIVEN** un arquero azul con 15 de vida sobre el tramo `(4, 5)` con vida 18, **WHEN** se resuelve un bruto de 18 sobre esa casilla, **THEN** el tramo queda en 0 y el arquero sobrevive con 1 de vida (neto 14 con escudo 4; con escudo 2 habría muerto). *(Integration con #9, PROVISIONAL)*
- **AC-11 (regla 8, daño compartido y cobertura)**: **GIVEN** un arquero azul (15 de vida) sobre el tramo `(4, 5)` (30 de vida), **WHEN** recibe bruto 8, **THEN** el arquero queda en 11 y el tramo en 22. **Y GIVEN** el arquero cubierto por un escudero (escudo 5, vida 20), **THEN** el arquero queda en 15, el escudero en 17 y el tramo en 22. *(Integration con #8 y #9, PROVISIONAL)*
- **AC-12 (regla 9, tramo roto bajo un arquero)**: **GIVEN** un arquero azul con 15 de vida sobre un tramo con vida 10, **WHEN** el bruto es 11, **THEN** el tramo queda en 0, el arquero en 8 sobre la casilla del hueco y `enMuro` es falso. **Y WHEN** el bruto es 20, **THEN** el arquero muere (neto 16 ≥ 15), la casilla queda sin ocupante y `hayTramo` es falso. *(Integration con #9, PROVISIONAL)*
- **AC-13 (regla 10, corte de reclutamiento)**: **GIVEN** el castillo azul con 1 tramo roto, **THEN** `tramosRotos(azul)` es 1 y `puedeReclutar(azul)` es verdadero. **Y WHEN** se rompe un segundo, **THEN** `tramosRotos` es 2, `puedeReclutar` es falso y la tabla efectiva del azul al abrir su siguiente turno pasa de 15/20/25/25/15 a 15/45/0/0/40 con más de 12 piezas sin el Rey, y a 15/85/0/0/0 con 12 o menos. **Y WHEN** pasan 5 turnos sin daño o los 16 tramos están rotos, **THEN** `puedeReclutar` sigue falso. *(Integration con #10 y #11)*
- **AC-14 (regla 10, momento del corte)**: **GIVEN** un turno del rojo cuyos ataques rompen el segundo tramo azul, **WHEN** empieza el turno del azul, **THEN** su tabla efectiva ya tiene las caras de reclutar en 0 y la tirada no puede dar reclutar. *(Integration con #3 y #11)*
- **AC-15 (regla 12, fin de partida)**: **GIVEN** los 16 tramos azules rotos y el Rey azul vivo, **WHEN** se llama `EvaluateMatchEnd`, **THEN** la partida sigue En curso. *(Integration con #3)*
- **AC-16 (regla 13, orden y consultas puras)**: **GIVEN** dos tramos azules con vida 8 que reciben bruto 10 cada uno en la misma resolución, **WHEN** se lee el resultado, **THEN** ambos quedan en 0, `tramosRotos` suma 2 y los eventos de rotura salen por casilla (fila, columna). **Y WHEN** se repite el fixture 100 veces, **THEN** la huella es idéntica, y consultar vida, `tramosRotos` y `enMuro` no la cambia. *(Logic)*
- **AC-17 (regla 14, config y huella)**: **GIVEN** una config con `vida_tramo` 30, **WHEN** se llama `CrearPartida`, **THEN** nace En curso con los 32 tramos en 30. **Y GIVEN** `vida_tramo` 0, −1 o 1.001, **THEN** `ConfigInvalida` y sin partida. **Y GIVEN** 1 o 1.000, **THEN** se acepta. **Y GIVEN** dos configs que difieren solo en `vida_tramo`, **THEN** sus huellas difieren. *(Integration con #1)*

**Fórmulas**

- **AC-18 (F1)**: **GIVEN** vida 30, **WHEN** el bruto es 0, 8, 30 o 32, **THEN** la vida nueva es 30, 22, 0 y 0. **Y GIVEN** vida 0 y bruto 5, **THEN** 0. **Y GIVEN** bruto 24.999.999, **THEN** 0 sin desborde. *(Logic)*
- **AC-19 (F2)**: **GIVEN** vida 5 y bruto 32, **THEN** quitada 5. **Y GIVEN** vida 30 y bruto 8, **THEN** 8. **Y GIVEN** vida 0 y bruto 8, **THEN** 0. *(Logic)*
- **AC-20 (F3, oráculo)**: **GIVEN** `vida_tramo` 30, **WHEN** se calcula `piezas_para_romper` para soldado, arquero, escudero, catapulta, jinete, demoledor y Rey, **THEN** 4, 5, 8, 3, 4, 1 y 4; con Grito de guerra, 3, 4, 5, 3, 3, 1 y 3. **Y GIVEN** `vida_tramo` 1, **THEN** todos 1. **Y GIVEN** `vida_tramo` 31 y demoledor, **THEN** 2. *(Logic; test de balance, no entra al núcleo)*
- **AC-21 (F4, oráculo)**: **GIVEN** los valores vigentes, **WHEN** se calcula `piezas_para_cortar` con soldados, **THEN** 8, que es mayor que `piezasParaMatar` del Rey con soldados (5). El test falla si un ajuste invierte esa relación. *(Logic; test de balance)*
- **AC-22 (F5)**: **GIVEN** un arquero (escudo 2, alcance 4), **THEN** en el muro (4, 6) y fuera (2, 4). **Y GIVEN** `BONO_*_MURO` 0, **THEN** (2, 4) en ambos casos. **Y GIVEN** 10, **THEN** (12, 14). *(Logic)*
- **AC-23 (F6)**: **GIVEN** un arquero con escudo efectivo 4, **WHEN** el bruto es 32, 8, 4 o 0, **THEN** pieza y tramo reciben 28 y 32, 4 y 8, 0 y 4, 0 y 0. *(Logic)*
- **AC-24 (F7)**: **GIVEN** 0, 1, 2 y 16 tramos rotos, **THEN** `puedeReclutar` es verdadero, verdadero, falso y falso. **Y GIVEN** `TRAMOS_ROTOS_CORTE` 1, **THEN** con 1 tramo roto es falso. *(Logic)*

**Casos borde**

- **AC-25 (esquina)**: **GIVEN** solo el tramo `(4, 5)` roto y las demás casillas libres, **WHEN** un soldado azul con 4 de movimiento parte de `(5, 5)`, **THEN** puede pisar `(4, 5)` pero ningún movimiento legal llega a una casilla del interior. *(Integration con #6)*
- **AC-26 (lado del borde)**: **GIVEN** solo el tramo `(0, 7)` roto, **WHEN** un soldado con movimiento 12 parte desde cualquier casilla del anillo, **THEN** el conjunto alcanzable no incluye `(0, 7)` ni el interior. **Y GIVEN** rotos solo `(0, 5)` y `(0, 6)`, **THEN** desde `(0, 4)` el interior es alcanzable. *(Integration con #6)*
- **AC-27 (Deshacer)**: **GIVEN** un arquero azul en `(5, 5)`, **WHEN** hace `Mover` a `(4, 5)` y después `Deshacer`, **THEN** vuelve a `(5, 5)`, el tramo queda libre, `EffectiveStats` da escudo 2 y alcance 4, y la huella es igual a la anterior al `Mover`. *(Integration con #3)*
- **AC-28 (fuego amigo y corte propio)**: **GIVEN** el azul rompe sus 2 tramos con fuego amigo, **WHEN** termina la resolución, **THEN** `puedeReclutar(azul)` es falso y `vidaQuitada` de ambos jugadores no cambia. *(Integration con #9, PROVISIONAL)*
- **AC-29 (Rey muerto y tramos)**: **GIVEN** el Rey azul muere en la misma resolución que rompe 2 tramos azules, **WHEN** termina, **THEN** la partida pasa a Terminada y el estado final conserva los tramos rotos. *(Integration con #3)*
- **AC-30 (demoledor de asedio)**: **GIVEN** un demoledor rojo en `(5, 7)` con ataque 30 y `vida_tramo` 30, **WHEN** explota, **THEN** los tramos `(4, 6)`, `(4, 7)` y `(4, 8)` quedan en 0, `tramosRotos(azul)` es 3 y `puedeReclutar(azul)` es falso. **Y GIVEN** `vida_tramo` 31, **THEN** quedan en 1 y `tramosRotos` es 0. *(Integration con #9, PROVISIONAL)*

**Visual**

- **AC-31 (estados del tramo)**: **GIVEN** un tramo con vida 30, 14 y 0, **WHEN** se dibuja el castillo, **THEN** se ve borde recto, melladuras en el borde superior y un hueco con escombro y borde quebrado; el arquero en el muro se ve con el parapeto y la silueta entera. Se guarda una captura de cada estado en `production/qa/evidence/`. *(Visual, dueño #18, PROVISIONAL)*

**Revisión de QA**

- **Cobertura:** cada regla (1–14), cada fórmula (F1–F7) y los casos borde que podían salir rotos tienen criterio. No quedó ninguna regla ni fórmula sin criterio.
- **Rendimiento:** no se fija un presupuesto en milisegundos: las pruebas del proyecto no pueden depender del tiempo, y estas consultas son O(16) sin asignaciones. Queda como pregunta para la Librería (#1).
- **No automatizable:** que el hueco y el contador de tramos rotos *se lean al instante* (Player Fantasy) es una pregunta de playtest, no un criterio. Va a Open Questions.
- **Tipos:** AC-1, 2, 9, 16 y 18 a 24 son Logic puros; AC-3 a 8, 10 a 15, 17 y 25 a 30 son Integration, PROVISIONAL los que dependen de #9; AC-31 es Visual con captura retenida.

## Open Questions

| Pregunta | Dueño | Para cuándo |
|---|---|---|
| ¿Es el ritmo deseado que un solo demoledor rompa hasta 3 tramos y corte el reclutamiento rival? Con `vida_tramo` 30 y ataque 30 es un borde exacto | Resolución (#9), playtest | MVP: medir cuántas partidas lo usan y qué efecto tienen |
| ¿Vale la pena atacar el muro antes que al ejército o al Rey? Cortar cuesta 8 soldados y el Rey 5 | Métricas (#19), playtest | MVP: medir ataques a tramos por partida |
| ¿Cortar el reclutamiento apaga la remontada (pilar 3)? ¿Alcanza el reparto del Modificador? | Modificador (#11), Métricas | MVP y tras las primeras 20 partidas online |
| El hueco de una esquina no abre paso al interior (el movimiento es en cruz) pero cuenta para el corte: por sí solo, ¿es intencional? | Movimiento (#6), playtest | En el GDD de #6 y en el MVP |
| ¿Es una estrategia degenerada cortar el propio reclutamiento para ganar probabilidad de Grito de guerra y Arremetida? | Modificador (#11), Métricas | MVP |
| ¿Protegen los muros al Rey de las piezas de alcance 1 y 2 como se quiere? Con el Rey en el centro y el muro entero, desde afuera solo lo alcanzan el arquero y la catapulta | Alcance (#7), Resolución (#9), playtest | MVP |
| ¿El hueco y el contador "se leen al instante"? No automatizable | Render (#18), HUD (#15), playtest | MVP |
| Presupuesto de rendimiento de `enMuro` y `tramosRotos` (sin aserción de tiempo en tests) | Librería (#1) | En su ADR |
| Qué hace la Librería con un registro cuya `vida_tramo` difiere de la config actual | Librería (#1) | En su ADR |
| Confirmar el contrato PROVISIONAL de #9 (reglas 7 y 8) y la explosión del demoledor sobre muros | #9 | En sus GDD |
| ¿Cuánto suma el muro a la duración de la partida (pilar 4)? Medir turnos hasta el primer tramo roto y turnos extra por brechas; declarar su `Δturnos` contra el presupuesto de la Máquina de turno | Métricas (#19), Máquina de turno (#3) | MVP: compuerta de duración |

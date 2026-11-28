# Catálogo de piezas

> **Status**: Approved
> **Author**: jugador + Claude (design-system)
> **Last Updated**: 2026-11-27 (re-review: F9 en CrearPartida, orden de los códigos F, contrajuego del demoledor, texto de I4; valores v5 sin cambios)
> **Last Verified**: 2026-11-27
> **Design Review**: APPROVED 2026-11-27 (re-review con revisiones aplicadas en la misma sesión y verificación puntual de CD-GDD-ALIGN; antes NEEDS REVISION dos veces ese mismo día) (log: `design/gdd/reviews/catalogo-de-piezas-review-log.md`)
> **Implements Pillar**: 2 La fuerza está en combinar (con 1 Planificar y cosechar y 3 Siempre hay vuelta, con límites)
> **Creative Director Review (CD-GDD-ALIGN)**: APPROVE 2026-11-27 (después de CONCERNS ese mismo día) — (a) I4, una explosión puede matar varias piezas no-Rey: compatible con el pilar 2, reemplaza la aprobación del 2026-11-20 ("mata solo a una pieza"); (b) el contrajuego del demoledor ahora es posicional (separación e intercepción), no un conteo de golpes. Antes: CONCERNS (accepted) 2026-11-20 (Player Fantasy REVISED; el demoledor mata solo a cualquier pieza menos el Rey, aceptado con sus contrapesos)

## Summary

El **Catálogo de piezas** es la tabla de datos que fija, para los siete tipos de pieza (soldado, arquero, escudero, catapulta, jinete, demoledor y Rey), cuánto se mueven, a qué distancia atacan, cuánto daño hacen, cuánto escudo y vida tienen y cuántas puede tener cada jugador. El jugador no lo ve como sistema: lo siente como un ejército idéntico para los dos, con roles donde ninguna pieza gana sola (salvo excepciones aprobadas). Existe porque todas las reglas de movimiento, combate y reclutamiento leen estas cifras, y para que ajustar el balance sea cambiar datos versionados con huella y no código.

> **Quick reference** — Layer: `Foundation` · Priority: `MVP` · Key deps: `None` (la Librería de reglas #1 consume su esquema)

## Overview

El **Catálogo de piezas** es la tabla de datos estática que define cada tipo de pieza del juego: cuánto se mueve, a qué distancia ataca, cuánto daño hace, cuánto escudo tiene, cuánta vida tiene y cuántas piezas de ese tipo puede tener vivas cada jugador. Son siete tipos: soldado, arquero, escudero, catapulta, jinete, demoledor y Rey. La tabla vive en un archivo de datos fuera del código. El núcleo de reglas (Librería de reglas, #1) la recibe ya cargada cuando se crea la partida, la valida contra el formato que define este documento y la identifica con una huella. Así, el cliente, el servidor, las repeticiones y los tests usan exactamente la misma tabla, y una partida guardada con una tabla distinta se rechaza en lugar de dar resultados distintos en silencio.

El Catálogo **no tiene lógica ni estado**: no calcula daño, no mueve piezas y no conoce los bonos del turno. Esos bonos (por ejemplo el +2 del Grito de guerra) viven en el estado de la partida, y las reglas combinan tabla y bonos con `EffectiveStats(estado, pieza)`; así el Catálogo se mantiene estático y el pilar 3 se cumple (las ayudas al que va perdiendo dan opciones, nunca estadísticas fijas). El jugador no interactúa con el Catálogo: lo percibe como el rol de cada pieza y como las cifras de ataque, escudo y vida que ve en cada ficha. Existe porque toda regla de movimiento, ataque, reclutamiento y despliegue lee estas cifras, y porque el equilibrio entre piezas sostiene el pilar 2 (nadie gana solo; el poder sale de combinar): ajustarlo es cambiar datos, no código, con cada cambio atado a su huella.

## Player Fantasy

**Mismo ejército, duelo justo.** El jugador nunca abre el Catálogo, pero lo siente en una certeza: *los dos arrancamos con lo mismo*. Cuando empieza una partida, su ejército (4 soldados, 4 arqueros, 4 escuderos y el Rey) es idéntico al del rival, con las mismas cifras de movimiento, alcance, ataque, escudo y vida. Si al final del duelo el Rey rival cae, fue porque pensó mejor el plan, no porque su ejército pesara más. Esa certeza es lo que hace que el momento central del juego (ver caer juntos los golpes planeados) valga como mérito propio.

**Qué siente y cuándo:**

- *Al empezar una partida online:* ve las dos formaciones espejadas y sabe que no hay un ejército "mejor"; la partida se decide jugando.
- *Al leer la ficha de una pieza enemiga:* las cifras son las mismas que las de su propia pieza del mismo tipo, sea cual sea la skin, la cuenta o la facción del rival.
- *Al perder:* no puede culpar a los números; la derrota se explica con una decisión suya, y eso la hace aprendible.

**Pilares que sirve:** el anti-pilar *"NOT pagar para ganar: las skins son solo estéticas y todos juegan con el mismo ejército"* y el pilar 3, *"una ayuda al que pierde vale solo si da opciones, nunca estadísticas"*: el Catálogo no cambia por ninguna vía (ni por skin, cuenta o facción); lo único que mueve una cifra durante la partida son los bonos temporales de otras reglas (Grito de guerra, arquero sobre el muro), con **la misma regla para los dos jugadores, aunque no el mismo acceso**: el Grito sale del dado con tablas que dependen de cómo va la partida (Modificador, #11). Esa tensión con el pilar 3 es de #11, no del Catálogo. Se apoya también en el pilar 2: como ninguna pieza es suficiente sola (salvo las excepciones aprobadas de I1 e I4, que nunca alcanzan al Rey), la igualdad de ejércitos no vuelve la partida un empate sin gracia, sino un problema de combinar mejor que el rival.

**Lo que esta fantasía le exige al diseño:** un único Catálogo para todos los jugadores de una partida (sin cifras por facción, por skin ni por cuenta; los tres grupos del cuaderno están descartados); los cambios de balance no se aplican en silencio sino como una nueva versión con otra huella; y las skins (#37–#39) nunca tocan una cifra.

## Detailed Design

### Core Rules

1. **Qué es el Catálogo.** Una versión de balance (`versionBalance`) más **siete filas**, una por tipo de pieza. Es un conjunto cerrado: el validador exige exactamente los siete tipos, ni uno más ni uno menos.
2. **Tipos y orden canónico.** El ID del tipo es su lugar en la tabla; el orden es el ID ascendente.

   | ID | Tipo |
   |---|---|
   | 0 | Soldado |
   | 1 | Arquero |
   | 2 | Escudero |
   | 3 | Catapulta |
   | 4 | Jinete |
   | 5 | Demoledor |
   | 6 | Rey |

   Ese orden fija los IDs de pieza del despliegue inicial (Librería, regla 7) y el "primer tipo disponible" del recluta automático (Librería, regla 17). El Rey es el tipo 6, no se recluta y su tope de piezas vivas es 1; lo coloca el Tablero (#4).
3. **Campos de cada fila** (todos enteros de 32 bits con signo):

   | Campo | Qué es | Quién lo lee |
   |---|---|---|
   | `movimiento` | Casillas que puede moverse por turno | Movimiento (#6), vía `EffectiveStats` |
   | `alcanceMin`, `alcanceMax` | Distancias mínima y máxima de ataque. El sentido geométrico lo define Alcance (#7) | Alcance (#7) |
   | `ataque` | Daño que aporta al atacar | Resolución (#9) |
   | `escudo` | Daño que resta al total recibido, una vez | Resolución (#9) |
   | `vida` | Vida máxima y de arranque | Resolución (#9), Ficha (#15) |
   | `danoArea` | Daño en las 8 casillas alrededor del punto elegido; 0 si el tipo no tiene área | Resolución (#9) |
   | `topeVivas` | Máximo de piezas vivas de ese tipo por jugador | Reclutamiento (#10), Tablero (#4) |

4. **Valores vigentes** (tabla del concepto, prototipo v5 desde el 2026-11-24, decididos con las partidas v4; se calibran con partidas humanas, así que son provisionales hasta la compuerta de duración).

   | ID | Tipo | Mov. | Alc. mín. | Alc. máx. | Ataque | Escudo | Vida | Daño área | Tope |
   |---|---|---|---|---|---|---|---|---|---|
   | 0 | Soldado | 4 | 1 | 1 | 8 | 3 | 25 | 0 | 12 |
   | 1 | Arquero | 4 | 1 | 4 | 6 | 2 | 15 | 0 | 16 |
   | 2 | Escudero | 4 | 1 | 2 | 4 | 5 | 20 | 0 | 12 |
   | 3 | Catapulta | 2 | 2 | 6 | 10 | 2 | 15 | 6 | 8 |
   | 4 | Jinete | 6 | 1 | 1 | 8 | 3 | 20 | 0 | 8 |
   | 5 | Demoledor | 6 | 1 | 1 | 30 | 1 | 10 | 0 | 4 |
   | 6 | Rey | 6 | 1 | 1 | 8 | 5 | 30 | 0 | 1 |

   - La explosión del demoledor no usa `danoArea` (queda en 0). La regla de que daña las 8 vecinas con su `ataque` es de Resolución (#9), que la confirmó (regla 8, cierre de la tanda 3).
   - Cambios de la v5 (2026-11-24), decididos por el jugador después de las partidas v4 del 2026-11-21: alcance máximo del arquero 6 → 4, alcance máximo del escudero 1 → 2 (su cobertura sigue siendo a un aliado vecino, regla del Escudero), ataque de la catapulta 12 → 10, su daño de área 8 → 6 y su escudo 3 → 2, escudo del soldado 2 → 3.
   - Los topes suman 60 más el Rey, igual que las 61 piezas vivas de la Librería.
5. **Estático.** El Catálogo no cambia durante la partida. Los bonos de turno (Grito de guerra: +2 de movimiento a todas y +2 de ataque salvo al demoledor, nunca al `danoArea`; #11) viven en el estado y se combinan con `EffectiveStats(estado, pieza)`. El +2 de escudo y el +2 de alcance del arquero sobre el muro (v5, 2026-11-25: alcance 1–4 → 1–6) son del Castillo (#5), no del Catálogo.
   - **Divergencia con el prototipo v5 (2026-11-27, manda el GDD).** El código v5 suma el +2 del Grito también al área de la catapulta (6 → 8, `AreaValue`) y a la explosión del demoledor (30 → 32, `AttackValue`), y su README dice «con Grito de guerra: 12 y 8». Los GDD no lo hacen (Modificador, #11, regla 7, decisión del jugador del 2026-11-25), y el jugador confirmó el 2026-11-27 que manda el GDD. Las partidas v4 que calibraron estos valores se jugaron con el bono sobre el área y la explosión, así que hay que corregir el prototipo antes de las partidas de la compuerta de duración. Hasta entonces, los valores vigentes **no están calibrados bajo la regla del GDD**.
6. **Formato válido.** Cualquier violación da `ConfigInvalida`, y el error **lleva el código de la regla que falló** (F1 a F9), para que los tests distingan, por ejemplo, un rechazo por F2 de uno por F8 (pedido a la Librería, 2026-11-27). **Dónde se valida:** F1 a F8 son de forma del Catálogo y las valida `PrepararContexto(catálogo)` (Librería, regla 2). F9 cruza el Catálogo con la config (`bono_grito_movimiento`), que `PrepararContexto` no recibe: la valida `CrearPartida` (Librería, regla 2), junto a la cota de la pila con el bono (decisión del jugador, 2026-11-27). **Orden (decisión del jugador, 2026-11-27):** `PrepararContexto` evalúa F1 → F8 en ese orden y devuelve el código de la **primera** que falla. El orden de F9 frente a los demás rechazos de `CrearPartida` es de la Librería (regla 2: después de los bonos del Grito y antes de T4 → T5 → T6); los AC de F9 usan casos que solo violan F9.
   - **F1:** exactamente 7 filas, con IDs de 0 a 6 en orden, sin repetir.
   - **F2:** `movimiento`, `ataque`, `vida` y `alcanceMax` entre 1 y 1.000.
   - **F3:** `alcanceMin` entre 1 y `alcanceMax`.
   - **F4:** `escudo` y `danoArea` entre 0 y 1.000.
   - **F5:** `topeVivas` entre 1 y 61, y el del Rey (ID 6) es exactamente 1. El máximo efectivo de un tipo es 55: el Rey vale 1 y los otros cinco tipos al menos 1, así que F6 corta antes que el 61.
   - **F6:** la suma de los 7 `topeVivas` es como mucho 61.
   - **F7:** `versionBalance` entre 1 y 1.000.
   - **F8:** `pila_max` = Σ `topeVivas_t` × `movimiento_t` es como mucho 256 (decisión del 2026-11-20; fórmula y criterio en la Máquina de turno, D4 y AC-27). Hoy 254. La cota con el bono de movimiento la verifica la Librería (regla 2; Modificador, #11).
   - **F9 (2026-11-27; se valida en `CrearPartida`):** para cada tipo, `movimiento_t + bono_grito_movimiento ≤ techo_movimiento` (12, Tablero, #4). `bono_grito_movimiento` es el campo de la config (hoy 2; Modificador, #11). Hoy el máximo es 6 + 2 = 8. Sin esta regla, `techo_movimiento` sería solo un oráculo de test (Tablero, Formulas) y un Catálogo que cumpla F8 podría dejar que una pieza llegue al castillo rival el turno en que aparece.
   - **Techos que no se alcanzan.** El 1.000 de `movimiento` en F2 no se alcanza: F8 lo corta antes en cualquier fila (con el resto vigente, el Rey acepta hasta 8 y el soldado hasta 4), y F9 lo corta en 12 − `bono_grito_movimiento` (10 con el bono vigente). El 61 de F5 tampoco: F6 deja como máximo 55 por tipo. Con el despliegue inicial (Tablero, regla 6) los máximos reales son 49 para soldado, arquero o escudero (61 − 4 − 4 − 1 − 1 − 1 − 1) y 46 para catapulta, jinete o demoledor (61 − 4 − 4 − 4 − 1 − 1 − 1), y el `topeTotal` mínimo es 16 (4 + 4 + 4 + 1 + 1 + 1 + 1).
   - Estas son reglas de forma, no de balance. Los rangos sanos de cada cifra van en Tuning Knobs.
7. **Identidad.** La huella del Catálogo la calcula `PrepararContexto`; el Catálogo no la guarda. Orden canónico de los datos para la huella: `versionBalance`, y después, por ID ascendente: `movimiento`, `alcanceMin`, `alcanceMax`, `ataque`, `escudo`, `vida`, `danoArea`, `topeVivas`. El formato exacto en bytes lo fija el ADR de la Librería.
8. **Un solo Catálogo por partida,** igual para los dos jugadores. No existe una fila por jugador, facción, skin ni cuenta (Player Fantasy).
9. **Sin texto, arte ni sonido.** Los nombres de las piezas los pone Localización (#14); las imágenes, Render (#18). Ambos buscan por ID de tipo.
10. **Todo cambio de una cifra sube `versionBalance`.** Un test del repositorio compara el archivo con un registro de pares (versión → huella) y pasa solo si el par (versión, huella) del archivo está registrado: falla si el contenido cambió con la misma versión, y también si se subió la versión sin registrar el par nuevo.

### States and Transitions

El Catálogo no tiene estados de juego. Su ciclo de vida:

| Estado | Qué es | Entra por | Sale por |
|---|---|---|---|
| Archivo | Datos externos al código, versionados en el repositorio | Edición | Lectura por el cliente o el servidor |
| Leído | Filas en memoria, sin validar | Lector (fuera del núcleo) | `PrepararContexto` |
| Validado y fijado | Contexto con huella, usado por toda la partida | `PrepararContexto` aceptado | Fin de la partida |
| Rechazado | Falló alguna regla F1–F8 | `PrepararContexto` devuelve `ConfigInvalida(Fn)`, con la primera que falla | Corregir el archivo |

F9 no rechaza el Catálogo sino la combinación con una config: `CrearPartida` devuelve `ConfigInvalida(F9)` y el contexto sigue validado para otra config.

Un archivo que no se puede leer no llega a ser un Catálogo: el cliente no ofrece crear partida y el núcleo no se entera. El formato del archivo y su carga son del ADR.

### Interactions with Other Systems

| Sistema | Qué recibe del Catálogo | Dueño de la interfaz |
|---|---|---|
| #1 Librería (indirecto, esquema) | Las filas ya cargadas en la config; esta sección define el formato | Librería (valida y calcula la huella) |
| #3 Máquina de turno | `movimiento` y `topeVivas` para el invariante de la pila de deshacer, y el orden de tipos para el recluta automático | Máquina de turno |
| #4 Tablero | Tipos, `topeVivas` y orden para IDs; valida que el ejército inicial (4 soldados, 4 arqueros, 4 escuderos y Rey) cabe en los topes | Tablero |
| #5 Castillo | `escudo` y `alcanceMax` del arquero y el ID del tipo arquero (el +2 de escudo y el +2 de alcance en el muro son suyos; la `vida` del arquero es de Resolución) | Castillo |
| #6 Movimiento | `movimiento`, vía `EffectiveStats` | Movimiento |
| #7 Alcance | `alcanceMin`, `alcanceMax` (sentido geométrico: su F1), `danoArea` (modo de ataque) y los IDs 2 y 5. Los alcances del demoledor son inertes: explota sobre su casilla | Alcance |
| #9 Resolución | `ataque`, `escudo`, `vida`, `danoArea` | Resolución |
| #8 Cobertura | ID del escudero; `escudo` y `vida` solo en su oráculo C2 | Cobertura |
| #10 Reclutamiento | `topeVivas` y el orden de tipos (recluta automático); las llamadas por tipo son suyas | Reclutamiento |
| #11 Modificador | `ataque` y `movimiento` base | Modificador |
| #15 Ficha y HUD | Cifras base y efectivas para mostrar | Ficha y HUD |
| #18 Render, #37 Skins | Solo el ID de tipo (para elegir el arte). Las skins nunca leen ni cambian una cifra | Render |
| #14 Localización | Solo el ID de tipo (nombre de pieza) | Localización |
| #19 Métricas, #32 Analítica | `versionBalance` y huella de cada partida | Métricas |
| #26 Registro, #28 Servidor | El Registro guarda la tabla en la config de cada partida (así el Catálogo no conserva versiones viejas); el Servidor carga el mismo archivo y compara huellas | Registro y Servidor (provisional hasta que existan) |

**Provisional:** #26 todavía no tiene GDD; su contrato es lo que este documento espera de él. El de #9 lo confirmó su GDD (tanda 3, 2026-11-25).

**Observaciones que pasan a Open Questions:** el demoledor mata solo a cualquier pieza que no sea el Rey, y con una explosión puede matar a varias (I4). El Grito (+2 de ataque) no suma al `danoArea` ni a la explosión; sí al impacto de la catapulta (Modificador, #11, regla 7).

## Formulas

El Catálogo no calcula nada durante la partida: el daño, el escudo y la resolución son de Resolución (#9). Define tres fórmulas de verificación sobre sus propios datos (`topeTotal`, `piezasParaMatar` y `piezasParaMatarEf`) y cuatro chequeos de balance (I1 a I4).

El **topeTotal** se define así:

`topeTotal = Σ topeVivas_t, para t = 0 a 6`

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| `topeVivas_t` | int | 1–61 (el del Rey, exactamente 1) | data file (Catálogo) | Tope de piezas vivas por jugador del tipo `t` |
| `t` | int | 0–6 | constant (los 7 tipos) | ID del tipo de pieza |

**Output Range:** 7 a 61. La regla F6 exige `topeTotal ≤ 61` (las 61 piezas vivas por jugador de la Librería). Con todos los topes en el mínimo da 7. Valores vigentes: 12 + 16 + 12 + 8 + 8 + 4 + 1 = **61**, justo en el tope. Subir cualquier tope exige bajar otro.
**Example:** subir el tope de los demoledores de 4 a 5 da 62: el Catálogo se rechaza con `ConfigInvalida` (F6).

---

El **piezasParaMatar** se define así:

`piezasParaMatar(a, t) = ceil((escudo_t + vida_t) / ataque_a) = (escudo_t + vida_t + ataque_a − 1) / ataque_a`

con división entera hacia abajo (todos los valores son no negativos, como exige la Librería). Es la consecuencia de la regla de Resolución (#9): un objetivo muere cuando `Σ ataque − escudo ≥ vida`. **Es un oráculo de tests de balance: no entra al núcleo ni a la vista previa.** Así no existe un segundo cálculo del daño.

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| `ataque_a` | int | 1–1.000 | data file (Catálogo) | Ataque del tipo atacante `a` |
| `escudo_t` | int | 0–1.000 | data file (Catálogo) | Escudo del tipo objetivo `t` |
| `vida_t` | int | 1–1.000 | data file (Catálogo) | Vida del tipo objetivo `t` |

**Output Range:** 1 a 2.000. El mínimo es 1 (escudo 0, vida 1); el máximo es 2.000 (escudo 1.000, vida 1.000, ataque 1), sin riesgo de desborde de 32 bits. `ataque = 0` daría división por cero, y por eso la regla F2 exige `ataque ≥ 1`.
**Example:** soldado contra escudero: `(5 + 20 + 8 − 1) / 8 = 32 / 8 = 4`. Cuatro soldados lo matan (32 − 5 = 27 ≥ 20). Tres no: 24 − 5 = 19 < 20.

**Matriz con los valores vigentes** (entre paréntesis, escudo + vida del objetivo):

| Atacante \ Objetivo | Soldado (28) | Arquero (17) | Escudero (25) | Catapulta (17) | Jinete (23) | Demoledor (11) | Rey (35) |
|---|---|---|---|---|---|---|---|
| Soldado (8) | 4 | 3 | 4 | 3 | 3 | 2 | 5 |
| Arquero (6) | 5 | 3 | 5 | 3 | 4 | 2 | 6 |
| Escudero (4) | 7 | 5 | 7 | 5 | 6 | 3 | 9 |
| Catapulta (10) | 3 | 2 | 3 | 2 | 3 | 2 | 4 |
| Jinete (8) | 4 | 3 | 4 | 3 | 3 | 2 | 5 |
| Demoledor (30) | **1** | **1** | **1** | **1** | **1** | **1** | 2 |
| Rey (8) | 4 | 3 | 4 | 3 | 3 | 2 | 5 |

---

El **piezasParaMatarEf** (con el Grito) se define así (decisión del jugador, 2026-11-27, W-12):

`piezasParaMatarEf(a, t) = ceil((escudo_t + vida_t) / (ataque_a + bonoGrito_a))`

con `bonoGrito_a` = `bono_grito_ataque` (campo de la config, hoy 2; Modificador, #11) para todo tipo menos el demoledor, y 0 para el demoledor (su explosión no recibe el bono, #11 regla 7). Misma división entera que `piezasParaMatar`; es otro oráculo de tests y tampoco entra al núcleo.

**Variables:** las de `piezasParaMatar`, más `bonoGrito_a` (int, 0–10, config, Modificador #11; la Librería acota el rango de dominio).
**Output Range:** 1 a 2.000, igual que `piezasParaMatar`; el bono solo puede bajar el resultado.
**Example:** catapulta contra demoledor con Grito: `(1 + 10 + 12 − 1) / 12 = 22 / 12 = 1`. Sin Grito da 2. Con los valores vigentes es el único par que cambia a 1 con el bono.

---

**Chequeos de balance sobre el Catálogo** (los hace un test; el pilar 2 es *"ninguna pieza gana sola"*). I1 e I2 se evalúan **dos veces**: con `piezasParaMatar` (sin bono) y con `piezasParaMatarEf` (con el Grito). Deben pasar las dos (decisión del jugador, 2026-11-27: *"ninguna pieza gana sola"* no tiene una cláusula "salvo durante el Grito").

- **I1, excepciones aprobadas (2026-11-19).** Los únicos pares con valor 1 pueden ser estos dos. (a) **Demoledor contra cualquier tipo menos el Rey.** A cambio muere al explotar, hay 4 como máximo y daña a los aliados vecinos. (b) **Catapulta contra demoledor.** Sin Grito este par ya no se da con los valores v5 (catapulta 10 contra escudo + vida 11). Con Grito sí (12 ≥ 11): es la excepción ya aprobada, durante un turno. **Contrajuego del demoledor (v5, reescrito el 2026-11-27 tras CD-GDD-ALIGN): es posicional, no un conteo de golpes.** El demoledor amenaza las piezas a `movimiento + 1` pasos en cruz de su casilla en línea recta y a `movimiento + 2` en diagonal (un vecino en diagonal de la casilla final está a 2 pasos en cruz): 7 y 8; 9 y 10 si al rival le sale el Grito, que el defensor no puede prever porque la tirada es al empezar el turno rival (fe de erratas del cierre de la tanda B, 2026-11-28), explota el mismo turno en que llega y muere al explotar. Las distancias son las de Movimiento (#6) y Alcance (#7). (1) **Separación**, el contrajuego principal: no dejar piezas valiosas vecinas entre sí dentro de ese radio, así cada explosión cambia una pieza por una. (2) **Intercepción:** lo matan dos piezas cualesquiera (escudo + vida 11; por ejemplo dos arqueros, 6 + 6 − 1 = 11 ≥ 10), o la catapulta sola con el Grito, si llegan a pegarle en su turno: su alcance de respuesta es `movimiento + alcanceMax` (arquero y catapulta 8; jinete, demoledor y Rey 7; escudero 6; soldado 5). **Dónde no hay respuesta, dicho con honestidad:** un demoledor a 9 casillas o más de todas tus piezas no lo alcanza nadie; a 8 solo llegan arqueros y catapulta (hacen falta dos, o la catapulta con tu Grito); y con el Grito rival igual explota en el turno siguiente; y si lo cubre un escudero, los golpes que le iban van al escudero (Cobertura, #8, regla 6), así que interceptarlo cuesta además romper la cobertura. En esos casos solo vale la separación. La justificación original (la catapulta sola a distancia 6) ya no vale sin Grito. **Contra los tramos de muro** (Castillo y muros, F3), el demoledor (ataque 30) rompe un tramo de `vida_tramo` 30 de un golpe y, pegado a un tramo recto, hasta 3 a la vez desde afuera, y hasta 5 desde una esquina del interior después de una brecha (Resolución, #9, F6): excepción aprobada el 2026-11-25, confirmada por Resolución (#9). Ninguna otra pieza rompe un tramo de un golpe, ni siquiera con Grito (la catapulta pega 12): `EvaluarI` lo comprueba como `ataque_ef_a < vida_tramo` (config, #5) para todo `a` distinto del demoledor (AC-19). Cualquier otro par en 1, con bono o sin él, hace fallar el test hasta que se apruebe.
- **I2, el Rey (regla dura, sin excepciones).** `piezasParaMatar(a, Rey) ≥ 2` y `piezasParaMatarEf(a, Rey) ≥ 2` para todo atacante `a`. Con las cifras vigentes el mínimo es 2 (el demoledor, en los dos casos): ninguna pieza gana la partida sola. Con el Grito, la catapulta (12) necesita 3 contra el Rey (35).
- **I3, el área no mata sola.** Para todo tipo con `danoArea > 0` y todo objetivo `t`, `danoArea − escudo_t < vida_t`. El área ablanda y no reemplaza a los demás (*"las piezas de área habilitan combos, no los reemplazan"*, pilar 2). El Grito no suma al `danoArea` (#11 regla 7), así que I3 se evalúa una sola vez. Con la catapulta (6) el daño neto máximo es 5, contra 10 de vida mínima del demoledor. I3 mira **un área sola**: dos catapultas sobre el mismo objetivo son un combo, y eso es lo que el pilar 2 busca.
- **I4, la explosión (decisión del jugador, 2026-11-27).** El demoledor explota y aplica su `ataque` a cada una de las 8 casillas vecinas, aliados y muros incluidos (Resolución, #9). No usa `danoArea`, así que I3 no la ve. **Excepción aprobada (CD-GDD-ALIGN, 2026-11-27):** una sola explosión puede matar a varias piezas que no sean el Rey, **hasta las 8 vecinas, sin cota** (decisión del jugador, 2026-11-27; hoy a cualquiera de los seis tipos: 30 − escudo ≥ vida en todos). **Reemplaza la aprobación del 2026-11-20**, que hablaba de "mata solo a una pieza". Es lo que define al demoledor: una pieza sacrificable que castiga amontonarse, y su contrajuego es la separación (I1). Las muertes por explosión se miden en el MVP (Métricas, #19; Open Questions). **Regla dura:** `ataque_5 − escudo_Rey < vida_Rey` (hoy 30 − 5 = 25 < 30): la explosión sola nunca mata al Rey. Da el mismo techo que I2 (ataque del demoledor ≤ 34). I4 vale **con cobertura y sin ella**: la cobertura solo desvía el daño al escudero (#8), nunca lo suma sobre el Rey. La explosión no recibe el Grito, así que se evalúa una sola vez.

**Bucles de retroalimentación:** ninguno. El Catálogo es una tabla estática.

## Edge Cases

**Archivo y formato**

- **If el archivo del Catálogo no se puede leer o le falta un campo**: no se crea ningún Catálogo. El cliente no ofrece crear partida y muestra un aviso neutro; el servidor no crea partidas y registra el error. El núcleo no se entera (el lector está fuera del núcleo).
- **If el Catálogo no tiene exactamente 7 filas, o repite IDs, o los trae fuera de orden (0 a 6)**: `PrepararContexto` devuelve `ConfigInvalida` (F1) y no se crea partida.
- **If un valor está fuera de rango** (por ejemplo `ataque` 0, `vida` 0, `alcanceMin` mayor que `alcanceMax`, `topeVivas` del Rey distinto de 1, `versionBalance` 0): `PrepararContexto` devuelve `ConfigInvalida` con el código de la regla (F2 a F5, F7). Se rechaza todo el Catálogo; no se corrige ni se descarta una fila en silencio.
- **If un `movimiento` con el Grito pasa de 12**: `CrearPartida` devuelve `ConfigInvalida(F9)` y no se crea la partida; el contexto sigue validado (regla 6).
- **If un Catálogo viola varias reglas F a la vez** (por ejemplo, un `movimiento` de 1.001 viola F2 y F8): se devuelve el código de la primera en el orden F1 → F8 (regla 6). Un solo rechazo, determinista.
- **If la suma de los `topeVivas` es mayor que 61**: `ConfigInvalida` (F6). Es la misma cuenta que protege contra el desborde de la Librería.
- **If el formato es válido pero falla un chequeo I1 a I4** (por ejemplo un Rey que cae de una sola pieza): el núcleo carga el Catálogo igual, porque los chequeos I son de test y no de runtime. Lo frena el test de integración del repositorio antes de publicar; un build de desarrollo local sí podría jugar con esa tabla.

**Versión, huella y compatibilidad**

- **If el cliente y el servidor tienen Catálogos con huella distinta**: el servidor rechaza al cliente con `VersionNoSoportada` ("hay que actualizar"). Nunca se juega con dos tablas.
- **If una partida cargada se aplica con el contexto de otro Catálogo**: `VersionNoSoportada` (Librería, AC-61).
- **If se cambia una cifra sin subir `versionBalance`**: el test de la regla 10 falla. **If** se sube `versionBalance` sin tocar ninguna cifra: se permite y da una huella nueva, porque la versión forma parte de los datos de la huella.
- **If una repetición o una partida guardada corresponde a un balance anterior**: se reproduce con la tabla guardada en su propia config (Registro, #26); el Catálogo actual no interviene. *Provisional:* si #26 guardara solo la huella, habría que conservar las versiones viejas del archivo.
- **If se edita el archivo mientras hay una partida en curso**: se ignora. El contexto se fijó al crearla; el cambio rige desde la partida siguiente.

**Cifras y reglas**

- **If el `ataque` de un atacante es menor o igual que el `escudo` del objetivo**: el daño neto de ese ataque suelto es 0 (la resta negativa se pone en 0, Librería), pero sumado a otros ataques sobre el mismo objetivo cuenta completo en la suma. Es válido.
- **If un bono de turno se suma a una cifra que está en su máximo (1.000)**: la cifra efectiva puede llegar a 1.010 (bono de 0 a 10). La Librería ya cuenta con ese tope en su análisis de desborde.
- **If el ejército inicial no cabe en los topes** (por ejemplo `topeVivas` de soldados menor que 4): el Tablero (#4) devuelve `ConfigInvalida` al crear la partida. (Tablero, regla 6.)
- **If una skin o un texto usa un ID de tipo fuera de 0 a 6**: la skin es inválida; la rechaza el validador del Kit (#38) o Render (#18). El Catálogo no tiene fila para ese ID.
- **If dos tipos tienen las mismas cifras**: se permite; no hay regla de unicidad entre tipos.
- **If un tipo distinto de la catapulta tiene `danoArea` mayor que 0**: Resolución (#9) le aplica el área igual que a la catapulta. Las cifras se leen siempre de forma genérica; solo los comportamientos propios de un tipo (el jinete pasa por encima de enemigos, el arquero sube al muro, el escudero cubre, el demoledor explota) dependen del ID. El chequeo I3 se evalúa para todo tipo con `danoArea` mayor que 0. La explosión es un comportamiento del tipo 5 y la cubre I4, no I3.

## Dependencies

**Depende de:** ningún sistema. El Catálogo es una tabla de datos sin lógica y no necesita nada de otro GDD.

**Topes que este GDD adopta como propios.** Dos topes que la Librería (#1) tenía como PROVISIONALES son del Catálogo (la Librería los apunta acá desde el 2026-11-25):

| Tope | Valor | Dónde se aplica aquí | Efecto de cambiarlo |
|---|---|---|---|
| Estadísticas | 0 a 1.000 | F2 y F4 | Revisar el análisis de desborde de la Librería (resolución completa menor que 25.000.000) y subir la versión de reglas |
| Piezas vivas por jugador | 61 | F5 y F6 (`topeTotal`) | Igual que arriba; además afecta el estado de la Librería |

**Restricción de presentación (pedida por la Ficha, #15, F1 y AC-4; cierre de la tanda 3).** El tope del esquema sigue en 1.000, pero el Catálogo **que se publica** mantiene cada cifra que muestra la ficha, sumado su bono máximo (Grito +2, muro +2), en **99 o menos**: con tres cifras la ficha compacta no entra en la casilla. Un balance que la rompa no es inválido (la ficha lo muestra igual, sin truncar), pero falla el test AC-4 de la Ficha y obliga a rediseñarla. **Segunda restricción (pedida por la Vista previa, #16, F2 y AC-11; cierre de la tanda 4):** `vida_max + escudo_ef_max ≤ 100` para cada tipo, así el golpe que hace falta para matar se muestra con dos cifras en la placa; hoy el máximo es el Rey, 30 + 5 = 35 (el +2 del muro es solo del arquero: 15 + 4 = 19; forma por tipo, decisión del 2026-11-26).

**Restricción hacia el Modificador (#11, 2026-11-27).** Con el Catálogo vigente, `bono_grito_ataque ≤ 2`: el techo es 11 − 1 − 8 (el ataque de los comunes más alto) = 2. Con 3, soldado, jinete y Rey pegan 11 y matan solos al demoledor, y se rompe I1 (AC-39). La Librería acepta el bono de 0 a 10 por dominio, así que este techo es de balance y lo frena el test, no `CrearPartida`.

Esto evita un ciclo: la Librería depende del esquema del Catálogo, y el Catálogo no depende de la Librería. Los demás topes de la Librería no son del Catálogo: el tablero 24 × 15 es del Tablero (#4), la pila de 256 de la Máquina de turno (#3), y C_MAX y el rango de dominio del bono de turno (0 a 10) siguen en la Librería. El valor del bono (+2 del Grito) es del Modificador (#11).

**Dependen de este sistema:**

| Sistema | Tipo | Qué usa | Estado del GDD |
|---|---|---|---|
| #1 Librería de reglas | Blanda | El esquema de datos; recibe las filas en la config, las valida y calcula la huella | Existe (en revisión) y ya nombra al Catálogo |
| #3 Máquina de turno | Blanda | `movimiento` y `topeVivas` (F8, invariante de la pila) y orden de tipos | Existe y ya nombra al Catálogo |
| #4 Tablero y despliegue | Dura | Tipos, `topeVivas`, orden para IDs; valida que el ejército inicial cabe en los topes; `movimiento` para sus oráculos | Existe y ya nombra al Catálogo |
| #5 Castillo y muros | Dura | `escudo` y `alcanceMax` del arquero y el ID del tipo arquero | Existe y ya nombra al Catálogo |
| #6 Movimiento | Dura | `movimiento`, vía `EffectiveStats`, y los IDs de arquero, jinete y Rey | Existe y ya nombra al Catálogo |
| #7 Alcance y asignación | Dura | `alcanceMin`, `alcanceMax`, `danoArea` y los IDs 2 y 5 | Existe y ya nombra al Catálogo |
| #9 Resolución simultánea | Dura | `ataque`, `escudo`, `vida`, `danoArea` | Existe y ya nombra al Catálogo |
| #10 Reclutamiento | Dura | `topeVivas`, el orden de tipos y la vida | Existe y ya nombra al Catálogo |
| #15 Ficha de pieza y HUD | Dura | Cifras base y efectivas para mostrar; pide que toda cifra con su bono máximo sea ≤ 99 (abajo) | Existe y ya nombra al Catálogo |
| #18 Render, #37 Skins | Blanda | Solo el ID de tipo | #18 existe (tanda 4); la carga de Render rechaza el tipo 7 |
| #14 Localización | Blanda | Solo el ID de tipo (nombre de pieza, singular y plural) | Existe y ya nombra al Catálogo |
| #16 Vista previa de ataques | Blanda | Cifras efectivas para la placa; pide `vida_t + escudo_ef_max_t ≤ 100` por tipo (restricción de arriba; #16 F2 y AC-11) | Existe y ya nombra al Catálogo |
| #17 Interfaz del Modificador | Blanda | Orden de tipos y `topeVivas` (orden de las tarjetas de reclutar y tope de vivas del motivo) | Existe; lo nombra en Cross-References (l.606), no en su tabla Dependencies |
| #19 Métricas | Dura | `versionBalance` y huella en el encabezado de cada registro (Métricas, regla 3). Dura, como la declaran #19 y el índice (W-21, 2026-11-27) | Existe (tanda 4) |
| #28 Servidor | Blanda | `versionBalance` y huella; carga el mismo archivo | Pendiente |
| #26 Registro | Blanda | Guarda la tabla en la config de cada partida | Pendiente |
| #8 Cobertura | Referencia | ID del Escudero (2); `escudo` y `vida` solo en su oráculo C2 | Existe y ya nombra al Catálogo |
| #11 Modificador | Blanda | `ataque` y `movimiento` base; el bono se suma en `EffectiveStats` | Existe y ya nombra al Catálogo |

**Bidireccionalidad.** La Librería (#1), la Máquina de turno (#3) y el Tablero (#4) tienen GDD y ya nombran al Catálogo. Cada uno de los otros, al escribirse, tiene que nombrar al Catálogo en su sección Dependencies. Queda marcado para `/design-review` y `/review-all-gdds`.

**Cambios fuera de este archivo:** la Librería ya apunta al Catálogo como dueño de las estadísticas y de las 61 piezas, y el índice ya tiene las aristas #8, #14, #19, #26 y #28.

## Tuning Knobs

Todas las cifras están en el archivo de datos del Catálogo; ninguna está en el código. Los techos de la tabla valen **con las demás cifras vigentes y con el Grito vigente (+2 de ataque, salvo el demoledor)** y se derivan de los chequeos I1 a I4 (Formulas), evaluados con el bono y sin él (decisión del 2026-11-27, W-12). Los pisos **no se pueden fundamentar todavía**: dependen de la duración de partida, que se mide con las Métricas locales (#19) en el MVP.

| Perilla | Vigente | Techo seguro | Piso | Si es demasiado alto | Si es demasiado bajo |
|---|---|---|---|---|---|
| `ataque` de soldado, arquero, escudero, jinete y Rey | 8 / 6 / 4 / 8 / 8 | **8** (el vigente de soldado, jinete y Rey está en el techo, sin margen) | ≥ 1 (F2); piso útil sin medir | Con 9 o más, con el Grito pega 11 y mata sola al demoledor (escudo + vida = 11): nuevo "gana sola" que rompe I1. Sin Grito fallaría con 11 o más | Hacen falta más piezas por golpe: partida más larga (pilar 4) |
| `ataque` de la catapulta | 10 | **14** | ≥ 1 (F2); sin medir | Con 9 a 14, con el Grito (11 a 16) mata sola al demoledor, y con 11 a 14 también sin Grito (excepción aprobada de I1). Con 15 o más, con el Grito pega 17 y mata sola también al arquero y a la catapulta (escudo + vida = 17): rompe I1 | Con 10 (v5) solo mata sola al demoledor con el Grito; más abajo se achica su rol de ablandar |
| `ataque` del demoledor | 30 | 34 (no recibe el Grito) | ≥ 1 (F2); sin medir | Con 35 o más mata solo al Rey, de un golpe o con la explosión (escudo + vida = 35): rompe I2 e I4, los chequeos sin excepciones | Deja de ser una amenaza que mata sola; baja el costo de ignorarlo |
| `escudo + vida` de cada tipo (no demoledor, no Rey) | 17 a 28 | sin techo de balance; techo de presentación: `vida + escudo` ≤ 100 − bono de escudo (98 en el arquero; #16 F2) y cada cifra ≤ 99 − su bono (#15) | **13** | Las piezas se vuelven demasiado resistentes para combinar: partida más larga | Por debajo de 13 la catapulta con el Grito (12) las mata sola: rompe I1 |
| `escudo + vida` del demoledor | 11 | ídem (techo de presentación) | **11** (el vigente, sin margen) | Con 13 o más ya no lo mata sola ni la catapulta con el Grito: la excepción (b) de I1 deja de darse y el contrajuego queda en los arqueros | Por debajo de 11 lo matan solas, con el Grito, piezas de ataque 8 (soldado, jinete y Rey pegan 10): rompe I1 |
| `escudo + vida` del Rey | 35 | ídem (techo de presentación) | 31 | Partida más larga: el Rey tarda más en caer | Con 30 o menos lo mata sola el demoledor, de un golpe o con la explosión: rompe I2 e I4 |
| `escudo` de un tipo | 1 a 5 | sin techo duro | 0 (F4) | Con `escudo` ≥ `ataque` de un atacante, ese atacante solo no hace daño (válido, empuja a combinar: pilar 2); con escudos de 30 o más ninguna pieza suelta hace daño | Sube el valor de las piezas sueltas y debilita el pilar 2 |
| `danoArea` | 6 (catapulta); 0 el resto | 10 | 0 (F4); el piso útil se calibra | Con 11 o más mata sola al demoledor (vida 10, escudo 1): rompe I3 | Con 0 la catapulta pierde su rol de ablandar |
| `movimiento` | 4 / 4 / 4 / 2 / 6 / 6 / 6 | 10 de base con el Grito de +2 (F9: 12 de movimiento efectivo, `techo_movimiento` del Tablero); F8 corta antes: hoy quedan 2 de margen en la pila, así que subir 1 al soldado ya la rompe | ≥ 1 (F2) | Con 13 o más, una pieza llega al castillo rival el mismo turno en que aparece | Cuesta llegar a atacar: partida más larga |
| `alcanceMax` | 1 / 4 / 2 / 6 / 1 / 1 / 1 | no verificable: el costo de la vista previa crece con el área del círculo; lo mide la Librería (presupuesto 5 ms) | ≥ `alcanceMin` (F3) | Más objetivos posibles por pieza: la vista previa pesa más y el tablero se vuelve ilegible | Menos objetivos posibles |
| `alcanceMin` | 1 (todos menos la catapulta: 2) | ≤ `alcanceMax` (F3) | 1 (F3) | Agranda la zona ciega alrededor de la pieza (su geometría la define Alcance, #7) | Con 1 la pieza ataca también a las 8 casillas vecinas |
| `topeVivas` | 12 / 16 / 12 / 8 / 8 / 4 / 1 | La suma no puede pasar de 61 (F6); la suma vigente ya es 61 | ≥ 1 (F5); soldados, arqueros y escuderos ≥ 4 (despliegue inicial) | Subir uno exige bajar otro o cambiar el tope de 61 (revisar el desborde de la Librería) | Un tope menor que 4 en esos tres tipos hace inválido el despliegue |

**Perillas que interactúan:**

- `ataque` del demoledor y `escudo + vida` del Rey (I2 e I4): hay que mirar los dos juntos.
- Los `ataque` y `bono_grito_ataque` (Modificador, #11): subir el bono del Grito baja todos los techos de ataque (salvo el del demoledor) y sube los pisos de `escudo + vida`. Con el bono en 3, el techo de los comunes pasa a 7 y el soldado vigente (8) rompe I1 (restricción hacia #11 en Dependencies; AC-39).
- `ataque` de la catapulta y el piso de `escudo + vida` de los comunes (I1): el piso es el ataque efectivo de la catapulta + 1 (hoy 10 + 2 + 1 = 13; con la catapulta en su techo 14, el piso sube a 17). Subir la catapulta sube el piso de todos.
- `danoArea` y `escudo + vida` del demoledor (I3): el demoledor es el que fija el techo del área de todos los tipos.
- `escudo + vida` del demoledor y los `ataque` de los demás (I1): su valor decide cuáles piezas lo matan solas.
- `topeVivas` de catapulta y demoledor, y las llamadas del Cuerno de guerra: con 4 llamadas de 2 catapultas y de 1 demoledor, los topes 8 y 4 se llenan justo. Con 3 llamadas de 4 jinetes (12 en total) el tope de 8 llega antes: con 5 o más jinetes vivos la llamada queda bloqueada, todo o nada (Reclutamiento, regla 4). Si Reclutamiento sube llamadas sin subir el tope, las llamadas sobrantes no reclutan.

**Perillas que son de otro GDD (no se duplican acá):** el +2 del Grito de guerra (Modificador, #11), el +2 de escudo y el +2 de alcance del arquero sobre el muro y la vida de los tramos (Castillo, #5), las llamadas por tipo (Reclutamiento, #10), el tamaño del tablero (Tablero, #4).

**No es perilla de balance:** `versionBalance`. Es contabilidad: sube con cualquier cambio de cifra (regla 10).

**Techos y pisos:** los techos salen de I1 a I4 (con el Grito y sin él) y se verifican con el test (AC-22); los pisos y los límites marcados "no verificable" quedan como Open Questions para calibrar con partidas humanas.

## Visual/Audio Requirements

No aplica: el Catálogo es un archivo de datos sin imagen ni sonido propios. El arte de cada tipo lo define Render (#18) y su formato de skin; el sonido, Audio (#22). Ambos buscan por ID de tipo (0 a 6).

## Game Feel

No aplica: el Catálogo no responde a la entrada del jugador. La sensación de peso de cada pieza (golpe combinado, pilar 2) es de Render (#18) y Audio (#22). Lo único que el Catálogo aporta al *feel* son las cifras: se leen en la ficha (#15) y se confirman en la vista previa (#16).

## UI Requirements

Sin pantalla propia. Las cifras de cada tipo se muestran en la ficha de pieza y en la vista de estadísticas del tutorial (#15, #24), que leen `EffectiveStats`. Requisito para esos GDD: mostrar siempre la cifra *efectiva* y, cuando difiere de la base, también la base. Los nombres de los tipos salen de Localización (#14), por ID.

## Cross-References

| Documento | Qué se referencia | Cómo |
|---|---|---|
| `design/gdd/libreria-de-reglas.md` | Regla 2 (`PrepararContexto`, `ConfigInvalida`), regla 7 (IDs por orden de tipos), regla 10 (`EffectiveStats`), regla 12 (huella y versión), tabla de dominios | Este GDD define el formato que la Librería valida y adopta sus topes 0–1.000 y 61 |
| `design/gdd/game-concept.md`, "Reglas vigentes" | Estadísticas, catapulta 10/6, demoledor, límites de reclutas | Fuente de la tabla de valores vigentes |
| `design/gdd/game-concept.md`, pilares y anti-pilares | Pilares 1, 2 y 3; "NOT pagar para ganar" | Base de Player Fantasy y de los chequeos I1 a I4 |
| `design/reglas-cuaderno.md` | Tabla de estadísticas y límites por jugador | Origen de los valores; el concepto manda donde difieren |
| `design/gdd/systems-index.md` | Sistema #2 y sus dependientes | El índice ya tiene las aristas #8, #14, #19, #26 y #28 (Dependencies); faltan #2 en la fila 16 (W-21) |
| `design/registry/entities.yaml` | Las 7 piezas, `topeTotal`, `piezasParaMatar`, `vida_escudo_max` y los topes del esquema | Registrados; faltan `piezasParaMatarEf`, F9 e I4 (2026-11-27) |

## Acceptance Criteria

**Convenciones.** El **Catálogo vigente** es el archivo con la tabla de Core Rules, guardado en el repositorio como fixture. Los casos inválidos se arman cambiando un campo de ese archivo. Todo `ConfigInvalida` esperado se afirma **con su código de regla** (por ejemplo `ConfigInvalida(F8)`; Core Rules, regla 6). La **config vigente** es la de la Librería con `bono_grito_ataque` y `bono_grito_movimiento` en 2 (Modificador, #11). Cada caso inválido viola **una sola** regla F, salvo los que prueban el orden (AC-38). Los chequeos I se prueban llamando a `EvaluarI(catalogo, config)`, que devuelve dos listas: los pares que fallan (vacía si pasa), y aparte, como dato informativo, los pares de las excepciones aprobadas que se dan (I1 e I4); el test del repositorio afirma sobre las dos. Toma los bonos de la config (`bono_grito_ataque`) y `vida_tramo`. `ValidarF2(fila)` evalúa F2 sola sobre una fila, sin F8 ni F9 (para AC-6). Las tres funciones son de la Librería y su firma la fija su ADR. Los criterios marcados **PROVISIONAL** dependen de un GDD o ADR todavía sin escribir y se actualizan cuando exista. Salvo que se indique otra cosa, son tests automáticos de tipo Logic (bloqueantes).

**Reglas del Catálogo (Core Rules)**

- **AC-1 (regla 1, forma del Catálogo)** — **GIVEN** el Catálogo vigente, **WHEN** se llama `PrepararContexto`, **THEN** se acepta; el contexto trae 7 filas con IDs 0 a 6 en orden y `versionBalance` leída del archivo.
- **AC-2 (regla 2, orden)** — **GIVEN** el Catálogo vigente con las filas 3 y 4 intercambiadas, **WHEN** se llama `PrepararContexto`, **THEN** `ConfigInvalida(F1)`. *(Integration con #4)* **GIVEN** el Catálogo vigente y la posición fija de los castillos (sectores 4 y 6, Tablero), **WHEN** se crea la partida, **THEN** los IDs de las piezas del despliegue siguen el orden de la Librería (jugador 0 primero y, dentro de cada jugador, por ID de tipo y casilla).
- **AC-3 (reglas 3 y 4, campos y valores)** — **GIVEN** el Catálogo vigente, **WHEN** se lee cada fila, **THEN** sus 8 campos coinciden, campo por campo, con un archivo de referencia de los valores vigentes guardado junto al fixture (los 56 valores de 7 filas por 8 campos). El archivo de referencia se actualiza en el mismo cambio que sube `versionBalance` (AC-15).
- **AC-4 (regla 5, estático)** — *(Integration con #3)* **GIVEN** una partida creada con el Catálogo vigente y un guion fijo de 20 comandos válidos guardado como fixture, con `bonoAtaque` = 2 puesto directamente en el estado del turno, **WHEN** se aplica el guion, **THEN** la huella del contexto y las 7 filas del Catálogo no cambian, y `EffectiveStats` de un Soldado, medido sobre el estado con `bonoAtaque` 2 antes y después del guion, devuelve ataque 10 (8 + 2) las dos veces.
- **AC-5 (regla 6, F1)** — **GIVEN** el Catálogo vigente con 6 filas, con 8, con un ID repetido, o con IDs desordenados, **WHEN** se llama `PrepararContexto`, **THEN** cada caso da `ConfigInvalida(F1)` (nunca `ProbabilidadesInvalidas`) y no se crea ninguna partida.
- **AC-6 (F2, límites)** — **GIVEN** la fila del Soldado, **WHEN** `ataque`, `vida` y `alcanceMax` toman 0, 1, 1.000 y 1.001 (cada uno por separado), **THEN** 0 y 1.001 dan `ConfigInvalida(F2)`, y 1 y 1.000 se aceptan. **WHEN** `movimiento` toma 0 y 1.001, **THEN** `ConfigInvalida(F2)` (1.001 viola también F8; gana F2 por el orden, regla 6); con 1 se acepta; con 5, `ConfigInvalida(F8)` (pila 266). El techo de 1.000 de F2 para `movimiento` no se alcanza (F8 y F9 cortan antes; Core Rules, regla 6), así que se prueba aparte con `ValidarF2` sobre la fila sola: 1.000 pasa F2.
- **AC-7 (F3)** — **GIVEN** la fila de la Catapulta, **WHEN** `alcanceMin` es 0, igual a `alcanceMax` y `alcanceMax` + 1, **THEN** 0 y `alcanceMax` + 1 dan `ConfigInvalida(F3)`; el igual se acepta.
- **AC-8 (F4, límites)** — **GIVEN** la fila del Soldado, **WHEN** `escudo` y `danoArea` toman −1, 0, 1.000 y 1.001 (cada uno por separado), **THEN** −1 y 1.001 dan `ConfigInvalida(F4)`; 0 y 1.000 se aceptan por F4 (`danoArea` 1.000 en el Soldado pasa la forma y falla I3 en el test, no en `PrepararContexto`).
- **AC-9 (F5)** — **GIVEN** el Catálogo vigente, **WHEN** el `topeVivas` de cualquier tipo es 0, **o** el del Rey es 2 con el de Soldado de 12 a 11 (suma 61, pila 256: solo viola F5), **o** el del Rey es 0, **THEN** `ConfigInvalida(F5)` en los tres casos.
- **AC-10 (F6, suma)** — **GIVEN** el Catálogo vigente (suma 61), **WHEN** se sube el tope de catapultas de 8 a 9 (suma 62, pila 256: solo viola F6), **THEN** `ConfigInvalida(F6)`; con soldados de 12 a 11 (suma 60) se acepta.
- **AC-11 (F7)** — **GIVEN** el Catálogo vigente, **WHEN** `versionBalance` es 0, 1, 1.000 y 1.001, **THEN** 0 y 1.001 dan `ConfigInvalida(F7)`; 1 y 1.000 se aceptan.
- **AC-12 (regla 7, identidad)** — **GIVEN** el Catálogo vigente deserializado dos veces de forma independiente (dos lecturas del archivo, dos objetos distintos), **WHEN** se calcula la huella, **THEN** son iguales entre sí e iguales a una constante de referencia guardada en el test (*PROVISIONAL* hasta que la fije el ADR de la Librería; mientras tanto se afirma solo la igualdad entre las dos). **GIVEN** dos Catálogos que difieren solo en el `escudo` del Soldado (3 y 4), **THEN** las huellas son distintas; lo mismo si difieren solo en `versionBalance`. *(El formato en bytes lo prueba el ADR de la Librería.)*
- **AC-13 (regla 8, un solo Catálogo)** — **GIVEN** una partida creada, **WHEN** se consulta `EffectiveStats` de un Soldado del jugador 0 y de uno del jugador 1 sin bonos, **THEN** devuelven los mismos valores en los 8 campos de la fila. Un test de forma (reflexión) verifica que ningún miembro público del tipo del Catálogo tiene un parámetro del tipo `JugadorId` (el jugador es un tipo propio, no un `int`; lo fija el ADR de la Librería).
- **AC-14 (regla 9, sin texto ni arte)** — **GIVEN** el esquema del Catálogo, **WHEN** se recorren sus campos, **THEN** todos son enteros de 32 bits (ninguno es texto ni decimal).
- **AC-15 (regla 10, versión registrada)** — **GIVEN** el registro de pares (`versionBalance` → huella) con el par del Catálogo vigente, **WHEN** se llama `VerificarRegistro(catalogo, registro)` con una cifra cambiada y la misma `versionBalance`, **THEN** devuelve `HuellaNoRegistrada`; **WHEN** además se sube `versionBalance` sin registrar el par nuevo, **THEN** también `HuellaNoRegistrada`; **WHEN** el par se registra, **THEN** devuelve `Ok`; y con el Catálogo vigente sin cambios, `Ok`.

**Fórmulas (Formulas)**

- **AC-16 (`topeTotal`)** — **GIVEN** topes {12, 16, 12, 8, 8, 4, 1} y topes {1, 1, 1, 1, 1, 1, 1}, **WHEN** se calcula `topeTotal`, **THEN** 61 y 7.
- **AC-17 (`piezasParaMatar`, valores)** — **GIVEN** el Catálogo vigente, **WHEN** se calcula la matriz de 7 × 7, **THEN** coincide celda por celda con la matriz de Formulas (49 valores). **GIVEN** `ataque` 1, `escudo` 1.000 y `vida` 1.000, **THEN** 2.000; con `ataque` 1.000, `escudo` 0 y `vida` 1, **THEN** 1; sin desborde en ningún caso.
- **AC-18 (`piezasParaMatar`, contra la regla real)** — *(Integration, #9)* **GIVEN** cada par (atacante `a`, objetivo `t`) de la matriz con N = `piezasParaMatar`, un estado con el objetivo enemigo en campo abierto (ningún tramo de muro vecino, #5), sin escudero aliado vecino que lo pueda cubrir (#8) y con bono 0, **WHEN** se resuelven con la Resolución real N ataques directos de tipo `a` y N − 1 ataques, **THEN** con N el objetivo muere y con N − 1 no. Para `a` = Rey (tope 1), los ataques son un Rey más soldados (mismo `ataque`, 8). Para `a` = demoledor, los ataques son las explosiones de N demoledores vecinos al objetivo (en la matriz vigente N vale 1, o 2 contra el Rey). Ningún N pasa el tope de su tipo. **Con el bono:** **GIVEN** `bonoAtaque` 2 en el estado, **WHEN** una catapulta ataca a un demoledor, **THEN** muere con 1 (`piezasParaMatarEf`); con bono 0 sobrevive con 1 y muere con 2.
- **AC-19 (I1)** — **GIVEN** el Catálogo y la config vigentes, **WHEN** se llama `EvaluarI`, **THEN** I1 pasa. Sin bono, los pares en 1 son exactamente 6: demoledor contra Soldado, Arquero, Escudero, Catapulta, Jinete y Demoledor. Con el Grito son exactamente 7: esos 6 más catapulta → Demoledor (excepción aprobada). **GIVEN** el `ataque` de la Catapulta en 11, **THEN** I1 pasa y catapulta → Demoledor aparece también sin bono, como excepción aprobada. **GIVEN** el `ataque` del Soldado en 9, **THEN** I1 falla y nombra Soldado → Demoledor (con Grito). **GIVEN** el `ataque` de la Catapulta en 15, **THEN** I1 falla y nombra catapulta → Arquero y catapulta → Catapulta (con Grito). **GIVEN** `vida_tramo` 30, **THEN** ningún `ataque_ef` (con el Grito y sin él) distinto del del demoledor llega a 30; con `vida_tramo` 12, **THEN** falla y nombra a la catapulta con Grito.
- **AC-20 (I2)** — **GIVEN** el Catálogo y la config vigentes, **WHEN** se llama `EvaluarI`, **THEN** el mínimo contra el Rey es 2 (demoledor) con el bono y sin él, y la catapulta con Grito necesita 3. **GIVEN** el `ataque` del Demoledor en 34, **THEN** pasa; en 35, **THEN** falla y nombra Demoledor → Rey.
- **AC-21 (I3)** — **GIVEN** el Catálogo vigente, **WHEN** se evalúa I3, **THEN** pasa (daño neto máximo 5 contra la vida mínima 10). **GIVEN** `danoArea` de la Catapulta en 10, **THEN** pasa; en 11, **THEN** falla. **GIVEN** `danoArea` 8 en el Arquero, **THEN** I3 lo evalúa y pasa. **GIVEN** `EvaluarI` con `bono_grito_ataque` 0 y con 2, **THEN** el resultado de I3 es el mismo (el bono no suma al área).
- **AC-22 (techos y pisos de Tuning Knobs)** — **GIVEN** el Catálogo y la config vigentes, cambiando **solo** el campo nombrado, **WHEN** se llama `EvaluarI`, **THEN** se cumple esta lista cerrada:
  - `ataque` de cada uno de Soldado, Arquero, Escudero, Jinete y Rey: 8 pasa, 9 falla (I1, con Grito).
  - `ataque` de la Catapulta: 14 pasa, 15 falla (I1, con Grito).
  - `ataque` del Demoledor: 34 pasa, 35 falla (I2 e I4).
  - `escudo + vida` de cada uno de Soldado, Arquero, Escudero, Catapulta y Jinete, cambiando solo `vida`: 13 pasa, 12 falla (I1, catapulta con Grito). Es decir, `vida` 10 / 11 / 8 / 11 / 10 pasa y 9 / 10 / 7 / 10 / 9 falla.
  - `escudo + vida` del Demoledor, cambiando solo `vida`: 11 pasa, 10 falla (I1, ataque 8 con Grito). Es decir, `vida` 10 pasa y 9 falla.
  - `escudo + vida` del Rey, cambiando solo `vida`: 31 pasa, 30 falla (I2 e I4). Es decir, `vida` 26 pasa y 25 falla.
  - `danoArea` de la Catapulta: 10 pasa, 11 falla (I3).

  Quedan fuera, de forma explícita: `movimiento` (lo cubren F8 y F9: AC-6, AC-34 y AC-35), `alcanceMin` y `alcanceMax` (F3 y techo no verificable), `escudo` suelto y `topeVivas` (F4 a F6 y el Tablero), y los pisos "sin medir".

**Casos borde (Edge Cases)**

- **AC-23 (archivo ilegible)** — *(PROVISIONAL hasta el ADR de carga)* (a) *(Logic)* **GIVEN** un archivo del Catálogo truncado, y otro al que le falta un campo, **WHEN** el lector los lee, **THEN** devuelve un error de lectura y no entrega ningún Catálogo. (b) *(UI, captura en `production/qa/evidence/`)* **GIVEN** el cliente con uno de esos archivos, **WHEN** se abre el menú, **THEN** no se ofrece crear partida y se ve el aviso neutro. (c) *(Integration, PROVISIONAL hasta #28)* el servidor no crea partidas y deja el error registrado.
- **AC-24 (los chequeos I son de test, no de runtime)** — **GIVEN** un Catálogo con `ataque` del Demoledor en 35 (válido por F1 a F8), **WHEN** se llama `PrepararContexto`, **THEN** se acepta; **WHEN** se llama `EvaluarI`, **THEN** devuelve Demoledor → Rey en I2 y en I4.
- **AC-25 (huella distinta en cliente y servidor)** — *(Integration, PROVISIONAL hasta #28)* **GIVEN** un servidor con el Catálogo X y un cliente con el Y, **WHEN** el cliente pide una partida, **THEN** `VersionNoSoportada` y no se juega.
- **AC-25b (partida cargada con otro Catálogo)** — *(Integration con #1)* **GIVEN** una partida guardada con el contexto del Catálogo vigente y un contexto preparado con el Soldado en `escudo` 4, **WHEN** se aplica la partida a ese contexto, **THEN** `VersionNoSoportada` (lo mismo que Librería, AC-61, sobre este fixture).
- **AC-26 (repetición de un balance viejo)** — *(Integration, PROVISIONAL hasta #26)* **GIVEN** el registro de una partida jugada con el Catálogo A y el archivo actual B, **WHEN** se reproduce, **THEN** usa la tabla A guardada y el estado final es idéntico al original.
- **AC-27 (editar con partida en curso)** — **GIVEN** una partida en curso con su contexto fijado, **WHEN** se reemplaza el archivo y se aplica un comando, **THEN** la huella y las cifras de esa partida no cambian; una partida nueva usa el archivo nuevo.
- **AC-28 (ataque menor o igual que el escudo)** — *(Integration, #9)* **GIVEN** un Escudero de J que ataca a un Escudero de K, sin escudero aliado vecino que lo cubra (#8), sin muro y con bono 0, **WHEN** se resuelve, **THEN** la vida perdida es `max(0, ataque_E − escudo_E)` (con el fixture vigente, 0) y la vida no cambia; **WHEN** atacan 3 Escuderos de J al mismo objetivo, **THEN** la vida perdida es `3 · ataque_E − escudo_E` (con el fixture vigente, 7). Los valores salen del fixture, no del texto.
- **AC-29 (bono sobre el máximo)** — *(depende del rango de bono 0 a 10 de la Librería)* **GIVEN** un Catálogo con `ataque` 1.000 en el Soldado y un bono de +10, **WHEN** se llama `EffectiveStats`, **THEN** devuelve 1.010 sin error.
- **AC-30 (ejército inicial)** — *(Integration con #4)* **GIVEN** el Catálogo vigente con `topeVivas` en 3 para los Soldados, y por separado para los Arqueros y para los Escuderos, **WHEN** se crea la partida, **THEN** `ConfigInvalida` en los tres casos (Tablero, regla 6), con el código `ConfigInvalida(T6)` del Tablero (regla 6); con 4 se acepta.
- **AC-31 (skin con ID inválido)** — *(Integration, PROVISIONAL hasta #38; la carga de Render ya lo rechaza, #18 regla 7 y AC-4)* **GIVEN** una skin que declara el tipo 7, **WHEN** el validador la revisa, **THEN** la rechaza.
- **AC-32 (cifras repetidas)** — **GIVEN** el Catálogo vigente con la fila del Soldado reemplazada por los 8 valores de la del Jinete (6/1/1/8/3/20/0/8; suma de topes 57, pila 254 − 12 × 4 + 8 × 6 = 254, F9 6 + 2 = 8; copiar al revés, el Jinete con tope 12, daría suma 65 y `ConfigInvalida(F6)`), **WHEN** se llama `PrepararContexto`, **THEN** se acepta, con una huella distinta de la del vigente.
- **AC-33 (área genérica)** — *(Integration, #9 y #7: con `danoArea` > 0 el arquero pasa al modo de área de Alcance)* **GIVEN** un Catálogo con `danoArea` 8 en el Arquero, **WHEN** un arquero ataca una casilla en alcance, **THEN** las 8 casillas alrededor reciben 8 de daño antes de escudo.

**Criterios agregados en la revisión del 2026-11-27**

- **AC-34 (F8, límite)** — **GIVEN** el Catálogo vigente (pila 254), **WHEN** el `movimiento` del Rey es 8, **THEN** pila 256 y se acepta; **WHEN** es 9, **THEN** pila 257 y `ConfigInvalida(F8)`.
- **AC-35 (F9, en `CrearPartida`)** — **GIVEN** el Catálogo vigente con los topes bajados para que F8 no corte (Soldado, Arquero y Escudero en 4, Catapulta, Jinete y Demoledor en 1; con el Jinete en 10 la pila es 4 × 4 × 3 + 2 + 10 + 6 + 6 = 72, y con 11, 73), preparado con `PrepararContexto` (pasa F1 a F8), y la config vigente (`bono_grito_movimiento` 2, `limite_normal` 12), **WHEN** se llama `CrearPartida` con el `movimiento` del Jinete en 10, **THEN** se crea (10 + 2 = 12); **WHEN** es 11, **THEN** `ConfigInvalida(F9)`. **GIVEN** el Catálogo vigente y la config con `bono_grito_movimiento` 7 (con `limite_normal` 12 la cota de la pila con bono no falla: Movimiento, F5, y Modificador, cota de la pila), **WHEN** se llama `CrearPartida`, **THEN** `ConfigInvalida(F9)` (6 + 7 = 13; Jinete, Demoledor y Rey), y el mismo contexto con la config vigente crea la partida.
- **AC-36 (I4, explosión)** — **GIVEN** el Catálogo vigente, **WHEN** se llama `EvaluarI`, **THEN** I4 pasa, y la lista informativa de la excepción nombra los seis tipos que no son el Rey como muertes posibles de una sola explosión. *(Integration, #9)* **GIVEN** un demoledor de J con un Soldado, un Escudero y el Rey de K en tres esquinas de su contorno (NO, NE y SO, que no son vecinas entre sí), `Coberturas()` vacío y sin muros vecinos, **WHEN** explota, **THEN** el Soldado y el Escudero mueren y el Rey queda con vida 5 (30 − (30 − 5)). **GIVEN** además, en otro estado, el Escudero de K cubriendo al Soldado de K (los dos vecinos del demoledor y vecinos entre sí), **WHEN** explota, **THEN** el Escudero muere (30 + 30 − 5 = 55 ≥ 20) y el Soldado no pierde vida (Cobertura, #8, AC-10).
- **AC-37 (trazabilidad de las restricciones de presentación)** — *(Config/Data, ADVISORY)* **GIVEN** la definición del pipeline de CI, **WHEN** se revisa, **THEN** el job que corre ante un cambio del fixture del Catálogo incluye Ficha, AC-4 (cada cifra con su bono máximo ≤ 99), y Vista previa, AC-11 (`vida_t + escudo_ef_max_t ≤ 100`), sobre **este mismo** fixture. El caso límite (la `vida` del Rey en 96) es de #16 AC-11.
- **AC-38 (orden de los códigos F)** — **GIVEN** el Catálogo vigente con el tope de demoledores en 5 (suma 62, pila 260: viola F6 y F8), **WHEN** se llama `PrepararContexto`, **THEN** `ConfigInvalida(F6)`. **GIVEN** el `movimiento` del Soldado en 1.001 (viola F2 y F8), **THEN** `ConfigInvalida(F2)`. **GIVEN** 8 filas (viola F1 y F6), **THEN** `ConfigInvalida(F1)`.
- **AC-39 (techo cruzado de `bono_grito_ataque`)** — **GIVEN** el Catálogo vigente y la config con `bono_grito_ataque` 3, **WHEN** se llama `EvaluarI`, **THEN** I1 falla y nombra Soldado → Demoledor, Jinete → Demoledor y Rey → Demoledor (8 + 3 = 11 ≥ 11); con 2 pasa (AC-19).

**Criterios que no se pueden automatizar** (van a playtest): que el jugador sienta que el duelo es justo y que no pueda culpar a los números (Player Fantasy). Que las skins nunca cambien una cifra lo hace cumplir el validador del Kit (#38); acá solo se prueba la forma (AC-13, AC-14).

**Sin criterio de tiempo:** `PrepararContexto` no tiene presupuesto propio (la Librería fija 1 ms por comando, no por esta llamada). Con 7 filas no es un riesgo; queda en Open Questions.

## Open Questions

| Pregunta | Dueño | Para cuándo |
|---|---|---|
| El demoledor mata solo a cualquier pieza que no sea el Rey, y mata al Rey con una segunda pieza de ataque 5 o más (3 o más con el Grito, así que entra el escudero); también la explosión más un arquero (30 + 6 − 5 = 31 ≥ 30). Es el ritmo más rápido del juego: ¿es el deseado? | Resolución (#9), Castillo (#5), playtest | En sus GDD y en el MVP |
| ¿Hace falta una cota de muertes por explosión (I4 la acepta sin cota, hasta 8)? Registrar las muertes por explosión de cada partida | Métricas (#19), playtest | En el MVP |
| F9 frente a `m + R ≤ 13` (Tablero, Formulas y Open Questions, 2026-11-27): F9 suma un bono que el recluta no tiene el turno en que aparece y no mira el alcance; un tipo de `movimiento` 8 y `alcance_max` 6 pasa F9 y rompe `m + R ≤ 13`. Opciones: F9 → `m + R ≤ 13`, sumarle `m + bono + R ≤ 13`, o dejarla. Si cambia, cambia también la nota «10 con el bono vigente». Los topes de `m + R` del Tablero (arquero ≤ 8, soldado ≤ 7, escudero ≤ 6: arquero y escudero sin margen) solo los cubren tests | Jugador, con el Tablero (#4) | Antes del ADR de la Librería |
| Pisos de `ataque`, `vida`, `escudo` y `danoArea` sin fundamentar | Métricas (#19), partidas humanas | Compuerta de duración |
| Techo de `alcanceMax` (costo de la vista previa). El de `movimiento` quedó en 12 (Tablero, #4) | Librería | Al medir la vista previa |
| `PrepararContexto` sin presupuesto de tiempo propio | Librería (ADR) | Antes de escribir código |
| ¿El jugador percibe el duelo como justo (Player Fantasy)? No automatizable | Playtest | En los playtests del MVP |
| El Rey (6/8/5/30) es mayor o igual que el soldado y el jinete en todas las cifras, y soldado, jinete y Rey atacan igual. ¿Bajar su `movimiento` a 4 (F8 254 → 252)? Decisión del jugador (2026-11-27): se decide junto con DW-1, la tortuga del Rey cubierto (Cobertura, #8) | Jugador, con Cobertura (#8) | Al decidir DW-1 |
| Prototipo v5 contra GDD: el código suma el Grito al área y a la explosión (Core Rules, regla 5). Manda el GDD; hay que corregir el prototipo antes de las partidas de la compuerta de duración | Prototipo (fuera de este GDD) | Antes de las partidas de duración |
| ¿Los arqueros en masa dominan? El arquero tiene el tope más alto (16) y 6 lo matan al Rey dentro del castillo (DW-2). Registrar el daño quitado por tipo de pieza | Métricas (#19) | En el MVP |

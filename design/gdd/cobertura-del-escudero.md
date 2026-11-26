# Cobertura del escudero

> **Status**: Designed (pendiente de `/design-review` en una sesión nueva)
> **Author**: Martin Arin + Claude
> **Last Updated**: 2026-11-25
> **Last Verified**: 2026-11-25
> **Implements Pillar**: Pilar 2 (La fuerza está en combinar); Pilar 1 (Planificar y cosechar) y Pilar 4 (Duelo corto) como restricciones
> **Creative Director Review (CD-GDD-ALIGN)**: CONCERNS (accepted) 2026-11-25 — pilar 4 (regla 13 y la tortuga alrededor del Rey: `Atacar` casi nunca se salta y nada limita cubrir cada turno salvo el costo del escudero), pilar 2 (regla 6 y C1: contra una pieza cubierta el combo topa en lo que aguanta el escudero, 25 hoy) y Player Fantasy (regla 5: "proteger cuesta" depende de que el ataque del escudero, 4, sea un sacrificio real; depende de números del Catálogo)

## Summary

La Cobertura del escudero decide quién protege a quién: en la fase de atacar, un escudero renuncia a su ataque para cubrir a un aliado pegado, y durante el turno del rival todo el daño dirigido a ese aliado lo recibe el escudero. Guarda las coberturas, valida `Cubrir` y `QuitarCobertura` y le dice a Resolución a quién va cada golpe. Existe para que el combo del rival tenga una respuesta que se ve y se decide antes de resolver, y para que las formaciones con escuderos al frente importen.

> **Quick reference** — Layer: `Feature` · Priority: `MVP` · Key deps: `Librería de reglas (#1), Catálogo de piezas (#2, solo el ID del Escudero), Máquina de turno (#3), Tablero (#4), Castillo y muros (#5, blanda), Alcance y asignación (#7), Reclutamiento (#10, blanda)`

## Overview

La **Cobertura del escudero** le da al escudero una alternativa a atacar: en la fase `Atacar`, en lugar de golpear, **cubre a un aliado** que esté en una de las 8 casillas vecinas. Desde ese momento y hasta que empiece su siguiente turno, todo el daño que el rival descargue sobre la pieza cubierta lo **recibe el escudero**, menos el escudo del escudero; el aliado sale intacto. Vale para los ataques directos, el área de la catapulta y la explosión del demoledor. No vale para la parte del daño que cae sobre un tramo de muro (Castillo, regla 8): el tramo no es una pieza.

Este sistema guarda **quién cubre a quién**, valida los comandos `Cubrir` y `QuitarCobertura`, borra una cobertura cuando muere cualquiera de las dos piezas, y le responde a Resolución (#9) una sola pregunta: *¿quién recibe el golpe dirigido a esta pieza?* No calcula daño, no mata a nadie y no mueve nada: la aritmética del reparto (sumar, restar el escudo una vez) es de Resolución.

El jugador lo usa de forma **activa y con un costo**: tocar al escudero, tocar al aliado, y el escudero renuncia a su ataque de ese turno. La cobertura es **información pública**: el rival la ve antes de decidir sus ataques y tiene que rodearla, que es lo que la convierte en una jugada y no en un truco. Sin este sistema el escudero sería un soldado con menos ataque y más vida; con él, las formaciones del concepto ("escuderos al frente") tienen una razón de ser y el combo del rival tiene una respuesta.

*Alternativa descartada:* cobertura automática por estar al lado de un aliado. Quita la decisión y el costo (el escudero dejaría de atacar), y con ella el contra-combo que hace al sistema interesante (pilar 2).

## Player Fantasy

**Nadie cae solo.** Sos el general que cuida a su gente. Poner al escudero junto a una pieza frágil no es una jugada de cálculo frío: es el gesto de "quedate detrás mío". El escudero, medio escondido tras su escudo enorme, se planta al lado del arquero y le presta el escudo por un turno.

**Momento ancla:** el rival junta tres soldados contra tu arquero (escudo 2, vida 15): 8 + 8 + 8 = 24, menos 2 de escudo, son 22 y el arquero habría muerto. Pero tu escudero lo cubría desde tu turno anterior. Las tres flechas se funden sobre el escudero (escudo 5, vida 20): 24 − 5 = 19, y queda en pie con 1 de vida. El arquero no perdió un punto. El rival hizo el combo perfecto y no cayó nadie.

**Pilares que sirve:**

- Pilar 2, *"Ninguna pieza gana sola: el poder sale de juntar varias sobre un mismo objetivo."* Dicho al revés: ninguna pieza se defiende sola. La cobertura es la versión protectora de la misma idea, la coordinación entre dos piezas pegadas.
- Pilar 1, *"Al mover armás la estrategia; al atacar ejecutás el plan y ves sus frutos."* La cobertura se arma en un turno y rinde en el siguiente, a la vista de los dos.

**Qué le exige al resto del diseño:**

- Que proteger **cueste** algo (el escudero renuncia a atacar): un cuidado gratis no se siente como un gesto.
- Que la cobertura se **vea** (una línea entre las dos piezas) y que la vista previa (#16) muestre sobre quién cae cada flecha.
- Que importe **quién queda al lado de quién**: las formaciones con escuderos al frente son parte de la fantasía (concepto, Discovery).
- Un tono cálido y con humor en la pose del escudero (art bible: *el escudero que se esconde detrás del escudo*), sin volverlo solemne.

*Encuadres considerados:* "el golpe que no llegó" (defender, pilar 1) y "la apuesta a la vista" (jugada pública que el rival debe leer y que el área castiga, pilar 2). El jugador eligió "nadie cae solo" el 2026-11-25. La lectura pública de la cobertura sigue siendo una regla (el rival la ve y la rodea), solo que no es el centro de la fantasía.

## Detailed Design

### Core Rules

1. **Quién cubre.** Solo un escudero (tipo con ID 2 del Catálogo), vivo, del jugador del turno, en la fase `Atacar`. Cubrir es su acción del turno: una pieza hace **una** cosa por turno (Alcance, #7, regla 9). No gasta cupo de movimiento ni usa la pila de deshacer. Haber movido o haber sido reclutado ese turno no lo impide (Reclutamiento, #10, reglas 9 y 10).
2. **A quién.** Una pieza **viva del mismo jugador** que esté en el **contorno de 8 casillas** del escudero (Tablero, #4, regla 2; F1). Sirve cualquier tipo: soldado, arquero (también sobre su tramo), catapulta, jinete, demoledor, otro escudero (con la regla 4) y el **Rey** (decisión del 2026-11-25). Nunca el propio escudero: su casilla no está en su contorno.
3. **Una cobertura por escudero, un escudero por aliado.** Un escudero cubre como mucho a un aliado y un aliado tiene como mucho un escudero (decisión del jugador, 2026-11-19). `Cubrir` sobre un escudero que ya cubría **reemplaza** la anterior en un solo comando, también si el aliado es el mismo (igual que `AsignarAtaque`, #7, regla 9). Un aliado que ya tiene a otro escudero se rechaza (`AliadoYaCubierto`): no se le puede quitar a un compañero.
4. **Sin cadenas (decisión del 2026-11-25).** Ninguna pieza es a la vez cubridora y cubierta. `Cubrir` se rechaza si el escudero ya está cubierto (`EscuderoCubierto`) o si el aliado ya cubre a otro (`AliadoYaCubre`). Por eso no hay reenvíos ni ciclos: un golpe se redirige **una sola vez**. Para cambiar de rol hay que quitar la cobertura primero (regla 8).
5. **Una acción por pieza, de los dos lados.** `Cubrir` sobre un escudero con ataque asignado **borra su ataque**: sale `AtaqueQuitado` (evento de #7, sin cambiar su forma) y después `CoberturaAsignada`. `AsignarAtaque` sobre un escudero que cubre **borra su cobertura**: sale `CoberturaQuitada` con motivo `Desplazada` y después `AtaqueAsignado` (#7, regla 15). Un comando rechazado no cambia nada: si el `Cubrir` no vale, queda el ataque anterior.
6. **Qué hace la cobertura.** Mientras está vigente (regla 7), todo daño dirigido a la **pieza** cubierta lo recibe el escudero, sea de un ataque directo, del centro o el área de una catapulta o de la explosión de un demoledor (F2). Tres límites:
   - **El tramo no se redirige.** Si el cubierto es un arquero sobre su tramo, la parte de la pieza va al escudero y el tramo recibe su golpe completo (Castillo, #5, regla 8).
   - **Solo el daño del rival.** Se redirige el daño cuyo origen es una pieza del rival del dueño de la cobertura. El fuego amigo cae donde apuntó.
   - **Lo que ya cae sobre el escudero** (por apuntarle a él o por área) lo recibe igual, **sumado** a lo desviado.

   El reparto numérico es de Resolución (#9, confirmado en su regla 7): lo desviado se suma al daño bruto del escudero, su escudo se resta **una sola vez**, y si el total supera su vida el exceso **se pierde** (no pasa al aliado). Este GDD solo fija a quién va cada golpe.
7. **Cuándo está vigente.** Se asigna en el `Atacar` de su dueño J y **redirige solo en la resolución del rival K**, la del turno siguiente. Se borra al abrir el siguiente turno de J (Máquina, #3, regla 4.1). En la resolución de J la cobertura de J no hace nada (regla 6, fuego amigo). La tabla de **States and Transitions** lo resume.
8. **Quitar.** `QuitarCobertura(escudero)` borra la cobertura de ese escudero (decisión del 2026-11-25; sube la versión de reglas de la Librería). Sin cobertura, se rechaza con `SinCobertura`, también si la pieza no es un escudero. Solo en `Atacar`, no toca ataques (un escudero que cubre no tiene uno) y no usa la pila de deshacer.
9. **Cuándo se borra una cobertura.** Siempre con un evento `CoberturaQuitada` y su motivo:

   | Motivo | Cuándo | Quién la dispara |
   |---|---|---|
   | `Quitada` | `QuitarCobertura` | Jugador |
   | `Desplazada` | `AsignarAtaque` sobre el escudero | #7 |
   | `Caducada` | Se abre el siguiente turno de su dueño | Máquina (#3) |
   | `EscuderoMuerto` | El escudero muere en una resolución (incluido el fuego amigo de su propio turno) | Resolución (#9) |
   | `AliadoMuerto` | El aliado muere en una resolución | Resolución (#9) |

   Una cobertura reemplazada por otra `Cubrir` no genera `CoberturaQuitada`: `CoberturaAsignada` trae `aliadoAnterior`. Si en la misma resolución mueren las dos piezas, sale un solo evento con `EscuderoMuerto`. Las coberturas de piezas muertas se borran **al terminar** la resolución: durante ella siguen valiendo (regla 10).
10. **Se evalúa con el estado al empezar la resolución.** Quién cubre a quién se lee **una vez**, antes de aplicar daño. Que el escudero muera en esa resolución no desvía ni un golpe de más ni de menos: los golpes simultáneos se reparten igual y la cobertura recién se borra después (mismo principio que el bono del arquero, Castillo, regla 7). Sin cascadas.
11. **Todo es público.** Las coberturas las ven los dos jugadores en cualquier fase: el rival decide sus ataques sabiendo quién cubre a quién. Es el diseño, no un descuido.
12. **Consultas puras.** Valen para cualquier pieza, de los dos bandos y en cualquier fase; con un ID inexistente devuelven vacío. Las listas salen ordenadas por ID ascendente, sin repetir.
    - `CubiertoPor(pieza)`: el escudero que la cubre, o ninguno.
    - `Cubre(escudero)`: el aliado que cubre, o ninguno.
    - `Coberturas()`: la lista `(escudero, aliado)` por ID de escudero ascendente.
    - `AliadosCubribles(escudero)`: los aliados que `Cubrir` aceptaría si el dueño estuviera en `Atacar`. **Coinciden exactamente** con las reglas de rechazo de la regla 14, incluido el aliado que ya cubre ese mismo escudero. Vacía si la pieza no es un escudero vivo o si está cubierta.
13. **Acción posible.** Para el salto de fase (Máquina, regla 10), un escudero tiene acción si `ObjetivosValidos` (#7) **o** `AliadosCubribles` no están vacías. Como cualquier aliado vecino sirve, un escudero con una pieza al lado casi siempre tiene acción: `Atacar` rara vez se salta.
14. **Códigos de rechazo (fila 14 de la Librería), en este orden.** Se devuelve el primero que falla, después de los códigos de turno, pieza y fase de la Librería (`NoEsTuTurno`, `PiezaInexistente`, `NoEsTuPieza`, `FaseIncorrecta`):

    | Orden | Código | Cuándo | ¿Lo ve el jugador? |
    |---|---|---|---|
    | 1 | `NoEsEscudero` | `Cubrir` con una pieza que no es de tipo 2 | Interno |
    | 2 | `AliadoInexistente` | El ID del aliado no existe o murió | Interno |
    | 3 | `AliadoNoEsTuyo` | El aliado es del rival | Sí |
    | 4 | `AliadoEsElMismo` | El aliado es el propio escudero | Interno |
    | 5 | `AliadoLejos` | El aliado no está en el contorno de 8 (F1) | Sí |
    | 6 | `EscuderoCubierto` | El escudero ya es cubierto por otro | Sí |
    | 7 | `AliadoYaCubre` | El aliado ya cubre a otro escudero | Sí |
    | 8 | `AliadoYaCubierto` | Otro escudero ya cubre al aliado | Sí |
    | — | `SinCobertura` | `QuitarCobertura` sobre una pieza que no cubre a nadie | Sí |

    Un `Cubrir` sobre el mismo aliado que el escudero ya cubría **no** es `AliadoYaCubierto` (regla 3): se acepta y emite `CoberturaAsignada` con `aliadoAnterior` igual al aliado.
15. **Eventos.** `CoberturaAsignada(escudero, aliado, aliadoAnterior)` (anterior vacío si no había) y `CoberturaQuitada(escudero, aliado, motivo)`. Confirma el nombre que #7 dejó PROVISIONAL. Las caducidades y las muertes salen en orden de ID de escudero ascendente; el lugar exacto dentro del cierre de turno lo fija la Máquina y, en la resolución, va después de `PiezaMuerta` (#9, regla 14).
16. **Entrega a Resolución.** La lista de `Coberturas()` y la pregunta `receptor(pieza, origen)` (F2), con el estado al empezar la resolución. El orden fija eventos, nunca resultados (Librería, regla 15).
17. **Lo que el jugador no puede hacer.** Cubrir y atacar con el mismo escudero en un turno, cubrir a una pieza que no está pegada, cubrir a un rival, cubrir a quien ya tiene otro escudero, encadenar coberturas, desviar el fuego amigo, ni asignar ni quitar coberturas fuera de `Atacar` o en el turno rival.

### States and Transitions

**Estado del escudero en `Atacar`** (extiende la tabla de #7):

| De → a | Disparador | Efecto |
|---|---|---|
| Libre → Cubriendo | `Cubrir` válido | Guarda la cobertura |
| Con ataque → Cubriendo | `Cubrir` válido | Borra el ataque (`AtaqueQuitado`) y guarda la cobertura |
| Cubriendo → Cubriendo | `Cubrir` válido (otro aliado, o el mismo) | Reemplaza; `CoberturaAsignada` con `aliadoAnterior` |
| Cubriendo → Libre | `QuitarCobertura` | Borra (`CoberturaQuitada`, `Quitada`) |
| Cubriendo → Con ataque | `AsignarAtaque` válido (#7) | Borra la cobertura (`Desplazada`) y guarda el ataque |
| cualquiera → el mismo | Comando rechazado | Nada cambia |

**Vida de una cobertura:**

| Estado | Desde | Hasta | ¿Redirige? |
|---|---|---|---|
| **Asignada** | `Cubrir`, en el `Atacar` de su dueño J | Cierre del turno de J | No: en la resolución de J el origen es del propio bando |
| **Vigente** | Apertura del turno del rival K | Fin de la resolución de K | **Sí**, todos los golpes de K sobre el aliado |
| **Borrada** | Apertura del siguiente turno de J, o la muerte de una de las dos piezas (al terminar la resolución en que muere) | — | — |

Un jugador no tiene coberturas fuera de `Atacar` de su turno, porque las borra al abrirlo. El estado `Terminado` conserva las coberturas (Librería, regla 11).

### Interactions with Other Systems

| Sistema | Qué entra | Qué sale | Dueño de la interfaz |
|---|---|---|---|
| #1 Librería | Comandos `Cubrir` y `QuitarCobertura`, orden de validación, forma canónica, `EstadoInvalido` | Códigos de la regla 14, eventos de la regla 15, consultas | Librería (contrato); este GDD (reglas) |
| #2 Catálogo | ID de tipo 2 (escudero) | — | Catálogo |
| #3 Máquina | Abre y cierra `Atacar`; borra las coberturas del jugador al abrir su turno | `AliadosCubribles` para el salto de fase | Máquina (momento); este GDD (consulta) |
| #4 Tablero | `ocupante`, contorno de 8 | — | Tablero |
| #5 Castillo | Arquero sobre tramo propio | La parte del tramo no se redirige (regla 6) | Castillo |
| #7 Alcance | Exclusión ataque/cobertura, evento `AtaqueQuitado` | Aliados cubribles, evento `CoberturaQuitada` | Compartida |
| #9 Resolución | Muertes y fin de la resolución | `Coberturas()` y `receptor(pieza, origen)` (F2) | Este GDD (quién); #9 (cuánto), confirmado |
| #10 Reclutamiento | Un recluta escudero puede cubrir ese turno | — | #10 |
| #13 Entrada, #15 HUD, #16 Vista previa, #18 Render, #23 Accesibilidad, #24 Tutorial | — | Consultas, códigos, eventos | Cliente, solo lectura |
| #14 Localización | — | Textos de los códigos visibles | Localización |
| #19 Métricas | — | Eventos de cobertura | Métricas |
| #25 Rival IA | — | `AliadosCubribles`, `CubiertoPor` | IA, solo lectura |

## Formulas

### F1. `en_contorno`

La fórmula `en_contorno` se define así:

`en_contorno(escudero, aliado) = max(|c_a − c_e|, |f_a − f_e|) == 1`

Es la pertenencia al **contorno de 8 casillas** que define el Tablero (#4, regla 2); este GDD no define otra vecindad. **No usa `alcanceMin` ni `alcanceMax`** del escudero (su alcance de ataque es 1–2): la cobertura llega solo a las casillas pegadas. En un tramo de muro, `(c_a, f_a)` es la casilla del arquero.

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| `c_e`, `f_e` | int | 0–23, 0–14 | calculated (estado, casilla del escudero) | Columna y fila del escudero |
| `c_a`, `f_a` | int | 0–23, 0–14 | calculated (estado, casilla del aliado) | Columna y fila del aliado |

**Output Range:** booleano. Falso con distancia 0 (misma casilla, regla 14, `AliadoEsElMismo` va antes) y con distancia 2 o más. Una pieza viva siempre está dentro de la grilla, así que no hay casos fuera de rango; en una esquina el contorno tiene 3 casillas, en el borde 5 y adentro 8.
**Example:** escudero en `(5, 5)`. Aliado en `(6, 4)`: `max(1, 1) = 1`, verdadero (diagonal). Aliado en `(7, 5)`: `max(2, 0) = 2`, falso. Escudero en `(0, 0)` con aliado en `(1, 1)`: verdadero.

### F2. `receptor`

La fórmula `receptor` se define así:

`receptor(pieza, origen) = e   si existe (e, pieza) ∈ Coberturas() y jugador(origen) ≠ jugador(e)`
`receptor(pieza, origen) = pieza   en cualquier otro caso`

Responde, para un golpe dirigido a la **pieza** (la parte de la pieza, nunca la del tramo), quién lo recibe. Resolución (#9) la consulta una vez por golpe, con las coberturas **al empezar la resolución** (regla 10).

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| `pieza` | int | ID de pieza viva | calculated (destino del golpe) | Pieza a la que apunta el golpe (directo, centro o área de catapulta, explosión) |
| `origen` | int | ID de pieza viva | calculated (atacante) | Pieza que causa el golpe |
| `Coberturas()` | lista de `(e, a)` | 0 a `coberturas_max` (F4) pares | calculated (estado) | Una entrada por escudero, sin aliados repetidos, sin pieza en los dos roles |
| `jugador(x)` | int | 0 o 1 | calculated (estado) | Dueño de la pieza `x` |

**Output Range:** un ID de pieza viva: la misma `pieza` o su escudero. **Es idempotente**: `receptor(receptor(p, o), o) = receptor(p, o)`, porque ningún escudero cubridor está cubierto (regla 4). Por eso un golpe se redirige a lo sumo una vez y no hace falta detectar ciclos. Con `Coberturas()` vacía, es la identidad. Con `jugador(origen) = jugador(e)` (fuego amigo) es la identidad.
**Example:** el rival (jugador 1) tiene tres soldados que apuntan al arquero 7 del jugador 0, cubierto por el escudero 3. Los tres golpes tienen `receptor = 3`. Si además su catapulta apunta al arquero 7, el centro tiene `receptor = 3`, y el área sobre el escudero 3 (vecino por regla 2) tiene `receptor = 3`: llegan juntos a él. Si el jugador 0 dispara su propia catapulta contra su arquero 7, `receptor = 7`.

### F3. `bruto_para_romper_cobertura` (oráculo de tests de balance)

La fórmula `bruto_para_romper_cobertura` se define así:

`bruto_para_romper_cobertura(e) = escudo_e + vida_e`

Es el daño bruto total que el rival tiene que juntar sobre el escudero para matarlo en una resolución, y es la **única** manera de terminar una cobertura por daño. **Es un oráculo de tests: no entra al núcleo ni a la vista previa** (como `piezasParaMatar`, Catálogo): la cuenta real es la de Resolución. Cuántos atacantes hacen falta ya lo da la columna "Escudero (25)" de la matriz del Catálogo.

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| `escudo_e` | int | 0–1.000 | data file (Catálogo, tipo 2) | Escudo del escudero (v5: 5) |
| `vida_e` | int | 1–1.000 | data file (Catálogo, tipo 2) | Vida del escudero (v5: 20) |

**Output Range:** 1 a 2.000, sin riesgo de desborde de 32 bits (mínimo con escudo 0 y vida 1). Con los valores v5, 25.
**Example:** el rival necesita 25 de bruto repartido sobre el escudero. Cuatro soldados juntan 32 y lo matan (32 − 5 = 27 ≥ 20); tres juntan 24 y no (24 − 5 = 19 < 20). **Y en los dos casos el aliado no pierde nada**: todo lo desviado se absorbe o se pierde.

**Chequeos de balance** (los hace un test, igual que I1–I3 del Catálogo):

- **C1, la cobertura vigente protege entera.** Con una cobertura vigente, el daño de la pieza cubierta por golpes del rival es 0 aunque el bruto sea 1.000.000. Solo el tramo puede recibir daño. Es la consecuencia de que el exceso se pierde.
- **C2, la cobertura no se rompe sola.** Con los valores vigentes, `piezasParaMatar(a, escudero) ≥ 2` para todo atacante `a` salvo el demoledor (que muere al explotar y es la excepción aprobada I1). Hoy: soldado 4, arquero 5, escudero 7, catapulta 3, jinete 4, Rey 4, demoledor 1. Si un cambio de Catálogo baja alguno de esos a 1, una sola pieza anula la cobertura: es una regresión.

### F4. `coberturas_max`

La fórmula `coberturas_max` se define así:

`coberturas_max(j) = min(escuderos_j, ⌊vivas_j / 2⌋)`

Cota del tamaño de `Coberturas()` por jugador: cada cobertura usa un escudero cubridor y un aliado distinto, y ninguna pieza cumple los dos roles (regla 4). La usan la validación de `EstadoInvalido` (Librería, regla 11), el tamaño de la forma canónica y el máximo de líneas que dibujan Render (#18) y la vista previa (#16).

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| `escuderos_j` | int | 0–12 | calculated (estado) | Escuderos vivos del jugador `j`; tope por `topeVivas` del tipo 2 |
| `vivas_j` | int | 0–61 | calculated (estado) | Piezas vivas del jugador `j`; la suma de topes del Catálogo es 61 |

**Output Range:** 0 a 12. Es 0 con un escudero solo (`vivas = 1`) y con ninguno. Con 12 escuderos y al menos 24 piezas, 12.
**Example:** ejército inicial, 4 escuderos y 13 piezas: `min(4, 6) = 4`. Un escudero y el Rey: `min(1, 1) = 1`. Un escudero solo: `min(1, 0) = 0`.

**Bucles de retroalimentación.** Hay uno, **negativo para el atacante**: el defensor renueva la cobertura cada turno y el atacante tiene que gastar 25 de bruto y varios ataques para quitársela. Lo amortiguan cuatro cosas: el escudero no ataca mientras cubre (su ataque es 4, el costo es bajo, y por eso la cobertura tiene que rendir), cubrir exige estar pegado (el escudero queda al alcance del rival), la salpicadura del área de la catapulta lo castiga (centro y área caen juntos sobre el escudero) y cada cobertura que se rompe le cuesta una pieza al defensor. No hay bucle positivo. Es la primera cosa a medir en playtest (Open Questions).

## Edge Cases

### Comandos y validación

- **If dos escuderos quieren cubrirse entre sí** (A cubre a B y después B envía `Cubrir(B, A)`): se rechaza con `EscuderoCubierto`, porque B ya es cubierto (código 6, antes de `AliadoYaCubre`). Quien quiera cambiar de rol quita la cobertura primero.
- **If un segundo escudero quiere cubrir a un aliado que ya tiene cubridor**: `AliadoYaCubierto`; no hay robo ni desplazamiento. El jugador quita la cobertura vieja con `QuitarCobertura` y vuelve a intentar.
- **If el escudero envía `Cubrir` sobre el aliado que ya cubre**: se acepta, no cambia nada y sale `CoberturaAsignada` con `aliadoAnterior` igual al aliado (igual que `AsignarAtaque` con el mismo objetivo, #7).
- **If `Cubrir` es válido sobre un escudero con ataque asignado**: el ataque se borra (`AtaqueQuitado`) y la cobertura se guarda, todo en un comando. **If es inválido**: queda el ataque y no pasa nada más.
- **If `QuitarAtaque` llega a un escudero que cubre y no ataca**: `SinAtaqueAsignado` y la cobertura sigue (#7, regla 10). Se quita con `QuitarCobertura`.
- **If `QuitarCobertura` llega a una pieza que no es un escudero, o a un escudero sin cobertura**: `SinCobertura`.
- **If `Cubrir` o `QuitarCobertura` llegan fuera de `Atacar` o en el turno del rival**: `FaseIncorrecta` o `NoEsTuTurno`, de la Librería, antes que cualquier código de este GDD.
- **If el aliado es un escudero que no cubre a nadie**: es válido. Pasa a ser cubierto y ya no puede cubrir en este turno (`AliadosCubribles` queda vacía y `Cubrir` da `EscuderoCubierto`); sí puede atacar.
- **If el aliado cubierto ataca, se mueve en un turno futuro o es objetivo de un ataque propio**: la cobertura no le quita ninguna acción. El fuego amigo sobre él cae sobre él, no sobre su escudero (regla 6).
- **If el escudero o el aliado fueron reclutados ese turno**: pueden cubrir y ser cubiertos como cualquier pieza (Reclutamiento, reglas 9 y 10).
- **If el aliado es el Rey**: se lo puede cubrir como a cualquier otra pieza (decisión del 2026-11-25).

### Resolución y daño

- **If un ataque directo, el centro o el área de una catapulta o una explosión caen sobre la pieza cubierta**: `receptor` es el escudero (F2), y los que ya apuntaban al escudero se suman a los desviados. Con 3 soldados (24 de bruto) sobre el arquero cubierto, el arquero no pierde nada y el escudero queda con 1 de vida.
- **If la catapulta apunta al aliado cubierto**: el centro (10) cae sobre el aliado y el área (6) sobre el escudero vecino, y `receptor` manda las dos a él: 16 sumados, menos 5 de escudo, 11 de daño. Es el contra-combo que el pilar 2 pide: el área castiga las coberturas.
- **If un demoledor rival explota junto al aliado cubierto y el escudero** (30 de ataque a cada vecina): los 30 del aliado van al escudero, que ya recibía sus propios 30: 60 menos 5 de escudo lo matan (vida 20) y el aliado no pierde nada. El demoledor solo mata al escudero, porque `piezasParaMatar` es 1 (excepción aprobada I1, Catálogo); el exceso se pierde.
- **If lo que cae sobre el escudero supera su vida**: muere y el exceso se pierde; no pasa al aliado. El aliado sale intacto de esa resolución (C1). Confirmado por Resolución (#9).
- **If el cubierto es un arquero sobre su tramo y le pegan**: la parte de la pieza va al escudero y el tramo recibe el golpe completo (Castillo, regla 8). El tramo puede romperse sin que el arquero sufra daño.
- **If el tramo bajo el arquero cubierto se rompe**: el arquero queda en esa casilla como arquero común (Castillo, regla 9). La cobertura **sigue**: ni su adyacencia ni la vida del arquero cambiaron. Solo pierde el bono de posición.
- **If el rival pega al escudero con un golpe que no lo mata y el aliado estaba en el mismo golpe de área**: el escudero recibe una sola suma (escudo restado una vez, #9). No hay dos descuentos de escudo.
- **If una cobertura vigente es de un jugador y el atacante es del mismo jugador** (fuego amigo): no se redirige. El jugador no puede usar a su escudero para absorber su propia explosión.

### Muertes y vigencia

- **If el escudero muere en una resolución en la que cubría**: la cobertura vale **durante toda esa resolución** (regla 10): todos los golpes que le correspondían ya se desviaron. Se borra al terminar, con `CoberturaQuitada` motivo `EscuderoMuerto`, después de los eventos de muerte.
- **If mueren el escudero y el aliado en la misma resolución** (por el fuego amigo de su propio turno, por ejemplo con su demoledor): sale un solo evento, `EscuderoMuerto`.
- **If el aliado muere y el escudero no**: motivo `AliadoMuerto`. Solo puede pasar por fuego amigo en la resolución de su propio turno, porque el daño enemigo a la pieza cubierta se desvía entero.
- **If el escudero muere en la resolución del rival y el Rey estaba cubierto**: el Rey salió intacto de esa resolución. Al abrir su turno, su dueño puede cubrirlo con otro escudero pegado, si lo hay y no está cubierto. Si no hay, el Rey queda expuesto en el turno siguiente del rival.
- **If se abre el turno de un jugador con coberturas**: se borran todas con motivo `Caducada`, en orden de ID de escudero, antes de que el estado pase a `EsperandoTirada` (Máquina, regla 4.1).
- **If llega `TurnTimeout` de `Atacar`**: las coberturas ya asignadas valen y se conservan hasta la apertura del siguiente turno propio (Máquina, reglas 11 y 12). No se asignan coberturas automáticas.
- **If la partida termina por `Rendirse` o `CerrarPartida` durante `Atacar`**: no se resuelve nada, y el estado `Terminado` conserva las coberturas (Librería, regla 11).
- **If la Distracción sale**: no hay fase de mover, se pasa a atacar. La cobertura funciona igual.

### Carga de estados

- **If se carga un estado con una cobertura que `Cubrir` rechazaría**: `EstadoInvalido` (Librería, regla 11). Eso incluye: un cubridor que no es de tipo 2 o está muerto; un aliado muerto, del rival, el propio escudero o fuera del contorno de 8; dos coberturas sobre el mismo aliado; una pieza en los dos roles; más coberturas que `coberturas_max` (F4); y coberturas del jugador del turno fuera de la etapa `Atacar` (las borra al abrir su turno).
- **If se carga un estado con un escudero del jugador del turno que ataca y cubre a la vez**: `EstadoInvalido` (#7, AC-26).
- **If se carga un estado con coberturas del rival del jugador del turno**: es válido, aunque la fase sea `EsperandoTirada` o `Mover`: son las que van a redirigir en esta resolución.

### Estrategias degeneradas revisadas

- **Cubrir al Rey cada turno (tortuga).** Con escuderos pegados al Rey, el rival tiene que gastar 25 de bruto por turno para quitarle uno, y el Rey nunca pierde vida mientras la cobertura esté vigente. No es una inmunidad: cada cobertura rota le cuesta una pieza al defensor, que tiene 4 al empezar y como mucho 12 si recluta todos, y un demoledor mata a un escudero solo. Es la primera cosa a medir contra el pilar 4 (Open Questions).
- **Cubrir siempre en lugar de atacar con 4.** El costo es bajo (un soldado pega 8). Lo contiene lo posicional: el escudero tiene que estar pegado al aliado, queda al alcance de todo el combo rival, y la catapulta y el demoledor lo castigan. Se vigila en playtest; si "cubrir" resulta siempre mejor, es un problema de números del Catálogo, no de esta regla.
- **Cubrir a una pieza que nadie va a atacar.** Es un movimiento perdido de 4 de ataque, sin ganancia: no es una estrategia.
- **Encadenar escuderos para extender el alcance de la cobertura.** Imposible por la regla 4: ninguna pieza cubre y es cubierta a la vez.

## Dependencies

**Depende de:**

| Sistema | Tipo | Interfaz | ¿El otro lo nombra? |
|---|---|---|---|
| #1 Librería de reglas | Dura | Lista cerrada de comandos (suma `QuitarCobertura`), orden de validación, códigos de la fila 14, eventos, forma canónica, `EstadoInvalido` | Sí |
| #2 Catálogo de piezas | Dura | ID de tipo 2 (escudero); `escudo` y `vida` solo en el oráculo F3 | Sí |
| #3 Máquina de turno | Dura | Fase `Atacar`, borrado de coberturas al abrir el turno (regla 4.1), el timeout las conserva, salto de fase | Sí |
| #4 Tablero | Dura | `ocupante`, contorno de 8 casillas | Sí |
| #5 Castillo y muros | Blanda | El tramo no se redirige (regla 8); el arquero sobre su tramo puede ser cubierto | Sí |
| #7 Alcance y asignación | Dura | Exclusión ataque/cobertura, evento `AtaqueQuitado`, `ObjetivosValidos` para la acción posible | Sí |
| #10 Reclutamiento | Blanda | Los reclutas pueden cubrir y ser cubiertos ese turno | Sí |

**Dependen de este sistema:**

| Sistema | Tipo | Qué toma |
|---|---|---|
| #1 Librería | Dura | Reglas, códigos y eventos de `Cubrir` y `QuitarCobertura` |
| #3 Máquina | Blanda | `AliadosCubribles` para el salto de fase; los eventos `Caducada` |
| #7 Alcance | Blanda | Evento `CoberturaQuitada` (motivo `Desplazada`) y la exclusión |
| #9 Resolución | Dura | `Coberturas()` y `receptor(pieza, origen)` (F2) |
| #13 Entrada, #16 Vista previa | Dura | `AliadosCubribles`, `CubiertoPor`, códigos, eventos |
| #25 Rival IA | Dura | `AliadosCubribles`, `CubiertoPor`, `Cubre` |
| #15 HUD, #18 Render, #19 Métricas, #23 Accesibilidad | Blanda | Eventos y líneas de cobertura a la vista |
| #14 Localización | Blanda | Textos de los códigos visibles (regla 14) |

**Supuestos provisionales:**

- ~~El reparto numérico de la cobertura y el momento de borrar las coberturas de piezas muertas~~: confirmados por Resolución (#9, reglas 7 y 14) al cerrar la tanda 3 (2026-11-25).
- **El evento `AtaqueQuitado` al desplazar un ataque con `Cubrir`** es un uso nuevo de un evento de #7.

## Tuning Knobs

Este sistema **no tiene perillas propias en un archivo de datos**: no define ninguna cifra de balance; toda cifra viene del Catálogo o de Resolución. Los números que más pesan sobre la cobertura están en otros GDD y se señalan aquí, no se duplican. Los rangos de abajo son **propuestas sin medir** hasta la compuerta de duración (pilar 4).

**Perillas heredadas (dueño: Catálogo, #2):**

| Perilla | Hoy | Rango seguro para esta regla | Si es muy alta | Si es muy baja | Interactúa con |
|---|---|---|---|---|---|
| `escudo + vida` del escudero (F3) | 25 (5 + 20) | 11 a 40 *(propuesta)* | Quitar una cobertura cuesta 5 o más soldados: tortuga alrededor del Rey, partidas largas (pilar 4) | 10 o menos: una sola pieza no demoledora la anula, y se rompe el chequeo C2 | `ataque` de las demás piezas (C2); `topeVivas` del tipo 2 |
| `ataque` del escudero | 4 | ≥ 1 (F2 del Catálogo); menor que el del soldado (8) *(propuesta)* | Cubrir pierde contra atacar: el escudero es un soldado y la cobertura no se usa | Cubrir es casi gratis: se cubre siempre y se vuelve la jugada por defecto | Costo de oportunidad de la regla 5 |
| `topeVivas` del tipo 2 | 12 | 4 a 12 (la partida inicial pide 4) | Más coberturas posibles (`coberturas_max`, F4) y tortuga más profunda | Con 4 se llega justo al despliegue inicial; menos no cabe | F4 |

**Perillas de otros GDD que cambian lo que esta regla vale:**

- `vida_tramo` y la regla 8 de Castillo: cuánto daño recibe el tramo que la cobertura no desvía.
- El reparto del exceso y la suma de daño (Resolución, #9): si el exceso pasara al aliado, la cobertura sería mucho más débil y C1 dejaría de valer.

**Variantes estructurales** (no son perillas: cambian reglas de este GDD y exigen una revisión, no un número):

| Variante | Qué cambia | Qué mirar |
|---|---|---|
| Radio 2 en lugar del contorno de 8 | F1 usaría `alcanceMax` (2) del escudero | Más formaciones posibles, el escudero sale del frente; más difícil de leer en pantalla táctil |
| Permitir cadenas con reenvío | Regla 4 y F2 | Alcance a 2 casillas, ciclos, vista previa con dos saltos |
| Excluir al Rey | Regla 2 | Menos tortuga, pero el escudero deja de proteger a la pieza que decide la partida |
| Duración fija en turnos de la cobertura | Regla 7 | Hoy dura una resolución del rival; más larga alarga el duelo |

## Visual/Audio Requirements

Principios del art bible: *El plan se lee antes que el detalle* (1), *El combo se ve como un solo golpe* (2) y *El ritmo del turno manda sobre la animación* (3). Secciones 3 (*Gramática de formas de la interfaz*), 4 (*Flechas de ataque*, *Daltonismo*) y 5 (*Pose y expresión*, el escudero medio escondido tras su escudo).

| Evento | Visual | Audio (dirección; lo define #22) |
|---|---|---|
| Escudero elegido en `Atacar` | Los aliados cubribles (`AliadosCubribles`) con una marca de **forma** propia, además de los objetivos de ataque; no depende del color | Toque corto |
| `CoberturaAsignada` | **Línea recta eslabonada**, acero, sin punta, entre el escudero y el aliado (art bible, Sección 4, *Flechas de ataque*); la línea aparece en el mismo cuadro en que se acepta el comando | Golpe metálico sordo, más grave que el de asignar un ataque |
| Reemplazo (`aliadoAnterior`) | La línea vieja desaparece en el mismo cuadro en que entra la nueva | Solo el sonido de asignar |
| `CoberturaQuitada`, `Quitada` o `Desplazada` | La línea se borra | Toque seco |
| La línea durante el turno del rival | **Queda visible para los dos jugadores** hasta que se borra. **Falta en el art bible**, que dibuja marcas solo del jugador del turno | — |
| Golpe desviado (vista previa y resolución) | La flecha **termina en el escudero**, no en el aliado, y se funde con las que ya iban a él (×1 / ×1,5 / ×2); la línea eslabonada se resalta mientras absorbe. **Falta en el art bible** | Golpe de escudo sobre el golpe combinado, que sube con la cantidad de atacantes |
| Aliado que sale intacto | Ninguna placa de daño sobre el aliado; la placa va sobre el escudero (vida que queda, o MUERE) | — |
| `CoberturaQuitada`, `EscuderoMuerto` / `AliadoMuerto` | La línea se rompe en el mismo cuadro de la caída | Se suma a la caída; sin sonido propio |
| `CoberturaQuitada`, `Caducada` | La línea se desvanece al abrir el turno del dueño | **Ninguno**: caducan varias a la vez y un sonido por línea sería ruido |
| Rechazo visible | Sacudida breve de la pieza y texto corto por código | Error suave |

**Restricciones:**

- Todo se distingue en grises: la cobertura es una línea recta eslabonada **sin punta** y la flecha es un arco continuo con punta y borde Tinta (art bible, Sección 4, *Daltonismo*).
- **Sin color de bando.** Los dos bandos usan la misma línea; se distinguen por la orientación de las figuras y por el emblema del HUD.
- **Sin animación nueva obligatoria.** El estado "cubriendo" lo lleva la línea y, si #15 lo decide, un ícono en la ficha. Una pose propia de "cubriendo" suma animaciones por pack (art bible, Sección 8), así que queda como opción de bajo costo: reusar la pose de reposo del escudero con el escudo adelantado, en 4 cuadros o menos.
- El desvío se anima **a la vez** que el resto de los golpes (principio 3): no agrega tiempo a la resolución.
- La columna de audio es una propuesta: no pasó por el lente de `audio-director`.

📌 **Asset Spec** — Visual/Audio requirements are defined. After the art bible is approved, run `/asset-spec system:cobertura-del-escudero` to produce per-asset visual descriptions, dimensions, and generation prompts from this section.

## Game Feel

- **Respuesta:** `Cubrir` y `QuitarCobertura` entran en `comando_presupuesto_ms` (1 ms p95); la línea aparece en el mismo cuadro en que se acepta el comando.
- **Dos toques y listo:** elegir al escudero y tocar al aliado. Sin confirmación para cubrir ni para quitar; reemplazar es un solo gesto (como asignar, #7). Con 13 piezas o más en 30 s, un gesto de más pesa (pilar 4).
- **Corregir no cuesta:** el jugador puede cubrir, cambiar de aliado y quitar la cobertura las veces que quiera hasta resolver.
- **La cobertura del rival cambia la vista previa desde el primer cuadro:** las flechas que apuntan a una pieza cubierta ya salen dibujadas hacia el escudero (pilar 1). La vista previa se recalcula dentro de `vista_previa_presupuesto_ms` (5 ms p95).
- **Peso del escudo (pilar 2):** el golpe combinado que cae sobre el escudero se siente más pesado que sobre una pieza común: la punta fundida crece y el sonido de escudo sube con la cantidad de atacantes. El aliado cubierto no recibe ninguna señal de daño.
- **Sin animación que bloquee:** se puede cubrir con otro escudero o trazar un ataque sin esperar a que termine una línea.
- **Verificación:** capturas retenidas de la línea, del desvío y del MUERE sobre un escudero; playtest con la pregunta "¿viste sobre quién iba a caer cada flecha antes de resolver?".

## UI Requirements

- **Aliados cubribles al elegir un escudero** en `Atacar`, marcados por forma y no solo por color, junto a sus objetivos de ataque. Un escudero cubierto no muestra ninguno; los aliados que ya cubren a otro o que ya tiene otro cubridor no se marcan.
- **La línea de cobertura se ve siempre**, durante `Atacar` del dueño y durante todo el turno del rival, para los dos jugadores.
- **Vista previa (#16):** la flecha dirigida a una pieza cubierta termina en su escudero. Si el golpe total mata al escudero, la placa MUERE va sobre él y el aliado se muestra intacto. Si el rival tiene piezas de área, muestra el centro y el área juntos sobre el escudero.
- **Ficha (#15):** una línea de texto "Cubre a [tipo]" o "Cubierto por [tipo]", para leerlo sin depender de la línea. Es una propuesta: el art bible no la define.
- **Quitar una cobertura:** hace falta un gesto propio para `QuitarCobertura`; Entrada (#13, regla 9): tocar de nuevo al aliado cubierto.
- **Rechazos:** texto por código desde Localización (#14) para `AliadoNoEsTuyo`, `AliadoLejos`, `EscuderoCubierto`, `AliadoYaCubre`, `AliadoYaCubierto` y `SinCobertura`. `NoEsEscudero`, `AliadoInexistente` y `AliadoEsElMismo` no deben salir por la interfaz.
- **Accesibilidad (#23):** la cobertura se distingue por forma (línea eslabonada, sin punta) en grises y con daltonismo, y su estado también está en texto en la ficha.

📌 **UX Flag — Cobertura del escudero**: This system has UI requirements. In Phase 4 (Pre-Production), run `/ux-design` to create a UX spec for each screen or HUD element this system contributes to **before** writing epics. Stories that reference UI should cite `design/ux/[screen].md`, not the GDD directly.

## Cross-References

| Qué | Documento | Dónde | Relación |
|---|---|---|---|
| Lista de comandos, orden de validación, fila 14, `EstadoInvalido`, eventos | `design/gdd/libreria-de-reglas.md` | Reglas 3, 4, 8, 10, 11, 15 | Completa; suma `QuitarCobertura` (cambio de contrato) |
| Borrado al abrir el turno, `Atacar`, salto de fase, cierre, `TurnTimeout` | `design/gdd/maquina-de-turno.md` | Reglas 4, 9, 10, 11, 12, 17 | Respeta; pide `QuitarCobertura` en la matriz de la fase |
| ID de tipo 2, `piezasParaMatar`, excepción I1 | `design/gdd/catalogo-de-piezas.md` | Core Rules, F2, I1 a I3 | Lee; F3 y C2 se apoyan en su matriz |
| Contorno de 8, `ocupante` | `design/gdd/tablero-y-despliegue-inicial.md` | Reglas 2 y 11 | Lee |
| El tramo no se redirige, arquero sobre el tramo, tramo roto | `design/gdd/castillo-y-muros.md` | Reglas 7, 8 y 9 | Respeta |
| Una acción por pieza, `QuitarAtaque`, evento `CoberturaQuitada`, acción posible | `design/gdd/alcance-y-asignacion-de-ataques.md` | Reglas 9, 10, 14, 15; Open Questions | Completa y cierra su pregunta sobre cómo se quita |
| Recién reclutados | `design/gdd/reclutamiento-y-limites.md` | Reglas 9 y 10 | Amplía "pueden atacar" a "pueden atacar o cubrir" |
| Gesto de cubrir, aliados cubribles, cómo se quita | `design/gdd/entrada-y-seleccion.md` | Regla 9, Edge Cases, Open Questions | Cierra su pregunta abierta |
| Escudero, un aliado por escudero, pilares | `design/gdd/game-concept.md` | Tabla de piezas; Pilares 1, 2 y 4 | Detalla |
| Línea eslabonada, flechas, daltonismo | `design/art/art-bible.md` | Secciones 4 y 5 | Aplica; señala lo que falta |

## Acceptance Criteria

Las piezas se nombran por tipo y casilla. Valores del Catálogo v5: soldado ataque 8 · arquero escudo 2, vida 15 · escudero ataque 4, escudo 5, vida 20 · catapulta ataque 10, área 6 · demoledor ataque 30 · Rey escudo 5, vida 30. Los criterios que dependían del reparto de daño de Resolución (#9) dejaron de ser PROVISIONAL al cerrar la tanda 3 (2026-11-25). Todos los de tipo **Logic** son tests automáticos de EditMode; los **Visual** y **UI** piden captura retenida en `production/qa/evidence/`.

**Comandos y validación**

- **AC-1 (regla 1, quién cubre)** *(Logic)* — **GIVEN** el jugador 0 en `Atacar`, un escudero en `(5, 5)` y un soldado propio en `(6, 5)`, **WHEN** envía `Cubrir(escudero, soldado)`, **THEN** se acepta, `Cubre(escudero)` es el soldado, `CubiertoPor(soldado)` es el escudero y sale `CoberturaAsignada(escudero, soldado, vacío)`; `movimientosUsados` y el tamaño de la pila de deshacer no cambian. **Y WHEN** el mismo jugador envía `Cubrir` con el soldado como actor, **THEN** `NoEsEscudero`.
- **AC-2 (regla 2, a quién)** *(Logic)* — **GIVEN** un escudero en `(5, 5)`, un aliado en `(6, 4)`, otro en `(7, 5)`, el Rey propio en `(5, 6)`, un arquero propio sobre su tramo pegado al escudero y un rival en `(4, 5)`, **WHEN** se envía `Cubrir` a cada uno, a sí mismo y a un ID inexistente, **THEN** se aceptan `(6, 4)` (diagonal), el Rey y el arquero sobre el tramo; y se rechazan `(7, 5)` con `AliadoLejos`, el rival con `AliadoNoEsTuyo`, el propio escudero con `AliadoEsElMismo` y el ID inexistente con `AliadoInexistente`.
- **AC-3 (regla 3, uno por uno)** *(Logic)* — **GIVEN** el escudero A que cubre a X, **WHEN** envía `Cubrir(A, Y)` con Y vecino, **THEN** sale `CoberturaAsignada(A, Y, X)`, `CubiertoPor(X)` queda en ninguno y no sale `CoberturaQuitada`. **GIVEN** Y cubierto por A y otro escudero B vecino de Y, **WHEN** B envía `Cubrir(B, Y)`, **THEN** `AliadoYaCubierto` y nada cambia. **Y WHEN** A envía `Cubrir(A, Y)` otra vez, **THEN** se acepta con `aliadoAnterior` igual a Y.
- **AC-4 (regla 4, sin cadenas)** *(Logic)* — **GIVEN** A que cubre al escudero B, **WHEN** B envía `Cubrir(B, C)`, **THEN** `EscuderoCubierto`; **WHEN** B envía `Cubrir(B, A)`, **THEN** también `EscuderoCubierto` (código 6, antes del 7). **GIVEN** B que cubre a X, **WHEN** A envía `Cubrir(A, B)`, **THEN** `AliadoYaCubre`. **Y GIVEN** que A quita su cobertura, **WHEN** B envía `Cubrir(B, A)`, **THEN** se acepta.
- **AC-5 (regla 5, una acción por pieza)** *(Logic, Integration #7)* — **GIVEN** un escudero con ataque asignado, **WHEN** `Cubrir` es válido, **THEN** salen `AtaqueQuitado` y después `CoberturaAsignada`, y su ataque ya no está en la lista de asignaciones; con un `Cubrir` inválido, el ataque sigue y no sale ningún evento. **GIVEN** un escudero que cubre, **WHEN** `AsignarAtaque` es válido, **THEN** salen `CoberturaQuitada` con motivo `Desplazada` y después `AtaqueAsignado`, y `Cubre` queda en ninguno; con un `AsignarAtaque` inválido, la cobertura sigue.
- **AC-6 (regla 8, `QuitarCobertura`)** *(Logic)* — **GIVEN** un escudero que cubre, **WHEN** envía `QuitarCobertura`, **THEN** sale `CoberturaQuitada(…, Quitada)`, `Cubre` queda en ninguno y no se usa la pila de deshacer; **WHEN** lo envía otra vez, o lo envía un soldado, **THEN** `SinCobertura`. **Y WHEN** llega en la fase `Mover`, **THEN** `FaseIncorrecta`; **y WHEN** llega en el turno del rival, **THEN** `NoEsTuTurno`.
- **AC-7 (regla 14, orden de códigos)** *(Logic)* — **GIVEN** casos con dos fallas a la vez, **WHEN** se envía `Cubrir`, **THEN** se devuelve el primero de la tabla: un aliado rival y lejano da `AliadoNoEsTuyo` (3, antes de `AliadoLejos`); un escudero cubierto y un aliado lejano dan `AliadoLejos` (5, antes de `EscuderoCubierto`); un aliado que cubre a otro y está cubierto por un tercero da `AliadoYaCubre` (7, antes de `AliadoYaCubierto`).

**Efecto y vigencia**

- **AC-8 (regla 6, desvío)** *(Integration #9)* — **GIVEN** un arquero del jugador 0 (escudo 2, vida 15) cubierto por un escudero vecino (escudo 5, vida 20) y tres soldados del jugador 1 apuntándole (24 de bruto), **WHEN** el jugador 1 resuelve, **THEN** el arquero queda con 15 de vida y el escudero con 1; **y GIVEN** el mismo caso sin cobertura, **THEN** el arquero muere (24 − 2 = 22 ≥ 15).
- **AC-9 (regla 6, suma y exceso)** *(Integration #9)* — **GIVEN** un arquero cubierto y 2 soldados sobre el arquero (16) más 1 sobre el escudero (8), **WHEN** se resuelve, **THEN** el escudero recibe 24 − 5 = 19 y queda con 1 de vida (si se restara el escudo dos veces, quedaría con 6). **Y GIVEN** 5 soldados (40) sobre el arquero cubierto, **THEN** el escudero muere, el exceso se pierde y el arquero conserva sus 15 de vida.
- **AC-10 (regla 6, área y explosión)** *(Integration #9)* — **GIVEN** un arquero cubierto y una catapulta rival con el centro (10) sobre el arquero y el área (6) sobre el escudero vecino, **WHEN** se resuelve, **THEN** el escudero recibe 16 − 5 = 11 y queda con 9; el arquero no pierde nada. **GIVEN** un demoledor rival (30) pegado a los dos, **THEN** el escudero recibe 60 − 5 y muere, y el arquero no pierde nada.
- **AC-11 (regla 6, límites)** *(Integration #5 y #9)* — **GIVEN** un arquero cubierto parado sobre su tramo (30 de vida) y 2 soldados rivales apuntándole (16), **WHEN** se resuelve, **THEN** el tramo queda en 14 (golpe completo), el escudero en 9 y el arquero en 15. **GIVEN** el jugador 0 que dispara su propia catapulta contra su arquero cubierto, **THEN** el arquero recibe el daño y el escudero no (fuego amigo).
- **AC-12 (regla 7, vigencia)** *(Integration #3)* — **GIVEN** el jugador 0 que asigna una cobertura en el turno T, **WHEN** resuelve sus propios ataques, **THEN** la cobertura no desvía nada (AC-11, fuego amigo); **WHEN** el jugador 1 resuelve en T+1, **THEN** desvía. **WHEN** se abre el turno T+2, **THEN** el jugador 0 no tiene coberturas y salieron `CoberturaQuitada(…, Caducada)` en orden de ID de escudero, antes de `EsperandoTirada`.
- **AC-13 (regla 9, muertes)** *(Integration #9)* — **GIVEN** una cobertura del jugador 0 y un demoledor propio que explota junto al escudero, **WHEN** resuelve el jugador 0 y el escudero muere, **THEN** `CoberturaQuitada(…, EscuderoMuerto)` sale **después** de los eventos de muerte; **y GIVEN** que mueren el escudero y el aliado, **THEN** sale un solo evento (`EscuderoMuerto`); **y GIVEN** que muere solo el aliado, **THEN** `AliadoMuerto`.
- **AC-14 (regla 10, estado al empezar)** *(Integration #9)* — **GIVEN** una cobertura del jugador 0 y ataques del jugador 1 que juntos matan al escudero y también apuntan al aliado, **WHEN** se resuelve, **THEN** todos los golpes sobre el aliado van al escudero (el aliado conserva su vida) y la cobertura se borra después de la resolución, no durante.

**Consultas y acción posible**

- **AC-15 (regla 12, consultas)** *(Logic)* — **GIVEN** coberturas de los dos jugadores en cualquier fase, **WHEN** se piden `CubiertoPor`, `Cubre` y `Coberturas()`, **THEN** valen para piezas de los dos bandos, `Coberturas()` sale ordenada por ID de escudero ascendente y un ID inexistente devuelve ninguno. **Y** para todo tablero generado, un aliado está en `AliadosCubribles(escudero)` **si y solo si** `Cubrir(escudero, aliado)` se aceptaría (propiedad); la lista sale por ID ascendente, sin repetir, y vacía para una pieza que no es un escudero, está muerta o está cubierta.
- **AC-16 (regla 13, acción posible)** *(Integration #3)* — **GIVEN** un único escudero con un aliado vecino y ningún enemigo en alcance, ninguna otra pieza con acción, **WHEN** se abre `Atacar`, **THEN** no se salta. **GIVEN** el mismo escudero sin aliados vecinos, **THEN** `Atacar` se salta.

**Estados, cierres y casos borde**

- **AC-17 (regla 15, eventos)** *(Logic)* — **GIVEN** un reemplazo, un quitar y una caducidad, **WHEN** se aceptan, **THEN** el reemplazo emite solo `CoberturaAsignada` con `aliadoAnterior`, el quitar emite `CoberturaQuitada(…, Quitada)` y las caducidades salen por ID de escudero ascendente.
- **AC-18 (determinismo)** *(Logic)* — **GIVEN** el mismo estado inicial y la misma secuencia de `Cubrir`, `QuitarCobertura` y `Resolver`, **WHEN** se aplica dos veces, **THEN** los eventos y la huella del estado son idénticos.
- **AC-19 (estado inválido)** *(Logic)* — **GIVEN** estados con (a) un cubridor que no es de tipo 2, (b) un aliado muerto, del rival o fuera del contorno de 8, (c) dos coberturas sobre el mismo aliado, (d) una pieza en los dos roles, (e) un escudero del turno que ataca y cubre, (f) coberturas del jugador del turno en la fase `Mover`, (g) más coberturas que `coberturas_max`, **WHEN** se cargan, **THEN** cada uno da `EstadoInvalido`; **y GIVEN** coberturas del rival en `EsperandoTirada`, **THEN** el estado es válido.
- **AC-20 (cierres)** *(Integration #3)* — **GIVEN** una cobertura asignada, **WHEN** llega `TurnTimeout` de `Atacar`, **THEN** el turno termina y la cobertura se conserva; **WHEN** llega `Rendirse` durante `Atacar`, **THEN** el estado `Terminado` la conserva y no se resuelve nada.
- **AC-21 (reclutas y Rey)** *(Integration #10)* — **GIVEN** un escudero reclutado ese turno y un aliado reclutado ese turno, **WHEN** el escudero envía `Cubrir`, **THEN** se acepta. **GIVEN** el Rey (escudo 5, vida 30) cubierto y 100 de bruto del rival sobre él, **WHEN** se resuelve, **THEN** el Rey conserva sus 30 de vida y el escudero muere *(#9)*.
- **AC-22 (tramo roto)** *(Integration #5)* — **GIVEN** un arquero cubierto sobre su tramo, **WHEN** el tramo se rompe en la resolución y el arquero sobrevive, **THEN** la cobertura sigue y el arquero pierde solo el bono de posición.

**Fórmulas**

- **AC-23 (F1, `en_contorno`)** *(Logic)* — **GIVEN** un escudero en `(5, 5)`, **WHEN** se evalúa con `(6, 4)`, `(7, 5)` y `(5, 5)`, **THEN** verdadero, falso y falso. **Y GIVEN** un escudero en `(0, 0)`, **THEN** `(1, 1)` es verdadero y `(2, 0)` es falso. **Y** con el escudero en `(5, 5)` y el aliado en `(7, 5)` a pesar de `alcanceMax` 2, `Cubrir` da `AliadoLejos`: la cobertura no usa el alcance.
- **AC-24 (F2, `receptor`)** *(Logic)* — **GIVEN** `Coberturas()` vacía, **THEN** `receptor(p, o) = p`; **GIVEN** `(e, p)` con `o` del rival, **THEN** `e`; **GIVEN** `o` del mismo jugador que `e`, **THEN** `p`. **Y** para todo estado válido generado, `receptor(receptor(p, o), o) = receptor(p, o)` (propiedad).
- **AC-25 (F3, oráculo y C1/C2)** *(Logic)* — **GIVEN** escudo 5 y vida 20, **THEN** `bruto_para_romper_cobertura` es 25; con (0, 1) es 1; con (1.000, 1.000) es 2.000. **GIVEN** una cobertura vigente, **WHEN** el rival descarga 1.000.000 de bruto sobre el cubierto, **THEN** su vida no cambia *(C1, #9)*. **GIVEN** el Catálogo vigente, **THEN** `piezasParaMatar(a, escudero)` es 4, 5, 7, 3, 4, 4 y 1 para soldado, arquero, escudero, catapulta, jinete, Rey y demoledor, y ningún atacante que no sea el demoledor da 1 *(C2)*.
- **AC-26 (F4, `coberturas_max`)** *(Logic)* — **GIVEN** `(escuderos, vivas)` = (4, 13), (1, 2), (1, 1), (12, 24) y (0, 5), **THEN** 4, 1, 0, 12 y 0.

**Presentación**

- **AC-27 (Visual)** *(Visual, captura)* — **GIVEN** una cobertura asignada y el turno que pasa al rival, **WHEN** se captura la pantalla en los dos turnos, **THEN** la línea eslabonada sin punta entre las dos piezas se ve en los dos y se distingue de una flecha en escala de grises y con los filtros protan, deutan y tritan.
- **AC-28 (UI, desvío)** *(UI, captura)* — **GIVEN** un aliado cubierto y ataques rivales apuntándole, **WHEN** el jugador ve la vista previa, **THEN** las flechas terminan en el escudero con la punta fundida y el aliado no tiene placa de daño; con un golpe letal, la placa MUERE está sobre el escudero.
- **AC-29 (UI, rechazos)** *(UI, Integration #14)* — **GIVEN** los códigos de la regla 14, **WHEN** se provocan desde la interfaz, **THEN** `AliadoNoEsTuyo`, `AliadoLejos`, `EscuderoCubierto`, `AliadoYaCubre`, `AliadoYaCubierto` y `SinCobertura` muestran un texto de Localización; `NoEsEscudero`, `AliadoInexistente` y `AliadoEsElMismo` no aparecen en pantalla.
- **AC-30 (rendimiento)** *(Logic)* — **GIVEN** un estado con `coberturas_max` coberturas por jugador, **WHEN** se aplican `Cubrir` y `QuitarCobertura`, **THEN** cada comando entra en `comando_presupuesto_ms` (1 ms p95).
- **AC-31 (casos borde de E)** *(Integration #3)* — **GIVEN** que sale la Distracción (no hay fase de mover), **WHEN** el jugador pasa a `Atacar` y envía `Cubrir`, **THEN** se acepta como en cualquier turno. **GIVEN** un escudero B cubierto por A, **WHEN** se piden `AliadosCubribles(B)` y `ObjetivosValidos(B)`, **THEN** la primera es vacía y B conserva sus objetivos de ataque; `AsignarAtaque(B, …)` válido se acepta y no toca la cobertura de A.

**Cobertura:** cada regla (1–17) tiene criterio, con estas correspondencias: 1 → AC-1; 2 → AC-2; 3 → AC-3; 4 → AC-4; 5 → AC-5; 6 → AC-8 a AC-11; 7 → AC-12; 8 → AC-6; 9 → AC-13; 10 → AC-14; 11 → AC-15 (consultas públicas, de los dos bandos); 12 → AC-15; 13 → AC-16; 14 → AC-7; 15 → AC-17; 16 → AC-14 y AC-18; 17 → AC-1, AC-5, AC-6, AC-12 y AC-31. Cada fórmula (F1–F4) tiene criterio, con un valor típico y uno en el borde. **Sin criterio automático:** el "peso" del golpe y la lectura de la línea a ojo se verifican en playtest (Open Questions).

## Open Questions

| Pregunta | Dueño | Cuándo |
|---|---|---|
| El reparto numérico (se suma al bruto del escudero, escudo una sola vez, el exceso se pierde) y el lugar del borrado de coberturas de piezas muertas dentro del cierre de la resolución | Resolución (#9) | En su GDD |
| ¿La tortuga alrededor del Rey (cubrir cada turno) alarga las partidas? Medir turnos con el Rey cubierto y duración de esas partidas | Métricas (#19), playtest | MVP, contra la compuerta del pilar 4 |
| ¿Cubrir resulta siempre mejor que atacar con 4? (el escudero ataca 4, el soldado 8) | Playtest; Catálogo (#2) si hay que tocar números | MVP |
| ¿Castiga el área de la catapulta (16 sumados sobre el escudero) lo suficiente a la cobertura, o hace falta más contrajuego? | Playtest, Resolución (#9) | MVP |
| ¿Se entiende a ojo sobre quién cae cada flecha? ¿Se lee la línea eslabonada sin depender del color? | Vista previa (#16), playtest | MVP |
| El art bible no define la línea de cobertura durante el turno del rival, el desvío de la flecha hacia el escudero ni una pose de "cubriendo" | Art bible (`art-director`) | Antes de producir arte |
| ¿Hace falta un paso del tutorial para cubrir, y qué le muestra al jugador novato? | Tutorial (#24) | En su GDD |

**Factibilidad:** regla pura de C#, sin superficie de motor; dominio Scripting, cubierto por `docs/engine-reference/unity/modules/core.md` (C# 9.0, .NET Standard 2.1: sin `init`-only setters ni `record` en tipos serializados, así que `Coberturas()` va como lista de structs de solo lectura). Todas las cifras de este GDD caben holgadas en 32 bits (la mayor, F3, llega a 2.000), así que lo que `core.md` marca como NOT SOURCEABLE (aritmética `checked` bajo IL2CPP) no cambia el diseño. `breaking-changes.md` no tiene entradas de C# puro. No hay ADR todavía.

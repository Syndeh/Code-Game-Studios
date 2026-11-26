<!-- PROTOTYPE - NOT FOR PRODUCTION -->
<!-- Reglas tal como las juega el prototipo v5 (Assets/Scripts/RolchessV5.cs), 2026-11-26. -->

# Cómo se juega (prototipo v5)

Dos jugadores en la misma computadora, por turnos, con el ratón: **Azul** (jugador 1) contra
**Rojo** (jugador 2). Empieza Azul.

## El objetivo

**Matar al Rey rival.** También se gana si al rival se le acaba su reloj de partida o si se
rinde. Si en una misma resolución mueren los dos Reyes, gana el que atacó.

## El tablero y el ejército

- Tablero de **24 × 15 casillas**. Cada castillo es un bloque de 5 × 5: el borde son
  **16 tramos de muro** de 30 de vida cada uno, y adentro queda un interior de 3 × 3 con el Rey
  en el centro.
- Los castillos van **siempre en el mismo lugar**: Azul a la izquierda y Rojo a la derecha, a
  media altura (los sectores 4 y 6 si se numera el tablero como el teclado de un teléfono).
- Cada jugador empieza con **13 piezas**: el Rey y, fuera del castillo, tres filas de 4 que
  miran al rival: 4 arqueros atrás, 4 soldados en el medio y 4 escuderos adelante.

## Las piezas

| Pieza | Mueve | Alcance | Ataque | Escudo | Vida | Máximo vivas |
|---|---|---|---|---|---|---|
| Soldado | 4 | 1 | 8 | 3 | 25 | 12 |
| Arquero | 4 | 1–4 (1–6 sobre su muro) | 6 | 2 (4 sobre su muro) | 15 | 16 |
| Escudero | 4 | 1–2 | 4 | 5 | 20 | 12 |
| Catapulta | 2 | 2–6 | 10 en la casilla, 6 alrededor | 2 | 15 | 8 |
| Jinete | 6 | 1 | 8 | 3 | 20 | 8 |
| Demoledor | 6 | sus 8 vecinas (explota) | 30 | 1 | 10 | 4 |
| Rey | 6 | 1 | 8 | 5 | 30 | 1 |

En la ficha de cada pieza: la letra del tipo, **V** vida, **A** ataque y **D** defensa (el
escudo).

## Cómo es un turno

1. **El Modificador.** Al empezar el turno se tira solo un dado de 5 caras que cambia lo que
   puedes hacer (ver abajo). En el **primer turno de la partida no se tira**: Azul mueve y
   ataca, sin reclutar.
2. **Reclutar**, si salió una cara de reclutar.
3. **Mover.** Puedes mover hasta **12 piezas, y además el Rey** (el Rey no cuenta en las 12).
   Cada pieza se mueve una sola vez por turno. Puedes **deshacer** los movimientos del turno,
   del último al primero, mientras sigas en la fase de mover. Cuando termines, pulsa
   **Terminar movimiento → Atacar**.
4. **Atacar.** Asignas los ataques de todas las piezas que quieras (cada una ataca una vez) y
   pulsas **Resolver ataques y terminar turno**. Todos los golpes caen **a la vez**.

### Las caras del Modificador

| Cara | Qué hace |
|---|---|
| **Distracción** | No mueves este turno: pasas directo a atacar |
| **Grito de guerra** | +2 de ataque y +2 de movimiento a todas tus piezas, solo en este turno (también suma +2 al daño de área de la catapulta) |
| **Llamada al servicio** | Reclutas 4 piezas comunes de un tipo: soldados, arqueros o escuderos |
| **Cuerno de guerra** | Reclutas especiales: 4 jinetes, 2 catapultas o 1 demoledor |
| **Arremetida** | Mueves todas las piezas que quieras, sin el límite de 12 |

Las probabilidades dependen de cómo vas: si tienes muchas menos piezas que el rival, salen más
caras de reclutar; si vas ganando, más Grito de guerra y Arremetida. Se pueden consultar en el
panel de información.

- **Reclutar es obligatorio:** si sale la cara, eliges un tipo. Las piezas aparecen en las
  casillas libres pegadas a tu castillo, primero del lado que mira al rival. Si el tiempo de
  mover se acaba mientras eliges, se recluta solo el primer tipo disponible.
- **Hay un tope de llamadas por partida:** Llamada al servicio sirve 6 veces; Cuerno de guerra,
  4 veces para catapultas, 3 para jinetes y 4 para demoledores. Cuando se gastan, o cuando
  tienes **2 tramos de tu muro derribados**, esa cara ya no sale y su probabilidad pasa a Grito
  de guerra y Arremetida.
- **Arremetida** solo sale si tienes **más de 12 piezas además del Rey**; si no, su
  probabilidad va entera a Grito de guerra.

## Moverse

- Se camina en **línea recta, sin diagonales**, una casilla por paso, hasta el movimiento de la
  pieza. Las casillas verdes muestran a dónde puede llegar.
- Puedes **atravesar a tus propias piezas**, pero no terminar encima de una.
- Las piezas rivales **bloquean el paso**. El **jinete** es la excepción: salta por encima de
  ellas.
- **Los muros bloquean** a casi todos. Solo dos piezas tienen permiso:
  - **El Rey** atraviesa sus propios muros como si no estuvieran, pero no puede quedarse
    encima de uno.
  - **El arquero** puede **subirse a un tramo libre de su propio muro**. Ahí termina su
    movimiento.
- Para entrar a un castillo, el resto de las piezas necesita que un tramo esté **roto**: el
  tramo roto deja un hueco por donde se pasa.

## Atacar

- Clic en tu pieza y después en su objetivo (las casillas rojizas muestran el alcance). Clic
  otra vez en la pieza para quitar su flecha.
- El alcance se mide **en todas las direcciones**, también en diagonal, y la zona es redonda:
  las esquinas más lejanas quedan afuera.
- Se puede atacar a **piezas rivales** y a **tramos del muro rival**.
- **Ataque combinado:** si varias piezas atacan al mismo objetivo, sus ataques **se suman** y
  el escudo se resta **una sola vez** del total. Ejemplo: dos soldados (8 + 8 = 16) contra un
  escudero (escudo 5) le quitan 11 de vida. Por separado, cada uno le quitaría solo 3.
- **Daño = ataque total − escudo** (nunca baja de 0). Una pieza con vida 0 o menos muere.
- Los muros no tienen escudo: reciben el daño completo. Un tramo con 0 de vida se rompe y deja
  un hueco.

### Las piezas especiales

- **Arquero sobre el muro.** Subido a un tramo de su propio muro, el arquero gana **+2 de
  escudo y +2 de alcance** (de 1–4 pasa a 1–6) y ataca desde ahí. Pero un golpe a esa casilla
  lo reciben **los dos**: el arquero (que resta su escudo) y el tramo de muro que está debajo.
- **Catapulta: daño en área.** Apunta a **cualquier casilla** de su alcance, aunque esté
  vacía. Hace **10 de daño en esa casilla y 6 en las 8 casillas de alrededor**. El área golpea
  todo lo que haya: piezas rivales, **tus propias piezas** y muros. No puede disparar a las 8
  casillas pegadas a ella (su alcance empieza en 2).
- **Demoledor: explota.** No elige objetivo: clic en él mismo y **explota** al resolver. Hace
  **30 de daño en sus 8 casillas vecinas**, a todo lo que haya, **tus piezas incluidas**, y el
  demoledor muere. Es la pieza para abrir muros: un tramo tiene 30 de vida, así que la
  explosión rompe cada tramo que toque.
- **Escudero: cubre a un aliado.** En la fase de ataque, en lugar de atacar, el escudero puede
  **cubrir a un aliado que esté a un cuadro** (también en diagonal; las casillas celestes).
  Durante el turno siguiente del rival, **todo ataque enemigo a ese aliado lo recibe el
  escudero**, con su propio escudo (5), y el aliado sale intacto. También desvía el daño de
  área de la catapulta y la explosión del demoledor.
  - Cada aliado puede tener **un solo escudero** cubriéndolo, y cada escudero cubre a uno.
  - La cobertura solo funciona si el escudero sigue vivo y **a un cuadro del aliado**.
  - No protege del fuego amigo: tu propia catapulta o tu demoledor le pegan igual al aliado.
  - La cobertura dura hasta que empieza tu próximo turno.
  - Si no cubre, el escudero ataca como cualquier pieza, a 1 o 2 casillas.

## El castillo y los muros

- Cada castillo tiene 16 tramos, con 30 de vida cada uno.
- Con **2 tramos de tu muro derribados ya no puedes reclutar**: las caras de reclutar dejan de
  salir. El panel muestra cuántos muros rotos tiene cada uno.
- Los tramos rotos no se reparan.

## El tiempo

- **Timer por etapa.** Cada turno tiene un tiempo para mover (reclutar cuenta como tiempo de
  mover) y otro para atacar. Se elige al empezar:

  | Modo | Mover + atacar |
  |---|---|
  | Variable 90 → 60 | 60 + 30 s; desde el minuto 10 de partida, 40 + 20 s |
  | Fijo 90 | 60 + 30 s toda la partida |
  | Fijo 60 | 40 + 20 s toda la partida |
  | Sin límite | Sin tiempo por turno y sin reloj de partida |

  Si se acaba el tiempo de mover, pasas a atacar. Si se acaba el de atacar, se resuelven los
  ataques que ya asignaste y termina el turno.
- **Reloj de partida.** Además, cada jugador tiene **12:30 en total**, que corre solo durante
  su turno. **Si llega a 0, ese jugador pierde**, en cualquier momento. Se ven los dos relojes.
- Si no te queda nada para mover o atacar, el turno avanza solo a los 2 segundos.

## Rendirse

El botón **Rendirse** solo aparece para el jugador del turno y solo cuando tiene **la mitad de
las piezas del rival o menos**. Pide confirmación. El que va ganando nunca lo ve.

## Al terminar

El panel muestra quién ganó y por qué (Rey muerto, reloj agotado o rendición), los relojes, la
vida quitada al rival y la duración. Cada partida se guarda en el archivo de métricas (ver el
[README](README.md)).

# Tanda B de `/design-review`: pedidos a sistemas sin GDD y a los ADR

> Cierre de la tanda B (2026-11-28). Máquina de turno (#3), Castillo y muros (#5) y Timer de turno (#12) quedaron Approved. Esta lista junta lo que esos tres GDD (y sus revisiones) les piden a sistemas que todavía no tienen GDD, y a los ADR que todavía no existen. Quien escriba cada GDD o ADR la lee primero. Lo que va a GDD que ya existen se aplicó en ellos, o quedó en sus Open Questions como «Pedidos de la tanda B».
>
> Fuentes: los logs `maquina-de-turno-review-log.md`, `castillo-y-muros-review-log.md` y `timer-de-turno-review-log.md`, y las notas de pendientes de las sesiones de revisión. Donde dos pedidos chocaban, vale el más nuevo. Lo anulado no figura: la placa que bloquea, el piso 1.442 de `T_tope` y el rechazo de los impares.

## #28 Servidor autoritativo de partida

Todo lo de abajo es **PROVISIONAL** hasta que exista el GDD del Servidor. Algunas cosas se deciden con el jugador.

**Gracia por latencia (g)**
- Cuánto vale g y si vale para la etapa además del reloj. Se cobra al reloj hasta 0 (corrimiento fijo del vencimiento). Si g también corre la etapa, la prioridad entre reloj y etapa se decide por los instantes crudos (V ≤ E ⇒ vale el reloj, sin `TurnTimeout`), aunque el envío salga en V + g.
- La garantía online es `T_tope` + 2g, una vez por jugador. Si g aplica a las etapas, `seg_turno_t` de D1 puede llegar a `turno_max_s` + 2g, y para quien cerró dentro de la gracia k = N − 1.
- Después de un cierre aceptado dentro de la gracia, la autoridad **no** envía `RelojAgotado` en V + g (daría `CierreInvalido`). El jugador sigue con el reloj en 0 durante el turno del rival y pierde al armarse su etapa siguiente. **A decidir con el jugador:** si un cierre dentro de la gracia que deja el reloj en 0 tiene que terminar la partida antes.
- Si una etapa se arma con el reloj ya en 0, también dentro de la gracia [V, V+g), vence en el instante del armado y corta lo que quedaba de gracia (decisión del jugador, 8.ª pasada de la Máquina).
- Un `Rendirse` del rival con hora dentro de la gracia se aplica como Rendición solo con la **misma** etapa que estaba armada al llegar a 0, es decir, el mismo par (numeroTurno, etapa). Si vale, gana azul con el reloj ya en 0, lo contrario de AC-44(i) en local. **A decidir con el jugador.**
- AC online de la gracia (van en la suite de #28): el corte de la gracia en el armado; el `Rendirse` con la misma etapa o con una etapa nueva; el orden de los eventos visibles cuando una etapa se arma por cascada dentro de la gracia y sale `RelojAgotado` en el mismo instante (con `EtapaIniciada` o sin ella).

**Turno 1 y `listo_max_s`**
- Que el primer comando de fase aceptado de azul también arme la etapa, o que no se acepte antes de armarla. Si no, azul mueve gratis hasta que llegan los dos «listo», y puede retener su «listo» para pensar. Se recomienda armar con el «listo» **de azul** y su primer comando, con un tope. Después hay que revisar el texto del Timer, regla 17, que hoy dice «los dos listos o `listo_max_s`».
- `listo_max_s` tiene una sola salida. Cubrir también el caso en que no confirma ninguno (Tablas, o no crear la partida). Falta fijar su orden respecto de `CrearPartida`.
- Un «tablero visible» explícito para el turno 1 en tests de EditMode, IA contra IA y servidor headless, o que lo arme el primer comando de azul (también va al ADR del Timer).

**La placa y la presentación online**
- La espera de la placa se cuenta desde el envío más la latencia estimada, o la cubre g. La placa se acorta en el cliente por la latencia de bajada (Timer, regla 15).
- La etapa se arma con el reloj del servidor (placa o primer comando).
- Si la `Tirada` llega mientras el cliente todavía presenta la cosecha, el contorno cambia cuando la placa ya empezó, así que la regla «nunca después de la placa» no se cumple (con Render, #18).

**Orden, auditoría y comandos**
- El servidor fecha cada comando al recibirlo (sigue PROVISIONAL). Registrar la hora del servidor de cada comando y la g aplicada, para auditar `RelojAgotado`.
- La autoridad nunca envía `TurnTimeout` de una etapa que no está armada.
- `CerrarPartida(Abandono)` va en la misma cola, ordenada por hora, que los vencimientos.
- El diálogo de Rendirse online fuera de turno (con #15).
- AC-31 de la Máquina: el caso online fuera de turno, con la autoridad de prueba.

## #22 Audio y SFX

- El cue de cambio de fase en `FaseIniciada` después de un timeout de mover (cubre el silencio de `TiempoAgotado`; lo pide el Timer en Visual/Audio). Hoy la Máquina solo define el cue de cambio de turno.
- La prioridad entre el cue de cambio de turno, el del renglón «Tramo final» y el de la placa, cuando caen en el mismo cuadro. El cue de tramo final va en una capa del sonido de la placa (con #17).
- El sonido de la cara que cierra tiene que ser audible al terminar la placa (con #17).
- Los golpes del tictac: uno por cada cambio de `venceEn_s` entre 1 y el umbral, nunca en 0, uno por cuadro, y el nivel 2 reemplaza el golpe de su valor. El Timer solo prueba el campo, así que el AC es de #22.
- La señal de ROTO suena cuando aparece la placa ROTO en la vista previa, una vez por transición (#16), no «al confirmar la vista previa» (art bible §4, ya corregido). El peso de ROTO no depende de `atacantesRivales`, y el golpe que cruza el corte lleva un acento propio (igual que en #18).
- Cuando una resolución rompe varios tramos y cruza el corte, una sola señal de corte tapa a los derrumbes. Si la señal de corte no suena (partida terminada, resincronización), decidir si suenan los derrumbes que habría tapado o manda el audio del final.

## #23 Accesibilidad

- Anunciar `TurnoIniciado` y `FaseIniciada` con el bando, la cara de la placa y «reloj detenido». Leer «Arquero → Rey» como «Arquero a Rey». Fijar un tiempo mínimo de lectura de las leyendas según su largo.
- Definir si se fusionan `hud.aviso_reloj_vinculante` y el `AvisoReloj` de nivel 1 del Timer cuando caen en el mismo turno.
- Una opción que alargue la espera de la placa para quien usa lector de pantalla (Timer, Open Questions).
- Anunciar el tiempo a pedido (los dos relojes; con #13 y #15). El lector anuncia solo el nivel 1, el nivel 2, `TramoFinalIniciado` (con la placa) y `TiempoAgotado` del jugador activo. Falta saber si Unity cubre el lector de pantalla en Web.
- Una descripción en texto, para lector de pantalla, del contador de tramos rotos, de «Sin reclutamiento» y de *Debilitado* (Castillo, UI Requirements).
- Enfocar e inspeccionar tramos sin puntero (teclado y mando): decidir con Entrada (#13) si entra al MVP. El HUD promete teclado y mando en cada control (#15, l.545), y Entrada dice que en el MVP el teclado no hace nada.

## #24 Tutorial básico

- El daño compartido: el golpe al arquero en el muro también daña el tramo, y el escudero no cubre esa parte.
- «2 tramos rotos, cualesquiera = sin reclutamiento».
- El gesto subir → mirar el alcance → `Deshacer`, y un tramo que cubre y otro que no (los seis tramos de Castillo, H4).

## ADR del Timer

- La firma del servicio: antes del comando, después del aceptado y en el tick. int64; nunca acumular `Time.deltaTime`; una cola sin reentrada.
- El orden entre los eventos del Timer y los de la Librería y la Máquina de un mismo Aceptado (por ejemplo `FaseIniciada(Atacar)` frente a `EtapaIniciada(atacar)`). La vista se publica después de la respuesta.
- El adaptador de la fuente: combina el tiempo monotónico con la hora de pared y nunca retrocede. Compensación del sueño `max(0, Δpared − Δmonotónica)`, solo después de una suspensión detectada.
- En Web: un `.jslib` de visibilidad y Run In Background; que la fuente avance con la página oculta o el equipo dormido (por plataforma); nada de `Task.Delay` ni `System.Threading.Timer`. Wake Lock en `Corriendo` (móvil).
- El tick del Timer corre antes de la lectura del HUD.
- Un harness de builds para AC-24 (ADVISORY), un perfil de config de pruebas en el Editor, los tests y los builds de desarrollo, y un analizador o una prueba de IL para AC-1b.
- Teclado y mando durante la placa van al tablero, porque la placa no bloquea (con #13 y #23).
- Un «tablero visible» explícito para el turno 1 en tests y servidor headless (ver #28).

## ADR de la Librería y técnico

- Cómo se garantiza el dominio de las constantes del Castillo: test de contrato (Castillo AC-17b), comprobación al compilar con `const`, o las dos. Qué hace el envoltorio público ante la falla inalcanzable, e `InternalsVisibleTo` para probar F5 y F7.
- El resultado tipado de `tramosRotos` y `puedeReclutar` no reserva memoria (struct; Castillo AC-16b).
- El nombre del código de config de la pila con el Grito (propuesto `PilaGrito`) y la ortografía del enum de motivo (`Rendicion` o `Rendición`).

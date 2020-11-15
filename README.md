# IA
Scripts necesarios para la funcionalidad del proyecto de IA

## Lógica del Test (responder.cs)
1. Por cada nivel habra 5 preguntas, organizadas de menor a mayor dificultad y con la siguiente puntuacion (1,2,2,2,3) 
2. El niño necesitara una puntuacion minima de 6 para poder aprobar y desbloquear el nivel siguiente.
3. Despues de que se formule una pregunta habra un contador con duracion de 40 segundos que ira disminuyendo SOLO SI el niño aun no ha elegido una opcion, cada vez que este contador llegue a 0 pasara lo siguiente:

- Se volvera a formular la pregunta.
- Se le sumara 1 al numero de veces que se formulo dicha pregunta.
- El contador se reiniciara (volviendo a 40 segundos). 

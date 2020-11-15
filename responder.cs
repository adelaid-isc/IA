using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Android;
using TextSpeech;
using UnityEngine.UI;
using UnityEngine;


[RequireComponent(typeof(AudioSource))]


public class responder : MonoBehaviour
{   

    [SerializeField]private AudioClip r_correcta = null; 
    [SerializeField]private AudioClip r_incorrecta = null;

    // Recurso de audio necesario para que el nivel pueda reproducir los audio clips
    [SerializeField]private AudioSource audioSource = null;

    // Arreglo con todos los audios que contienen las preguntas
    public AudioClip[] vozpreguntas;

    // Con esto se ira recorriendo el arreglo de audios que formulan las preguntas
    public int idAudio = 0;
    
    // Muestra en pantalla el tiempo transcurrido 
    public Text txtTiempo; 

    // Se inicializa el tiempo en 40 segundos 
    public float tiempo = 40f;

    // El tiempo no se encuentra corriendo
    public bool tiempoCorriendo = false;

    // Variables que llevan la cuenta de las veces en que se le repitio al niño cada pregunta
    private int cP1, cP2, cP3, cP4, cP5 = 0;

    // Registra cuantas veces se respondio correcta e incorrectamente una pregunta en especifico
    public int[] rC, rI;

    // Contiene los sprites por default de cada boton, asi como el sprite de cuando una respuesta seleccionada 
    // es correcta o incorrecta
    public Sprite btn_correcto, btn_incorrecto, voz_correcto, voz_incorrecto, defA, defB, defC, defD;

    // Se inicializan los botones para responder
    public Button btnA, btnB, btnC, btnD;

    // El numero del cuento o nivel que se esta jugando
    private int idCuento;

    // Imprime en pantalla las palabras detectadas con el reconocimiento de voz
    public Text txtEscucha;

    // Indicamos el idioma que tendra que procesar el reconocimiento de voz
    const string LANG_CODE = "es-MX"; 

    // Muestra en pantalla las preguntas, respuestas e informacion que nos indica en que pregunta estamos
    // y cuantas preguntas son en total
    public Text pregunta, respuestaA, RespuestaB, RespuestaC, infoRespuestas;

    //Arreglo que contiene todas las preguntas, respuestas y opciones correctas
    public string[] preguntas, opcionA, opcionB, opcionC, opcionCorrecta;

    // Esta opcion corresponde a la "Voz" y la cadena guardara aquello que digamos
    public string opcionD; 

    // Esta variable registra la pregunta en que nos encontramos, de esta manera podremos ir recorriendo
    // el arreglo de preguntas y respuestas sin necesidad de crear una escena nueva por pregunta
    private int idPregunta;

    // almacenamos los aciertos obtenidos, el total de preguntas que hay en el nivel
    // y la media que corresponde a -> 10*(aciertos / totalPreguntas)
    private float aciertos, totalPreguntas, media;
    
    // Guardamos el puntaje obtenido en el cuento o nivel actual 
    private int notaFinal;


    // Start es llamado antes de la primera actualizacion de frames
    void Start()
    {   
        // Se muestra en pantalla el tiempo restante antes de que la pregunta se vuelva a formular
        txtTiempo.text = " " + tiempo;

        // El tiempo empieza a disminuir
        tiempoCorriendo = true;

        // Guardamos las preguntas en las PlayerPrefs
        PlayerPrefs.SetString("p1", preguntas[0]);
        PlayerPrefs.SetString("p2", preguntas[1]);
        PlayerPrefs.SetString("p3", preguntas[2]);
        PlayerPrefs.SetString("p4", preguntas[3]);
        PlayerPrefs.SetString("p5", preguntas[4]);

        // Asignamos los valores almacenados en las PlayerPrefs correspondientes al 
        // numero de veces que se le formulo cada pregunta al niño
        cP1 = PlayerPrefs.GetInt("cP1");
        cP2 = PlayerPrefs.GetInt("cP2");
        cP3 = PlayerPrefs.GetInt("cP3");
        cP4 = PlayerPrefs.GetInt("cP4");
        cP5 = PlayerPrefs.GetInt("cP5");

        // Asignamos las PlayerPrefs correspondientes al contador de respuestas correctas e incorrectas
        rC[idPregunta] = PlayerPrefs.GetInt("rC" + idPregunta.ToString());
        rI[idPregunta] = PlayerPrefs.GetInt("rI" + idPregunta.ToString());

        // Obtenemos el componente de recurso de audio
        audioSource = GetComponent<AudioSource>();

        // A este componente le asignamos el audio que formulara la pregunta
        audioSource.clip = vozpreguntas[idAudio];

        // Se reproduce el recurso de audio
        audioSource.Play();

        // Se le pasa como parametro al metodo Setup el LANG_CODE a utilizar que es "es-MX"
        Setup(LANG_CODE);  

        // Resultado parcial del reconocimiento de voz (itera por cada palabra dicha esperando a que dejemos de hablar)
        SpeechToText.instance.onPartialResultsCallback = OnPartialSpeechResult;
        // Resultado final del reconocimiento de voz
        SpeechToText.instance.onResultCallback = OnFinalSpeechResult;

        // Se obtiene el PlayerPrefs que indica en que cuento o nivel estamos 
        idCuento = PlayerPrefs.GetInt("idCuento");

        // Se solicitan los permisos para que la aplicacion pueda acceder al microfono del dispositivo movil (solo funciona para Android)
        CheckPermission();

        // Inicializamos el idPregunta en 0 (correspondiente a la primera pregunta)
        idPregunta = 0;

        // Obtenemos la cantidad de preguntas que hay en el arreglo y lo asignamos
        totalPreguntas = preguntas.Length;

        // Se muestra el texto de la primera pregunta y sus respectivas opciones
        pregunta.text = preguntas[idPregunta];
        respuestaA.text = opcionA[idPregunta];
        RespuestaB.text = opcionB[idPregunta];
        RespuestaC.text = opcionC[idPregunta];
        
        // Se imprime en pantalla la pregunta que estamos respondiendo actualmente y la cantidad de preguntas
        // Ejemplo -> Pregunta 1 de 5
        infoRespuestas.text = "Pregunta " + (idPregunta+1).ToString() + " de " + totalPreguntas.ToString();

    }

    // Update se llama una vez por frame
    void Update () {
        
        if (tiempoCorriendo)
        {
            if(tiempo > 0)
            {
                tiempo -= Time.deltaTime; //El tiempo disminuye 
                txtTiempo.text = " " + tiempo.ToString("f0");
            }

            else // Si el tiempo llega a 0...
            {
                tiempo = 40f; // El tiempo se reinicia cada 40 segundos 
                audioSource.Play(); // Se repite el recurso de audio que formula la pregunta 

                if (idPregunta == 0){ // Si la pregunta actual es la primera...
                    cP1++;
                    PlayerPrefs.SetInt("cP1", cP1);
                }
                else if (idPregunta == 1){ // Si la pregunta actual es la segunda...
                    cP2++;
                    PlayerPrefs.SetInt("cP2", cP2);
                }
                else if (idPregunta == 2){ // Si la pregunta actual es la tercera...
                    cP3++;
                    PlayerPrefs.SetInt("cP3", cP3);
                }
                else if (idPregunta == 3) // Si la pregunta actual es la cuarta...
                {
                    cP4++;
                    PlayerPrefs.SetInt("cP4", cP4);
                }
                else if (idPregunta == 4) // Si la pregunta actual es la quinta...
                {
                    cP5++;
                    PlayerPrefs.SetInt("cP5", cP5);
                }
            }
        } 
    }

    // Recibimos la opcion que selecciono el usuario
    public void respuesta (string opcion)
    {
        // Si la opcion elegida es A entonces...
        if (opcion == "A")
        {
            // Si la opcion elegida coincide con la respuesta correcta en el indice de la pregunta actual entonces...
            if(opcionA[idPregunta] == opcionCorrecta[idPregunta])
            {
                aciertos += 1; // Aciertos aumenta en 1

                rC[idPregunta]++; // La cantidad de veces que se respondio correctamente esta pregunta en particular aumenta en 1
                PlayerPrefs.SetInt("rC" + idPregunta.ToString(), rC[idPregunta]); // Se guarda rC en las PlayerPrefs

                audioSource.clip = r_correcta; // Se obtiene el audio que indica una respuesta correcta
                btnA.GetComponent<Image>().sprite = btn_correcto; // El boton seleccionado se pinta de verde
            }
            // Si la opcion elegida no coincide con la respuesta correcta...
            else
            {
                rI[idPregunta]++; // La cantidad de veces que se respondio incorrectamente esta pregunta en particular aumenta en 1
                PlayerPrefs.SetInt("rI" + idPregunta.ToString(), rI[idPregunta]); // Se guarda rI en las PlayerPrefs

                audioSource.clip = r_incorrecta; // Se obtiene el audio que indica una respuesta incorrecta
                btnA.GetComponent<Image>().sprite = btn_incorrecto; // El boton seleccionado se pinta de rojo
            }

            audioSource.Play();  // Se reproduce el audio que indica respuesta correcta o incorrecta
            btnB.interactable = false; // Opcion A queda deshabilitada para seleccionar
            btnC.interactable = false; // Opcion B queda deshabilitada para seleccionar
            btnD.interactable = false; // Opcion C queda deshabilitada para seleccionar
        }

        else if (opcion == "B")
        {
            if(opcionB[idPregunta] == opcionCorrecta[idPregunta])
            {
                aciertos += 1;
                rC[idPregunta]++;
                PlayerPrefs.SetInt("rC" + idPregunta.ToString(), rC[idPregunta]);
                audioSource.clip = r_correcta;
                btnB.GetComponent<Image>().sprite = btn_correcto;
            }
            else
            {
                rI[idPregunta]++;
                PlayerPrefs.SetInt("rI" + idPregunta.ToString(), rI[idPregunta]);
                audioSource.clip = r_incorrecta;
                btnB.GetComponent<Image>().sprite = btn_incorrecto;
            }

            audioSource.Play();
            btnA.interactable = false;
            btnC.interactable = false;
            btnD.interactable = false;
        }

        else if (opcion == "C")
        {
            if(opcionC[idPregunta] == opcionCorrecta[idPregunta])
            {
                aciertos += 1;
                rC[idPregunta]++;
                PlayerPrefs.SetInt("rC" + idPregunta.ToString(), rC[idPregunta]);
                audioSource.clip = r_correcta;
                btnC.GetComponent<Image>().sprite = btn_correcto;
            }
            else
            {
                rI[idPregunta]++;
                PlayerPrefs.SetInt("rI" + idPregunta.ToString(), rI[idPregunta]);
                audioSource.clip = r_incorrecta;
                btnC.GetComponent<Image>().sprite = btn_incorrecto;
            }

            audioSource.Play();
            btnA.interactable = false;
            btnB.interactable = false;
            btnD.interactable = false;
        }

        // Al seleccionar una respuesta y haber pasado 1.3 segundos se invoca al metodo proximaPregunta()
        Invoke("proximaPregunta", 1.3f);
    }

    void proximaPregunta ()
    {   
        // Aumentamos los contadores relevantes en 1
        idPregunta += 1; 
        idAudio += 1;

        // Reiniciamos el tiempo a 40 segundos
        tiempo = 40f;

        // Si aun hay preguntas que formular entonces...
        if (idPregunta <= (totalPreguntas - 1))
        {
        
        audioSource.clip = vozpreguntas[idAudio];
        audioSource.Play();

        // Volvemos a activar los botones
        btnA.interactable = true;
        btnB.interactable = true;
        btnC.interactable = true;
        btnD.interactable = true;

        // Cargamos los scripts por default
        btnA.GetComponent<Image>().sprite = defA;
        btnB.GetComponent<Image>().sprite = defB;
        btnC.GetComponent<Image>().sprite = defC;
        btnD.GetComponent<Image>().sprite = defD;

        // Cargamos el texto de la nueva pregunta y sus opciones
        pregunta.text = preguntas[idPregunta];
        respuestaA.text = opcionA[idPregunta];
        RespuestaB.text = opcionB[idPregunta];
        RespuestaC.text = opcionC[idPregunta];
        
        // Imprimimos la nueva informacion
        infoRespuestas.text = "Pregunta " + (idPregunta+1).ToString() + " de " + totalPreguntas.ToString();
        
        }

        // Si ya no hay mas preguntas entonces...
        else 
        {   
            // Paramos el tiempo
            tiempoCorriendo = false;

            // Calculamos la media (originalmente de tipo flotante)
            media = 10 * (aciertos / totalPreguntas);

            // Redondeamos la media y la asignamos a notaFinal de tipo entero
            notaFinal = Mathf.RoundToInt(media);

            // Si la nota final obtenida en esta partida es mayor a la ya registrada previamente entonces...
            if (notaFinal > PlayerPrefs.GetInt("notaFinal" + idCuento.ToString()))
            {   
                // Registramos la nueva nota final
                PlayerPrefs.SetInt("notaFinal" + idCuento.ToString(), notaFinal); 
                // Registramos la nueva cantidad de aciertos
                PlayerPrefs.SetInt("aciertos" + idCuento.ToString(), (int) aciertos);
            }

            // Guardamos una notaFinal temporal
            PlayerPrefs.SetInt("notaFinalTemp" + idCuento.ToString(), notaFinal);
            // Guardamos los aciertos temporalmente
            PlayerPrefs.SetInt("aciertosTemp" + idCuento.ToString(), (int) aciertos);

            // Cargamos la escena que mostrara la retroalimentacion
            SceneManager.LoadScene("notaFinal");
        }

    }

    // Metodo de la configuracion de idioma
    void Setup (string code)
    {
        SpeechToText.instance.Setting(code);
    }

    #region Speech to Text

    // Comenzamos a escuchar (mientras el boton este presionado = Resultado Parcial)
    public void StartListening()
    {
        SpeechToText.instance.StartRecording();
    }

    // Dejamos de escuchar (cuando el boton se suelte = Resultado Final)
    public void StopListening()
    {
        SpeechToText.instance.StopRecording();
    }

    // En un resultado final de las palabras dichas tratamos la informacion igual que con el resto de opciones
    void OnFinalSpeechResult(string result)
    {
        opcionD = result; 
        txtEscucha.text = opcionD;

        if(opcionD == opcionCorrecta[idPregunta])
        {
            aciertos += 1;
            audioSource.clip = r_correcta;
            btnD.GetComponent<Image>().sprite = voz_correcto;
        } 
        
        else
        {
            audioSource.clip = r_incorrecta;
            btnD.GetComponent<Image>().sprite = voz_incorrecto;
        }

        audioSource.Play();
        btnA.interactable = false;
        btnB.interactable = false;
        btnC.interactable = false;

        // Al seleccionar una respuesta y haber pasado 1.3 segundos se invoca al metodo proximaPregunta()
        Invoke("proximaPregunta", 1.3f);
    }

    // En un resultado parcial simplemente vamos imprimiendo en pantalla lo dicho hasta el momento
    void OnPartialSpeechResult(string result)
    {   
        opcionD = result;
        txtEscucha.text = opcionD;
    }

    // Metodo para pedir permisos de microfono en Android
    void CheckPermission()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
        }
    }

    #endregion

}

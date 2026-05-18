using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Paneles del Menú")]
<<<<<<< Updated upstream
    [Tooltip("El objeto padre que contiene los botones New Game, Continue y Exit")]
    public GameObject panelMenuPrincipal; 
    
    [Tooltip("Panel que pregunta '¿Seguro que quieres borrar tu progreso?'")]
    public GameObject panelConfirmacion;

    [Header("Botones del Menú")]
    public Button nuevaPartidaButton;  
    public Button continuarButton;     
    public Button salirButton;         

    [Header("Botones de Confirmación")]
    public Button confirmSiButton;   
    public Button confirmNoButton;   

    [Header("Escenas")]
    public int primerNivelIndex = 1;

    // ──────────────────────────────────────────────────────────────
=======
    public GameObject panelMenuPrincipal;

    public GameObject panelConfirmacion;

    public OptionsManager panelOpciones;

    [Header("Botones del Menú")]
    public Button nuevaPartidaButton;
    public Button continuarButton;
    public Button opcionesButton;   

    public Button salirButton;

    [Header("Botones de Confirmación")]
    public Button confirmSiButton;
    public Button confirmNoButton;

    [Header("Escenas")]
    public int primerNivelIndex = 1;
>>>>>>> Stashed changes
    void Start()
    {
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (SaveSystem.instance == null)
<<<<<<< Updated upstream
        {
            Debug.LogError("❌ No hay SaveSystem en la escena.");
        }

        ActualizarBotones();

        // Al iniciar, nos aseguramos de mostrar el menú principal y ocultar la confirmación
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(true);
        if (panelConfirmacion != null) panelConfirmacion.SetActive(false);
    }

    // ──────────────────────────────────────────────────────────────
    //  BOTÓN: NUEVA PARTIDA
    // ──────────────────────────────────────────────────────────────
=======
            Debug.LogError("❌ No hay SaveSystem en la escena.");

        ActualizarBotones();

        //Estado inicial de los paneles
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(true);
        if (panelConfirmacion  != null) panelConfirmacion.SetActive(false);
        if (panelOpciones      != null) panelOpciones.gameObject.SetActive(false);
    }

>>>>>>> Stashed changes
    public void NuevaPartida()
    {
        if (SaveSystem.instance == null || !SaveSystem.instance.HasSaveFile())
        {
            IniciarNuevaPartida();
            return;
        }

<<<<<<< Updated upstream
        // Si HAY guardado, MOSTRAMOS confirmación y OCULTAMOS menú principal
        if (panelConfirmacion != null) panelConfirmacion.SetActive(true);
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(false); 
    }

    // Confirmó que quiere borrar
=======
        //Si hay guardado se muestra la confirmación
        if (panelConfirmacion  != null) panelConfirmacion.SetActive(true);
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(false);
    }

>>>>>>> Stashed changes
    public void ConfirmarNuevaPartida()
    {
        IniciarNuevaPartida();
    }

<<<<<<< Updated upstream
    // Canceló, vuelve al menú
    public void CancelarNuevaPartida()
    {
        // OCULTAMOS confirmación y VOLVEMOS A MOSTRAR el menú principal
        if (panelConfirmacion != null) panelConfirmacion.SetActive(false);
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(true); 
=======
    public void CancelarNuevaPartida()
    {
        if (panelConfirmacion  != null) panelConfirmacion.SetActive(false);
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(true);
>>>>>>> Stashed changes
    }

    private void IniciarNuevaPartida()
    {
        SaveSystem.instance?.DeleteSave();

        if (SaveSystem.instance != null)
        {
            SaveSystem.instance.pendingLoad   = null;
            SaveSystem.instance.isLoadingGame = false;
        }

<<<<<<< Updated upstream
        SceneManager.LoadScene(primerNivelIndex);
    }

    // ──────────────────────────────────────────────────────────────
    //  BOTÓN: CONTINUAR
    // ──────────────────────────────────────────────────────────────
=======
        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.LoadScene(primerNivelIndex);
        }
        else
        {
            SceneManager.LoadScene(primerNivelIndex);
        }
    }

>>>>>>> Stashed changes
    public void Continuar()
    {
        if (SaveSystem.instance == null) return;

        SaveData data = SaveSystem.instance.Load();
        if (data == null) return;

        SaveSystem.instance.pendingLoad   = data;
        SaveSystem.instance.isLoadingGame = true;

<<<<<<< Updated upstream
        SceneManager.LoadScene(data.sceneIndex);
    }

    // ──────────────────────────────────────────────────────────────
    //  BOTÓN: SALIR
    // ──────────────────────────────────────────────────────────────
=======
        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.LoadScene(data.sceneIndex);
        }
        else
        {
            SceneManager.LoadScene(data.sceneIndex);
        }
    }

    public void AbrirOpciones()
    {
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(false);
        if (panelOpciones      != null) panelOpciones.OpenOptions();
    }

    public void CerrarOpciones()
    {
        if (panelOpciones      != null) panelOpciones.CloseOptions();
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(true);
    }
>>>>>>> Stashed changes
    public void Salir()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();

<<<<<<< Updated upstream
        // Esto detiene el modo Play en el editor de Unity
=======
>>>>>>> Stashed changes
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

<<<<<<< Updated upstream
    // ──────────────────────────────────────────────────────────────
=======
>>>>>>> Stashed changes
    private void ActualizarBotones()
    {
        bool haySave = SaveSystem.instance != null && SaveSystem.instance.HasSaveFile();

        if (continuarButton != null)
            continuarButton.gameObject.SetActive(haySave);
    }
}
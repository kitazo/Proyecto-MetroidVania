using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Paneles del Menú")]
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
    void Start()
    {
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (SaveSystem.instance == null)
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
    public void NuevaPartida()
    {
        if (SaveSystem.instance == null || !SaveSystem.instance.HasSaveFile())
        {
            IniciarNuevaPartida();
            return;
        }

        // Si HAY guardado, MOSTRAMOS confirmación y OCULTAMOS menú principal
        if (panelConfirmacion != null) panelConfirmacion.SetActive(true);
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(false); 
    }

    // Confirmó que quiere borrar
    public void ConfirmarNuevaPartida()
    {
        IniciarNuevaPartida();
    }

    // Canceló, vuelve al menú
    public void CancelarNuevaPartida()
    {
        // OCULTAMOS confirmación y VOLVEMOS A MOSTRAR el menú principal
        if (panelConfirmacion != null) panelConfirmacion.SetActive(false);
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(true); 
    }

    private void IniciarNuevaPartida()
    {
        SaveSystem.instance?.DeleteSave();

        if (SaveSystem.instance != null)
        {
            SaveSystem.instance.pendingLoad   = null;
            SaveSystem.instance.isLoadingGame = false;
        }

        SceneManager.LoadScene(primerNivelIndex);
    }

    // ──────────────────────────────────────────────────────────────
    //  BOTÓN: CONTINUAR
    // ──────────────────────────────────────────────────────────────
    public void Continuar()
    {
        if (SaveSystem.instance == null) return;

        SaveData data = SaveSystem.instance.Load();
        if (data == null) return;

        SaveSystem.instance.pendingLoad   = data;
        SaveSystem.instance.isLoadingGame = true;

        SceneManager.LoadScene(data.sceneIndex);
    }

    // ──────────────────────────────────────────────────────────────
    //  BOTÓN: SALIR
    // ──────────────────────────────────────────────────────────────
    public void Salir()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();

        // Esto detiene el modo Play en el editor de Unity
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    // ──────────────────────────────────────────────────────────────
    private void ActualizarBotones()
    {
        bool haySave = SaveSystem.instance != null && SaveSystem.instance.HasSaveFile();

        if (continuarButton != null)
            continuarButton.gameObject.SetActive(haySave);
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Paneles del Menú")]
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
    void Start()
    {
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (SaveSystem.instance == null)
            Debug.LogError("❌ No hay SaveSystem en la escena.");

        ActualizarBotones();

        //Estado inicial de los paneles
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(true);
        if (panelConfirmacion  != null) panelConfirmacion.SetActive(false);
        if (panelOpciones      != null) panelOpciones.gameObject.SetActive(false);
    }

    public void NuevaPartida()
    {
        if (SaveSystem.instance == null || !SaveSystem.instance.HasSaveFile())
        {
            IniciarNuevaPartida();
            return;
        }

        //Si hay guardado se muestra la confirmación
        if (panelConfirmacion  != null) panelConfirmacion.SetActive(true);
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(false);
    }

    public void ConfirmarNuevaPartida()
    {
        IniciarNuevaPartida();
    }

    public void CancelarNuevaPartida()
    {
        if (panelConfirmacion  != null) panelConfirmacion.SetActive(false);
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

        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.LoadScene(primerNivelIndex);
        }
        else
        {
            SceneManager.LoadScene(primerNivelIndex);
        }
    }

    public void Continuar()
    {
        if (SaveSystem.instance == null) return;

        SaveData data = SaveSystem.instance.Load();
        if (data == null) return;

        SaveSystem.instance.pendingLoad   = data;
        SaveSystem.instance.isLoadingGame = true;

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
    public void Salir()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    private void ActualizarBotones()
    {
        bool haySave = SaveSystem.instance != null && SaveSystem.instance.HasSaveFile();

        if (continuarButton != null)
            continuarButton.gameObject.SetActive(haySave);
    }
}
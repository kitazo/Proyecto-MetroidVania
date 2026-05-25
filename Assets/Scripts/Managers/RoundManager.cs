using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using System.Collections;

public class RoundManager : MonoBehaviour
{
    public static RoundManager instance;

    [Header("Control de Rondas")]
    public int currentRound = 1;
    public float timeRound1 = 0f;
    public float timeRound2 = 0f;
    public bool hostEsDiosActual = true;

    [Header("UI Interfaz de Resultados")]
    public GameObject panelResultados;
    public TextMeshProUGUI textoResultados;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            FileLogger.Write($"[RoundManager] Awake - nueva instancia. currentRound={currentRound}");
        }
        else
        {
            FileLogger.Write($"[RoundManager] Awake - duplicado destruido. currentRound persistente={instance.currentRound}");
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FileLogger.Write($"[RoundManager] OnSceneLoaded: {scene.name} round={currentRound}");
        ReconectarUIDeEscena();
    }

    private void ReconectarUIDeEscena()
    {
        if (panelResultados == null)
        {
            GameObject obj = GameObject.Find("PanelResultados");
            if (obj != null)
            {
                panelResultados = obj;
                FileLogger.Write("[RoundManager] panelResultados reconectado.");
            }
        }

        if (textoResultados == null)
        {
            GameObject obj = GameObject.Find("TextoResultados");
            if (obj != null)
            {
                textoResultados = obj.GetComponent<TextMeshProUGUI>();
                FileLogger.Write("[RoundManager] textoResultados reconectado.");
            }
        }
    }

    public void FinalizarRonda(float tiempoGanado)
    {
        FileLogger.Write($"[RoundManager] FinalizarRonda llamado. currentRound={currentRound} tiempoGanado={tiempoGanado:F2}");

        if (currentRound == 1)
        {
            timeRound1 = tiempoGanado;
            currentRound = 2;
            hostEsDiosActual = !hostEsDiosActual;

            FileLogger.Write($"[RoundManager] Ronda 1 terminada. Iniciando carga de escena. hostEsDiosActual ahora={hostEsDiosActual}");
            StartCoroutine(CargarEscenaConRetrasoSeguro());
        }
        else
        {
            timeRound2 = tiempoGanado;
            FileLogger.Write($"[RoundManager] Ronda 2 terminada. t1={timeRound1:F2} t2={timeRound2:F2}");

            PlayerControllerComplete player = FindAnyObjectByType<PlayerControllerComplete>();
            FileLogger.Write($"[RoundManager] Buscando PlayerControllerComplete: {(player != null ? "ENCONTRADO" : "NULL")}");

            if (player != null)
            {
                FileLogger.Write("[RoundManager] Llamando MostrarResultadosFinalesClientRpc...");
                player.MostrarResultadosFinalesClientRpc(timeRound1, timeRound2);
            }
        }
    }

    private IEnumerator CargarEscenaConRetrasoSeguro()
    {
        FileLogger.Write("[RoundManager] CargarEscenaConRetrasoSeguro START");

        Time.timeScale = 0f;
        FileLogger.Write("[RoundManager] Time.timeScale = 0. Esperando 1.5s reales...");

        yield return new WaitForSecondsRealtime(1.5f);

        Time.timeScale = 1f;
        FileLogger.Write("[RoundManager] Time.timeScale = 1. Procediendo a cargar escena.");

        FileLogger.Write($"[RoundManager] NetworkManager.Singleton={NetworkManager.Singleton != null}");
        FileLogger.Write($"[RoundManager] NetworkManager.SceneManager={NetworkManager.Singleton?.SceneManager != null}");
        FileLogger.Write($"[RoundManager] Escena actual: {SceneManager.GetActiveScene().name}");

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
        {
            string sceneName = SceneManager.GetActiveScene().name;
            FileLogger.Write($"[RoundManager] Llamando NetworkManager.SceneManager.LoadScene({sceneName})...");
            AsymmetricNetworkManager.MarcarCargandoEscena();
            panelResultados = null;
            textoResultados = null;

            var status = NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);

            FileLogger.Write($"[RoundManager] LoadScene resultado: {status}");
        }
        else
        {
            FileLogger.Write("[RoundManager] ERROR: NetworkManager o SceneManager es null!");
        }
    }
}
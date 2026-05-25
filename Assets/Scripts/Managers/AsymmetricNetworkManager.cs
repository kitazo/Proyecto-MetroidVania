using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AsymmetricNetworkManager : MonoBehaviour
{
    [Header("Prefabs de Jugadores")]
    public GameObject platformerPrefab;
    public GameObject godPrefab;

    [Header("Puntos de Aparición")]
    public Transform platformerSpawnPoint;

    [Header("Control de Rondas")]
    public bool hostEsDios = true;

    [Header("Pantalla de Espera (Host)")]
    public GameObject panelEsperandoJugador;

    private bool esModoSolitario = false;
    private bool remoteClientYaConectado = false;

    //Flag que indica que una carga de escena en red está en progreso.
    //Mientras sea true, los callbacks de desconexión se ignoran para no
    //confundir el despawn de NGO durante la recarga con una desconexión real.
    private static bool cargandoEscena = false;

    public static void MarcarCargandoEscena() => cargandoEscena = true;

    private void Start()
    {
        //Al iniciar la escena nueva, la carga ya terminó → resetear el flag.
        cargandoEscena = false;

        FileLogger.Write("=== AsymmetricNetworkManager.Start() ===");
        FileLogger.Write($"  IsListening       : {NetworkManager.Singleton.IsListening}");
        FileLogger.Write($"  IsHost            : {NetworkManager.Singleton.IsHost}");
        FileLogger.Write($"  IsServer          : {NetworkManager.Singleton.IsServer}");
        FileLogger.Write($"  IsClient          : {NetworkManager.Singleton.IsClient}");
        FileLogger.Write($"  ModoSeleccionado  : {NetworkModeData.modoSeleccionado}");
        FileLogger.Write($"  RoundManager round: {(RoundManager.instance != null ? RoundManager.instance.currentRound.ToString() : "NULL")}");
        FileLogger.Write($"  hostEsDiosActual  : {(RoundManager.instance != null ? RoundManager.instance.hostEsDiosActual.ToString() : "NULL")}");

        if (RoundManager.instance != null)
            hostEsDios = RoundManager.instance.hostEsDiosActual;

        esModoSolitario = (NetworkModeData.modoSeleccionado == NetworkModeData.Mode.Solitario);

        //Red activa
        if (NetworkManager.Singleton.IsListening)
        {
            FileLogger.Write("  >> Red ya activa (Ronda 2). Saltando StartHost/StartClient.");

            NetworkManager.Singleton.OnClientConnectedCallback  += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;

            if (NetworkManager.Singleton.IsServer)
            {
                List<ulong> ids = new List<ulong>(NetworkManager.Singleton.ConnectedClientsIds);
                FileLogger.Write($"  Clientes conectados: {ids.Count}");

                foreach (ulong clientId in ids)
                {
                    //Si hay algún cliente que NO sea el host → el remoto ya está
                    if (clientId != NetworkManager.ServerClientId)
                    {
                        remoteClientYaConectado = true;
                        FileLogger.Write($"  remoteClientYaConectado=true (clientId={clientId} ya presente)");
                    }

                    FileLogger.Write($"  Spawneando jugador para clientId={clientId}");
                    SpawnJugadorParaCliente(clientId);
                }

                //Si el remoto ya estaba conectado, asegura de que el
                // panel de espera esté oculto y el juego corriendo desde el inicio.
                if (remoteClientYaConectado)
                {
                    if (panelEsperandoJugador != null)
                        panelEsperandoJugador.SetActive(false);
                    Time.timeScale = 1f;
                    FileLogger.Write("  >> Panel espera ocultado. Time.timeScale=1 (ronda 2).");
                }
            }
            else
            {
                Time.timeScale = 1f;
                FileLogger.Write("  >> Cliente puro Ronda 2: Time.timeScale=1 asegurado.");
            }

            return;
        }

        //Primera conexion
        FileLogger.Write("  >> Primera conexión (Ronda 1). Arrancando red.");

        NetworkManager.Singleton.OnClientConnectedCallback  += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        if (panelEsperandoJugador != null)
            panelEsperandoJugador.SetActive(false);

        switch (NetworkModeData.modoSeleccionado)
        {
            case NetworkModeData.Mode.Solitario:
                FileLogger.Write("  StartHost (Solitario)");
                NetworkManager.Singleton.StartHost();
                break;

            case NetworkModeData.Mode.Host:
                FileLogger.Write("  StartHost (Multijugador)");
                transport.ConnectionData.Address = "0.0.0.0";
                NetworkManager.Singleton.StartHost();
                StartCoroutine(MostrarPanelEsperaConFlag());
                break;

            case NetworkModeData.Mode.Cliente:
                FileLogger.Write($"  StartClient → IP: {NetworkModeData.ipDelHost}");
                transport.ConnectionData.Address = NetworkModeData.ipDelHost;
                NetworkManager.Singleton.StartClient();
                break;

            default:
                FileLogger.Write("  WARN: Modo desconocido → Solitario por defecto");
                esModoSolitario = true;
                NetworkManager.Singleton.StartHost();
                break;
        }
    }

    private IEnumerator MostrarPanelEsperaConFlag()
    {
        //Espera un frame para que HandleClientConnected del host
        //haya podido ejecutarse y actualizar remoteClientYaConectado si aplica.
        yield return null;

        if (remoteClientYaConectado)
        {
            Time.timeScale = 1f;
        }
        else
        {
            if (panelEsperandoJugador != null)
                panelEsperandoJugador.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    private void OnDestroy()
    {
        FileLogger.Write("AsymmetricNetworkManager.OnDestroy()");

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback  -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        FileLogger.Write($"HandleClientConnected: clientId={clientId} IsServer={NetworkManager.Singleton.IsServer}");

        if (NetworkManager.Singleton.IsServer && clientId != NetworkManager.ServerClientId)
        {
            remoteClientYaConectado = true;

            if (panelEsperandoJugador != null)
                panelEsperandoJugador.SetActive(false);

            Time.timeScale = 1f;
        }

        if (!NetworkManager.Singleton.IsServer)
            return;

        SpawnJugadorParaCliente(clientId);
    }

    private void SpawnJugadorParaCliente(ulong clientId)
    {
        FileLogger.Write($"SpawnJugadorParaCliente: clientId={clientId} esSolitario={esModoSolitario} hostEsDios={hostEsDios}");

        GameObject prefabASpawnear;
        Vector3 posicionSpawn = Vector3.zero;

        if (esModoSolitario)
        {
            prefabASpawnear = platformerPrefab;
            if (platformerSpawnPoint != null)
                posicionSpawn = platformerSpawnPoint.position;
        }
        else
        {
            bool esHost = (clientId == NetworkManager.ServerClientId);
            FileLogger.Write($"  esHost={esHost}  hostEsDios={hostEsDios}");

            if ((esHost && hostEsDios) || (!esHost && !hostEsDios))
            {
                FileLogger.Write("  → Spawneando GOD");
                prefabASpawnear = godPrefab;
            }
            else
            {
                FileLogger.Write("  → Spawneando PLATFORMER");
                prefabASpawnear = platformerPrefab;
                if (platformerSpawnPoint != null)
                    posicionSpawn = platformerSpawnPoint.position;
            }
        }

        if (prefabASpawnear == null)
        {
            FileLogger.Write($"  ERROR: prefabASpawnear es NULL para clientId={clientId}");
            return;
        }

        FileLogger.Write($"  Instanciando en posición {posicionSpawn}");
        GameObject instancia = Instantiate(prefabASpawnear, posicionSpawn, Quaternion.identity);
        NetworkObject netObj = instancia.GetComponent<NetworkObject>();

        if (netObj != null)
        {
            FileLogger.Write($"  SpawnAsPlayerObject OK para clientId={clientId}");
            netObj.SpawnAsPlayerObject(clientId, true);
        }
        else
        {
            FileLogger.Write($"  ERROR: No hay NetworkObject en el prefab");
        }
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        FileLogger.Write($"HandleClientDisconnect: clientId={clientId} cargandoEscena={cargandoEscena}");

        //Ignora desconexiones que NGO genera internamente al recargar la escena.
        //NGO destruye los NetworkObjects de la escena anterior y dispara este
        //callback, pero NO es una desconexión real del jugador.
        if (cargandoEscena)
        {
            FileLogger.Write($"  >> Ignorado: es despawn de carga de escena, no desconexión real.");
            return;
        }

        if (clientId == NetworkManager.ServerClientId)
            return;

        Boton_SalirYDesconectar();
    }

    public void Boton_SalirYDesconectar()
    {
        FileLogger.Write("Boton_SalirYDesconectar llamado");
        cargandoEscena = false;
        Time.timeScale = 1f;

        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.Shutdown();

        SceneManager.LoadScene(0);
    }
}
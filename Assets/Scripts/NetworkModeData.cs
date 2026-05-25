public static class NetworkModeData
{
    public enum Mode { Ninguno, Solitario, Host, Cliente }
    
    //Esta variable recordará qué botón presionó el jugador en el Menú
    public static Mode modoSeleccionado = Mode.Ninguno; 
    public static string ipDelHost = "127.0.0.1";
}
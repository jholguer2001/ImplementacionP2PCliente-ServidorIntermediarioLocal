using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class ClienteChat
{
    private static Socket _socketServidor;
    private static string _nombreCliente;
    private static bool _chatListo = false;
    private static bool _mensajeInicioMostrado = false; // Nueva bandera para controlar el mensaje de inicio

    public static void Main()
    {
        Console.Title = "Cliente de Chat";
        Console.Write("Ingrese su nombre: ");
        _nombreCliente = Console.ReadLine();

        IPAddress direccionIp = IPAddress.Parse("127.0.0.1");
        int puerto = 11000;

        _socketServidor = new Socket(direccionIp.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            _socketServidor.Connect(new IPEndPoint(direccionIp, puerto));
            Console.WriteLine($"Conectado al servidor como {_nombreCliente}");

            _socketServidor.Send(Encoding.ASCII.GetBytes($"NOMBRE|{_nombreCliente}"));

            Thread hiloRecepcion = new Thread(RecibirMensajes);
            hiloRecepcion.IsBackground = true;
            hiloRecepcion.Start();

            // En lugar de llamar a EnviarMensajes directamente, ahora se espera a que _chatListo sea true
            // Esto asegura que el prompt de "nombre:" solo aparezca después del mensaje de bienvenida.
            while (!_chatListo)
            {
                Thread.Sleep(50); // Pequeña espera para no consumir CPU inútilmente
            }
            // Una vez que _chatListo es true, significa que el mensaje de bienvenida ya se mostró.
            // Ahora podemos iniciar el bucle de envío y el prompt.
            EnviarMensajes();

        }
        catch (SocketException sex)
        {
            Console.WriteLine($"Error de conexión: {sex.Message}");
            if (sex.SocketErrorCode == SocketError.ConnectionRefused)
            {
                Console.WriteLine("Asegúrese de que el servidor esté en ejecución y sea accesible.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ocurrió un error inesperado: {ex.Message}");
        }
        finally
        {
            _socketServidor?.Close();
            Environment.Exit(0);
        }
    }

    private static void RecibirMensajes()
    {
        try
        {
            while (true)
            {
                byte[] buffer = new byte[2048];
                int bytesRecibidos = _socketServidor.Receive(buffer);
                if (bytesRecibidos == 0)
                {
                    Console.WriteLine("\nEl servidor se desconectó.");
                    break;
                }

                string mensaje = Encoding.ASCII.GetString(buffer, 0, bytesRecibidos);

                if (mensaje.StartsWith("CHAT_LISTO|"))
                {
                    if (!_mensajeInicioMostrado) // Solo mostrar una vez
                    {
                        string mensajeListo = mensaje.Split('|')[1];
                        LimpiarLineaConsolaActual(); // Limpiar antes de mostrar el mensaje
                        Console.WriteLine($"{mensajeListo}"); // Sin el signo de interrogación
                        _chatListo = true;
                        _mensajeInicioMostrado = true; // Marcar que ya se mostró
                        Console.Write($"{_nombreCliente}: "); // Mostrar el prompt después del mensaje
                    }
                    continue;
                }
                else if (mensaje.StartsWith("COMPAÑERO_DESCONECTADO|"))
                {
                    string mensajeDesconexion = mensaje.Split('|')[1];
                    LimpiarLineaConsolaActual();
                    Console.WriteLine($"{mensajeDesconexion}");
                    _chatListo = false;
                    Console.WriteLine("Presione cualquier tecla para salir.");
                    break;
                }

                string[] partes = mensaje.Split('|');
                if (partes.Length == 2)
                {
                    string nombreRemitente = partes[0];
                    string mensajeChat = partes[1];

                    LimpiarLineaConsolaActual();

                    Console.WriteLine($"{nombreRemitente}: {mensajeChat}");

                    Console.Write($"{_nombreCliente}: ");
                }
            }
        }
        catch (SocketException)
        {
            Console.WriteLine("\nDesconectado del servidor.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nError al recibir mensaje: {ex.Message}");
        }
        finally
        {
            _socketServidor?.Close();
            Environment.Exit(0);
        }
    }

    private static void EnviarMensajes()
    {
        try
        {
            while (true)
            {
                // El prompt solo se escribe aquí si el chat está listo.
                // En RecibirMensajes, se encarga de escribirlo la primera vez después del mensaje CHAT_LISTO.
                string mensaje = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(mensaje)) continue;

                string mensajeCompleto = $"{_nombreCliente}|{mensaje}";
                _socketServidor.Send(Encoding.ASCII.GetBytes(mensajeCompleto));

                if (mensaje.ToLower() == "adios")
                {
                    Thread.Sleep(500);
                    break;
                }
            }
        }
        catch (SocketException)
        {
            Console.WriteLine("\nError al enviar mensaje. Conexión perdida.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nOcurrió un error al enviar: {ex.Message}");
        }
        finally
        {
            _socketServidor?.Close();
            Environment.Exit(0);
        }
    }

    private static void LimpiarLineaConsolaActual()
    {
        int cursorLineaActual = Console.CursorTop;
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(0, cursorLineaActual);
    }
}
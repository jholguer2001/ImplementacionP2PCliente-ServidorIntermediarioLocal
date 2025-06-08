using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Linq;

class ServidorChat
{
    private static readonly Dictionary<Socket, string> _conexionesClientes = new Dictionary<Socket, string>();
    private static readonly object _bloqueo = new object();

    public static void Main()
    {
        Console.Title = "Servidor Intermediario";
        IPAddress direccionIp = IPAddress.Parse("127.0.0.1");
        int puerto = 11000;

        Socket escuchador = new Socket(direccionIp.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            escuchador.Bind(new IPEndPoint(direccionIp, puerto));
            escuchador.Listen(2);
            Console.WriteLine($"Servidor listo para conectar 2 clientes en {direccionIp}:{puerto}");

            Socket cliente1 = escuchador.Accept();
            Console.WriteLine($"Cliente 1 conectado desde: {cliente1.RemoteEndPoint}");

            Socket cliente2 = escuchador.Accept();
            Console.WriteLine($"Cliente 2 conectado desde: {cliente2.RemoteEndPoint}");

            new Thread(() => NegociarCliente(cliente1)).Start();
            new Thread(() => NegociarCliente(cliente2)).Start();

            Console.WriteLine("Esperando que ambos clientes proporcionen sus nombres...");

            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error del servidor: {ex.Message}");
        }
        finally
        {
            escuchador.Close();
        }
    }

    private static void NegociarCliente(Socket socketCliente)
    {
        try
        {
            byte[] buffer = new byte[2048];
            int bytesRecibidos = socketCliente.Receive(buffer);
            string mensaje = Encoding.ASCII.GetString(buffer, 0, bytesRecibidos);

            if (mensaje.StartsWith("NOMBRE|"))
            {
                string nombreCliente = mensaje.Split('|')[1];
                lock (_bloqueo)
                {
                    _conexionesClientes.Add(socketCliente, nombreCliente);
                }
                Console.WriteLine($"Cliente {socketCliente.RemoteEndPoint} identificado como '{nombreCliente}'.");

                while (true)
                {
                    lock (_bloqueo)
                    {
                        if (_conexionesClientes.Count == 2)
                        {
                            // Enviar el mensaje de listo para chatear una sola vez a ambos
                            EnviarATodos("CHAT_LISTO|Pueden comenzar a chatear! Escriba 'adios' para terminar.");
                            break;
                        }
                    }
                    Thread.Sleep(100);
                }

                ManejarCliente(socketCliente);
            }
            else
            {
                Console.WriteLine($"Mensaje inesperado recibido de {socketCliente.RemoteEndPoint}: {mensaje}");
                socketCliente.Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error durante la negociación del cliente {socketCliente.RemoteEndPoint}: {ex.Message}");
            socketCliente.Close();
            RemoverCliente(socketCliente);
        }
    }


    private static void ManejarCliente(Socket socketCliente)
    {
        string nombreCliente = "Desconocido";
        lock (_bloqueo)
        {
            _conexionesClientes.TryGetValue(socketCliente, out nombreCliente);
        }

        try
        {
            while (true)
            {
                byte[] buffer = new byte[2048];
                int bytesRecibidos = socketCliente.Receive(buffer);
                if (bytesRecibidos == 0) break;

                string mensaje = Encoding.ASCII.GetString(buffer, 0, bytesRecibidos);
                Console.WriteLine($"Recibido de '{nombreCliente}' ({socketCliente.RemoteEndPoint}): {mensaje}");

                ReenviarMensaje(socketCliente, mensaje);
            }
        }
        catch (SocketException sex)
        {
            Console.WriteLine($"Cliente '{nombreCliente}' ({socketCliente.RemoteEndPoint}) se desconectó forzadamente. Error: {sex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al manejar cliente '{nombreCliente}' ({socketCliente.RemoteEndPoint}): {ex.Message}");
        }
        finally
        {
            RemoverCliente(socketCliente);
            Console.WriteLine($"Conexión del cliente '{nombreCliente}' ({socketCliente.RemoteEndPoint}) cerrada.");
        }
    }

    private static void ReenviarMensaje(Socket remitente, string mensaje)
    {
        lock (_bloqueo)
        {
            foreach (var entrada in _conexionesClientes)
            {
                if (entrada.Key != remitente)
                {
                    try
                    {
                        entrada.Key.Send(Encoding.ASCII.GetBytes(mensaje));
                    }
                    catch (SocketException)
                    {
                        Console.WriteLine($"Fallo al enviar mensaje a {entrada.Value}. El cliente podría estar desconectado.");
                    }
                }
            }
        }
    }

    private static void EnviarATodos(string mensaje)
    {
        lock (_bloqueo)
        {
            foreach (var entrada in _conexionesClientes)
            {
                try
                {
                    entrada.Key.Send(Encoding.ASCII.GetBytes(mensaje));
                }
                catch (SocketException)
                {
                    Console.WriteLine($"Fallo al difundir a {entrada.Value}. El cliente podría estar desconectado.");
                }
            }
        }
    }

    private static void RemoverCliente(Socket socketCliente)
    {
        lock (_bloqueo)
        {
            string nombreCliente = "Desconocido";
            if (_conexionesClientes.ContainsKey(socketCliente))
            {
                nombreCliente = _conexionesClientes[socketCliente];
                _conexionesClientes.Remove(socketCliente);
            }
            socketCliente.Close();
            Console.WriteLine($"Cliente '{nombreCliente}' removido. Clientes activos: {_conexionesClientes.Count}");

            if (_conexionesClientes.Count < 2 && _conexionesClientes.Count > 0)
            {
                EnviarATodos("COMPAÑERO_DESCONECTADO|Tu compañero de chat se ha desconectado. Finalizando chat.");
                Console.WriteLine("Un cliente se desconectó, cerrando el servidor.");
                Environment.Exit(0);
            }
            else if (_conexionesClientes.Count == 0)
            {
                Console.WriteLine("Todos los clientes desconectados.");
                Environment.Exit(0);
            }
        }
    }
}
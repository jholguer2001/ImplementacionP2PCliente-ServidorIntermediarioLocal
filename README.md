 Implementación P2P (Cliente-Servidor Intermediario Local)

# Chat P2P Local (Servidor Intermediario) en C#

Este proyecto implementa un sistema de chat Peer-to-Peer (P2P) simple utilizando sockets en C#. Aunque es un chat "P2P", funciona a través de un **servidor intermediario local** que reenvía mensajes entre dos clientes conectados. El servidor está diseñado para aceptar solo dos clientes a la vez, creando una conversación privada entre ellos.

## Estructura del Proyecto

El proyecto consta de dos aplicaciones de consola separadas:

1.  **`ServidorChat.cs`**: El servidor intermediario que gestiona las conexiones y reenvía los mensajes.
2.  **`ClienteChat.cs`**: La aplicación cliente que los usuarios ejecutan para conectarse al servidor y chatear.

## Requisitos

* .NET SDK (preferiblemente .NET 6 o superior)
* Un entorno de desarrollo como Visual Studio, Visual Studio Code o cualquier editor de texto para compilar y ejecutar C#.

## Cómo Ejecutar

Para utilizar este chat, deberás ejecutar el servidor y dos instancias del cliente **en la misma máquina local**.

### 1. Iniciar el Servidor

1.  Abre una terminal (CMD, PowerShell, Bash, etc.).
2.  Navega hasta el directorio donde se encuentra el archivo `ServidorChat.cs`.
3.  Compila y ejecuta el servidor con el siguiente comando:
    ```bash
    dotnet run ServidorChat.cs
    ```
4.  Verás la siguiente salida en la consola del servidor, indicando que está esperando conexiones:
    ```
    Servidor iniciado.
    Esperando conexión...
    ```

### 2. Conectar los Clientes

Necesitarás abrir **dos terminales nuevas** para ejecutar cada cliente.

1.  **Cliente 1:**
    * Abre una nueva terminal y navega hasta el directorio donde se encuentra `ClienteChat.cs`.
    * Ejecuta el cliente:
        ```bash
        dotnet run ClienteChat.cs
        ```
    * Se te pedirá que ingreses un nombre. Por ejemplo: `Maria`
        ```
        Ingrese su nombre: Maria
        Conectado al servidor como Maria
        ```
    * La consola del cliente esperará a que el segundo cliente se conecte.

2.  **Cliente 2:**
    * Abre otra terminal nueva y navega hasta el directorio de `ClienteChat.cs`.
    * Ejecuta el cliente:
        ```bash
        dotnet run ClienteChat.cs
        ```
    * Ingresa otro nombre. Por ejemplo: `Juan`
        ```
        Ingrese su nombre: Juan
        Conectado al servidor como Juan
        ```

### 3. ¡Comenzar a Chatear!

Una vez que ambos clientes estén conectados y hayan ingresado sus nombres:

* En la consola de **ambos clientes**, verás el mensaje de bienvenida y la instrucción para chatear:
    ```
    ¡Pueden comenzar a chatear! Escriba 'adios' para terminar.
    Maria: _ (o Juan: _ si es el cliente de Juan)
    ```
* Ahora pueden escribir mensajes en la consola de su respectivo cliente. Los mensajes se reenviarán al otro cliente.
* **Para finalizar la conversación**, cualquiera de los clientes puede escribir `adios` y presionar Enter. Esto cerrará la conexión de ambos clientes y el servidor finalizará.




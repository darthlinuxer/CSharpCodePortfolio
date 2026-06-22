using CSharpCodePortfolio.Shared;
using CSharpCodePortfolio.Tutorials.Abstractions;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CSharpCodePortfolio.Tutorials.Tutorial02;

[Tutorial("02", "client-server-socket", "Comunicação cliente-servidor com sockets")]
public sealed class ClientServerSocketTutorial : ITutorial
{
    private const string Message = "Mensagem enviada pelo cliente <EOF>";
    private const string EndMarker = "<EOF>";
    private static readonly Encoding MessageEncoding = Encoding.UTF8;

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        TutorialConsole.WriteHeader("02", "Comunicação cliente-servidor com sockets TCP");
        TutorialConsole.WriteContext(
            ("Tema", "Socket TCP em loopback"),
            ("Conceito", "Cliente conecta, servidor recebe bytes e envia eco"),
            ("Runtime", ".NET 10"),
            ("Slug", "client-server-socket"));
        TutorialConsole.WriteQuestion("Quais passos mínimos fazem um cliente e um servidor trocarem dados por socket?");
        TutorialConsole.WriteHypothesis(
            "O servidor precisa abrir um endpoint, escutar conexões e aceitar um cliente.",
            "O cliente precisa conectar ao endpoint, enviar bytes e ler a resposta.",
            "Um delimitador como `<EOF>` permite saber quando a mensagem lógica terminou.");
        TutorialConsole.WritePreparation(
            "O tutorial usa `IPAddress.Loopback`, portanto a conexão fica restrita à própria máquina.",
            "A porta é escolhida pelo sistema operacional para evitar conflito com portas fixas.",
            "A mensagem é codificada em UTF-8 e o servidor devolve os mesmos bytes como eco.");

        TutorialConsole.WriteExperiment(
            1,
            "Servidor TCP local",
            "Cria um socket, vincula a uma porta livre e inicia a escuta.");
        TutorialConsole.WriteCodeSnippet(
            "O servidor escuta em loopback e aceita uma conexão.",
            "ServidorTcp.cs",
            """
            using var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(new IPEndPoint(IPAddress.Loopback, 0));
            listener.Listen();
            var serverTask = AcceptAndEchoAsync(listener, cancellationToken);
            """);

        var exchange = await RunSocketExchangeAsync(cancellationToken).ConfigureAwait(false);

        TutorialConsole.WriteEvidence(
            "Endpoint",
            ("Endereço", exchange.Endpoint.Address.ToString()),
            ("Porta", exchange.Endpoint.Port.ToString()),
            ("Protocolo", "TCP"));

        TutorialConsole.WriteExperiment(
            2,
            "Cliente TCP local",
            "Conecta ao endpoint do servidor, envia uma mensagem e lê o eco.");
        TutorialConsole.WriteCodeSnippet(
            "O cliente envia bytes e aguarda a resposta do servidor.",
            "ClienteTcp.cs",
            """
            using var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await client.ConnectAsync(endpoint, cancellationToken);
            await client.SendAsync(payload, SocketFlags.None, cancellationToken);
            var bytesRead = await client.ReceiveAsync(buffer, SocketFlags.None, cancellationToken);
            """);
        TutorialConsole.WriteEvidence(
            "Troca de mensagens",
            ("Cliente enviou", exchange.ClientSent),
            ("Servidor recebeu", exchange.ServerReceived),
            ("Cliente recebeu", exchange.ClientReceived),
            ("Bytes trafegados", exchange.BytesTransferred.ToString()));

        TutorialConsole.WriteObservation(
            "Um socket trabalha com bytes. Texto, JSON ou qualquer outro formato precisa de codificação e de uma regra de fim de mensagem.");
        TutorialConsole.WriteConclusion(
            "A comunicação TCP básica exige endpoint, conexão, envio, leitura e fechamento explícito; protocolos de aplicação adicionam contrato sobre esses bytes.",
            TutorialConclusionKind.Success);
    }

    private static async Task<SocketExchange> RunSocketExchangeAsync(CancellationToken cancellationToken)
    {
        using var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        listener.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        listener.Listen();

        var endpoint = (IPEndPoint)listener.LocalEndPoint!;
        var serverTask = AcceptAndEchoAsync(listener, cancellationToken);
        var clientExchange = await SendAndReceiveAsync(endpoint, cancellationToken).ConfigureAwait(false);
        var serverExchange = await serverTask.ConfigureAwait(false);

        return new SocketExchange(
            endpoint,
            clientExchange.Sent,
            serverExchange.Received,
            clientExchange.Received,
            clientExchange.BytesTransferred + serverExchange.BytesTransferred);
    }

    private static async Task<ClientExchange> SendAndReceiveAsync(
        IPEndPoint endpoint,
        CancellationToken cancellationToken)
    {
        using var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        await client.ConnectAsync(endpoint, cancellationToken).ConfigureAwait(false);

        var payload = MessageEncoding.GetBytes(Message);
        var bytesSent = await client.SendAsync(payload, SocketFlags.None, cancellationToken).ConfigureAwait(false);

        var buffer = new byte[1024];
        var bytesRead = await client.ReceiveAsync(buffer, SocketFlags.None, cancellationToken).ConfigureAwait(false);
        client.Shutdown(SocketShutdown.Both);

        return new ClientExchange(
            Message,
            MessageEncoding.GetString(buffer, 0, bytesRead),
            bytesSent + bytesRead);
    }

    private static async Task<ServerExchange> AcceptAndEchoAsync(
        Socket listener,
        CancellationToken cancellationToken)
    {
        using var server = await listener.AcceptAsync(cancellationToken).ConfigureAwait(false);
        var buffer = new byte[1024];
        var received = new StringBuilder();
        var bytesTransferred = 0;

        while (!received.ToString().Contains(EndMarker, StringComparison.Ordinal))
        {
            var bytesRead = await server.ReceiveAsync(buffer, SocketFlags.None, cancellationToken).ConfigureAwait(false);
            if (bytesRead == 0)
            {
                break;
            }

            bytesTransferred += bytesRead;
            received.Append(MessageEncoding.GetString(buffer, 0, bytesRead));
        }

        var echo = MessageEncoding.GetBytes(received.ToString());
        bytesTransferred += await server.SendAsync(echo, SocketFlags.None, cancellationToken).ConfigureAwait(false);
        server.Shutdown(SocketShutdown.Both);

        return new ServerExchange(received.ToString(), bytesTransferred);
    }

    private sealed record ClientExchange(string Sent, string Received, int BytesTransferred);

    private sealed record ServerExchange(string Received, int BytesTransferred);

    private sealed record SocketExchange(
        IPEndPoint Endpoint,
        string ClientSent,
        string ServerReceived,
        string ClientReceived,
        int BytesTransferred);
}

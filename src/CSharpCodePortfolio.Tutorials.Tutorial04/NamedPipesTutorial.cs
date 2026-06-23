using CSharpCodePortfolio.Shared;
using CSharpCodePortfolio.Tutorials.Abstractions;
using System.IO.Pipes;
using System.Text;

namespace CSharpCodePortfolio.Tutorials.Tutorial04;

[Tutorial("04", "named-pipes", "Comunicação cliente-servidor com named pipes")]
public sealed class NamedPipesTutorial : ITutorial
{
    private const string ClientMessage = "Olá do cliente";
    private const string EndMessage = "Fim";
    private static readonly Encoding MessageEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        TutorialConsole.WriteHeader("04", "Comunicação cliente-servidor com named pipes");
        TutorialConsole.WriteContext(
            ("Tema", "Named pipes"),
            ("Conceito", "Servidor cria um pipe nomeado e cliente conecta pelo mesmo nome"),
            ("Runtime", ".NET 10"),
            ("Slug", "named-pipes"));
        TutorialConsole.WriteQuestion("Como duas partes trocam mensagens usando um pipe nomeado local?");
        TutorialConsole.WriteHypothesis(
            "O servidor fica aguardando conexão em um nome conhecido.",
            "O cliente conecta no mesmo nome e passa a usar o pipe como stream.",
            "Como o pipe é um stream, as duas partes precisam combinar uma regra simples de mensagem.");
        TutorialConsole.WritePreparation(
            "O nome do pipe é único para cada execução, evitando conflito com outros processos.",
            "O tutorial usa `PipeDirection.InOut` para permitir pergunta e resposta no mesmo canal.",
            "As mensagens são linhas UTF-8; `Fim` encerra o diálogo de forma explícita.");

        TutorialConsole.WriteExperiment(
            1,
            "Servidor nomeado",
            "Cria um `NamedPipeServerStream`, aguarda o cliente e responde a cada linha recebida.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: o servidor publica o nome do pipe e espera uma conexão.",
            typeof(NamedPipesTutorial),
            nameof(RunServerAsync),
            new CodeExcerpt(5, 12, "Criação do servidor e espera pelo cliente"));

        var exchange = await RunExchangeAsync(cancellationToken).ConfigureAwait(false);

        TutorialConsole.WriteEvidence(
            "Servidor",
            ("Nome do pipe", exchange.PipeName),
            ("Conectado", exchange.ServerConnected ? "sim" : "não"),
            ("Mensagens recebidas", string.Join(" -> ", exchange.ServerReceivedMessages)));

        TutorialConsole.WriteExperiment(
            2,
            "Cliente do pipe",
            "Conecta ao nome publicado, envia uma linha e lê a confirmação do servidor.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: o cliente usa o mesmo nome para abrir o stream.",
            typeof(NamedPipesTutorial),
            nameof(RunClientAsync),
            new CodeExcerpt(5, 11, "Abertura do cliente e conexão ao pipe"),
            new CodeExcerpt(19, 22, "Envio, leitura da resposta e encerramento"));
        TutorialConsole.WriteEvidence(
            "Cliente",
            ("Mensagem enviada", exchange.ClientSentMessage),
            ("Resposta recebida", exchange.ClientReceivedReply),
            ("Encerramento enviado", EndMessage));

        TutorialConsole.WriteObservation(
            "Named pipes são úteis quando processos na mesma máquina precisam de um canal simples, nomeado e sem porta TCP.");
        TutorialConsole.WriteConclusion(
            "O contrato mínimo é o nome do pipe, a direção do stream, a codificação e a regra de término da mensagem.",
            TutorialConclusionKind.Success);
    }

    private static async Task<NamedPipeExchange> RunExchangeAsync(CancellationToken cancellationToken)
    {
        var pipeName = $"portfolio-named-pipe-{Guid.NewGuid():N}";
        var serverTask = RunServerAsync(pipeName, cancellationToken);
        var clientExchange = await RunClientAsync(pipeName, cancellationToken).ConfigureAwait(false);
        var serverExchange = await serverTask.ConfigureAwait(false);

        return new NamedPipeExchange(
            pipeName,
            serverExchange.Connected,
            serverExchange.ReceivedMessages,
            clientExchange.SentMessage,
            clientExchange.ReceivedReply);
    }

    private static async Task<ServerExchange> RunServerAsync(
        string pipeName,
        CancellationToken cancellationToken)
    {
        await using var server = new NamedPipeServerStream(
            pipeName,
            PipeDirection.InOut,
            maxNumberOfServerInstances: 1,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous);

        await server.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);

        using var reader = new StreamReader(server, MessageEncoding, leaveOpen: true);
        await using var writer = new StreamWriter(server, MessageEncoding, leaveOpen: true)
        {
            AutoFlush = true
        };

        var receivedMessages = new List<string>();
        while (await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false) is { } message)
        {
            receivedMessages.Add(message);
            if (message == EndMessage)
            {
                break;
            }

            var reply = $"Servidor recebeu: {message}";
            await writer.WriteLineAsync(reply.AsMemory(), cancellationToken).ConfigureAwait(false);
        }

        return new ServerExchange(server.IsConnected, receivedMessages);
    }

    private static async Task<ClientExchange> RunClientAsync(
        string pipeName,
        CancellationToken cancellationToken)
    {
        await using var client = new NamedPipeClientStream(
            ".",
            pipeName,
            PipeDirection.InOut,
            PipeOptions.Asynchronous);

        await client.ConnectAsync(cancellationToken).ConfigureAwait(false);

        using var reader = new StreamReader(client, MessageEncoding, leaveOpen: true);
        await using var writer = new StreamWriter(client, MessageEncoding, leaveOpen: true)
        {
            AutoFlush = true
        };

        await writer.WriteLineAsync(ClientMessage.AsMemory(), cancellationToken).ConfigureAwait(false);
        var reply = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("O servidor não respondeu ao cliente.");
        await writer.WriteLineAsync(EndMessage.AsMemory(), cancellationToken).ConfigureAwait(false);

        return new ClientExchange(ClientMessage, reply);
    }

    private sealed record ServerExchange(bool Connected, IReadOnlyList<string> ReceivedMessages);

    private sealed record ClientExchange(string SentMessage, string ReceivedReply);

    private sealed record NamedPipeExchange(
        string PipeName,
        bool ServerConnected,
        IReadOnlyList<string> ServerReceivedMessages,
        string ClientSentMessage,
        string ClientReceivedReply);
}

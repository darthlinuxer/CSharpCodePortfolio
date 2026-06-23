using CSharpCodePortfolio.Shared;
using CSharpCodePortfolio.Tutorials.Abstractions;
using System.IO.Pipes;
using System.Text;

namespace CSharpCodePortfolio.Tutorials.Tutorial05;

[Tutorial("05", "anonymous-pipes", "Comunicação entre tarefas com anonymous pipes")]
public sealed class AnonymousPipesTutorial : ITutorial
{
    private const string EndMessage = "fim";
    private static readonly Encoding MessageEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    private static readonly string[] Messages = ["Mensagem 1", "Mensagem 2", EndMessage];

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        TutorialConsole.WriteHeader("05", "Comunicação entre tarefas com anonymous pipes");
        TutorialConsole.WriteContext(
            ("Tema", "Anonymous pipes"),
            ("Conceito", "Um lado cria o pipe e entrega o handle para o outro lado"),
            ("Runtime", ".NET 10"),
            ("Slug", "anonymous-pipes"));
        TutorialConsole.WriteQuestion("Como duas tarefas no mesmo processo podem trocar linhas por um anonymous pipe?");
        TutorialConsole.WriteHypothesis(
            "O servidor cria o pipe e expõe um handle de cliente.",
            "O cliente abre o outro lado usando esse handle, sem depender de um nome global.",
            "A conversa precisa de uma regra de término para o leitor saber quando parar.");
        TutorialConsole.WritePreparation(
            "O pipe é unidirecional: servidor lê e cliente escreve.",
            "As mensagens são linhas UTF-8.",
            "A mensagem `fim` encerra a leitura.");

        TutorialConsole.WriteExperiment(
            1,
            "Servidor anônimo",
            "Cria o lado leitor e obtém o handle que será entregue ao escritor.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: o servidor não publica um nome; ele compartilha o handle do cliente.",
            typeof(AnonymousPipesTutorial),
            nameof(RunExchangeAsync),
            new CodeExcerpt(3, 7, "Criação do leitor e handle do cliente"));

        var exchange = await RunExchangeAsync(cancellationToken).ConfigureAwait(false);

        TutorialConsole.WriteEvidence(
            "Servidor",
            ("Handle entregue", exchange.ClientHandle),
            ("Handle mantido durante a troca", exchange.ClientHandleKeptDuringExchange ? "sim" : "não"),
            ("Mensagens lidas", string.Join(" -> ", exchange.ReadMessages)));

        TutorialConsole.WriteExperiment(
            2,
            "Cliente escritor",
            "Abre o lado de saída com o handle recebido e envia linhas até a mensagem de término.");
        TutorialConsole.WriteCodeSnippet(
            "Código real: o cliente usa o handle para construir seu stream de saída.",
            typeof(AnonymousPipesTutorial),
            nameof(RunExchangeAsync),
            new CodeExcerpt(8, 10, "Abertura do lado escritor com o handle"));
        TutorialConsole.WriteCodeSnippet(
            "Código real: o cliente escreve cada linha no pipe.",
            typeof(AnonymousPipesTutorial),
            nameof(WriteMessagesAsync),
            new CodeExcerpt(14, 17, "Envio das mensagens até o marcador final"));
        TutorialConsole.WriteEvidence(
            "Cliente",
            ("Direção", "saída"),
            ("Mensagens enviadas", string.Join(" -> ", exchange.WrittenMessages)));

        TutorialConsole.WriteObservation(
            "Anonymous pipes funcionam bem quando o handle pode ser entregue diretamente para a outra parte da comunicação.");
        TutorialConsole.WriteConclusion(
            "O contrato mínimo é o handle compartilhado, a direção do pipe, a codificação e a mensagem de término.",
            TutorialConclusionKind.Success);
    }

    private static async Task<AnonymousPipeExchange> RunExchangeAsync(CancellationToken cancellationToken)
    {
        await using var pipeReader = new AnonymousPipeServerStream(
            PipeDirection.In,
            HandleInheritability.None);

        var clientHandle = pipeReader.GetClientHandleAsString();
        await using var pipeWriter = new AnonymousPipeClientStream(
            PipeDirection.Out,
            clientHandle);

        var readTask = ReadMessagesAsync(pipeReader, cancellationToken);
        var writeTask = WriteMessagesAsync(pipeWriter, cancellationToken);

        await Task.WhenAll(readTask, writeTask).ConfigureAwait(false);

        return new AnonymousPipeExchange(
            clientHandle,
            ClientHandleKeptDuringExchange: true,
            readTask.Result,
            Messages);
    }

    private static async Task<IReadOnlyList<string>> ReadMessagesAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(
            stream,
            MessageEncoding,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 1024,
            leaveOpen: true);

        var messages = new List<string>();
        while (await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false) is { } message)
        {
            messages.Add(message);
            if (message == EndMessage)
            {
                break;
            }
        }

        return messages;
    }

    private static async Task WriteMessagesAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        await using var writer = new StreamWriter(
            stream,
            MessageEncoding,
            bufferSize: 1024,
            leaveOpen: true)
        {
            AutoFlush = true
        };

        foreach (var message in Messages)
        {
            await writer.WriteLineAsync(message.AsMemory(), cancellationToken).ConfigureAwait(false);
        }
    }

    private sealed record AnonymousPipeExchange(
        string ClientHandle,
        bool ClientHandleKeptDuringExchange,
        IReadOnlyList<string> ReadMessages,
        IReadOnlyList<string> WrittenMessages);
}

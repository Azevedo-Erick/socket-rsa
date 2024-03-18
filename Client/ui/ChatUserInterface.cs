using Client.config;
using Client.models;
using Client.utils;
using Spectre.Console;

namespace Client.ui;

public class ChatUserInterface
{
    private readonly ChatService ChatService;
    private readonly string Nome;
    public ChatUserInterface(String nome)
    {

        if (string.IsNullOrEmpty(nome))
        {
            AnsiConsole.Write(new Panel("[red]Nome inválido![/]")
                        .Expand().HeaderAlignment(Justify.Center)
                        .SquareBorder()
                        .Padding(1, 4));
            Environment.Exit(1);
        }

        ChatService = new ChatService();
        if (!ChatService.Connect(nome, Configuration.Server, Configuration.Port))
        {
            AnsiConsole.Write(new Panel("[red]Falha ao conectar ao servidor![/]")
                        .Expand().HeaderAlignment(Justify.Center)
                        .SquareBorder()
                        .Padding(1, 4));
            Environment.Exit(1);
        }
        Nome = nome;
    }

    public void iniciarAplicacao()
    {
        AnsiConsole.Write(new Panel("[underline]Chat super secreto[/]"));
        AnsiConsole.Write(new Rule("[red]Digite algo![/]"));

        var mensagem = "";
        Destinatario destinatario = new Destinatario();

        var destinatarios = ChatService.GetDestinatarios(Configuration.PublicKeysDirectory);

        if (destinatarios.Count == 0)
        {
            AnsiConsole.Write(new Panel("[red]Nenhum destinatário encontrado![/]")
                        .Expand().HeaderAlignment(Justify.Center)
                        .SquareBorder()
                        .Padding(1, 4));
            Environment.Exit(1);
        }

        var dest = AnsiConsole.Prompt(
             new SelectionPrompt<string>()
             .Title("Escolha um destinatário:")
             .PageSize(10)
             .AddChoices(destinatarios.Where(d => !d.Nome.ToLower().Equals(Nome.ToLower())).Select(d => d.Nome).ToArray())
             );
        var destinoFinal = destinatarios.First(d => d.Nome == dest);
        do
        {
            var message = AnsiConsole.Prompt(
                new TextPrompt<string>("[green]Mensagem:[/]")
                .PromptStyle("green")
                .AllowEmpty()
                .DefaultValue(mensagem)
                .ShowDefaultValue(!(mensagem == "")).DefaultValueStyle("yellow")
                );

            var sendButton = "Encriptar e Enviar";
            var editar = "Editar";
            var destino = "Alterar destinatário";
            var historico = "Histórico";
            var quitButton = "Sair";

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                .Title("Escolha uma ação:")
                .PageSize(10)
                .AddChoices([sendButton, destino, historico, editar, quitButton]));

            if (choice == sendButton)
            {
                message = getSendUi(destinoFinal, message);
            }
            else if (choice == destino)
            {
                alterarDestinatario(out destinatario, destinatarios,  dest);
            }
            else if (choice == historico)
            {
                getHistorico(destinoFinal);

            }
            else if (choice == editar)
            {
                mensagem = message;
            }
            else if (choice == quitButton)
            {
                Environment.Exit(1);
            }
        } while (true);
    }

    private string getSendUi(Destinatario destinoFinal, string message)
    {
        try
        {
            if (!string.IsNullOrEmpty(message))
            {

                RSAUtils.EncryptMessage(message, UserInfo.PublicKeyPath);

                if (ChatService.SendMessage(Nome, destinoFinal.FileName, "message.enc"))
                {
                    AnsiConsole.MarkupLine("[green]Mensagem enviada com sucesso![/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]Falha ao enviar a mensagem[/]");
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            AnsiConsole.MarkupLine("[red]Ocorreu um erro:[/] {0}", e.Message);
        }
        message = "";
        return message;
    }

    private static void alterarDestinatario(out Destinatario destinatario, List<Destinatario> destinatarios,  string dest)
    {
        dest = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                            .Title("Escolha um destinatário:")
                            .PageSize(10)
                            .AddChoices(destinatarios.Select(d => d.Nome).ToArray())
                            );
        destinatario = destinatarios.First(d => d.Nome == dest);
    }

    private void getHistorico(Destinatario destinoFinal)
    {
        var mensagens = ChatService.history(Nome, destinoFinal.FileName);
        Console.WriteLine(mensagens.Count);
        var grid = new Grid();
        grid.AddColumn();
        grid.AddColumn();
        grid.AddColumn();

        grid.AddRow("Nome", "Mensagem", "Data");
        grid.AddRow("--------", "--------", "--------");

        foreach (var item in mensagens)
        {
            if (item.Author == Nome)
                grid.AddRow("Você", item.Message, item.Timestamp.ToString()).RightAligned();
            else
                grid.AddRow(item.Author, item.Message, item.Timestamp.ToString()).LeftAligned();
        }
        AnsiConsole.Write(grid);
    }
}

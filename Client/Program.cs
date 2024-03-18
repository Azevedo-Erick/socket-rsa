using Client;
using Client.ui;
using Client.utils;
using Spectre.Console;


var nome = AnsiConsole.Ask<string>("Qual é o seu nome?").ToLower().Replace(" ", "_").Trim();

if (!File.Exists($"./keys/{nome}.key.private.pem"))
{
    var criarChaves = AnsiConsole.Confirm("Deseja criar as chaves RSA agora?");
    if (criarChaves)
    {
        RSAKeyGenerator.GenerateKeysForClient(nome);
    }
    else
    {
        AnsiConsole.Write(new Panel("[red]Impossível prosseguir sem gerar as chaves![/]")
                    .Expand().HeaderAlignment(Justify.Center)
                    .SquareBorder()
                    .Padding(1, 4));
        Environment.Exit(1);
    }
}

UserInfo.Nome = nome;
UserInfo.PrivateKeyPath = $"./keys/{nome}.key.private.pem";
UserInfo.PublicKeyPath = $"./keys/{nome}.key.public.pem";

new ChatUserInterface(nome)
    .iniciarAplicacao();

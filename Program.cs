

using lembrai;

if (!args.Any())
    throw new Exception("Lembrar de que?");

List<string> Opcoes = new() { "GET", "SET" };

if (!Opcoes.Contains(args[0]))
    throw new Exception("O que vc quer? GET OU SET?");

if (args[0] == "GET")
    Devolve.Lembrar(args[1]);

if (args[0] == "SET")
    Devolve.Escrever(args[1], args[2]);


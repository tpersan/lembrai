

using lembrai;

if (!args.Any())
    throw new Exception("Lembrar de que?");

List<string> Opcoes = new() { "GET", "SET" };

var comando = args[0].ToUpperInvariant();

if (!Opcoes.Contains(comando))
    throw new Exception("O que vc quer? GET OU SET?");

if (comando == "GET")
    Devolve.Lembrar(args[1]);

if (comando == "SET")
    Devolve.Escrever(args[1], args[2]);

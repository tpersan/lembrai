

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using System.Windows.Forms;
using static lembrai.Devolve;

namespace lembrai
{
    internal static class Devolve
    {
        internal class Memoria
        {

            public Memoria(string _chave, string _valor)
            {
                Chave = _chave;
                Valor = _valor;
            }

            public string Chave { get; set; }
            public string Valor { get; set; }
        }


        internal static void Escrever(string nomeDaMemoria, string valor)
        {
            var gerenciadorDeMemoria = new FileManager();
            var memorias = gerenciadorDeMemoria.ObterItens();

            var memoria = memorias.FirstOrDefault(m => m.Chave.ToUpperInvariant() == nomeDaMemoria.ToUpperInvariant());

            if (memoria == null)
            {
                memoria = new Memoria(nomeDaMemoria, valor);
                memorias.Add(memoria);
            }

            memoria.Valor = valor;

            gerenciadorDeMemoria.Salvar(memorias);
        }


        //[STAThread]
        internal static void Lembrar(string nomeDaMemoria)
        {
            var memorias = new FileManager().ObterItens();
            var memoria = memorias.FirstOrDefault(m => m.Chave.ToUpperInvariant() == nomeDaMemoria.ToUpperInvariant());

            if (memoria == null)
                throw new Exception($"Memória '{nomeDaMemoria}' não encontrada!");


            RunAsSTAThread(
            () =>
            {
                Clipboard.SetText(memoria.Valor);
            });
        }


        internal static void RunAsSTAThread(Action goForIt)
        {
            AutoResetEvent @event = new AutoResetEvent(false);
            Thread thread = new Thread(
                () =>
                {
                    goForIt();
                    @event.Set();
                });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            @event.WaitOne();
        }

    }

    internal class FileManager
    {
        private readonly string arquivo = "oquemesmo.txt";

        internal List<Memoria> ObterItens()
        {
            var decriptografia = new Encryptor();
            var retorno = new List<Memoria>();

            using (StreamReader sr = new StreamReader(arquivo))
            {
                string line;

                while ((line = sr.ReadLine()) != null)
                {
                    string[] arr = line.Split(' ');
                    int i = 0;
                    while (i < arr.Length)
                    {
                        retorno.Add(new Memoria(arr[i], decriptografia.Decrypt(arr[i + 1])));
                        i += 2;
                    }
                }
            }

            return retorno;
        }

        internal void Salvar(List<Memoria> itens)
        {
            var decriptografia = new Encryptor();

            using (StreamWriter file = new StreamWriter(arquivo))
                foreach (var item in itens)
                    file.WriteLine("{0} {1}", item.Chave, decriptografia.Encrypt(item.Valor));
        }
    }

    internal class Encryptor
    {
        public string password = "b94ca5898a4e4133bbce2ea2315a1916";

        private byte[] IV =
{
    0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
    0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16
};


        public string Encrypt(string texto)
        {
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(password);
                aes.IV = IV;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using MemoryStream memoryStream = new();
                using CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write);
                using (StreamWriter streamWriter = new(cryptoStream))
                {
                    streamWriter.Write(texto);
                }

                array = memoryStream.ToArray();
            }

            return Convert.ToBase64String(array);
        }

        public string Decrypt(string encrypted)
        {

            byte[] buffer = Convert.FromBase64String(encrypted);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(password);
                aes.IV = IV;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using MemoryStream memoryStream = new(buffer);
                using CryptoStream cryptoStream = new(memoryStream, decryptor, CryptoStreamMode.Read);
                using StreamReader streamReader = new(cryptoStream);
                return streamReader.ReadToEnd();
            }
        }

    }
}

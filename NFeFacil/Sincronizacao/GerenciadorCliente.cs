﻿using static NFeFacil.Sincronizacao.ConfiguracoesSincronizacao;
using NFeFacil.Log;
using NFeFacil.Sincronizacao.Pacotes;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System;

namespace NFeFacil.Sincronizacao
{
    public sealed class GerenciadorCliente
    {
        private ILog Log { get; }

        public GerenciadorCliente(ILog log)
        {
            Log = log;
        }

        public async Task EstabelecerConexao(int senha)
        {
            var info = await RequestAsync<InfoSegurancaConexao>("BrechaSeguranca", HttpMethod.Get, senha, null);
            SenhaPermanente = info.Senha;
            Log.Escrever(TitulosComuns.Sucesso, "Chave de segurança decodificada e salva com sucesso.");
        }

        internal async Task Sincronizar()
        {
            using (var db = new AplicativoContext())
            {
                var momento = UltimaSincronizacao;

                var receb = await RequestAsync<ConjuntoBanco>(
                    $"Sincronizar",
                    HttpMethod.Get,
                    SenhaPermanente,
                    new ConjuntoBanco(db, momento));
                receb.AnalisarESalvar(db);

                Log.Escrever(TitulosComuns.Sucesso, "Sincronização simples concluida.");
                db.SaveChanges();
            }
        }

        internal async Task SincronizarTudo()
        {
            using (var db = new AplicativoContext())
            {
                var receb = await RequestAsync<ConjuntoBanco>(
                    $"Sincronizar",
                    HttpMethod.Get,
                    SenhaPermanente,
                    new ConjuntoBanco(db));
                receb.AnalisarESalvar(db);

                Log.Escrever(TitulosComuns.Sucesso, "Sincronização total concluida.");
                db.SaveChanges();
            }
        }

        async Task<T> RequestAsync<T>(string nomeMetodo, HttpMethod metodo, int senha, IPacote corpo)
        {
            string caminho = $"http://{IPServidor}:8080/{nomeMetodo}/{senha}";
            using (var proxy = new HttpClient())
            {
                var mensagem = new HttpRequestMessage(metodo, caminho);
                if (metodo == HttpMethod.Post && corpo != null)
                {
                    var json = JsonConvert.SerializeObject(corpo);
                    mensagem.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }
                var resposta = await proxy.SendAsync(mensagem);
                var texto = await resposta.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(texto);
            }
        }
    }
}

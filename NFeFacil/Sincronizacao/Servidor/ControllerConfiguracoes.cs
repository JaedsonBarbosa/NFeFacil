﻿using static NFeFacil.Configuracoes.ConfiguracoesSincronizacao;
using Restup.Webserver.Attributes;
using Restup.Webserver.Models.Contracts;
using Restup.Webserver.Models.Schemas;
using System;

namespace NFeFacil.Sincronizacao.Servidor
{
    [RestController(InstanceCreationType.PerCall)]
    class ControllerConfiguracoes
    {
        [UriFormat("/Configuracoes/GET/{senha}")]
        public IGetResponse Get(int senha)
        {
            return SupervisionarOperacao.Iniciar(() =>
            {
                if (senha != SenhaPermanente)
                    throw new SenhaErrada(senha);

                return new GetResponse(GetResponse.ResponseStatus.OK, new Pacotes.ConfiguracoesServidor
                {
                    DadosBase = SincDadoBase,
                    Notas = SincNotaFiscal
                });
            }, DateTime.Now, TipoDado.Configuracao);
        }
    }
}

﻿using BaseGeral.ModeloXML.PartesDetalhes;
using BaseGeral.ModeloXML.PartesDetalhes.PartesProduto;
using BaseGeral.ModeloXML.PartesDetalhes.PartesProduto.PartesImpostos;

namespace Venda.Impostos.DetalhamentoICMS
{
    public sealed class DetalharVazio : IProcessamentoImposto
    {
        readonly string CSOSN;
        readonly int Origem;
        public PrincipaisImpostos Tipo => PrincipaisImpostos.ICMS;

        public DetalharVazio(string csosn, int origem)
        {
            CSOSN = csosn;
            Origem = origem;
        }

        public IImposto[] Processar(DetalhesProdutos prod)
        {
            return new IImposto[1] { new ICMS
            {
                Corpo = (ComumICMS)new DadosSN.TipoNT
                {
                    CSOSN = CSOSN,
                    Origem = Origem
                }.Processar(prod)
            }};
        }
    }
}

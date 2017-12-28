﻿using NFeFacil.ItensBD.Produto;
using NFeFacil.ModeloXML.PartesProcesso.PartesNFe.PartesDetalhes;
using NFeFacil.ModeloXML.PartesProcesso.PartesNFe.PartesDetalhes.PartesProduto;
using NFeFacil.ModeloXML.PartesProcesso.PartesNFe.PartesDetalhes.PartesProduto.PartesImpostos;
using System.Xml.Linq;

namespace NFeFacil.ViewNFe.Impostos.DetalhamentoIPI
{
    public sealed class Processamento : ProcessamentoImposto
    {
        DadosIPI dados;

        public override ImpostoBase[] Processar(DetalhesProdutos prod)
        {
            var resultado = dados.Processar(prod.Produto);
            return new ImpostoBase[1] { (IPI)resultado };
        }

        public override bool ValidarDados() => !string.IsNullOrEmpty(dados?.PreImposto?.cEnq);

        public override void ProcessarEntradaDados(object Tela)
        {
            if (Detalhamento is Detalhamento detalhamento)
            {
                if (Tela is DetalharAliquota aliq)
                {
                    ProcessarDados(TiposCalculo.PorAliquota, aliq.Aliquota, 0, detalhamento.CST, aliq.Conjunto);
                }
                else if (Tela is DetalharQtde valor)
                {
                    ProcessarDados(TiposCalculo.PorValor, 0, valor.ValorUnitario, detalhamento.CST, valor.Conjunto);
                }
                else if (Tela is DetalharSimples outr)
                {
                    ProcessarDados(TiposCalculo.Inexistente, 0, 0, detalhamento.CST, outr.Conjunto);
                }
            }
            else if (Detalhamento is DadoPronto pronto)
            {
                ProcessarDadosProntos(pronto.ImpostoPronto);
            }
        }

        protected override void ProcessarDadosProntos(ImpostoArmazenado imposto)
        {
            if (imposto is ImpSimplesArmazenado imp)
            {
                var bruto = XElement.Parse(imp.IPI).FromXElement<ImpSimplesArmazenado.XMLIPIArmazenado>();
                var ipi = new IPI()
                {
                    cEnq = bruto.CEnq,
                    clEnq = bruto.ClEnq,
                    cSelo = bruto.CSelo,
                    CNPJProd = bruto.CNPJProd,
                    qSelo = bruto.QSelo,
                    Corpo = new IPINT()
                };
                ProcessarDados(imp.TipoCalculo, imp.Aliquota, imp.Valor, imp.CST, ipi);
            }
        }

        void ProcessarDados(TiposCalculo tpCalculo, double aliquota, double valor, int cst, IPI preImposto)
        {
            switch (tpCalculo)
            {
                case TiposCalculo.PorAliquota:
                    dados = new DadosTrib()
                    {
                        CST = cst.ToString("00"),
                        Aliquota = aliquota,
                        PreImposto = preImposto,
                        TipoCalculo = TiposCalculo.PorAliquota
                    };
                    break;
                case TiposCalculo.PorValor:
                    dados = new DadosTrib()
                    {
                        CST = cst.ToString("00"),
                        Valor = valor,
                        PreImposto = preImposto,
                        TipoCalculo = TiposCalculo.PorValor
                    };
                    break;
                default:
                    dados = new DadosNT
                    {
                        CST = cst.ToString("00"),
                        PreImposto = preImposto
                    };
                    break;
            }
            dados.CST = cst.ToString("00");
        }
    }
}

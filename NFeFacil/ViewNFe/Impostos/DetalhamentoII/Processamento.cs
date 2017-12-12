﻿using NFeFacil.Log;
using NFeFacil.ModeloXML.PartesProcesso.PartesNFe.PartesDetalhes.PartesProduto;

namespace NFeFacil.ViewNFe.Impostos.DetalhamentoII
{
    sealed class Processamento : ProcessamentoImposto
    {
        IDadosII dados;

        public override Imposto[] Processar(ProdutoOuServico prod)
        {
            var imposto = dados.Imposto;
            return new Imposto[1] { imposto };
        }

        public override bool ValidarDados(ILog log)
        {
            var imposto = dados.Imposto;
            if (string.IsNullOrEmpty(imposto.vBC))
            {
                log.Escrever(TitulosComuns.Atenção, "O valor da base de cálculo é obrigatório.");
            }
            else if (string.IsNullOrEmpty(imposto.vDespAdu))
            {
                log.Escrever(TitulosComuns.Atenção, "O valor das despesas aduaneiras é obrigatório.");
            }
            else if (string.IsNullOrEmpty(imposto.vII))
            {
                log.Escrever(TitulosComuns.Atenção, "É necessário que o valor do II seja informado.");
            }
            else if (string.IsNullOrEmpty(imposto.vIOF))
            {
                log.Escrever(TitulosComuns.Atenção, "O valor do imposto sobre operações financeiras deve ser informado.");
            }
            else
            {
                return true;
            }
            return false;
        }

        public override bool ValidarEntradaDados(ILog log)
        {
            if (Detalhamento is Detalhamento detalhamento
                && Tela?.GetType() == typeof(Detalhar))
            {
                dados = (IDadosII)Tela;
                return true;
            }
            return false;
        }
    }
}

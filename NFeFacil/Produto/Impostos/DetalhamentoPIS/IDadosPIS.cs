﻿using NFeFacil.ModeloXML.PartesDetalhes.PartesProduto;

// O modelo de item de Página em Branco está documentado em https://go.microsoft.com/fwlink/?LinkId=234238

namespace NFeFacil.Produto.Impostos.DetalhamentoPIS
{
    interface IDadosPIS
    {
        string CST { set; }
        object Processar(ProdutoOuServico prod);
    }
}

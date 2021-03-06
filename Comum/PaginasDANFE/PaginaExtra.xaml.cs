﻿using Comum.PacotesDANFE;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static BaseGeral.ExtensoesPrincipal;

// O modelo de item de Página em Branco está documentado em https://go.microsoft.com/fwlink/?LinkId=234238

namespace Comum.PaginasDANFE
{
    /// <summary>
    /// Uma página vazia que pode ser usada isoladamente ou navegada dentro de um Quadro.
    /// </summary>
    public sealed partial class PaginaExtra : UserControl, IPagina
    {
        double LarguraPagina => CMToPixel(21);
        double AlturaPagina => CMToPixel(29.7);
        Thickness MargemPadrao => new Thickness(CMToPixel(1));

        DadosNFe ContextoNFe { get; }

        public PaginaExtra(DadosNFe cabecalho, IEnumerable<DadosProduto> produtos, RichTextBlock infoAdicional, UIElementCollection paiPaginas, MotivoCriacaoPaginaExtra motivo, PaginaPrincipal principal)
        {
            InitializeComponent();
            ContextoNFe = cabecalho;
            if (motivo == MotivoCriacaoPaginaExtra.Ambos)
            {
                double total = 0, maximo = espacoParaProdutos.ActualHeight;
                var produtosNestaPagina = produtos.TakeWhile(x =>
                {
                    var item = new PartesDANFE.ItemProduto() { DataContext = x };
                    item.Measure(new Windows.Foundation.Size(PartesDANFE.DimensoesPadrao.LarguraTotalStatic, espacoParaProdutos.ActualHeight));
                    total += item.DesiredSize.Height;
                    return total <= maximo;
                });
                campoProdutos.DataContext = produtosNestaPagina.ToArray();

                bool excessoProdutos = produtos.Count() - produtosNestaPagina.Count() > 0;

                if (excessoProdutos)
                {
                    var produtosRestantes = produtos.Except(produtosNestaPagina);
                    paiPaginas.Add(new PaginaExtra(cabecalho, produtosRestantes, infoAdicional, paiPaginas, MotivoCriacaoPaginaExtra.Ambos, principal));
                }
                else
                {
                    infoAdicional.OverflowContentTarget = campoInfo;
                    if (infoAdicional.HasOverflowContent)
                    {
                        infoAdicional.OverflowContentTarget = null;
                        grd.Children.Remove(geralCampoInfo);
                        paiPaginas.Add(new PaginaExtra(cabecalho, null, infoAdicional, paiPaginas, MotivoCriacaoPaginaExtra.Observacao, principal));
                    }
                }
            }
            else if (motivo == MotivoCriacaoPaginaExtra.Observacao)
            {
                grd.Children.Remove(campoProdutos);
                espacoParaProdutos.Height = new GridLength(0);
                infoAdicional.OverflowContentTarget = campoInfo;
            }
            else
            {
                grd.Children.Remove(geralCampoInfo);

                double total = 0, maximo = espacoParaProdutos.ActualHeight;
                var produtosNestaPagina = produtos.TakeWhile(x =>
                {
                    var item = new PartesDANFE.ItemProduto() { DataContext = x };
                    item.Measure(new Windows.Foundation.Size(PartesDANFE.DimensoesPadrao.LarguraTotalStatic, espacoParaProdutos.ActualHeight));
                    total += item.DesiredSize.Height;
                    return total <= maximo;
                });
                produtosNestaPagina.ToList().ForEach(campoProdutos.Contexto.Add);

                bool excessoProdutos = produtos.Count() - produtosNestaPagina.Count() > 0;

                if (excessoProdutos)
                {
                    var produtosRestantes = produtos.Except(produtosNestaPagina);
                    paiPaginas.Add(new PaginaExtra(cabecalho, produtosRestantes, infoAdicional, paiPaginas, MotivoCriacaoPaginaExtra.Produtos, principal));
                }
            }
            principal.OnPaginaCarregada();
        }

        public void DefinirPagina(int atual, int total)
        {
            parteDadosNFe.DefinirPagina(atual, total);
        }
    }
}

﻿using NFeFacil.ItensBD.Produto;
using NFeFacil.ModeloXML.PartesProcesso.PartesNFe.PartesDetalhes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Navigation;

// O modelo de item de Página em Branco está documentado em https://go.microsoft.com/fwlink/?LinkId=234238

namespace NFeFacil.ViewNFe.Impostos
{
    [View.DetalhePagina("\uE825", "Impostos")]
    public sealed partial class EscolhaImpostos : Page
    {
        ICollectionView Impostos { get; set; }

        Dictionary<int, IDetalhamentoImposto> Escolhidos { get; set; } = new Dictionary<int, IDetalhamentoImposto>();

        DetalhesProdutos ProdutoCompleto;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var conjunto = (DadosAdicaoProduto)e.Parameter;
            ProdutoCompleto = conjunto.Completo;

            var caixa = new MessageDialog("Qual o tipo de imposto que é usado neste dado?", "Entrada");
            caixa.Commands.Add(new UICommand("ICMS"));
            caixa.Commands.Add(new UICommand("ISSQN"));
            List<ImpostoEscolhivel> impostos;
            if ((await caixa.ShowAsync()).Label == "ICMS")
            {
                impostos = new List<ImpostoEscolhivel>
                {
                    new ImpostoEscolhivel(new ImpostoPadrao(PrincipaisImpostos.ICMS)),
                    new ImpostoEscolhivel(new ImpostoPadrao(PrincipaisImpostos.IPI)),
                    new ImpostoEscolhivel(new ImpostoPadrao(PrincipaisImpostos.PIS)),
                    new ImpostoEscolhivel(new ImpostoPadrao(PrincipaisImpostos.COFINS)),
                    new ImpostoEscolhivel(new ImpostoPadrao(PrincipaisImpostos.II)),
                    new ImpostoEscolhivel(new ImpostoPadrao(PrincipaisImpostos.ICMSUFDest))
                };
                var icms = conjunto.Auxiliar.GetICMSArmazenados().Select(x => new ImpostoEscolhivel(x));
                impostos.AddRange(icms);
            }
            else
            {
                impostos = new List<ImpostoEscolhivel>
                {
                    new ImpostoEscolhivel(new ImpostoPadrao(PrincipaisImpostos.IPI)),
                    new ImpostoEscolhivel(new ImpostoPadrao(PrincipaisImpostos.PIS)),
                    new ImpostoEscolhivel(new ImpostoPadrao(PrincipaisImpostos.COFINS)),
                    new ImpostoEscolhivel(new ImpostoPadrao(PrincipaisImpostos.ISSQN)),
                    new ImpostoEscolhivel(new ImpostoPadrao(PrincipaisImpostos.ICMSUFDest))
                };
            }
            var imps = conjunto.Auxiliar.GetImpSimplesArmazenados().Select(x => new ImpostoEscolhivel(x));
            impostos.AddRange(imps);

            int i = 0;
            impostos.ForEach(x => x.Id = i++);
            Impostos = new CollectionViewSource()
            {
                IsSourceGrouped = true,
                Source = from imp in impostos
                         group imp by imp.Template.Tipo
            }.View;
            InitializeComponent();
        }

        async void Grd_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var input = (GridView)sender;
            var quantAdicionada = e.AddedItems.Count;
            var quantRemovida = e.RemovedItems.Count;
            if (quantRemovida > 0)
            {
                for (int i = 0; i < quantRemovida; i++)
                {
                    var item = (ImpostoEscolhivel)e.RemovedItems[i];
                    Escolhidos.Remove(item.Id);
                }
            }
            if (quantAdicionada > 0)
            {
                for (int i = 0; i < quantAdicionada; i++)
                {
                    var item = (ImpostoEscolhivel)e.AddedItems[i];
                    bool sucesso = true;
                    if (item.Template is ImpostoPadrao)
                    {
                        switch (item.Template.Tipo)
                        {
                            case PrincipaisImpostos.ICMS:
                                sucesso = await DetalharICMS(item.Id);
                                break;
                            case PrincipaisImpostos.IPI:
                                sucesso = await DetalharIPI(item.Id);
                                break;
                            case PrincipaisImpostos.II:
                                Escolhidos.Add(item.Id, new DetalhamentoII.Detalhamento());
                                break;
                            case PrincipaisImpostos.ISSQN:
                                await DetalharISSQN(item.Id);
                                break;
                            case PrincipaisImpostos.PIS:
                                sucesso = await DetalharPIS(item.Id);
                                break;
                            case PrincipaisImpostos.COFINS:
                                sucesso = await DetalharCOFINS(item.Id);
                                break;
                            case PrincipaisImpostos.ICMSUFDest:
                                Escolhidos.Add(item.Id, new DetalhamentoICMSUFDest.Detalhamento());
                                break;
                        }
                    }
                    else
                    {
                        Escolhidos.Add(item.Id, new DadoPronto { ImpostoPronto = item.Template });
                    }

                    if (sucesso)
                    {
                        var antigo = Escolhidos
                            .FirstOrDefault(x => x.Value.Tipo == item.Template.Tipo && x.Key != item.Id);
                        if (antigo.Value != null)
                        {
                            var itemExib = input.Items.FirstOrDefault(x => ((ImpostoEscolhivel)x).Id == antigo.Key);
                            var index = input.Items.IndexOf(itemExib);
                            input.DeselectRange(new ItemIndexRange(index, 1));
                        }
                    }
                    else
                    {
                        var index = input.Items.IndexOf(item);
                        input.DeselectRange(new ItemIndexRange(index, 1));
                    }
                }
            }
        }

        async Task<bool> DetalharICMS(int id)
        {
            var caixa = new EscolherTipoICMS();
            if (await caixa.ShowAsync() == ContentDialogResult.Primary)
            {
                Escolhidos.Add(id, new DetalhamentoICMS.Detalhamento
                {
                    Origem = caixa.Origem,
                    TipoICMSRN = caixa.TipoICMSRN,
                    TipoICMSSN = caixa.TipoICMSSN
                });
                return true;
            }
            return false;
        }

        async Task<bool> DetalharIPI(int id)
        {
            var caixa = new EscolherTipoIPI();
            if (await caixa.ShowAsync() == ContentDialogResult.Primary)
            {
                Escolhidos.Add(id, new DetalhamentoIPI.Detalhamento
                {
                    CST = int.Parse(caixa.CST),
                    TipoCalculo = caixa.TipoCalculo
                });
                return true;
            }
            return false;
        }

        async Task DetalharISSQN(int id)
        {
            var caixa = new MessageDialog("Qual o tipo de ISSQN desejado?", "Entrada");
            caixa.Commands.Add(new UICommand("Nacional"));
            caixa.Commands.Add(new UICommand("Exterior"));
            Escolhidos.Add(id, new DetalhamentoISSQN.Detalhamento
            {
                Exterior = (await caixa.ShowAsync()).Label == "Exterior"
            });
        }

        async Task<bool> DetalharPIS(int id)
        {
            var caixa = new EscolherTipoPISouCOFINS();
            if (await caixa.ShowAsync() == ContentDialogResult.Primary)
            {
                Escolhidos.Add(id, new DetalhamentoPIS.Detalhamento
                {
                    CST = int.Parse(caixa.CST),
                    TipoCalculo = caixa.TipoCalculo,
                });
                return true;
            }
            return false;
        }

        async Task<bool> DetalharCOFINS(int id)
        {
            var caixa = new EscolherTipoPISouCOFINS();
            if (await caixa.ShowAsync() == ContentDialogResult.Primary)
            {
                Escolhidos.Add(id, new DetalhamentoCOFINS.Detalhamento
                {
                    CST = int.Parse(caixa.CST),
                    TipoCalculo = caixa.TipoCalculo,
                });
                return true;
            }
            return false;
        }

        private void Avancar(object sender, RoutedEventArgs e)
        {
            var roteiro = new RoteiroAdicaoImpostos(Escolhidos.Values.ToArray(), ProdutoCompleto);
            MainPage.Current.Navegar<DetalhamentoGeral>(roteiro);
        }

        sealed class ImpostoPadrao : ImpostoArmazenado
        {
            public ImpostoPadrao(PrincipaisImpostos tipo)
            {
                Tipo = tipo;
                NomeTemplate = "Template padrão";
            }
        }
    }

    sealed class ImpostoEscolhivel
    {
        public ImpostoEscolhivel(ImpostoArmazenado template)
        {
            Id = 0;
            Template = template;
        }

        public int Id { get; set; }
        public ImpostoArmazenado Template { get; set; }
    }
}

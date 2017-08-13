﻿using NFeFacil.DANFE;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using NFeFacil.View.PaginasDANFE;
using NFeFacil.ModeloXML;

// O modelo de item de Página em Branco está documentado em https://go.microsoft.com/fwlink/?LinkId=234238

namespace NFeFacil.View
{
    /// <summary>
    /// Uma página vazia que pode ser usada isoladamente ou navegada dentro de um Quadro.
    /// </summary>
    public sealed partial class ViewDANFE : Page
    {
        private GerenciadorImpressao gerenciadorImpressão;

        public double Largura => PartesDANFE.DimensoesPadrao.CentimeterToPixel(21);
        public double Altura => PartesDANFE.DimensoesPadrao.CentimeterToPixel(29.7);

        public ViewDANFE()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            MainPage.Current.SeAtualizar(Symbol.View, "DANFE");

            gerenciadorImpressão = new GerenciadorImpressao();

            var processo = (Processo)e.Parameter;
            var principal = new PaginaPrincipal(processo, stk.Children);
            principal.PaginasCarregadas += Principal_PaginasCarregadas;
            stk.Children.Add(principal);
            btnImprimir.IsEnabled = true;
        }

        private void Principal_PaginasCarregadas(object sender, System.EventArgs e)
        {
            var filhos = stk.Children;
            for (int i = 0; i < filhos.Count; i++)
            {
                ((IPagina)filhos[i]).DefinirPagina(i + 1, filhos.Count);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) => Dispose();
        private async void btnImprimir_Click(object sender, RoutedEventArgs e) => await gerenciadorImpressão.Imprimir(stk.Children[0]);
        public void Dispose() => gerenciadorImpressão.Dispose();
    }
}

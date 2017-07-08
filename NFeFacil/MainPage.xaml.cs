﻿using BibliotecaCentral.ItensBD;
using System;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.System.Profile;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// O modelo de item de Página em Branco está documentado em https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x416

namespace NFeFacil
{
    /// <summary>
    /// Uma página vazia que pode ser usada isoladamente ou navegada dentro de um Quadro.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        internal static MainPage Current { get; private set; }

        public MainPage()
        {
            InitializeComponent();
            Current = this;
            AnalisarBarraTituloAsync();
            btnRetornar.Click += (x, y) => Retornar();
            SystemNavigationManager.GetForCurrentView().BackRequested += (x,e) =>
            {
                e.Handled = true;
                Retornar();
            };
            frmPrincipal.CacheSize = 4;
            AbrirFunçao(typeof(View.Inicio));
            using (var db = new BibliotecaCentral.AplicativoContext())
            {
                BibliotecaCentral.Propriedades.Ativo = db.Emitentes.FirstOrDefault();
            }
        }

        private void AnalisarBarraTituloAsync()
        {
            var familia = AnalyticsInfo.VersionInfo.DeviceFamily;
            if (familia.Contains("Mobile"))
            {
                btnRetornar.Visibility = Visibility.Collapsed;
                var barra = StatusBar.GetForCurrentView();
                var cor = new View.Estilos.Auxiliares.BibliotecaCores().Cor1;
                barra.BackgroundColor = cor;
                barra.BackgroundOpacity = 1;
            }
            else if (familia.Contains("Desktop"))
            {
                CoreApplicationViewTitleBar tb = CoreApplication.GetCurrentView().TitleBar;
                tb.ExtendViewIntoTitleBar = true;
                tb.IsVisibleChanged += (sender, e) => TitleBar.Visibility = sender.IsVisible ? Visibility.Visible : Visibility.Collapsed;
                tb.LayoutMetricsChanged += (sender, e) => TitleBar.Height = sender.Height;

                Window.Current.SetTitleBar(MainTitleBar);
                Window.Current.Activated += (sender, e) => TitleBar.Opacity = e.WindowActivationState != CoreWindowActivationState.Deactivated ? 1 : 0.5;

                var novoTB = ApplicationView.GetForCurrentView().TitleBar;
                var corBackground = new Color { A = 0 };
                novoTB.ButtonBackgroundColor = corBackground;
                novoTB.ButtonInactiveBackgroundColor = corBackground;
                novoTB.ButtonHoverBackgroundColor = new Color { A = 50 };
                novoTB.ButtonPressedBackgroundColor = new Color { A = 100 };
            }
        }

        public void AbrirFunçao(Type tela, object parametro = null)
        {
            frmPrincipal.Navigate(tela, parametro);
        }

        public void SeAtualizar(Symbol símbolo, string texto)
        {
            txtTitulo.Text = texto;
            symTitulo.Content = new SymbolIcon(símbolo);
            UIElement conteudo = null;
            if (frmPrincipal.Content is IHambuguer hambuguer)
            {
                menuPermanente.Visibility = btnHambuguer.Visibility = Visibility.Visible;
                conteudo = hambuguer.ConteudoMenu;

                AtualizarPosicaoMenu(Window.Current.Bounds.Width >= 720);

                grupoTamanhoTela.CurrentStateChanged += TamanhoTelaMudou;
            }
            else
            {
                splitView.CompactPaneLength = 0;
                menuPermanente.Visibility = btnHambuguer.Visibility = Visibility.Collapsed;
                menuPermanente.Content = splitView.Pane = null;
                grupoTamanhoTela.CurrentStateChanging -= TamanhoTelaMudou;
            }

            void TamanhoTelaMudou(object sender, VisualStateChangedEventArgs e)
            {
                AtualizarPosicaoMenu(e.NewState.Name == "TelaGrande");
            }

            void AtualizarPosicaoMenu(bool telaGrande)
            {
                if (telaGrande)
                {
                    splitView.Pane = null;
                    splitView.CompactPaneLength = 0;
                    menuPermanente.Content = conteudo;
                }
                else
                {
                    menuPermanente.Content = null;
                    splitView.CompactPaneLength = 48;
                    splitView.Pane = conteudo;
                }
            }

            AtualizarExibicaoExtra(frmPrincipal.Content is View.Inicio ? ExibicaoExtra.EscolherEmitente : ExibicaoExtra.ExibirEmitente);
        }

        public void SeAtualizar(string glyph, string texto)
        {
            txtTitulo.Text = texto;
            symTitulo.Content = new FontIcon
            {
                Glyph = glyph,
            };
            AtualizarExibicaoExtra(ExibicaoExtra.ExibirEmitente);
        }

        public async void Retornar()
        {
            if (frmPrincipal.Content is IValida retorna)
            {
                if (!await retorna.Verificar())
                {
                    return;
                }
            }
            else if ((frmPrincipal.Content as FrameworkElement).DataContext is IValida retornaDC)
            {
                if (!await retornaDC.Verificar())
                {
                    return;
                }
            }

            if (frmPrincipal.CanGoBack)
            {
                frmPrincipal.GoBack();
            }
            else
            {
                Application.Current.Exit();
            }
        }

        private void AbrirHamburguer(object sender, RoutedEventArgs e)
        {
            splitView.IsPaneOpen = !splitView.IsPaneOpen;
        }

        void AtualizarExibicaoExtra(ExibicaoExtra ativa)
        {
            switch (ativa)
            {
                case ExibicaoExtra.ExibirEmitente:
                    var emit = BibliotecaCentral.Propriedades.Ativo;
                    if (emit != null)
                    {
                        txtEmitente.Text = emit.Nome;
                    }
                    else
                    {
                        txtEmitente.Text = null;
                    }
                    txtEmitente.Visibility = Visibility.Visible;
                    cmbEmitente.Visibility = Visibility.Collapsed;
                    cmbEmitente.SelectionChanged -= SelecaoMudou;
                    cmbEmitente.ItemsSource = null;
                    break;
                case ExibicaoExtra.EscolherEmitente:
                    using (var db = new BibliotecaCentral.AplicativoContext())
                    {
                        var emits = db.Emitentes;
                        cmbEmitente.ItemsSource = emits;
                        cmbEmitente.SelectionChanged += SelecaoMudou;
                        if (cmbEmitente.SelectedIndex == -1) cmbEmitente.SelectedIndex = 0;
                        txtEmitente.Text = string.Empty;
                        txtEmitente.Visibility = Visibility.Collapsed;
                        cmbEmitente.Visibility = Visibility.Visible;
                    }
                    break;
                default:
                    txtEmitente.Visibility = Visibility.Collapsed;
                    cmbEmitente.Visibility = Visibility.Collapsed;
                    break;
            }

            void SelecaoMudou(object sender, SelectionChangedEventArgs e)
            {
                var novoEmit = (EmitenteDI)e.AddedItems[0];
                BibliotecaCentral.Propriedades.Ativo = novoEmit;
            }
        }

        enum ExibicaoExtra
        {
            ExibirEmitente,
            EscolherEmitente
        }
    }
}

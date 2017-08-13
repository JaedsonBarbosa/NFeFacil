﻿using NFeFacil.Log;
using NFeFacil.Validacao;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using NFeFacil.ViewModel;
using System;
using NFeFacil.ItensBD;

// O modelo de item de Página em Branco está documentado em https://go.microsoft.com/fwlink/?LinkId=234238

namespace NFeFacil.View
{
    /// <summary>
    /// Uma página vazia que pode ser usada isoladamente ou navegada dentro de um Quadro.
    /// </summary>
    public sealed partial class AdicionarDestinatario : Page
    {
        private ClienteDI cliente;
        private TipoOperacao tipoRequisitado;
        private ILog Log = Popup.Current;

        public AdicionarDestinatario()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            GrupoViewBanco<ClienteDI> parametro;
            if (e.Parameter == null)
            {
                parametro = new GrupoViewBanco<ClienteDI>
                {
                    ItemBanco = new ClienteDI(),
                    OperacaoRequirida = TipoOperacao.Adicao
                };
            }
            else
            {
                parametro = (GrupoViewBanco<ClienteDI>)e.Parameter;
            }
            cliente = parametro.ItemBanco;
            tipoRequisitado = parametro.OperacaoRequirida;
            switch (tipoRequisitado)
            {
                case TipoOperacao.Adicao:
                    MainPage.Current.SeAtualizar(Symbol.Add, "Cliente");
                    break;
                case TipoOperacao.Edicao:
                    MainPage.Current.SeAtualizar(Symbol.Edit, "Cliente");
                    break;
            }
            DataContext = new ClienteDataContext(ref cliente);
        }

        private void Confirmar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (new ValidadorDestinatario(cliente).Validar(Log))
                {
                    using (var db = new AplicativoContext())
                    {
                        cliente.UltimaData = DateTime.Now;
                        if (tipoRequisitado == TipoOperacao.Adicao)
                        {
                            db.Add(cliente);
                            Log.Escrever(TitulosComuns.Sucesso, "Cliente salvo com sucesso.");
                        }
                        else
                        {
                            db.Update(cliente);
                            Log.Escrever(TitulosComuns.Sucesso, "Cliente alterado com sucesso.");
                        }
                        db.SaveChanges();
                    }
                    MainPage.Current.Retornar();
                }
            }
            catch (System.Exception erro)
            {
                erro.ManipularErro();
            }
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            MainPage.Current.Retornar();
        }
    }
}

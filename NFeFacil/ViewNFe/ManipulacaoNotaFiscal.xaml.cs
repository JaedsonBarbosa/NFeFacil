﻿using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using System.Collections;
using NFeFacil.ModeloXML.PartesProcesso;
using NFeFacil.View.Controles;
using NFeFacil.ItensBD;
using System.Collections.Generic;
using System.Linq;
using NFeFacil.ModeloXML.PartesProcesso.PartesNFe.PartesDetalhes.PartesTransporte;
using NFeFacil.ModeloXML.PartesProcesso.PartesNFe.PartesDetalhes.PartesIdentificacao;
using NFeFacil.IBGE;
using NFeFacil.ModeloXML;
using NFeFacil.Log;
using NFeFacil.Validacao;
using NFeFacil.ModeloXML.PartesProcesso.PartesNFe.PartesDetalhes;
using NFeFacil.ModeloXML.PartesProcesso.PartesNFe.PartesDetalhes.PartesProduto;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using NFeFacil.ViewNFe.CaixasDialogoNFe;

// O modelo de item de Página em Branco está documentado em https://go.microsoft.com/fwlink/?LinkId=234238

namespace NFeFacil.ViewNFe
{
    /// <summary>
    /// Uma página vazia que pode ser usada isoladamente ou navegada dentro de um Quadro.
    /// </summary>
    public sealed partial class ManipulacaoNotaFiscal : Page, IHambuguer, IValida
    {
        public ManipulacaoNotaFiscal()
        {
            InitializeComponent();
        }

        public IEnumerable ConteudoMenu
        {
            get
            {
                var retorno = new ObservableCollection<ItemHambuguer>
                {
                    new ItemHambuguer(Symbol.Tag, "Identificação"),
                    new ItemHambuguer(Symbol.People, "Cliente"),
                    new ItemHambuguer(Symbol.Street, "Retirada/Entrega"),
                    new ItemHambuguer(Symbol.Shop, "Produtos"),
                    new ItemHambuguer(Symbol.Calculator, "Totais"),
                    new ItemHambuguer("\uE806", "Transporte"),
                    new ItemHambuguer("\uE825", "Cobrança"),
                    new ItemHambuguer(Symbol.Comment, "Informações adicionais"),
                    new ItemHambuguer(Symbol.World, "Exportação e compras"),
                    new ItemHambuguer(new Uri("ms-appx:///Assets/CanaAcucar.png"), "Cana-de-açúcar")
                };
                pvtPrincipal.SelectionChanged += (sender, e) => MainMudou?.Invoke(this, new NewIndexEventArgs { NewIndex = pvtPrincipal.SelectedIndex });
                return retorno;
            }
        }

        public event EventHandler MainMudou;
        public void AtualizarMain(int index) => pvtPrincipal.SelectedIndex = index;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var Dados = (NFe)e.Parameter;
            if (string.IsNullOrEmpty(Dados.Informacoes.Id))
            {
                MainPage.Current.SeAtualizar(Symbol.Add, "Nota fiscal");
            }
            else
            {
                MainPage.Current.SeAtualizar(Symbol.Edit, "Nota fiscal");
            }

            using (var db = new AplicativoContext())
            {
                ClientesDisponiveis = db.Clientes.ToList();
                EmitentesDisponiveis = db.Emitentes.ToList();
                MotoristasDisponiveis = db.Motoristas.ToList();
                ProdutosDisponiveis = db.Produtos.ToList();
            }

            if (Dados.Informacoes.total == null)
                Dados.Informacoes.total = new Total(Dados.Informacoes.produtos);
            NotaSalva = Dados;

            NFesReferenciadas = new ObservableCollection<DocumentoFiscalReferenciado>(NotaSalva.Informacoes.identificacao.DocumentosReferenciados.Where(x => !string.IsNullOrEmpty(x.RefNFe)));
            NFsReferenciadas = new ObservableCollection<DocumentoFiscalReferenciado>(NotaSalva.Informacoes.identificacao.DocumentosReferenciados.Where(x => x.RefNF != null));
            Produtos = new ObservableCollection<DetalhesProdutos>(NotaSalva.Informacoes.produtos);
            Reboques = new ObservableCollection<Reboque>(NotaSalva.Informacoes.transp.Reboque);
            Volumes = new ObservableCollection<Volume>(NotaSalva.Informacoes.transp.Vol);
            Duplicatas = new ObservableCollection<Duplicata>(NotaSalva.Informacoes.cobr.Dup);
            FornecimentosDiarios = new ObservableCollection<FornecimentoDiario>(NotaSalva.Informacoes.cana.ForDia);
            Deducoes = new ObservableCollection<Deducoes>(NotaSalva.Informacoes.cana.Deduc);
            Observacoes = new ObservableCollection<Observacao>(NotaSalva.Informacoes.infAdic.ObsCont);
            ProcessosReferenciados = new ObservableCollection<ProcessoReferenciado>(NotaSalva.Informacoes.infAdic.ProcRef);
            Modalidades = ExtensoesPrincipal.ObterItens<ModalidadesTransporte>();

            AtualizarTotais();
        }

        const string NomeClienteHomologacao = "NF-E EMITIDA EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL";

        public NFe NotaSalva { get; private set; }

        #region Dados base

        public List<ClienteDI> ClientesDisponiveis { get; set; }
        public List<EmitenteDI> EmitentesDisponiveis { get; set; }
        public List<ProdutoDI> ProdutosDisponiveis { get; set; }
        public List<MotoristaDI> MotoristasDisponiveis { get; set; }

        private ClienteDI clienteSelecionado;
        public ClienteDI ClienteSelecionado
        {
            get
            {
                var dest = NotaSalva.Informacoes.destinatário;
                if (clienteSelecionado == null && dest != null)
                {
                    clienteSelecionado = ClientesDisponiveis.FirstOrDefault(x => x.Documento == dest.Documento);
                }
                return clienteSelecionado;
            }
            set
            {
                clienteSelecionado = value;
                NotaSalva.Informacoes.destinatário = value.ToDestinatario();
                if (NotaSalva.AmbienteTestes)
                {
                    NotaSalva.Informacoes.destinatário.Nome = NomeClienteHomologacao;
                }
            }
        }

        private EmitenteDI emitenteSelecionado;
        public EmitenteDI EmitenteSelecionado
        {
            get
            {
                var emit = NotaSalva.Informacoes.emitente;
                if (emitenteSelecionado == null && emit != null)
                {
                    emitenteSelecionado = EmitentesDisponiveis.FirstOrDefault(x => long.Parse(x.CNPJ) == emit.CNPJ);
                }
                return emitenteSelecionado;
            }
            set
            {
                emitenteSelecionado = value;
                NotaSalva.Informacoes.emitente = value.ToEmitente();
            }
        }

        public ProdutoDI ProdutoSelecionado { get; set; }

        private MotoristaDI motoristaSelecionado;
        public MotoristaDI MotoristaSelecionado
        {
            get
            {
                var mot = NotaSalva.Informacoes.transp?.Transporta;
                if (motoristaSelecionado == null && mot != null && mot.Documento != null)
                {
                    motoristaSelecionado = MotoristasDisponiveis.FirstOrDefault(x => x.Documento == mot.Documento);
                }
                return motoristaSelecionado;
            }
            set
            {
                motoristaSelecionado = value;
                var transporte = NotaSalva.Informacoes.transp;
                transporte.Transporta = value.ToMotorista();
                if (value.Veiculo != default(Guid))
                {
                    using (var db = new AplicativoContext())
                        transporte.VeicTransp = db.Veiculos.Find(value.Veiculo).ToVeiculo();
                }
                else
                {
                    transporte.VeicTransp = new Veiculo();
                }
                AtualizarVeiculo();
            }
        }

        void AtualizarVeiculo()
        {
            ctrPaiVeiculo.DataContext = NotaSalva.Informacoes.transp.VeicTransp;
        }

        #endregion

        #region Identificação

        public DateTimeOffset DataEmissao
        {
            get
            {
                if (string.IsNullOrEmpty(NotaSalva.Informacoes.identificacao.DataHoraEmissão))
                {
                    var agora = DateTime.Now;
                    NotaSalva.Informacoes.identificacao.DataHoraEmissão = agora.ToStringPersonalizado();
                    return agora;
                }
                return DateTimeOffset.Parse(NotaSalva.Informacoes.identificacao.DataHoraEmissão);
            }
            set
            {
                var anterior = DateTimeOffset.Parse(NotaSalva.Informacoes.identificacao.DataHoraEmissão);
                var novo = new DateTime(value.Year, value.Month, value.Day, anterior.Hour, anterior.Minute, anterior.Second);
                NotaSalva.Informacoes.identificacao.DataHoraEmissão = novo.ToStringPersonalizado();
            }
        }

        public TimeSpan HoraEmissao
        {
            get => DataEmissao.TimeOfDay;
            set
            {
                var anterior = DateTimeOffset.Parse(NotaSalva.Informacoes.identificacao.DataHoraEmissão);
                var novo = new DateTime(anterior.Year, anterior.Month, anterior.Day, value.Hours, value.Minutes, value.Seconds);
                NotaSalva.Informacoes.identificacao.DataHoraEmissão = novo.ToStringPersonalizado();
            }
        }

        public DateTimeOffset DataSaidaEntrada
        {
            get
            {
                if (string.IsNullOrEmpty(NotaSalva.Informacoes.identificacao.DataHoraSaídaEntrada))
                {
                    return DataEmissao;
                }
                return DateTimeOffset.Parse(NotaSalva.Informacoes.identificacao.DataHoraSaídaEntrada);
            }
            set
            {
                var anterior = DateTimeOffset.Parse(NotaSalva.Informacoes.identificacao.DataHoraSaídaEntrada);
                var novo = new DateTime(value.Year, value.Month, value.Day, anterior.Hour, anterior.Minute, anterior.Second);
                NotaSalva.Informacoes.identificacao.DataHoraSaídaEntrada = novo.ToStringPersonalizado();
            }
        }

        public TimeSpan HoraSaidaEntrada
        {
            get => DataSaidaEntrada.TimeOfDay;
            set
            {
                var anterior = DateTimeOffset.Parse(NotaSalva.Informacoes.identificacao.DataHoraSaídaEntrada);
                var novo = new DateTime(anterior.Year, anterior.Month, anterior.Day, value.Hours, value.Minutes, value.Seconds);
                NotaSalva.Informacoes.identificacao.DataHoraSaídaEntrada = novo.ToStringPersonalizado();
            }
        }

        ObservableCollection<Municipio> MunicipiosIdentificacao { get; } = new ObservableCollection<Municipio>();

        public ushort EstadoIdentificacao
        {
            get => NotaSalva.Informacoes.identificacao.CódigoUF;
            set
            {
                NotaSalva.Informacoes.identificacao.CódigoUF = value;
                MunicipiosIdentificacao.Clear();
                foreach (var item in Municipios.Get(value))
                {
                    MunicipiosIdentificacao.Add(item);
                }
            }
        }

        #endregion

        #region Transporte

        public ObservableCollection<Municipio> MunicipiosTransporte { get; } = new ObservableCollection<Municipio>();

        private Estado ufEscolhida;
        public Estado UFEscolhida
        {
            get
            {
                if (NotaSalva.Informacoes.transp.RetTransp.CMunFG != 0 && ufEscolhida == null)
                {
                    foreach (var item in Estados.EstadosCache)
                    {
                        var lista = Municipios.Get(item);
                        if (lista.Count(x => x.Codigo == NotaSalva.Informacoes.transp.RetTransp.CMunFG) > 0)
                        {
                            ufEscolhida = item;
                            break;
                        }
                    }
                }
                return ufEscolhida;
            }
            set
            {
                ufEscolhida = value;
                MunicipiosTransporte.Clear();
                foreach (var item in Municipios.Get(value))
                {
                    MunicipiosTransporte.Add(item);
                }
            }
        }

        public ModalidadesTransporte ModFrete
        {
            get => (ModalidadesTransporte)NotaSalva.Informacoes.transp.ModFrete;
            set => NotaSalva.Informacoes.transp.ModFrete = (ushort)value;
        }

        public double CFOP
        {
            get => NotaSalva.Informacoes.transp.RetTransp.CFOP;
            set => NotaSalva.Informacoes.transp.RetTransp.CFOP = (long)value;
        }

        #endregion

        Popup Log = Popup.Current;

        void Confirmar()
        {
            try
            {
                if (new ValidarDados(new ValidadorEmitente(NotaSalva.Informacoes.emitente),
                    new ValidadorDestinatario(NotaSalva.Informacoes.destinatário)).ValidarTudo(Log))
                {
                    NotaSalva.Informacoes.AtualizarChave();
                    Log.Escrever(TitulosComuns.ValidaçãoConcluída, "A nota fiscal foi validada. Aparentemente, não há irregularidades");
                }
            }
            catch (Exception e)
            {
                e.ManipularErro();
            }
        }

        #region ColecoesExibicaoView

        ObservableCollection<DocumentoFiscalReferenciado> NFesReferenciadas { get; set; }
        ObservableCollection<DocumentoFiscalReferenciado> NFsReferenciadas { get; set; }
        ObservableCollection<DetalhesProdutos> Produtos { get; set; }
        ObservableCollection<Reboque> Reboques { get; set; }
        ObservableCollection<Volume> Volumes { get; set; }
        ObservableCollection<Duplicata> Duplicatas { get; set; }
        ObservableCollection<FornecimentoDiario> FornecimentosDiarios { get; set; }
        ObservableCollection<Deducoes> Deducoes { get; set; }
        ObservableCollection<Observacao> Observacoes { get; set; }
        ObservableCollection<ProcessoReferenciado> ProcessosReferenciados { get; set; }
        ObservableCollection<ModalidadesTransporte> Modalidades { get; set; }

        #endregion

        #region Adição e remoção básica

        void AdicionarProduto()
        {
            var detCompleto = new DetalhesProdutos
            {
                Produto = ProdutoSelecionado != null ? ProdutoSelecionado.ToProdutoOuServico() : new ProdutoOuServico()
            };
            MainPage.Current.Navegar<ManipulacaoProdutoCompleto>(detCompleto);
        }

        void EditarProduto(DetalhesProdutos produto)
        {
            MainPage.Current.Navegar<ManipulacaoProdutoCompleto>(produto);
        }

        void RemoverProduto(DetalhesProdutos produto)
        {
            NotaSalva.Informacoes.produtos.Remove(produto);
            Produtos.Remove(produto);
            AtualizarTotais();
        }

        void AtualizarTotais()
        {
            pvtTotais.DataContext = NotaSalva.Informacoes.total;
        }

        async void AdicionarNFeReferenciada()
        {
            var caixa = new CaixasDialogoNFe.AdicionarReferenciaEletronica();
            if (await caixa.ShowAsync() == ContentDialogResult.Primary)
            {
                var novo = new DocumentoFiscalReferenciado
                {
                    RefNFe = caixa.Chave
                };
                NotaSalva.Informacoes.identificacao.DocumentosReferenciados.Add(novo);
                NFesReferenciadas.Add(novo);
            }
        }

        async void AdicionarNFReferenciada()
        {
            var caixa = new CaixasDialogoNFe.AdicionarNF1AReferenciada();
            if (await caixa.ShowAsync() == ContentDialogResult.Primary)
            {
                var contexto = (NF1AReferenciada)caixa.DataContext;
                var novo = new DocumentoFiscalReferenciado
                {
                    RefNF = contexto
                };
                NotaSalva.Informacoes.identificacao.DocumentosReferenciados.Add(novo);
                NFsReferenciadas.Add(novo);
            }
        }

        void RemoverDocReferenciado(DocumentoFiscalReferenciado doc)
        {
            NotaSalva.Informacoes.identificacao.DocumentosReferenciados.Remove(doc);
            if (string.IsNullOrEmpty(doc.RefNFe))
            {
                NFsReferenciadas.Remove(doc);
            }
            else
            {
                NFesReferenciadas.Remove(doc);
            }
        }

        async void AdicionarReboque()
        {
            var add = new CaixasDialogoNFe.AdicionarReboque();
            if (await add.ShowAsync() == ContentDialogResult.Primary)
            {
                var novo = add.DataContext as Reboque;
                NotaSalva.Informacoes.transp.Reboque.Add(novo);
                Reboques.Add(novo);
            }
        }

        void RemoverReboque(Reboque reboque)
        {
            NotaSalva.Informacoes.transp.Reboque.Remove(reboque);
            Reboques.Remove(reboque);
        }

        async void AdicionarVolume()
        {
            var add = new CaixasDialogoNFe.AdicionarVolume();
            if (await add.ShowAsync() == ContentDialogResult.Primary)
            {
                var novo = add.DataContext as Volume;
                NotaSalva.Informacoes.transp.Vol.Add(novo);
                Volumes.Add(novo);
            }
        }

        void RemoverVolume(Volume volume)
        {
            NotaSalva.Informacoes.transp.Vol.Remove(volume);
            Volumes.Remove(volume);
        }

        async void AdicionarDuplicata()
        {
            var caixa = new CaixasDialogoNFe.AdicionarDuplicata();
            if (await caixa.ShowAsync() == ContentDialogResult.Primary)
            {
                var novo = caixa.DataContext as Duplicata;
                NotaSalva.Informacoes.cobr.Dup.Add(novo);
                Duplicatas.Add(novo);
            }
        }

        void RemoverDuplicata(Duplicata duplicata)
        {
            NotaSalva.Informacoes.cobr.Dup.Remove(duplicata);
            Duplicatas.Remove(duplicata);
        }

        async void AdicionarFornecimento()
        {
            var caixa = new CaixasDialogoNFe.AdicionarFornecimentoDiario();
            if (await caixa.ShowAsync() == ContentDialogResult.Primary)
            {
                var novo = caixa.DataContext as FornecimentoDiario;
                NotaSalva.Informacoes.cana.ForDia.Add(novo);
                FornecimentosDiarios.Add(novo);
            }
        }

        void RemoverFornecimento(FornecimentoDiario fornecimento)
        {
            NotaSalva.Informacoes.cana.ForDia.Remove(fornecimento);
            FornecimentosDiarios.Remove(fornecimento);
        }

        async void AdicionarDeducao()
        {
            var caixa = new CaixasDialogoNFe.AdicionarDeducao();
            if (await caixa.ShowAsync() == ContentDialogResult.Primary)
            {
                var novo = caixa.DataContext as Deducoes;
                NotaSalva.Informacoes.cana.Deduc.Add(novo);
                Deducoes.Add(novo);
            }
        }

        void RemoverDeducao(Deducoes deducao)
        {
            NotaSalva.Informacoes.cana.Deduc.Remove(deducao);
            Deducoes.Remove(deducao);
        }

        async void AdicionarObsContribuinte()
        {
            var caixa = new CaixasDialogoNFe.AdicionarObservacaoContribuinte();
            if (await caixa.ShowAsync() == ContentDialogResult.Primary)
            {
                var novo = (Observacao)caixa.DataContext;
                NotaSalva.Informacoes.infAdic.ObsCont.Add(novo);
                Observacoes.Add(novo);
            }
        }

        void RemoverObsContribuinte(Observacao obs)
        {
            NotaSalva.Informacoes.infAdic.ObsCont.Remove(obs);
            Observacoes.Remove(obs);
        }

        async void AdicionarProcReferenciado()
        {
            var caixa = new CaixasDialogoNFe.AdicionarProcessoReferenciado();
            if (await caixa.ShowAsync() == ContentDialogResult.Primary)
            {
                var novo = caixa.Item;
                NotaSalva.Informacoes.infAdic.ProcRef.Add(novo);
                ProcessosReferenciados.Add(novo);
            }
        }

        void RemoverProcReferenciado(ProcessoReferenciado proc)
        {
            NotaSalva.Informacoes.infAdic.ProcRef.Remove(proc);
            ProcessosReferenciados.Remove(proc);
        }

        #endregion

        async Task<bool> IValida.Verificar()
        {
            var retorno = true;
            var mensagem = new MessageDialog("Se você sair agora, os dados serão perdidos, se tiver certeza, escolha Sair, caso contrário, Cancelar.", "Atenção");
            mensagem.Commands.Add(new UICommand("Sair"));
            mensagem.Commands.Add(new UICommand("Cancelar", x => retorno = false));
            await mensagem.ShowAsync();
            return retorno;
        }

        #region Métodos View - Adição e remoção básica

        private void AdicionarNFeReferenciada(object sender, RoutedEventArgs e)
        {
            AdicionarNFeReferenciada();
        }

        private void RemoverDocReferenciado(object sender, RoutedEventArgs e)
        {
            var contexto = ((FrameworkElement)sender).DataContext;
            RemoverDocReferenciado((DocumentoFiscalReferenciado)contexto);
        }

        private void AdicionarNFReferenciada(object sender, RoutedEventArgs e)
        {
            AdicionarNFReferenciada();
        }

        private void AdicionarProduto(object sender, RoutedEventArgs e)
        {
            AdicionarProduto();
        }

        private void EditarProduto(object sender, RoutedEventArgs e)
        {
            var contexto = ((FrameworkElement)sender).DataContext;
            EditarProduto((DetalhesProdutos)contexto);
        }

        private void RemoverProduto(object sender, RoutedEventArgs e)
        {
            var contexto = ((FrameworkElement)sender).DataContext;
            RemoverProduto((DetalhesProdutos)contexto);
        }

        private void AdicionarReboque(object sender, RoutedEventArgs e)
        {
            AdicionarReboque();
        }

        private void RemoverReboque(object sender, RoutedEventArgs e)
        {
            var contexto = ((FrameworkElement)sender).DataContext;
            RemoverReboque((Reboque)contexto);
        }

        private void AdicionarVolume(object sender, RoutedEventArgs e)
        {
            AdicionarVolume();
        }

        private void RemoverVolume(object sender, RoutedEventArgs e)
        {
            var contexto = ((FrameworkElement)sender).DataContext;
            RemoverVolume((Volume)contexto);
        }

        private void AdicionarDuplicata(object sender, RoutedEventArgs e)
        {
            AdicionarDuplicata();
        }

        private void RemoverDuplicata(object sender, RoutedEventArgs e)
        {
            var contexto = ((FrameworkElement)sender).DataContext;
            RemoverDuplicata((Duplicata)contexto);
        }

        private void AdicionarObsContribuinte(object sender, RoutedEventArgs e)
        {
            AdicionarObsContribuinte();
        }

        private void RemoverObsContribuinte(object sender, RoutedEventArgs e)
        {
            var contexto = ((FrameworkElement)sender).DataContext;
            RemoverObsContribuinte((Observacao)contexto);
        }

        private void AdicionarProcReferenciado(object sender, RoutedEventArgs e)
        {
            AdicionarProcReferenciado();
        }

        private void RemoverProcReferenciado(object sender, RoutedEventArgs e)
        {
            var contexto = ((FrameworkElement)sender).DataContext;
            RemoverProcReferenciado((ProcessoReferenciado)contexto);
        }

        private void AdicionarFornecimento(object sender, RoutedEventArgs e)
        {
            AdicionarFornecimento();
        }

        private void RemoverFornecimento(object sender, RoutedEventArgs e)
        {
            var contexto = ((FrameworkElement)sender).DataContext;
            RemoverFornecimento((FornecimentoDiario)contexto);
        }

        private void AdicionarDeducao(object sender, RoutedEventArgs e)
        {
            AdicionarDeducao();
        }

        private void RemoverDeducao(object sender, RoutedEventArgs e)
        {
            var contexto = ((FrameworkElement)sender).DataContext;
            RemoverDeducao((Deducoes)contexto);
        }

        #endregion

        private async void AlterarEnderecoRetirada(object sender, RoutedEventArgs e)
        {
            var controle = (ToggleSwitch)sender;
            if (controle.IsOn)
            {
                var caixa = new EscolherTipoEndereco();
                if (await caixa.ShowAsync() == ContentDialogResult.Primary)
                {
                    var tipo = caixa.TipoEscolhido;
                    if (tipo == TipoEndereco.Exterior)
                    {
                        var caixa2 = new EnderecoDiferenteExterior();
                        if (await caixa2.ShowAsync() == ContentDialogResult.Primary)
                        {
                            NotaSalva.Informacoes.Retirada = caixa2.Endereco;
                        }
                    }
                    else
                    {
                        var caixa2 = new EnderecoDiferenteNacional();
                        if (await caixa2.ShowAsync() == ContentDialogResult.Primary)
                        {
                            NotaSalva.Informacoes.Retirada = caixa2.Endereco;
                        }
                    }
                }
            }
            else
            {
                NotaSalva.Informacoes.Retirada = null;
            }

            if (controle.IsOn && NotaSalva.Informacoes.Retirada == null)
            {
                controle.IsOn = false;
            }
        }

        private async void AlterarEnderecoEntrega(object sender, RoutedEventArgs e)
        {
            var controle = (ToggleSwitch)sender;
            if (controle.IsOn)
            {
                var caixa = new EscolherTipoEndereco();
                if (await caixa.ShowAsync() == ContentDialogResult.Primary)
                {
                    var tipo = caixa.TipoEscolhido;
                    if (tipo == TipoEndereco.Exterior)
                    {
                        var caixa2 = new EnderecoDiferenteExterior();
                        if (await caixa2.ShowAsync() == ContentDialogResult.Primary)
                        {
                            NotaSalva.Informacoes.Entrega = caixa2.Endereco;
                        }
                    }
                    else
                    {
                        var caixa2 = new EnderecoDiferenteNacional();
                        if (await caixa2.ShowAsync() == ContentDialogResult.Primary)
                        {
                            NotaSalva.Informacoes.Entrega = caixa2.Endereco;
                        }
                    }
                }
            }
            else
            {
                NotaSalva.Informacoes.Entrega = null;
            }

            if (controle.IsOn && NotaSalva.Informacoes.Entrega == null)
            {
                controle.IsOn = false;
            }
        }
    }
}

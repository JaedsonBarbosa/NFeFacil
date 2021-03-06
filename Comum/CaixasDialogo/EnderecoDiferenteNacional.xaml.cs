﻿using BaseGeral.IBGE;
using BaseGeral.ModeloXML;
using BaseGeral.ModeloXML.PartesDetalhes;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml.Controls;

// O modelo de item de Caixa de Diálogo de Conteúdo está documentado em https://go.microsoft.com/fwlink/?LinkId=234238

namespace Comum.CaixasDialogo
{
    public sealed partial class EnderecoDiferenteNacional : ContentDialog
    {
        public EnderecoDiferenteNacional()
        {
            InitializeComponent();
        }

        public RetiradaOuEntrega Endereco { get; } = new RetiradaOuEntrega();
        ObservableCollection<Municipio> MunicipiosDoEstado { get; } = new ObservableCollection<Municipio>();

        TiposDocumento tipoDocumento;
        public int TipoDocumento
        {
            get => (int)(tipoDocumento = string.IsNullOrEmpty(Endereco.CNPJ) ? TiposDocumento.CPF : TiposDocumento.CNPJ);
            set => tipoDocumento = (TiposDocumento)value;
        }

        public string Documento
        {
            get
            {
                return tipoDocumento == TiposDocumento.CNPJ ? Endereco.CNPJ : Endereco.CPF;
            }
            set
            {
                if (tipoDocumento == TiposDocumento.CPF)
                {
                    Endereco.CNPJ = null;
                    Endereco.CPF = value;
                }
                else
                {
                    Endereco.CPF = null;
                    Endereco.CNPJ = value;
                }
            }
        }

        public string UFEscolhida
        {
            get => Endereco.SiglaUF;
            set
            {
                Endereco.SiglaUF = value;
                MunicipiosDoEstado.Clear();
                foreach (var item in Municipios.Get(value))
                {
                    MunicipiosDoEstado.Add(item);
                }
            }
        }

        public Municipio ConjuntoMunicipio
        {
            get => MunicipiosDoEstado.FirstOrDefault(x => x.Codigo == Endereco.CodigoMunicipio);
            set
            {
                if (value != null)
                {
                    Endereco.CodigoMunicipio = value.Codigo;
                    Endereco.NomeMunicipio = value.Nome;
                }
            }
        }
    }
}

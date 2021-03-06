﻿using BaseGeral.View;
using BaseGeral.ModeloXML.PartesDetalhes.PartesIdentificacao;
using System.Collections.Generic;
using System.Xml.Serialization;
using Windows.ApplicationModel;

namespace BaseGeral.ModeloXML.PartesDetalhes
{
    public sealed class Identificacao : IIdentificacao
    {
        public Identificacao() : this(true) { }
        public Identificacao(bool nfe)
        {
            if (nfe && Modelo == default(ushort))
            {
                Modelo = 55;
                DocumentosReferenciados = new List<DocumentoFiscalReferenciado>();
            }
            else
            {
                Modelo = 65;
            }
        }

        [XmlElement(ElementName = "cUF", Order = 0), PropriedadeExtensivel("Estado", MetodosObtencao.Estado)]
        public ushort CódigoUF { get; set; }

        [XmlElement(ElementName = "cNF", Order = 1), DescricaoPropriedade("Chave da NFe")]
        public int ChaveNF { get; set; }

        [XmlElement(ElementName = "natOp", Order = 2), DescricaoPropriedade("Natureza da operação")]
        public string NaturezaDaOperacao { get; set; }

        [XmlElement(ElementName = "mod", Order = 4)]
        public ushort Modelo { get; set; }

        [XmlElement(ElementName = "serie", Order = 5), DescricaoPropriedade("Série")]
        public ushort Serie { get; set; } = 1;

        [XmlElement(ElementName = "nNF", Order = 6), DescricaoPropriedade("Número")]
        public int Numero { get; set; }

        [XmlElement(ElementName = "dhEmi", Order = 7), DescricaoPropriedade("Data e hora de emissão")]
        public string DataHoraEmissão { get; set; }

        [XmlElement(ElementName = "dhSaiEnt", Order = 8), DescricaoPropriedade("Data e hora de saída ou entrada")]
        public string DataHoraSaídaEntrada { get; set; }

        [XmlElement(ElementName = "tpNF", Order = 9), DescricaoPropriedade("Tipo de operação")]
        public ushort TipoOperacao { get; set; } = 1;

        [XmlElement(ElementName = "idDest", Order = 10), DescricaoPropriedade("Identificador de destino")]
        public int IdentificadorDestino { get; set; } = 1;

        [XmlElement(ElementName = "cMunFG", Order = 11), PropriedadeExtensivel("Município", MetodosObtencao.Municipio)]
        public int CodigoMunicipio { get; set; }

        [XmlElement(ElementName = "tpImp", Order = 12), DescricaoPropriedade("Tipo de impressão")]
        public ushort TipoImpressão { get; set; } = 1;

        [XmlElement(ElementName = "tpEmis", Order = 13), DescricaoPropriedade("Tipo de emissão")]
        public ushort TipoEmissão { get; set; } = 1;

        [XmlElement(ElementName = "cDV", Order = 14), DescricaoPropriedade("Dígito verificador")]
        public int DígitoVerificador { get; set; }

        [XmlElement(ElementName = "tpAmb", Order = 15), DescricaoPropriedade("Tipo de ambiente")]
        public ushort TipoAmbiente { get; set; } = 1;

        [XmlElement(ElementName = "finNFe", Order = 16), DescricaoPropriedade("Finalidade de emissão")]
        public int FinalidadeEmissao { get; set; } = 1;

        [XmlElement(ElementName = "indFinal", Order = 17), DescricaoPropriedade("Operação com consumidor final")]
        public ushort OperacaoConsumidorFinal { get; set; } = 1;

        [XmlElement(ElementName = "indPres", Order = 18), DescricaoPropriedade("Indicador de presença")]
        public ushort IndicadorPresenca { get; set; } = 1;

        [XmlElement(ElementName = "procEmi", Order = 19), DescricaoPropriedade("Processo de emissão")]
        public ushort ProcessoEmissão { get; set; } = 0;

        [XmlElement(ElementName = "verProc", Order = 20), DescricaoPropriedade("Versão do aplicativo")]
        public string VersaoAplicativo { get; set; }

        [XmlElement("NFref", Order = 21)]
        public List<DocumentoFiscalReferenciado> DocumentosReferenciados { get; set; }

        public void DefinirVersãoAplicativo()
        {
            var version = Package.Current.Id.Version;
            VersaoAplicativo = $"{version.Major}.{version.Minor}.{version.Build}";
        }
    }
}

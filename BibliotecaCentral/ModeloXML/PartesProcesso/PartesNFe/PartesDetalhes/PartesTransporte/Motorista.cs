﻿using System;
using System.Xml.Serialization;

namespace BibliotecaCentral.ModeloXML.PartesProcesso.PartesNFe.PartesDetalhes.PartesTransporte
{
    public sealed class Motorista
    {
        [XmlIgnore]
        public Guid Id { get; set; }

        public string CPF { get; set; }
        public string CNPJ { get; set; }

        [XmlElement(ElementName = "xNome")]
        public string Nome { get; set; }

        [XmlElement(ElementName = "IE")]
        public string InscricaoEstadual { get; set; }
        /// <summary>
        /// (Opcional)
        /// endereco Completo.
        /// </summary>
        [XmlElement("xEnder")]
        public string XEnder { get; set; }

        /// <summary>
        /// (Opcional)
        /// Nome do município.
        /// </summary>
        [XmlElement("xMun")]
        public string XMun { get; set; }

        /// <summary>
        /// (Opcional)
        /// Sigla da UF.
        /// Informar "EX" para Exterior.
        /// </summary>
        public string UF { get; set; }

        [XmlIgnore]
        public string Documento
        {
            get => CNPJ ?? CPF;
            set
            {
                if (!string.IsNullOrEmpty(CNPJ))
                {
                    CNPJ = value;
                }
                else
                {
                    CPF = value;
                }
            }
        }
        [XmlIgnore]
        public TiposDocumento TipoDocumento => !string.IsNullOrEmpty(CNPJ) ? TiposDocumento.CNPJ : TiposDocumento.CPF;
    }
}

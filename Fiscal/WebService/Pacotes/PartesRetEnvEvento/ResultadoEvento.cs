﻿using BaseGeral.ModeloXML.PartesAssinatura;
using System.Xml.Serialization;

namespace Fiscal.WebService.Pacotes.PartesRetEnvEvento
{
    public struct ResultadoEvento
    {
        [XmlAttribute("versao")]
        public string Versao { get; set; }

        [XmlElement("infEvento")]
        public InformacoesRegistroEvento InfEvento { get; set; }

        [XmlElement("Signature", Namespace = "http://www.w3.org/2000/09/xmldsig#")]
        public Assinatura Signature { get; set; }
    }
}

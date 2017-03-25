﻿using System.Xml.Serialization;

namespace NFeFacil.WebService.AutorizarNota
{
    [XmlRoot("nfeDadosMsg", Namespace = ConjuntoServicos.AutorizarServico)]
    public struct Request
    {
        [XmlElement(Namespace = "http://www.portalfiscal.inf.br/nfe")]
        public CorpoRequest enviNFe { get; set; }
    }
}
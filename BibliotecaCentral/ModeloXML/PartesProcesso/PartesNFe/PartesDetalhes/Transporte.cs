﻿using BibliotecaCentral.ModeloXML.PartesProcesso.PartesNFe.PartesDetalhes.PartesTransporte;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace BibliotecaCentral.ModeloXML.PartesProcesso.PartesNFe.PartesDetalhes
{
    public class Transporte
    {
        [XmlElement(Order = 0)]
        public int modFrete { get; set; }

        [XmlElement(Order = 1)]
        public Motorista transporta { get; set; } = new Motorista();

        [XmlElement(Order = 2)]
        public ICMSTransporte retTransp { get; set; } = new ICMSTransporte();

        [XmlElement(Order = 3)]
        public Veiculo veicTransp { get; set; } = new Veiculo();

        [XmlElement(nameof(reboque), Order = 4)]
        public List<Reboque> reboque { get; set; } = new List<Reboque>();

        [XmlElement(Order = 5)]
        public string vagao { get; set; }

        [XmlElement(Order = 6)]
        public string balsa { get; set; }

        [XmlElement(nameof(vol), Order = 7)]
        public List<Volume> vol { get; set; } = new List<Volume>();
    }
}
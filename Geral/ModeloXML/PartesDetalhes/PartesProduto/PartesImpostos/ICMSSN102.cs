﻿namespace BaseGeral.ModeloXML.PartesDetalhes.PartesProduto.PartesImpostos
{
    /// <summary>
    /// Grupo CRT=1 – Simples Nacional e CSOSN=102, 103, 300 ou 400.
    /// </summary>
    public class ICMSSN102 : ComumICMS, ISimplesNacional
    {
        public ICMSSN102()
        {
        }

        public ICMSSN102(int origem, string csosn) : base(origem, csosn, true) { }
    }
}

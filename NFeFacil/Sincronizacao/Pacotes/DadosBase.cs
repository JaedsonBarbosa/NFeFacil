﻿using NFeFacil.ItensBD;
using System.Collections.Generic;

namespace NFeFacil.Sincronizacao.Pacotes
{
    public sealed class DadosBase : PacoteBase
    {
        public IEnumerable<EmitenteDI> Emitentes { get; set; }
        public IEnumerable<ClienteDI> Clientes { get; set; }
        public IEnumerable<MotoristaDI> Motoristas { get; set; }
        public IEnumerable<ProdutoDI> Produtos { get; set; }
    }
}

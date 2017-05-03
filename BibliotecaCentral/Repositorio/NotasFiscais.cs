﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using BibliotecaCentral.ItensBD;

namespace BibliotecaCentral.Repositorio
{
    public sealed class NotasFiscais : ConexaoBanco
    {
        public IEnumerable<NFeDI> Registro => Contexto.NotasFiscais;

        public async Task<IEnumerable<(NFeDI nota, XElement xml)>> RegistroAsync()
        {
            PastaNotasFiscais pasta = new PastaNotasFiscais();
            var registroXml = await pasta.RegistroCompleto();
            var retorno = new List<(NFeDI nota, XElement xml)>();
            foreach (var item in Registro)
            {
                retorno.Add((item, registroXml.First(x => x.nome == item.Id).xml));
            }
            return retorno;
        }

        public async Task Adicionar(NFeDI nota, XElement xml)
        {
            Contexto.Add(nota);
            PastaNotasFiscais pasta = new PastaNotasFiscais();
            await pasta.AdicionarOuAtualizar(xml, nota.Id);
        }

        public async Task Atualizar(NFeDI nota, XElement xml)
        {
            Contexto.Update(nota);
            PastaNotasFiscais pasta = new PastaNotasFiscais();
            await pasta.AdicionarOuAtualizar(xml, nota.Id);
        }

        public async Task Remover(NFeDI nota)
        {
            Contexto.Remove(nota);
            PastaNotasFiscais pasta = new PastaNotasFiscais();
            await pasta.Remover(nota.Id);
        }

        public long ObterNovoNumero(string cnpjEmitente, ushort serieNota)
        {
            return (from nota in Contexto.NotasFiscais
                    where nota.CNPJEmitente == cnpjEmitente
                    where nota.SerieNota == serieNota
                    select nota.NumeroNota).Max() + 1;
        }
    }
}
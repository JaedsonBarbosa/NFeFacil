﻿using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace BibliotecaCentral.IBGE
{
    public sealed class Municipio
    {
        public ushort CodigoUF { get; set; }
        public string Nome { get; set; }
        public int Codigo { get; set; }

        public Municipio() { }

        public Municipio(XElement xmlMunicípio)
        {
            var elementos = xmlMunicípio.Elements().GetEnumerator();
            elementos.MoveNext();
            CodigoUF = ushort.Parse(elementos.Current.Value, CultureInfo.InvariantCulture);
            elementos.MoveNext();
            Nome = RemoverAcentuacao(elementos.Current.Value);
            elementos.MoveNext();
            Codigo = int.Parse(elementos.Current.Value);
            elementos.Dispose();
        }

        public static bool operator ==(Municipio mun1, Municipio mun2) {
            return Equals(mun1, mun2);
        }

        public static bool operator !=(Municipio mun1, Municipio mun2)
        {
            return !Equals(mun1, mun2);
        }

        public override bool Equals(object obj)
        {
            if (obj is Municipio mun)
            {
                return GetHashCode() == mun.GetHashCode();
            }
            return false;
        }

        public override int GetHashCode()
        {
            return CodigoUF * Codigo * Nome.Length;
        }

        private static string RemoverAcentuacao(string text)
        {
            return new string(text
                .Normalize(NormalizationForm.FormD)
                .Where(x => char.IsLetter(x) || x == ' ')
                .ToArray());
        }
    }
}
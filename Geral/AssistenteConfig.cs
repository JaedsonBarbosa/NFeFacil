﻿using System;
using Windows.Storage;

namespace BaseGeral
{
    static class AssistenteConfig
    {
        static ApplicationDataContainer Pasta = ApplicationData.Current.LocalSettings;

        public static T Get<T>(string nome, T padrao)
        {
            var contem = Pasta.Values.ContainsKey(nome);
            if (contem)
            {
                try
                {
                    var valor = Pasta.Values[nome];
                    if (valor is T convertido)
                    {
                        return convertido;
                    }
                    else
                    {
                        return (T)Convert.ChangeType(valor, typeof(T));
                    }
                }
                catch (Exception)
                {
                    Pasta.Values.Remove(nome);
                }
            }
            return padrao;
        }

        public static void Set(string nome, object valor)
        {
            try
            {
                Pasta.Values[nome] = valor;
            }
            catch (Exception)
            {
                try
                {
                    var contem = Pasta.Values.ContainsKey(nome);
                    if (contem) Pasta.Values.Remove(nome);
                    Pasta.Values[nome] = valor;
                }
                catch (Exception e)
                {
                    e.ManipularErro();
                }
            }
        }
    }
}

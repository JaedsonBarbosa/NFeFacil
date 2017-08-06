﻿namespace BibliotecaCentral.Log
{
    internal static class Titulos
    {
        internal static string ObterString(TitulosComuns título)
        {
            switch (título)
            {
                case TitulosComuns.Atenção:
                    return "Atenção";
                case TitulosComuns.Sucesso:
                    return "Sucesso";
                case TitulosComuns.Erro:
                    return "Erro";
                case TitulosComuns.Iniciando:
                    return "Iniciando";
                case TitulosComuns.Log:
                    return nameof(Log);
                case TitulosComuns.ValidaçãoConcluída:
                    return "Tudo certo";
                default:
                    return null;
            }
        }
    }

    public enum TitulosComuns
    {
        Atenção, Sucesso, Erro, Iniciando, Log, ValidaçãoConcluída
    }
}

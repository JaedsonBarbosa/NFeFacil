﻿using Windows.UI.Xaml.Controls;

// O modelo de item de Caixa de Diálogo de Conteúdo está documentado em https://go.microsoft.com/fwlink/?LinkId=234238

namespace NFeFacil.ViewNFe.CaixasImpostos
{
    public sealed partial class AddPISouCOFINSAliquota : ContentDialog
    {
        public AddPISouCOFINSAliquota()
        {
            this.InitializeComponent();
        }

        public double Aliquota { get; private set; }
    }
}

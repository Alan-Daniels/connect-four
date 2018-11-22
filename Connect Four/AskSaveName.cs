using System;
using System.Windows.Forms;

namespace Connect_Four
{
    public partial class AskSaveName : Form
    {
        public AskSaveName()
        {
            InitializeComponent();
        }

        public string NameText
        {
            get
            {
                return TxtName.Text.Trim();
            }
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (NameText != "")
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}

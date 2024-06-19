namespace Discord.cs
{
    public partial class CodeForm : Form
    {
        public CodeForm()
        {
            InitializeComponent();
        }

        public static void ShowPopup(Form? parent, Action<string> callback)
        {
            try
            {
                var form = new CodeForm();
                form.Show(parent);
                form.Focus();
                form.textBox1.Focus();
                form.button1.Click += (sender, e) =>
                {
                    if (form.textBox1.Text == string.Empty) return;
                    callback(form.textBox1.Text);
                    form.Close();
                    parent?.Focus();
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}

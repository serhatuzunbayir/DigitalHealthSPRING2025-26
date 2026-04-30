using DigitalHealthTrainer.Forms;

namespace DigitalHealthTrainer
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            // TODO: Bağlantı testi sonrası LoginForm'a geri dönülecek şu anlık dümenden bağlantı testi
            Application.Run(new TestConnectionForm());
        }
    }
}

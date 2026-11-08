using System.Windows.Forms;

namespace AccountingSystem.Helpers
{
    public class DoubleBufferedGrid : DataGridView
    {
        public DoubleBufferedGrid()
        {
            DoubleBuffered = true;
        }
    }
}
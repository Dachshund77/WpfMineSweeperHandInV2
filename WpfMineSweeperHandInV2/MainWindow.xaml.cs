using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfMineSweeperHandInV2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MineSweeperGame mineSweeperGame;

        public MainWindow()
        {
            InitializeComponent();
            this.mineSweeperGame = new MineSweeperGame(10, 10, 10);
            this.ccGameArea.Content = mineSweeperGame;
            
        }

        private void BtnResetButton_Click(object sender, RoutedEventArgs e)
        {
            this.mineSweeperGame = new MineSweeperGame(10, 10, 10);
            this.ccGameArea.Content = mineSweeperGame;
        }
    }
}

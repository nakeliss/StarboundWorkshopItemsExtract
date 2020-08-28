using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using MaterialDesignThemes.Wpf;
using Microsoft.Win32;

namespace StarboundWorkshopItemsExtract
{


    public delegate void StatsChange(string s);
    public delegate void StatsInfoChange(string s);
    public delegate void ButtonInspectStats(bool b);

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            btnExtract.IsEnabled = false;

            eSIC += StatsInfoChange;
            eSC += StatsChange;
            eBIS += ButtonInspectStats;
        }

        //游戏ID
        string gameID;
        //源目录
        string fullPath;
        //输出目录
        string outputPath;

        //选择steam库文件夹
        private void btnGetSteamLibraryFolderPath_Click(object sender, RoutedEventArgs e)
        {
            var folderBrowser = new FolderBrowserDialog()
            {
                ShowNewFolderButton = false,
                Description = ">>>选择Steam库文件夹",
                RootFolder = Environment.SpecialFolder.ProgramFiles,
                UseDescriptionForTitle=true,
            };
            folderBrowser.ShowDialog();
            fieldSteamLibraryFolderPath.Text = folderBrowser.SelectedPath;
        }

        //ID输入框只能输入数字
        private void fieldContentID_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9.\\-]+");
            e.Handled = re.IsMatch(e.Text);
        }

        //
        private void fieldSteamLibraryFolderPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            fullPath = fieldSteamLibraryFolderPath.Text+ @"\steamapps\workshop\content\"+gameID;
        }
        //
        private void fieldGameID_TextChanged(object sender, TextChangedEventArgs e)
        {
            gameID = fieldGameID.Text;
        }
        //
        private void fieldGameID_Loaded(object sender, RoutedEventArgs e)
        {
            fieldGameID.Text = "211820";
        }
        //选择输出文件夹
        private void btnGetOutputFolderPath_Click(object sender, RoutedEventArgs e)
        {
            var folderBrowser = new FolderBrowserDialog()
            {
                ShowNewFolderButton = true,
                Description = ">>>选择输出文件夹",
                RootFolder = Environment.SpecialFolder.ProgramFiles,
                UseDescriptionForTitle = true,
            };
            folderBrowser.ShowDialog();
            outputPath = folderBrowser.SelectedPath;
            fieldOutputFolderPath.Text= folderBrowser.SelectedPath;
        }


        public static event StatsChange eSC;
        public static event StatsInfoChange eSIC;
        public static event ButtonInspectStats eBIS;

        void StatsChange(string s)
        {
            laberStats.Dispatcher.Invoke(new Action(() =>
            {
                laberStats.Content = s;
            }));
        }
        void StatsInfoChange(string s)
        {
            laberStatsInfo.Dispatcher.Invoke(new Action(() =>
            {
                laberStatsInfo.Content = s;
            }));
        }
        void ButtonInspectStats(bool b)
        {
            btnInspect.IsEnabled = b;
        }

        //
        private void btnExtract_Click(object sender, RoutedEventArgs e)
        {
            btnInspect.IsEnabled = false;
            btnExtract.IsEnabled = false;
            laberStats.Content = "正在提取";
        }
        //
        private void btnInspect_Click(object sender, RoutedEventArgs e)
        {
            labelGameID.Content ="游戏ID："+ gameID;
            labelSourceFolderPath.Content ="源目录："+ fullPath;
            labelOutputFolderPath.Content="输出目录："+ outputPath;

            try
            {
                DirectoryInfo root = new DirectoryInfo(fullPath);
                DirectoryInfo[] dis = root.GetDirectories();
                labelItemsAmount.Content = "寻找到项目数：" + dis.Length;

                btnExtract.Click += async (o, j) => 
                {
                    await Task.Run(new Action(() =>
                    {
                        foreach (DirectoryInfo di in dis)
                        {
                            eSIC(string.Format("正在尝试提取：{0}", di.Name));
                            foreach (FileInfo fi in di.GetFiles())
                            {
                                File.Copy(fi.FullName, outputPath + @"/" + di.Name + @".pak", true);
                            }
                        }
                        eSIC("提取已完成");
                        eSC("待机");
                        eBIS(true);
                    }));
                };

                btnExtract.IsEnabled = true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }
            
        }

        //打开github
        private void Chip_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "https://github.com/nakeliss/StarboundWorkshopItemsExtract");
        }
    }
}

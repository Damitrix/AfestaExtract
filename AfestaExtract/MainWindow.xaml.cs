using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

//using System.Windows.Shapes;

namespace AfestaExtract
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string DPLayPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "//DPlay";
        private bool isPrecise = false;
        private int Mode = 0;
        private bool Loading = false;
        private int Progress = 0;
        private Timer ProgTimer;

        public MainWindow()
        {
            InitializeComponent();
            if (Directory.Exists(DPLayPath))
            {
                foreach (var file in Directory.GetFiles(DPLayPath))
                {
                    if (Path.GetExtension(file) == ".vcz")
                    {
                        ListBoxFiles.Items.Add(new ListBoxItem() { Content = Path.GetFileNameWithoutExtension(file) });
                    }
                }
            }
            else
            {
                ButtonRecentMode.IsEnabled = false;
            }

            ProgTimer = new System.Timers.Timer(100) {AutoReset = true};
            ProgTimer.Elapsed += Timer_Elapsed;
            ProgTimer.Start();

            var da = new DoubleAnimation(360, 0, new Duration(TimeSpan.FromSeconds(1)));
            var rt = new RotateTransform();
            LoadingLabel.RenderTransform = rt;
            LoadingLabel.RenderTransformOrigin = new Point(0.5, 0.5);
            da.RepeatBehavior = RepeatBehavior.Forever;
            rt.BeginAnimation(RotateTransform.AngleProperty, da);
        }

        private async void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Loading)
            {
                await this.Dispatcher.InvokeAsync(delegate
                {
                    LoadingLabel.Visibility = Visibility.Visible;
                    ProgBar.Value = Progress;
                    ButtonExtract.IsEnabled = false;
                });
            }
            else
            {
                await this.Dispatcher.InvokeAsync(delegate
                {
                    ProgBar.Value = Progress;
                    LoadingLabel.Visibility = Visibility.Hidden;
                    ButtonExtract.IsEnabled = true;
                });
                
            }
        }

        private void AnimateEnterButton(Control tb)
        {
            ColorAnimation colorAnim = new ColorAnimation
            {
                To = (Color)Application.Current.Resources["AccentColor"],
                Duration = new TimeSpan(0, 0, 0, 0, 400),
                AutoReverse = false,
                AccelerationRatio = 0.1
            };

            SolidColorBrush brush = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            tb.Background = brush;

            brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);
        }

        private void AnimateEnterTextBox(Control tb)
        {
            AnimateEnterButton(tb);
        }

        private void AnimateGridLength(RowDefinition rowDef, GridLength toLength)
        {
            GridLengthAnimation PrecTopAnim = new GridLengthAnimation();
            PrecTopAnim.GridUnitType = GridUnitType.Pixel;
            PrecTopAnim.From = rowDef.Height;
            PrecTopAnim.To = toLength;
            PrecTopAnim.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 300));
            PrecTopAnim.AccelerationRatio = 0.1;
            PrecTopAnim.DecelerationRatio = 0.9;
            rowDef.BeginAnimation(RowDefinition.HeightProperty, PrecTopAnim);
        }

        private void AnimateLeaveButton(Control tb)
        {
            ColorAnimation colorAnim = new ColorAnimation
            {
                To = (Color)Application.Current.Resources["BG_Dark"],
                Duration = new TimeSpan(0, 0, 0, 0, 500),
                AutoReverse = false,
                AccelerationRatio = 0.1
            };

            SolidColorBrush brush = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            tb.Background = brush;

            brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);
        }

        private void AnimateLeaveTextBox(Control tb)
        {
            ColorAnimation colorAnim = new ColorAnimation
            {
                To = Color.FromArgb(0, 0, 0, 0),
                Duration = new TimeSpan(0, 0, 0, 0, 500),
                AutoReverse = false,
                AccelerationRatio = 0.1
            };

            SolidColorBrush brush = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            tb.Background = brush;

            brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);
        }

        private void AnimateFinishedBrighten()
        {
            ColorAnimation colorAnim = new ColorAnimation
            {
                To = (Color)FindResource("AccentColor"),
                Duration = new TimeSpan(0, 0, 0, 0, 200),
                AutoReverse = false,
                AccelerationRatio = 0.1
            };

            SolidColorBrush brush = new SolidColorBrush(((SolidColorBrush)MainGrid.Background).Color);
            MainGrid.Background = brush;

            brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);

            Timer bgBackTimer = new Timer()
            {
                Interval = 200,
                AutoReset = false
            };
            bgBackTimer.Elapsed += BgBackTimer_Elapsed;
            bgBackTimer.Start();
        }

        private void AnimateFinishedDarken()
        {
            ColorAnimation colorAnim = new ColorAnimation
            {
                To = (Color)FindResource("BG_Bright"),
                Duration = new TimeSpan(0, 0, 0, 0, 200),
                AutoReverse = false,
                AccelerationRatio = 0.1
            };

            SolidColorBrush brush = new SolidColorBrush(((SolidColorBrush)MainGrid.Background).Color);
            MainGrid.Background = brush;

            brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);
        }

        private void BgBackTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(AnimateFinishedDarken);
        }

        private void ButtoBrowseInput_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofDiag = new OpenFileDialog()
            {
                Filter = "Afesta Video File (*.avi;*.mp4) |*.avi;*.mp4;*.*"
            };
            if ((bool)ofDiag.ShowDialog())
            {
                TextBoxVideo.Text = ofDiag.FileName;
            }
        }

        private void ButtonBrowseOutput_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDiag = new SaveFileDialog()
            {
                Filter = "CSV Output (*.csv)|*.csv"
            };
            if ((bool)saveDiag.ShowDialog())
            {
                TextBoxCSV.Text = saveDiag.FileName;
            }
        }

        private void ButtonExtract_Click(object sender, RoutedEventArgs e)
        {
            Progress = 0;
            if (Mode == 1)
            {
                ExtractFromDPlay();
            }
            else if (Mode == 0)
            {
                ExtractFromUrl();
            }
        }

        private void ButtonFullPrecision_Click(object sender, RoutedEventArgs e)
        {
            if (!isPrecise)
            {
                SwitchPrecision();
            }
        }

        private void ButtonOnlineMode_Click(object sender, RoutedEventArgs e)
        {
            if (Mode != 1) return;
            Mode = 0;
            SwitchMode();
        }

        private void ButtonRecentMode_Click(object sender, RoutedEventArgs e)
        {
            if (Mode != 0) return;
            Mode = 1;
            SwitchMode();
        }

        private void ButtonVorzeCompatibility_Click(object sender, RoutedEventArgs e)
        {
            if (isPrecise)
            {
                SwitchPrecision();
            }
        }

        private void ConvertToCSV(string inFile, string outFile)
        {
            if (File.Exists(outFile))
            {
                File.Delete(outFile);
            }
            var writer = File.CreateText(outFile);

            var inputBytes = File.ReadAllBytes(inFile);
            int index = 32; //End of Type Declaration (VCSX.Vorze_CycloneSA + something)

            bool endOfFile = false;

            while (!endOfFile)
            {
                index += 1;
                string timeHex = $"{inputBytes[index]:X2}{inputBytes[index + 1]:X2}{inputBytes[index + 2]:X2}"; // 3 Hex, that together form the Time
                double time = int.Parse(timeHex, NumberStyles.HexNumber);

                if (!isPrecise)
                {
                    time = Math.Floor(time / 100);

                    if (index + 7 < inputBytes.Length)
                    {
                        string nextTimeHex = $"{inputBytes[index + 5]:X2}{inputBytes[index + 6]:X2}{inputBytes[index + 7]:X2}";
                        double nextTime = int.Parse(nextTimeHex, NumberStyles.HexNumber) / 100;
                        if (time == nextTime)
                        {
                            index += 4;
                            continue;
                        }
                    }
                }
                string encStrengthHex = $"{inputBytes[index + 3]:X2}";                                          // Last Hex for Time
                int intensity = int.Parse(encStrengthHex, NumberStyles.HexNumber);

                int direction = 0;
                if (intensity > 127)
                {
                    direction = 1;
                    intensity -= 128;
                }

                writer.WriteLine($"{time},{direction},{intensity}");

                index += 4;
                endOfFile = index + 4 > inputBytes.Length;
            }
            writer.Close();
        }

        private void ExtractBin(string file, string output)
        {
            ZipInputStream stream = new ZipInputStream(File.OpenRead(file));
            ZipFile zip = new ZipFile(file);

            ZipEntry zEntry;
            while ((zEntry = stream.GetNextEntry()) != null)
            {
                if (zEntry.Name != "Vorze_CycloneSA.bin")
                {
                    continue;
                }
                using (FileStream streamWriter = File.Create(output))
                {
                    int size = 2048;
                    byte[] data = new byte[2048];
                    while (true)
                    {
                        size = stream.Read(data, 0, data.Length);
                        if (size > 0)
                        {
                            streamWriter.Write(data, 0, size);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }

        private async void DownloadFromUrl(string url, FormUrlEncodedContent content, string outCsv)
        {
            HttpClientHandler hdl = new HttpClientHandler();
            hdl.AllowAutoRedirect = false;
            HttpClient client = new HttpClient(hdl);

            client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue()
            {
                NoCache = true
            };
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue("FID")));
            
            var postLocation = await client.PostAsync(url, content);
            Progress += 25;
            var getData = await client.GetAsync(postLocation.Headers.Location, HttpCompletionOption.ResponseHeadersRead);
            Progress += 25;

            int index = 0;
            var buffer = new byte[getData.Content.Headers.ContentLength.Value];

            int prevProgress = Progress;

            using (var dlStream = await getData.Content.ReadAsStreamAsync())
            {
                while (index < buffer.Length)
                {
                    buffer[index] = (byte)dlStream.ReadByte();
                    Progress = prevProgress + Convert.ToInt32(Math.Floor((Convert.ToDouble(index) / Convert.ToDouble(buffer.Length)) * 100));
                    index++;
                }
            }

            var tempFileVCZ = Path.GetTempFileName();

            var fileStream = File.Create(tempFileVCZ, buffer.Length);
            fileStream.Write(buffer, 0, buffer.Length);
            fileStream.Close();
            Progress += 10;

            await ExtractAndConvert(tempFileVCZ, outCsv);
        }

        private Task ExtractAndConvert(string inVCZ, string outCSV)
        {
            var tempFileBin = Path.GetTempFileName();

            ExtractBin(inVCZ, tempFileBin);
            Progress += 20;
            ConvertToCSV(tempFileBin, outCSV);
            Progress = 0;
            this.Dispatcher.Invoke(AnimateFinishedBrighten);
            Loading = false;
            return Task.CompletedTask;
        }

        private void ExtractFromDPlay()
        {
            Loading = true;
            foreach (var selectedItem in ListBoxFiles.SelectedItems)
            {
                var cFile = ((ListBoxItem)selectedItem).Content;
                string archive = $"{DPLayPath}//{cFile}.vcz";
                string bin = $"{DPLayPath}//{cFile}.bin";
                string csv = $"{Directory.GetCurrentDirectory()}//{cFile}.csv";

                ExtractBin(archive, bin);
                ConvertToCSV(bin, csv);
            }
            Loading = false;
            AnimateFinishedBrighten();
        }

        private async void ExtractFromUrl()
        {
            if (File.Exists(TextBoxVideo.Text) && System.IO.Path.IsPathRooted(TextBoxCSV.Text))
            {
                if (GetPid() == "Not set")
                {
                    MessageBox.Show("You need to be locally registered at Afesta. Please Refer to this Post for help: *Message me if i forget to include it*");
                }

                Loading = true;

                var videoName = System.IO.Path.GetFileNameWithoutExtension(TextBoxVideo.Text);
                
                var postValues = new Dictionary<string, string>
                {
                    {"FID", videoName},
                    {"PID", GetPid()}
                };

                var content = new FormUrlEncodedContent(postValues);
                
                try
                {
                    string output = TextBoxCSV.Text;
                    await Task.Run(() => DownloadFromUrl(GetManageUrl(), content, output));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            else
            {
                MessageBox.Show("You might wanna look over your FilePaths again... Something is not right.");
            }
        }

        private string GetPid()
        {
            return (string)Registry.GetValue("HKEY_CURRENT_USER\\Software\\LAB\\LPEG", "pid", "Not set");
        }

        private readonly string baseUrl = "https://www.lpeg.jp";

        private string GetManageUrl()
        {
            return $"{baseUrl}/manage/vcs_dl.php";
        }

        private void MainButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Control tb = (Control)sender;

            AnimateEnterButton(tb);
        }

        private void MainButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Control tb = (Control)sender;

            AnimateLeaveButton(tb);
        }
        private void MainTextBox_MouseEnter(object sender, MouseEventArgs e)
        {
            Control tb = (Control)sender;

            AnimateEnterTextBox(tb);
        }

        private void MainTextBox_MouseLeave(object sender, MouseEventArgs e)
        {
            Control tb = (Control)sender;

            AnimateLeaveTextBox(tb);
        }
        private void SwitchMode()
        {
            double height = ((Grid)PrecisionChoosePaddingTopDef.Parent).ActualHeight / 2;
            if (Mode == 0)
            {
                RowDefinitionOnline.Height = new GridLength(0, GridUnitType.Star);
                RowDefinitionRecent.Height = new GridLength(1, GridUnitType.Star);
                AnimateGridLength(ModeChoosePaddingTopDef, new GridLength(height, GridUnitType.Pixel));
                AnimateGridLength(ModeChoosePaddingBotDef, new GridLength(0, GridUnitType.Star));
            }
            else
            {
                RowDefinitionOnline.Height = new GridLength(1, GridUnitType.Star);
                RowDefinitionRecent.Height = new GridLength(0, GridUnitType.Star);
                AnimateGridLength(ModeChoosePaddingTopDef, new GridLength(0, GridUnitType.Star));
                AnimateGridLength(ModeChoosePaddingBotDef, new GridLength(height, GridUnitType.Pixel));
            }
        }

        private void SwitchPrecision()
        {
            isPrecise = !isPrecise;
            double height = ((Grid)PrecisionChoosePaddingTopDef.Parent).ActualHeight / 2;
            if (!isPrecise)
            {
                AnimateGridLength(PrecisionChoosePaddingTopDef, new GridLength(height, GridUnitType.Pixel));
                AnimateGridLength(PrecisionChoosePaddingBotDef, new GridLength(0, GridUnitType.Star));
            }
            else
            {
                AnimateGridLength(PrecisionChoosePaddingTopDef, new GridLength(0, GridUnitType.Star));
                AnimateGridLength(PrecisionChoosePaddingBotDef, new GridLength(height, GridUnitType.Pixel));
            }
        }

        public class GridLengthAnimation : AnimationTimeline
        {
            public static readonly DependencyProperty FromProperty =
                DependencyProperty.Register("From", typeof(GridLength), typeof(GridLengthAnimation));

            public static readonly DependencyProperty GridUnitTypeProperty =
                DependencyProperty.Register("GridUnitType", typeof(GridUnitType), typeof(GridLengthAnimation), new UIPropertyMetadata(GridUnitType.Pixel));

            public static readonly DependencyProperty ToProperty =
                DependencyProperty.Register("To", typeof(GridLength), typeof(GridLengthAnimation));

            public GridLength From
            {
                get
                {
                    return (GridLength)GetValue(GridLengthAnimation.FromProperty);
                }
                set
                {
                    SetValue(GridLengthAnimation.FromProperty, value);
                }
            }

            public GridUnitType GridUnitType
            {
                get { return (GridUnitType)GetValue(GridUnitTypeProperty); }
                set { SetValue(GridUnitTypeProperty, value); }
            }

            public override Type TargetPropertyType
            {
                get { return typeof(GridLength); }
            }

            public GridLength To
            {
                get
                {
                    return (GridLength)GetValue(GridLengthAnimation.ToProperty);
                }
                set
                {
                    SetValue(GridLengthAnimation.ToProperty, value);
                }
            }

            private double FromAsDouble
            {
                get
                {
                    return ((GridLength)From).Value;
                }
            }

            private double ToAsDouble
            {
                get
                {
                    return ((GridLength)To).Value;
                }
            }

            public override object GetCurrentValue(object defaultOriginValue,
                                                    object defaultDestinationValue,
                                                    AnimationClock animationClock)
            {
                if (FromAsDouble > ToAsDouble)
                    return new GridLength((1 - animationClock.CurrentProgress.Value) *
                        (FromAsDouble - ToAsDouble) + ToAsDouble, this.GridUnitType);

                return new GridLength(animationClock.CurrentProgress.Value *
                    (ToAsDouble - FromAsDouble) + FromAsDouble, this.GridUnitType);
            }

            protected override System.Windows.Freezable CreateInstanceCore()
            {
                return new GridLengthAnimation();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ProgTimer.Stop();
        }
    }
}
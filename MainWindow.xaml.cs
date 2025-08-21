using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DailyOrderPanel
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private List<string> rules = new List<string>
        {
            "保持安静，专注学习📕",
            "有问题憋着下课问‍",
            "合理规划自习时间⏰",
            "今天不学习，明天变垃圾🚮",
            "珍惜每分每秒🕙",
            "物品轻拿轻放🐾🐈🐑🦘🦥🦛",
            "作业做完了吗就讲话，闭嘴👊🔥"
        };

        private List<HomeworkItem> homeworkItems = new List<HomeworkItem>();
        private bool isDarkMode = false;
        private DispatcherTimer timer = new DispatcherTimer();
        private string currentTime = "";
        private string countdown = "";

        public List<string> Rules
        {
            get { return rules; }
            set { rules = value; OnPropertyChanged("Rules"); }
        }

        public List<HomeworkItem> HomeworkItems
        {
            get { return homeworkItems; }
            set { homeworkItems = value; OnPropertyChanged("HomeworkItems"); }
        }

        public string CurrentTime
        {
            get { return currentTime; }
            set { currentTime = value; OnPropertyChanged("CurrentTime"); }
        }

        public string Countdown
        {
            get { return countdown; }
            set { countdown = value; OnPropertyChanged("Countdown"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            // 初始化计时器
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();

            // 加载作业数据
            LoadHomeworkData();

            // 加载自定义图片
            LoadCustomImage();

            // 设置初始主题
            CheckSystemTheme();

            // 初始显示学生端界面
            StudentPage.Visibility = Visibility.Visible;
            TeacherPage.Visibility = Visibility.Collapsed;

            // 设置切换按钮初始状态和文本
            ModeToggleButton.Background = new SolidColorBrush(Color.FromArgb(34, 255, 255, 255));
            ModeToggleButton.Content = "教师端";
        }

        // 注意：最小化、最大化和关闭按钮已从UI中移除
        // 仅保留全屏功能

        // 全屏切换
        private void FullScreenBtn_Click(object sender, RoutedEventArgs e)
        {
            if (WindowStyle == WindowStyle.None)
            {
                // 退出全屏模式
                WindowStyle = WindowStyle.SingleBorderWindow;
                WindowState = WindowState.Normal;
                ResizeMode = ResizeMode.CanResize;
                FullScreenBtn.Content = "□";
            }
            else
            {
                // 进入全屏模式
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
                ResizeMode = ResizeMode.NoResize;
                FullScreenBtn.Content = "◻";
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateTime();
        }

        private void UpdateTime()
        {
            // 更新当前时间
            CurrentTime = DateTime.Now.ToString("HH:mm:ss");

            // 计算剩余时间（晚自习结束时间为21:50）
            DateTime endTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 21, 50, 0);
            TimeSpan diff = endTime - DateTime.Now;

            if (diff.TotalMilliseconds <= 0)
            {
                Countdown = "自习结束";
                return;
            }

            // 格式化剩余时间
            string countdownText = "";
            if (diff.Hours > 0)
            {
                countdownText += $"{diff.Hours}时";
            }
            countdownText += $"{diff.Minutes}分{diff.Seconds}秒";
            Countdown = countdownText;
        }

        private void ModeToggleButton_Click(object sender, RoutedEventArgs e)
        {
            // 检查当前显示的页面并执行动画切换
            if (StudentPage.Visibility == Visibility.Visible)
            {
                // 切换到教师端
                AnimatePageChange(true);
            }
            else
            {
                // 切换到学生端
                AnimatePageChange(false);
            }
        }

        // 带渐变动画的页面切换
        private void AnimatePageChange(bool switchToTeacherMode)
        {
            // 创建一个覆盖层
            Grid overlay = new Grid();
            // 设置适当的背景色，与当前主题匹配
            overlay.Background = isDarkMode ? 
                new SolidColorBrush(Color.FromArgb(255, 30, 30, 30)) : 
                new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            overlay.Visibility = Visibility.Visible;
            overlay.HorizontalAlignment = HorizontalAlignment.Stretch;
            overlay.VerticalAlignment = VerticalAlignment.Stretch;
            overlay.PreviewMouseDown += (s, e) => e.Handled = true;
            overlay.IsHitTestVisible = true;

            // 将覆盖层添加到主窗口
            var mainGrid = (Grid)this.Content;
            Panel.SetZIndex(overlay, 1000);
            mainGrid.Children.Add(overlay);

            // 创建淡入动画
            DoubleAnimation fadeInAnimation = new DoubleAnimation();
            fadeInAnimation.From = 0;
            fadeInAnimation.To = 1;
            fadeInAnimation.Duration = TimeSpan.FromMilliseconds(200);
            fadeInAnimation.EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseInOut };

            // 创建淡出动画
            DoubleAnimation fadeOutAnimation = new DoubleAnimation();
            fadeOutAnimation.From = 1;
            fadeOutAnimation.To = 0;
            fadeOutAnimation.Duration = TimeSpan.FromMilliseconds(200);
            fadeOutAnimation.EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseInOut };

            // 设置动画完成事件
            fadeInAnimation.Completed += (s, e) =>
            {
                // 动画中间点，切换页面
                if (switchToTeacherMode)
                {
                    // 切换到教师端
                    StudentPage.Visibility = Visibility.Collapsed;
                    TeacherPage.Visibility = Visibility.Visible;
                    
                    // 更新按钮状态和文本
                    ModeToggleButton.Background = new SolidColorBrush(Color.FromArgb(51, 255, 255, 255));
                    ModeToggleButton.Content = "学生端";
                    
                    // 更新底部文本
                    FooterText.Text = "布置的作业将实时显示在学生端";
                    
                    // 刷新教师端作业列表
                    LoadHomeworkData();
                }
                else
                {
                    // 切换到学生端
                    StudentPage.Visibility = Visibility.Visible;
                    TeacherPage.Visibility = Visibility.Collapsed;
                    
                    // 更新按钮状态和文本
                    ModeToggleButton.Background = new SolidColorBrush(Color.FromArgb(34, 255, 255, 255));
                    ModeToggleButton.Content = "教师端";
                    
                    // 更新底部文本
                    FooterText.Text = "© 2025 DOP每日委托面板 ZFTONY制 | 版本 Beta0.3.7.1";
                }

                // 开始淡出动画
                overlay.BeginAnimation(UIElement.OpacityProperty, fadeOutAnimation);
            };

            // 淡出动画完成后移除覆盖层
            fadeOutAnimation.Completed += (s, e) =>
            {
                mainGrid.Children.Remove(overlay);
            };

            // 开始动画
            overlay.BeginAnimation(UIElement.OpacityProperty, fadeInAnimation);
        }

        private void ThemeToggle_Click(object sender, RoutedEventArgs e)
        {
            isDarkMode = !isDarkMode;
            AnimateThemeChange();

            // 保存主题偏好
            Properties.Settings.Default.IsDarkMode = isDarkMode;
            Properties.Settings.Default.Save();
        }

        private void CheckSystemTheme()
        {
            // 检查用户设置
            if (Properties.Settings.Default.IsDarkMode.HasValue)
            {
                isDarkMode = Properties.Settings.Default.IsDarkMode.Value;
            }
            else
            {
                // 默认使用浅色主题
                isDarkMode = false;
            }

            UpdateTheme();
        }

        // 直接更新主题（用于初始化）
        private void UpdateTheme()
        {
            // 清除现有主题资源
            Resources.MergedDictionaries.Clear();

            // 加载新主题
            if (isDarkMode)
            {
                Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("Themes/DarkTheme.xaml", UriKind.Relative) });
                ThemeToggle.Content = new TextBlock() { Text = "☀️", FontSize = 18 };
            }
            else
            {
                Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("Themes/LightTheme.xaml", UriKind.Relative) });
                ThemeToggle.Content = new TextBlock() { Text = "🌙", FontSize = 18 };
            }
        }

        // 带渐变动画的主题切换
        private void AnimateThemeChange()
        {
            // 创建一个覆盖层
            Grid overlay = new Grid();
            // 设置适当的背景色，alpha值设为255使其可见
            overlay.Background = isDarkMode ? 
                new SolidColorBrush(Color.FromArgb(255, 30, 30, 30)) : 
                new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            overlay.Visibility = Visibility.Visible;
            overlay.HorizontalAlignment = HorizontalAlignment.Stretch;
            overlay.VerticalAlignment = VerticalAlignment.Stretch;
            overlay.PreviewMouseDown += (s, e) => e.Handled = true;
            overlay.IsHitTestVisible = true;

            // 将覆盖层添加到主窗口
            var mainGrid = (Grid)this.Content;
            Panel.SetZIndex(overlay, 1000);
            mainGrid.Children.Add(overlay);

            // 创建淡入动画，调整持续时间使切换速度更快
            DoubleAnimation fadeInAnimation = new DoubleAnimation();
            fadeInAnimation.From = 0;
            fadeInAnimation.To = 1;
            fadeInAnimation.Duration = TimeSpan.FromMilliseconds(200);
            fadeInAnimation.EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseInOut };

            // 创建淡出动画，调整持续时间使切换速度更快
            DoubleAnimation fadeOutAnimation = new DoubleAnimation();
            fadeOutAnimation.From = 1;
            fadeOutAnimation.To = 0;
            fadeOutAnimation.Duration = TimeSpan.FromMilliseconds(200);
            fadeOutAnimation.EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseInOut };

            // 设置动画完成事件
            fadeInAnimation.Completed += (s, e) =>
            {
                // 动画中间点，切换主题
                UpdateTheme();
                // 开始淡出动画
                overlay.BeginAnimation(UIElement.OpacityProperty, fadeOutAnimation);
            };

            // 淡出动画完成后移除覆盖层
            fadeOutAnimation.Completed += (s, e) =>
            {
                mainGrid.Children.Remove(overlay);
            };

            // 开始动画
            overlay.BeginAnimation(UIElement.OpacityProperty, fadeInAnimation);
        }

        private void LoadHomeworkData()
        {
            // 从本地存储加载作业数据
            try
            {
                string dataPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DailyOrderPanel");
                string homeworkFile = System.IO.Path.Combine(dataPath, "homeworkData.json");

                if (File.Exists(homeworkFile))
                {
                    string json = File.ReadAllText(homeworkFile);
                    Dictionary<string, string> homeworkData = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                    // 创建作业项列表
                    List<HomeworkItem> items = new List<HomeworkItem>();
                    string[] subjects = { "语文", "数学", "英语", "物理", "化学", "生物" };

                    foreach (string subject in subjects)
                    {
                        string content = homeworkData.ContainsKey(subject) ? homeworkData[subject] : string.Empty;
                        items.Add(new HomeworkItem(subject, content));
                    }

                    HomeworkItems = items;
                }
                else
                {
                    // 初始化空作业列表
                    InitializeEmptyHomeworkList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载作业数据时出错: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                InitializeEmptyHomeworkList();
            }
        }

        private void InitializeEmptyHomeworkList()
        {
            List<HomeworkItem> items = new List<HomeworkItem>();
            string[] subjects = { "语文", "数学", "英语", "物理", "化学", "生物" };

            foreach (string subject in subjects)
            {
                items.Add(new HomeworkItem(subject, string.Empty));
            }

            HomeworkItems = items;
        }

        private void SaveHomeworkData()
        {
            try
            {
                string dataPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DailyOrderPanel");
                Directory.CreateDirectory(dataPath);
                string homeworkFile = System.IO.Path.Combine(dataPath, "homeworkData.json");

                Dictionary<string, string> homeworkData = new Dictionary<string, string>();
                foreach (HomeworkItem item in HomeworkItems)
                {
                    if (!string.IsNullOrEmpty(item.Content))
                    {
                        homeworkData[item.Subject] = item.Content;
                    }
                }

                string json = JsonSerializer.Serialize(homeworkData, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(homeworkFile, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存作业数据时出错: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadCustomImage()
        {
            try
            {
                string dataPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DailyOrderPanel");
                string imageFile = System.IO.Path.Combine(dataPath, "customImage.png");

                if (File.Exists(imageFile))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imageFile);
                    bitmap.EndInit();

                    CustomImage.Source = bitmap;
                    CustomImage.Visibility = Visibility.Visible;
                    NoImageText.Visibility = Visibility.Collapsed;
                }
                else
                {
                    CustomImage.Visibility = Visibility.Collapsed;
                    NoImageText.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载图片时出错: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                CustomImage.Visibility = Visibility.Collapsed;
                NoImageText.Visibility = Visibility.Visible;
            }
        }

        private void SaveCustomImage(string imagePath)
        {
            try
            {
                string dataPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DailyOrderPanel");
                Directory.CreateDirectory(dataPath);
                string targetFile = System.IO.Path.Combine(dataPath, "customImage.png");

                // 复制并覆盖现有文件
                File.Copy(imagePath, targetFile, true);

                // 重新加载图片
                LoadCustomImage();
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存图片时出错: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearCustomImage()
        {
            try
            {
                string dataPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DailyOrderPanel");
                string imageFile = System.IO.Path.Combine(dataPath, "customImage.png");

                if (File.Exists(imageFile))
                {
                    File.Delete(imageFile);
                }

                CustomImage.Visibility = Visibility.Collapsed;
                NoImageText.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show("清除图片时出错: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SubmitBtn_Click(object sender, RoutedEventArgs e)
        {
            string subject = (SubjectComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            string content = HomeworkTextBox.Text.Trim();

            if (string.IsNullOrEmpty(subject))
            {
                MessageBox.Show("请选择科目", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (string.IsNullOrEmpty(content))
            {
                MessageBox.Show("请输入作业内容", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 更新作业数据
            foreach (HomeworkItem item in HomeworkItems)
            {
                if (item.Subject == subject)
                {
                    item.Content = content;
                    break;
                }
            }

            // 保存数据
            SaveHomeworkData();

            // 刷新列表
            LoadHomeworkData();

            // 清空输入框
            HomeworkTextBox.Text = string.Empty;

            MessageBox.Show($"{subject}作业已布置成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("确定要清空所有已布置的作业吗？", "确认", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                // 清除所有作业
                foreach (HomeworkItem item in HomeworkItems)
                {
                    item.Content = string.Empty;
                }

                // 保存数据
                SaveHomeworkData();

                // 刷新列表
                LoadHomeworkData();

                MessageBox.Show("所有作业已清空", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void UploadImageBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "图片文件|*.jpg;*.jpeg;*.png;*.gif;*.bmp";

            if (openFileDialog.ShowDialog() == true)
            {
                SaveCustomImage(openFileDialog.FileName);
                MessageBox.Show("图片上传成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ClearImageBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("确定要清除当前图片吗？", "确认", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                ClearCustomImage();
                MessageBox.Show("图片已清除", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ExportBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 准备导出数据
                string dataPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DailyOrderPanel");
                string homeworkFile = System.IO.Path.Combine(dataPath, "homeworkData.json");
                string imageFile = System.IO.Path.Combine(dataPath, "customImage.png");

                Dictionary<string, string> homeworkData = new Dictionary<string, string>();
                if (File.Exists(homeworkFile))
                {
                    string json = File.ReadAllText(homeworkFile);
                    homeworkData = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                }

                // 检查是否有内容可导出
                if (homeworkData.Count == 0 && !File.Exists(imageFile))
                {
                    MessageBox.Show("当前没有内容可以导出", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // 创建导出数据对象
                ExportData exportData = new ExportData
                {
                    meta = new MetaData
                    {
                        version = "1.1",
                        exportTime = DateTime.Now.ToString("o"),
                        system = "晚自习管理系统"
                    },
                    homework = homeworkData,
                    customImage = File.Exists(imageFile) ? Convert.ToBase64String(File.ReadAllBytes(imageFile)) : null
                };

                // 保存导出文件
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "JSON文件|*.json";
                saveFileDialog.FileName = "晚自习作业.json";

                if (saveFileDialog.ShowDialog() == true)
                {
                    string json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(saveFileDialog.FileName, json);
                    MessageBox.Show("作业和图片已成功导出为JSON文件", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("导出数据时出错: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImportBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "JSON文件|*.json";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string json = File.ReadAllText(openFileDialog.FileName);
                    ExportData importedData = JsonSerializer.Deserialize<ExportData>(json);

                    // 验证数据格式
                    if (importedData == null || (importedData.homework == null && importedData.customImage == null))
                    {
                        MessageBox.Show("导入的文件格式不正确，缺少有效数据", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // 验证科目有效性
                    string[] validSubjects = { "语文", "数学", "英语", "物理", "化学", "生物" };
                    if (importedData.homework != null)
                    {
                        foreach (string subject in importedData.homework.Keys)
                        {
                            if (!validSubjects.Contains(subject))
                            {
                                MessageBox.Show("导入的文件包含无效科目，请确保是有效的作业数据", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }
                    }

                    if (MessageBox.Show("确定要导入作业和图片吗？这将覆盖当前的数据。", "确认", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        // 保存作业数据
                        if (importedData.homework != null)
                        {
                            string dataPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DailyOrderPanel");
                            Directory.CreateDirectory(dataPath);
                            string homeworkFile = System.IO.Path.Combine(dataPath, "homeworkData.json");

                            string homeworkJson = JsonSerializer.Serialize(importedData.homework, new JsonSerializerOptions { WriteIndented = true });
                            File.WriteAllText(homeworkFile, homeworkJson);
                        }

                        // 保存图片数据
                        if (!string.IsNullOrEmpty(importedData.customImage))
                        {
                            string dataPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DailyOrderPanel");
                            Directory.CreateDirectory(dataPath);
                            string imageFile = System.IO.Path.Combine(dataPath, "customImage.png");

                            byte[] imageBytes = Convert.FromBase64String(importedData.customImage);
                            File.WriteAllBytes(imageFile, imageBytes);
                        }
                        else
                        {
                            // 清除现有图片
                            ClearCustomImage();
                        }

                        // 刷新数据
                        LoadHomeworkData();
                        LoadCustomImage();

                        MessageBox.Show("数据导入成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("导入数据时出错: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    // 作业项类
    public class HomeworkItem : INotifyPropertyChanged
    {
        private string subject;
        private string content;
        private string icon;
        private SolidColorBrush subjectColor;

        public string Subject
        {
            get { return subject; }
            set { subject = value; OnPropertyChanged("Subject"); UpdateIconAndColor(); }
        }

        public string Content
        {
            get { return content; }
            set { content = value; OnPropertyChanged("Content"); }
        }

        public string Icon
        {
            get { return icon; }
            set { icon = value; OnPropertyChanged("Icon"); }
        }

        public SolidColorBrush SubjectColor
        {
            get { return subjectColor; }
            set { subjectColor = value; OnPropertyChanged("SubjectColor"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public HomeworkItem(string subject, string content)
        {
            Subject = subject;
            Content = content;
            UpdateIconAndColor();
        }

        private void UpdateIconAndColor()
        {
            // 设置科目图标和颜色
            switch (Subject)
            {
                case "语文":
                    Icon = "📖";
                    SubjectColor = new SolidColorBrush(Color.FromRgb(231, 76, 60));
                    break;
                case "数学":
                    Icon = "🧮";
                    SubjectColor = new SolidColorBrush(Color.FromRgb(52, 152, 219));
                    break;
                case "英语":
                    Icon = "🔠";
                    SubjectColor = new SolidColorBrush(Color.FromRgb(243, 156, 18));
                    break;
                case "物理":
                    Icon = "⚛️";
                    SubjectColor = new SolidColorBrush(Color.FromRgb(155, 89, 182));
                    break;
                case "化学":
                    Icon = "🧪";
                    SubjectColor = new SolidColorBrush(Color.FromRgb(26, 188, 156));
                    break;
                case "生物":
                    Icon = "🧬";
                    SubjectColor = new SolidColorBrush(Color.FromRgb(46, 204, 113));
                    break;
                default:
                    Icon = "📚";
                    SubjectColor = new SolidColorBrush(Color.FromRgb(127, 140, 141));
                    break;
            }
        }
    }

    // 导出数据类
    public class ExportData
    {
        public MetaData meta { get; set; }
        public Dictionary<string, string> homework { get; set; }
        public string customImage { get; set; }
    }

    public class MetaData
    {
        public string version { get; set; }
        public string exportTime { get; set; }
        public string system { get; set; }
    }
}
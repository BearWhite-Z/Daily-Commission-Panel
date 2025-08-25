using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using System.Text.Json;
using DailyCommissionPanel.Models;

namespace DailyCommissionPanel
{
    /// <summary>
    /// SettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private string settingsFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
        private int defaultEndHour = 21;
        private int defaultEndMinute = 50;
        private List<string> defaultRules = new List<string>
        {
            "保持安静，专注学习📕",
            "有问题憋着下课问‍",
            "合理规划自习时间⏰",
            "今天不学习，明天变垃圾🚮",
            "珍惜每分每秒🕙",
            "物品轻拿轻放🐾🐈🐑🦘🦥🦛",
            "作业做完了吗就讲话，闭嘴👊🔥"
        };

        private void OnThemeChanged(object? sender, EventArgs e)
        {
            // 重新加载主题
            LoadTheme();
        }

        public SettingsWindow()
        {
            InitializeComponent();
            try
            {
                LoadTheme();
                InitializeTimeComboBoxes();
                LoadSettings();

                // 监听主题变化事件
                MainWindow? mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.ThemeChanged += OnThemeChanged;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("初始化设置窗口时出错: " + ex.Message + "\n\n堆栈跟踪: " + ex.StackTrace, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        private void LoadTheme()
        {
            try
            {
                // 清空现有的MergedDictionaries以避免资源冲突
                Resources.MergedDictionaries.Clear();

                // 从应用程序设置中获取当前主题模式
                bool isDarkMode = Properties.Settings.Default.IsDarkMode.GetValueOrDefault(true); // 默认使用深色主题

                // 使用相对路径加载主题，与MainWindow保持一致
                if (isDarkMode)
                {
                    Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("Themes/DarkTheme.xaml", UriKind.Relative) });
                }
                else
                {
                    Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("Themes/LightTheme.xaml", UriKind.Relative) });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载主题时出错: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeTimeComboBoxes()
        {
            // 初始化小时下拉框 (0-23)
            for (int i = 0; i < 24; i++)
            {
                HourComboBox.Items.Add(i.ToString("D2"));
            }

            // 初始化分钟下拉框 (0-59，间隔5分钟)
            for (int i = 0; i < 60; i += 5)
            {
                MinuteComboBox.Items.Add(i.ToString("D2"));
            }
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(settingsFilePath))
                {
                    string json = File.ReadAllText(settingsFilePath);

                    try
                    {
                        Models.Settings settings = JsonSerializer.Deserialize<Models.Settings>(json) ?? new Models.Settings();

                        // 加载结束时间
                        HourComboBox.SelectedItem = settings.EndHour.ToString("D2");
                        MinuteComboBox.SelectedItem = settings.EndMinute.ToString("D2");

                        // 加载规则
                        RulesTextBox.Text = string.Join(Environment.NewLine, settings.Rules ?? defaultRules);
                    }
                    catch (JsonException jex)
                    {
                        MessageBox.Show("解析设置文件时出错: " + jex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        // 使用默认设置
                        HourComboBox.SelectedItem = defaultEndHour.ToString("D2");
                        MinuteComboBox.SelectedItem = defaultEndMinute.ToString("D2");
                        RulesTextBox.Text = string.Join(Environment.NewLine, defaultRules);
                    }
                }
                else
                {
                    // 使用默认设置
                    HourComboBox.SelectedItem = defaultEndHour.ToString("D2");
                    MinuteComboBox.SelectedItem = defaultEndMinute.ToString("D2");
                    RulesTextBox.Text = string.Join(Environment.NewLine, defaultRules);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载设置时出错: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                // 使用默认设置
                HourComboBox.SelectedItem = defaultEndHour.ToString("D2");
                MinuteComboBox.SelectedItem = defaultEndMinute.ToString("D2");
                RulesTextBox.Text = string.Join(Environment.NewLine, defaultRules);
            }
        }

        private void SaveSettings()
        {
            try
            {
                // 验证并获取所选时间
                int endHour = defaultEndHour;
                if (HourComboBox.SelectedItem != null && int.TryParse(HourComboBox.SelectedItem.ToString(), out int selectedHour))
                {
                    endHour = selectedHour;
                }

                int endMinute = defaultEndMinute;
                if (MinuteComboBox.SelectedItem != null && int.TryParse(MinuteComboBox.SelectedItem.ToString(), out int selectedMinute))
                {
                    endMinute = selectedMinute;
                }

                Models.Settings settings = new Models.Settings
                {
                    EndHour = endHour,
                    EndMinute = endMinute,
                    Rules = new List<string>(RulesTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                };

                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(settingsFilePath, json);

                // 通知主窗口设置已更改
                (Application.Current.MainWindow as MainWindow)?.UpdateSettings(settings);

                MessageBox.Show("设置已保存");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存设置时出错: " + ex.Message);
            }
        }

        private void AddRuleButton_Click(object sender, RoutedEventArgs e)
        {
            // 创建自定义输入对话框
            Window inputWindow = new Window
            {
                Title = "添加规则",
                Width = 400,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize
            };

            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.Margin = new Thickness(10);

            TextBlock label = new TextBlock
            {
                Text = "请输入新的规则:",
                Margin = new Thickness(0, 0, 0, 10),
                FontSize = 14
            };
            Grid.SetRow(label, 0);

            TextBox textBox = new TextBox
            {
                Margin = new Thickness(0, 0, 0, 10),
                FontSize = 14,
                MaxLength = 100
            };
            Grid.SetRow(textBox, 1);

            StackPanel buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            Grid.SetRow(buttonPanel, 2);

            Button okButton = new Button
            {
                Content = "确定",
                Width = 80,
                Margin = new Thickness(0, 0, 10, 0),
                IsDefault = true
            };
            okButton.Click += (s, args) =>
            {
                string newRule = textBox.Text.Trim();
                if (!string.IsNullOrWhiteSpace(newRule))
                {
                    if (RulesTextBox.Text.Length > 0)
                    {
                        RulesTextBox.Text += Environment.NewLine;
                    }
                    RulesTextBox.Text += newRule;
                }
                inputWindow.Close();
            };

            Button cancelButton = new Button
            {
                Content = "取消",
                Width = 80,
                IsCancel = true
            };
            cancelButton.Click += (s, args) => inputWindow.Close();

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);

            grid.Children.Add(label);
            grid.Children.Add(textBox);
            grid.Children.Add(buttonPanel);

            inputWindow.Content = grid;
            inputWindow.ShowDialog();
        }

        private void ClearRulesButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("确定要清空所有规则吗?", "确认", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                RulesTextBox.Text = string.Empty;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

}
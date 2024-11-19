using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Windows.UI.ViewManagement;
using Windows.Foundation;
using Windows.UI.Core;

namespace HaokeeEncoder
{
    public partial class MainWindow : Window
    {
        private static readonly Dictionary<char, string> MorseCode = new()
        {
            {'A', ".-"}, {'B', "-..."}, {'C', "-.-."}, {'D', "-.."}, {'E', "."}, {'F', "..-."},
            {'G', "--."}, {'H', "...."}, {'I', ".."}, {'J', ".---"}, {'K', "-.-"}, {'L', ".-.."},
            {'M', "--"}, {'N', "-."}, {'O', "---"}, {'P', ".--."}, {'Q', "--.-"}, {'R', ".-."},
            {'S', "..."}, {'T', "-"}, {'U', "..-"}, {'V', "...-"}, {'W', ".--"}, {'X', "-..-"},
            {'Y', "-.--"}, {'Z', "--.."}, {'1', ".----"}, {'2', "..---"}, {'3', "...--"},
            {'4', "....-"}, {'5', "....."}, {'6', "-...."}, {'7', "--..."}, {'8', "---.."},
            {'9', "----."}, {'0', "-----"}, {' ', "/"}
        };

        public MainWindow()
        {
            InitializeComponent();
            this.Title = "Haokee Encoder"; // 设置窗口标题
            this.AppWindow.Resize(new Windows.Graphics.SizeInt32(350, 750));
            this.AppWindow.SetIcon(System.Environment.CurrentDirectory + "\\icon.ico");
        }

        private void CodingClick(object sender, RoutedEventArgs e)
        {
            // 获取选择的操作
            var selectedOperation = (OperationSelector.SelectedItem as ComboBoxItem)?.Content.ToString();
            var selectedEncoding = (EncodingSelector.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (selectedOperation == "编码")
            {
                switch (selectedEncoding)
                {
                    case "Base64":
                        Output.Text = Convert.ToBase64String(Encoding.UTF8.GetBytes(Input.Text));
                        break;
                    case "Base32":
                        Output.Text = Base32Encode(Input.Text);
                        break;
                    case "Base58":
                        Output.Text = Base58Encode(Input.Text);
                        break;
                    case "Base85":
                        Output.Text = Base85Encode(Input.Text);
                        break;
                    case "摩斯电码":
                        Output.Text = MorseEncode(Input.Text);
                        break;
                    case "Quoted-Printable":
                        Output.Text = QuotedPrintableEncode(Input.Text);
                        break;
                }
            }
            else if (selectedOperation == "解码")
            {
                try
                {
                    switch (selectedEncoding)
                    {
                        case "Base64":
                            Output.Text = Encoding.UTF8.GetString(Convert.FromBase64String(Input.Text));
                            break;
                        case "Base32":
                            Output.Text = Base32Decode(Input.Text);
                            break;
                        case "Base58":
                            Output.Text = Base58Decode(Input.Text);
                            break;
                        case "Base85":
                            Output.Text = Base85Decode(Input.Text);
                            break;
                        case "摩斯电码":
                            Output.Text = MorseDecode(Input.Text);
                            break;
                        case "Quoted-Printable":
                            Output.Text = QuotedPrintableDecode(Input.Text);
                            break;
                    }
                }
                catch (FormatException)
                {
                    Output.Text = "无效的编码字符串";
                }
            }
        }

        private void SwapClick(object sender, RoutedEventArgs e)
        {
            // 交换输入框和输出框的内容
            var temp = Input.Text;
            Input.Text = Output.Text;
            Output.Text = temp;
        }

        private async void PasteFromClipboard(object sender, RoutedEventArgs e)
        {
            var dataPackageView = Windows.ApplicationModel.DataTransfer.Clipboard.GetContent();
            if (dataPackageView.Contains(Windows.ApplicationModel.DataTransfer.StandardDataFormats.Text))
            {
                string text = await dataPackageView.GetTextAsync();
                Input.Text = text;
            }
            PasteButton.Content = "已复制";
             
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, args) =>
            {
                PasteButton.Content = "从剪切板粘贴";
                timer.Stop(); // 停止计时器
            };
            timer.Start(); // 启动计时器
        }

        private void CopyToClipboard(object sender, RoutedEventArgs e)
        {
            var dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
            dataPackage.SetText(Output.Text);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
            CopyButton.Content = "已复制";

            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, args) =>
            {
                CopyButton.Content = "复制到剪切板";
                timer.Stop(); // 停止计时器
            };
            timer.Start(); // 启动计时器
        }

        private string Base32Encode(string input)
        {
            // 实现 Base32 编码
            var bytes = Encoding.UTF8.GetBytes(input);
            return Base32.ToBase32String(bytes);
        }

        private string Base32Decode(string input)
        {
            // 实现 Base32 解码
            var bytes = Base32.FromBase32String(input);
            return Encoding.UTF8.GetString(bytes);
        }

        private string Base58Encode(string input)
        {
            // 实现 Base58 编码
            var bytes = Encoding.UTF8.GetBytes(input);
            return Base58.Encode(bytes);
        }

        private string Base58Decode(string input)
        {
            // 实现 Base58 解码
            var bytes = Base58.Decode(input);
            return Encoding.UTF8.GetString(bytes);
        }

        private string Base85Encode(string input)
        {
            // 实现 Base85 编码
            var bytes = Encoding.UTF8.GetBytes(input);
            return Base85.Encode(bytes);
        }

        private string Base85Decode(string input)
        {
            // 实现 Base85 解码
            var bytes = Base85.Decode(input);
            return Encoding.UTF8.GetString(bytes);
        }

        private string MorseEncode(string input)
        {
            return string.Join(" ", input.ToUpper().Select(c => MorseCode.ContainsKey(c) ? MorseCode[c] : "?"));
        }

        private string MorseDecode(string input)
        {
            var reverseMorseCode = MorseCode.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
            return string.Join("", input.Split(' ').Select(m => reverseMorseCode.ContainsKey(m) ? reverseMorseCode[m].ToString() : "?"));
        }

        private string QuotedPrintableEncode(string input)
        {
            // 实现 Quoted-Printable 编码
            var bytes = Encoding.UTF8.GetBytes(input);
            return QuotedPrintable.Encode(bytes);
        }

        private string QuotedPrintableDecode(string input)
        {
            // 实现 Quoted-Printable 解码
            var bytes = QuotedPrintable.Decode(input);
            return Encoding.UTF8.GetString(bytes);
        }
    }

    // Base32 编码和解码实现
    public static class Base32
    {
        private const string Base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

        public static string ToBase32String(byte[] bytes)
        {
            StringBuilder result = new StringBuilder();
            int byteIndex = 0, bitIndex = 0, currentByte = 0, digit = 0;

            while (byteIndex < bytes.Length)
            {
                int currentBit = (bytes[byteIndex] >> (7 - bitIndex)) & 1;
                currentByte = (currentByte << 1) | currentBit;
                bitIndex++;

                if (bitIndex == 8)
                {
                    byteIndex++;
                    bitIndex = 0;
                }
                
                digit++;
                if (digit == 5)
                {
                    result.Append(Base32Chars[currentByte]);
                    currentByte = 0;
                    digit = 0;
                }
            }

            if (digit > 0)
            {
                currentByte <<= (5 - digit);
                result.Append(Base32Chars[currentByte]);
            }

            return result.ToString();
        }

        public static byte[] FromBase32String(string base32)
        {
            List<byte> result = new List<byte>();
            int bitIndex = 0, currentByte = 0;

            foreach (char c in base32)
            {
                int charValue = Base32Chars.IndexOf(c);
                if (charValue < 0)
                    throw new FormatException("Invalid Base32 character.");

                for (int i = 4; i >= 0; i--)
                {
                    int currentBit = (charValue >> i) & 1;
                    currentByte = (currentByte << 1) | currentBit;
                    bitIndex++;

                    if (bitIndex == 8)
                    {
                        result.Add((byte)currentByte);
                        currentByte = 0;
                        bitIndex = 0;
                    }
                }
            }

            return result.ToArray();
        }
    }

    // Base58 编码和解码实现
    public static class Base58
    {
        private const string Base58Chars = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

        public static string Encode(byte[] bytes)
        {
            var sb = new StringBuilder();
            var intData = bytes.Aggregate(0, (current, t) => current * 256 + t);

            while (intData > 0)
            {
                var remainder = intData % 58;
                intData /= 58;
                sb.Insert(0, Base58Chars[remainder]);
            }

            foreach (var b in bytes)
            {
                if (b == 0)
                    sb.Insert(0, Base58Chars[0]);
                else
                    break;
            }

            return sb.ToString();
        }

        public static byte[] Decode(string base58)
        {
            var intData = base58.Aggregate(0, (current, c) => current * 58 + Base58Chars.IndexOf(c));
            var result = new List<byte>();

            while (intData > 0)
            {
                var remainder = intData % 256;
                intData /= 256;
                result.Insert(0, (byte)remainder);
            }

            foreach (var c in base58)
            {
                if (c == Base58Chars[0])
                    result.Insert(0, 0);
                else
                    break;
            }

            return result.ToArray();
        }
    }

    // Base85 编码和解码实现
    public static class Base85
    {
        private const string Base85Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!#$%&()*+-;<=>?@^_`{|}~";

        public static string Encode(byte[] bytes)
        {
            var sb = new StringBuilder();
            var intData = bytes.Aggregate(0, (current, t) => current * 256 + t);

            while (intData > 0)
            {
                var remainder = intData % 85;
                intData /= 85;
                sb.Insert(0, Base85Chars[remainder]);
            }

            foreach (var b in bytes)
            {
                if (b == 0)
                    sb.Insert(0, Base85Chars[0]);
                else
                    break;
            }

            return sb.ToString();
        }

        public static byte[] Decode(string base85)
        {
            var intData = base85.Aggregate(0, (current, c) => current * 85 + Base85Chars.IndexOf(c));
            var result = new List<byte>();

            while (intData > 0)
            {
                var remainder = intData % 256;
                intData /= 256;
                result.Insert(0, (byte)remainder);
            }

            foreach (var c in base85)
            {
                if (c == Base85Chars[0])
                    result.Insert(0, 0);
                else
                    break;
            }

            return result.ToArray();
        }
    }

    // Quoted-Printable 编码和解码实现
    public static class QuotedPrintable
    {
        public static string Encode(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (var b in bytes)
            {
                if (b >= 33 && b <= 126 && b != 61) // 可打印字符且不是 '='
                {
                    sb.Append((char)b);
                }
                else
                {
                    sb.AppendFormat("={0:X2}", b);
                }
            }
            return sb.ToString();
        }

        public static byte[] Decode(string input)
        {
            var result = new List<byte>();
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '=')
                {
                    var hex = input.Substring(i + 1, 2);
                    result.Add(Convert.ToByte(hex, 16));
                    i += 2;
                }
                else
                {
                    result.Add((byte)input[i]);
                }
            }
            return result.ToArray();
        }
    }
}
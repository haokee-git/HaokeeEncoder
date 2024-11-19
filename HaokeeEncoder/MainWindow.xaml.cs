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
            this.Title = "Haokee Encoder"; // ���ô��ڱ���
            this.AppWindow.Resize(new Windows.Graphics.SizeInt32(350, 750));
            this.AppWindow.SetIcon(System.Environment.CurrentDirectory + "\\icon.ico");
        }

        private void CodingClick(object sender, RoutedEventArgs e)
        {
            // ��ȡѡ��Ĳ���
            var selectedOperation = (OperationSelector.SelectedItem as ComboBoxItem)?.Content.ToString();
            var selectedEncoding = (EncodingSelector.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (selectedOperation == "����")
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
                    case "Ħ˹����":
                        Output.Text = MorseEncode(Input.Text);
                        break;
                    case "Quoted-Printable":
                        Output.Text = QuotedPrintableEncode(Input.Text);
                        break;
                }
            }
            else if (selectedOperation == "����")
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
                        case "Ħ˹����":
                            Output.Text = MorseDecode(Input.Text);
                            break;
                        case "Quoted-Printable":
                            Output.Text = QuotedPrintableDecode(Input.Text);
                            break;
                    }
                }
                catch (FormatException)
                {
                    Output.Text = "��Ч�ı����ַ���";
                }
            }
        }

        private void SwapClick(object sender, RoutedEventArgs e)
        {
            // ��������������������
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
            PasteButton.Content = "�Ѹ���";
             
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, args) =>
            {
                PasteButton.Content = "�Ӽ��а�ճ��";
                timer.Stop(); // ֹͣ��ʱ��
            };
            timer.Start(); // ������ʱ��
        }

        private void CopyToClipboard(object sender, RoutedEventArgs e)
        {
            var dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
            dataPackage.SetText(Output.Text);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
            CopyButton.Content = "�Ѹ���";

            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, args) =>
            {
                CopyButton.Content = "���Ƶ����а�";
                timer.Stop(); // ֹͣ��ʱ��
            };
            timer.Start(); // ������ʱ��
        }

        private string Base32Encode(string input)
        {
            // ʵ�� Base32 ����
            var bytes = Encoding.UTF8.GetBytes(input);
            return Base32.ToBase32String(bytes);
        }

        private string Base32Decode(string input)
        {
            // ʵ�� Base32 ����
            var bytes = Base32.FromBase32String(input);
            return Encoding.UTF8.GetString(bytes);
        }

        private string Base58Encode(string input)
        {
            // ʵ�� Base58 ����
            var bytes = Encoding.UTF8.GetBytes(input);
            return Base58.Encode(bytes);
        }

        private string Base58Decode(string input)
        {
            // ʵ�� Base58 ����
            var bytes = Base58.Decode(input);
            return Encoding.UTF8.GetString(bytes);
        }

        private string Base85Encode(string input)
        {
            // ʵ�� Base85 ����
            var bytes = Encoding.UTF8.GetBytes(input);
            return Base85.Encode(bytes);
        }

        private string Base85Decode(string input)
        {
            // ʵ�� Base85 ����
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
            // ʵ�� Quoted-Printable ����
            var bytes = Encoding.UTF8.GetBytes(input);
            return QuotedPrintable.Encode(bytes);
        }

        private string QuotedPrintableDecode(string input)
        {
            // ʵ�� Quoted-Printable ����
            var bytes = QuotedPrintable.Decode(input);
            return Encoding.UTF8.GetString(bytes);
        }
    }

    // Base32 ����ͽ���ʵ��
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

    // Base58 ����ͽ���ʵ��
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

    // Base85 ����ͽ���ʵ��
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

    // Quoted-Printable ����ͽ���ʵ��
    public static class QuotedPrintable
    {
        public static string Encode(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (var b in bytes)
            {
                if (b >= 33 && b <= 126 && b != 61) // �ɴ�ӡ�ַ��Ҳ��� '='
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
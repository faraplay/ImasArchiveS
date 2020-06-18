using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImasArchiveApp
{
    /// <summary>
    /// Interaction logic for HexViewer.xaml
    /// </summary>
    public partial class HexViewer : UserControl
    {
        public HexViewer()
        {
            InitializeComponent();
            this.DataContext = this;
            SetHeaderText();
            _offset = 0;
            UpdateDataText();
        }

        private int _header_length = 16;
        private string _header_text;
        public int HeaderLength
        {
            get => _header_length;
            set
            {
                if (value > 0 && value <= 256)
                {
                    _header_length = value;
                    SetHeaderText();
                    UpdateDataText();
                }
            }
        }

        private void SetHeaderText()
        {
            _header_text = " Offset(h)  ";
            for (int i = 0; i < _header_length; i++)
            {
                _header_text += i.ToString("X2") + " ";
            }
            tbHeader.Text = _header_text;
        }

        private Stream _stream;
        public Stream Stream
        {
            get => _stream;
            set
            {
                _stream = value;
                _offset = 0;
                UpdateDataText();
            }
        }
        private readonly StringBuilder _data_text = new StringBuilder();
        private int _offset;
        private int _data_line_count;
        private byte[] _data_buffer;
        private int _buffer_size;
        private int _buffer_offset;
        private const int DefaultBufferSize = 0x10000;

        private void UpdateDataText()
        {
            if (_stream != null && _stream.CanRead && _stream.CanSeek && _offset >= 0 && _offset < _stream.Length)
            {
                int lineOffset = _offset;
                double lineHeight;
                if (!Double.IsNaN(tbData.LineHeight))
                    lineHeight = tbData.LineHeight;
                else
                    lineHeight = tbData.FontSize * tbData.FontFamily.LineSpacing;
                _data_line_count = (int)(tbData.ActualHeight / lineHeight) + 1;

                int totalBytes = _header_length * _data_line_count;
                if (_data_buffer == null || _buffer_size < 2 * totalBytes ||
                    _offset < _buffer_offset || _offset + totalBytes > _buffer_offset + _buffer_size)
                {
                    _buffer_size = (2 * totalBytes > DefaultBufferSize) ? 2 * totalBytes : DefaultBufferSize;
                    if (_data_buffer == null || _buffer_size > _data_buffer.Length)
                        _data_buffer = new byte[_buffer_size];
                    _buffer_offset = _offset - (_buffer_size / 2) + (totalBytes / 2);
                    if (_buffer_offset < 0)
                        _buffer_offset = 0;
                    _stream.Position = _buffer_offset;
                    _buffer_size = _stream.Read(_data_buffer);
                }

                _data_text.Clear();
                _data_text.Capacity = (totalBytes * 4 + 13 * _data_line_count);
                int index = 0;
                for (int h = 0; h < _data_line_count; h++)
                {
                    _data_text.Append(" ");
                    _data_text.Append(lineOffset.ToString("X8"));
                    _data_text.Append("   ");
                    for (int i = 0; i < _header_length; i++)
                    {
                        index = lineOffset + i - _buffer_offset;
                        if (index < _buffer_size)
                        {
                            byte b = _data_buffer[index];
                            _data_text.Append(b.ToString("X2"));
                            _data_text.Append(" ");
                        }
                        else
                        {
                            _data_text.Append("   ");
                        }
                    }
                    for (int i = 0; i < _header_length; i++)
                    {
                        index = lineOffset + i - _buffer_offset;
                        if (index >= _buffer_size)
                            break;
                        char c = Convert.ToChar(_data_buffer[index]);
                        _data_text.Append(Char.IsControl(c) ? '.' : c);
                    }
                    if (index >= _buffer_size)
                        break;
                    _data_text.Append('\n');
                    lineOffset += _header_length;
                }
            tbData.Text = _data_text.ToString();
            }
            else
            {
                tbData.Text = "            Could not read stream.";
            }
        }

        private const int MouseWheelDelta = 120;
        private const int MinFontSize = 8;
        private const int MaxFontSize = 72;
        private void tbData_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                double newFontSize = FontSize + (2 * (e.Delta / MouseWheelDelta));
                if (newFontSize >= MinFontSize && newFontSize <= MaxFontSize)
                {
                    FontSize = newFontSize;
                    UpdateDataText();
                }
            }
            else
            {
                if (_stream != null && _stream.CanRead && _stream.CanSeek)
                {
                    int newOffset = _offset + (_header_length * -(e.Delta / MouseWheelDelta));
                    if (newOffset < 0)
                        newOffset = 0;
                    if (newOffset < _stream.Length)
                        _offset = newOffset;
                    UpdateDataText();
                }
            }
        }

        private void hx_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateDataText();
        }
    }
}

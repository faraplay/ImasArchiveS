using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;

namespace ImasArchiveApp
{
    class HexViewModel : INotifyPropertyChanged
    {
        #region Fields
        private int _headerLength = 16;
        private string _headerText;
        private string _dataText;
        private Stream _stream;
        private int _offset = 0;
        private int _lineCount = 10;

        private byte[] _dataBuffer;
        private int _bufferSize;
        private int _bufferOffset;
        private int streamLength;
        private const int DefaultBufferSize = 0x10000;
        private StringBuilder dataStringBuilder = new StringBuilder();
        #endregion
        #region Properties
        public int HeaderLength
        {
            get => _headerLength;
            set
            {
                if (value > 0 && value < 256)
                {
                    _headerLength = value;
                    UpdateHeaderText();
                    UpdateDataText();
                    OnPropertyChanged();
                }
            }
        }
        public string HeaderText
        {
            get => _headerText;
            set
            {
                _headerText = value;
                OnPropertyChanged();
            }
        }
        public string DataText
        {
            get => _dataText;
            set
            {
                _dataText = value;
                OnPropertyChanged();
            }
        }
        public Stream Stream
        {
            get => _stream;
            set
            {
                _stream?.Dispose();
                _stream = value;
                streamLength = (int)(_stream == null ? 0 : _stream.Length);
                ClearBuffer();
                UpdateDataText();
                OnPropertyChanged();
            }
        }
        public int Offset
        {
            get => _offset;
            set
            {
                _offset = value;
                UpdateDataText();
                OnPropertyChanged();
            }
        }
        public int LineCount
        {
            get => _lineCount;
            set
            {
                if (value > 0 && value < 256 && value != _lineCount)
                {
                    _lineCount = value;
                    UpdateDataText();
                    OnPropertyChanged();
                }
            }
        }
        #endregion
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        #region Constructors
        public HexViewModel()
        {
            UpdateHeaderText();
            UpdateDataText();
        }
        #endregion
        #region Commands
        public void Scroll(object sender, MouseWheelEventArgs e)
        {
            if (_stream != null)
            {
                int delta = e.Delta / 120;
                int newOffset = _offset + (_headerLength * -delta);
                if (newOffset < 0)
                    newOffset = 0;
                if (newOffset < _stream.Length)
                    Offset = newOffset;
            }
        }
        #endregion
        #region RelayCommands
        private RelayCommand _updateDataTextCommand;
        public ICommand UpdateDataTextCommand
        {
            get
            {
                if (_updateDataTextCommand== null)
                {
                    _updateDataTextCommand = new RelayCommand(
                        param => UpdateDataText());
                }
                return _updateDataTextCommand;
            }
        }
        #endregion
        #region Methods
        private void UpdateHeaderText()
        {
            StringBuilder _headerStringBuilder = new StringBuilder(" Offset(h)  ");
            for (int i = 0; i < _headerLength; i++)
            {
                _headerStringBuilder.Append(i.ToString("X2") + " ");
            }
            HeaderText = _headerStringBuilder.ToString();
        }
        private void UpdateDataText()
        {
            if (_stream != null && _stream.CanRead && _stream.CanSeek && _offset >= 0 && _offset < _stream.Length)
            {

                int totalBytes = _headerLength * _lineCount;
                if (_dataBuffer == null || 
                    (_bufferSize < 2 * totalBytes && 2 * totalBytes < streamLength) ||
                    _offset < _bufferOffset || 
                    (_offset + totalBytes > _bufferOffset + _bufferSize && _bufferOffset + _bufferSize < streamLength)
                    )
                {
                    _bufferSize = (2 * totalBytes > DefaultBufferSize) ? 2 * totalBytes : DefaultBufferSize;
                    if (_dataBuffer == null || _bufferSize > _dataBuffer.Length)
                        _dataBuffer = new byte[_bufferSize];
                    _bufferOffset = _offset - (_bufferSize / 2) + (totalBytes / 2);
                    if (_bufferOffset < 0)
                        _bufferOffset = 0;
                    _stream.Position = _bufferOffset;
                    _bufferSize = _stream.Read(_dataBuffer);
                }

                dataStringBuilder.Clear();
                dataStringBuilder.Capacity = (totalBytes * 4 + 13 * _lineCount);
                int index = 0;
                int lineOffset = _offset;
                for (int h = 0; h < _lineCount; h++)
                {
                    dataStringBuilder.Append(" ");
                    dataStringBuilder.Append(lineOffset.ToString("X8"));
                    dataStringBuilder.Append("   ");
                    for (int i = 0; i < _headerLength; i++)
                    {
                        index = lineOffset + i - _bufferOffset;
                        if (index < _bufferSize)
                        {
                            byte b = _dataBuffer[index];
                            dataStringBuilder.Append(b.ToString("X2"));
                            dataStringBuilder.Append(" ");
                        }
                        else
                        {
                            dataStringBuilder.Append("   ");
                        }
                    }
                    for (int i = 0; i < _headerLength; i++)
                    {
                        index = lineOffset + i - _bufferOffset;
                        if (index >= _bufferSize)
                            break;
                        char c = Convert.ToChar(_dataBuffer[index]);
                        dataStringBuilder.Append(Char.IsControl(c) ? '.' : c);
                    }
                    if (index >= _bufferSize)
                        break;
                    dataStringBuilder.Append('\n');
                    lineOffset += _headerLength;
                }
                DataText = dataStringBuilder.ToString();
            }
            else
            {
                DataText = "            Could not read stream.";
            }
        }
        private void ClearBuffer()
        {
            _dataBuffer = null;
            _bufferOffset = 0;
            _bufferSize = 0;
            _offset = 0;
        }
        #endregion
    }
}

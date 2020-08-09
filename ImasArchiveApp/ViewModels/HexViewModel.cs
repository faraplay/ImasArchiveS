using System;
using System.IO;
using System.Text;
using System.Windows.Input;

namespace ImasArchiveApp
{
    public class HexViewModel : FileModel, IDisposable
    {
        #region Fields
        private int _headerLength = 16;
        private string _headerText;
        private string _dataText;
        private readonly Stream _stream;
        private int _offset = 0;
        private int _lineCount = 10;
        private HexViewerEncoding _encoding = HexViewerEncoding.Latin1;

        private byte[] _dataBuffer;
        private int _bufferSize;
        private int _bufferOffset;
        private const int DefaultBufferSize = 0x10000;
        private readonly StringBuilder dataStringBuilder = new StringBuilder();
        private int scrollDelta = 0;
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
        public HexViewerEncoding Encoding
        {
            get => _encoding;
            set
            {
                _encoding = value;
                UpdateDataText();
                OnPropertyChanged();
            }
        }
        #endregion
        #region Constructors
        public HexViewModel(IReport report, string fileName, Stream stream) : base(report, fileName)
        {
            _stream = stream;
            UpdateHeaderText();
            UpdateDataText();
        }
        internal static FileModelFactory.FileModelBuilder Builder { get; set; } =
            (report, filename, getFilename, stream) => new HexViewModel(report, filename, stream);
        #endregion
        #region IDisposable
        private bool disposed = false;
        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                _stream?.Dispose();
            }
            disposed = true;
            base.Dispose(disposing);
        }
        #endregion
        #region Commands
        public void Scroll(object sender, MouseWheelEventArgs e)
        {
            if (_stream != null)
            {
                scrollDelta += e.Delta;
                int delta = scrollDelta / 120;
                if (delta != 0)
                {
                    scrollDelta -= delta * 120;
                    int newOffset = _offset + (_headerLength * -delta);
                    if (newOffset < 0)
                        newOffset = 0;
                    if (newOffset < _stream.Length)
                        Offset = newOffset;
                }
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
        private RelayCommand _selectAsciiEncodingCommand;
        private RelayCommand _selectLatin1EncodingCommand;
        private RelayCommand _selectUTF16BEEncodingCommand;
        public ICommand SelectAsciiEncodingCommand
        {
            get
            {
                if (_selectAsciiEncodingCommand == null)
                {
                    _selectAsciiEncodingCommand = new RelayCommand(
                        _ => Encoding = HexViewerEncoding.ASCII);
                }
                return _selectAsciiEncodingCommand;
            }
        }
        public ICommand SelectLatin1EncodingCommand
        {
            get
            {
                if (_selectLatin1EncodingCommand == null)
                {
                    _selectLatin1EncodingCommand = new RelayCommand(
                        _ => Encoding = HexViewerEncoding.Latin1);
                }
                return _selectLatin1EncodingCommand;
            }
        }
        public ICommand SelectUTF16BEEncodingCommand
        {
            get
            {
                if (_selectUTF16BEEncodingCommand == null)
                {
                    _selectUTF16BEEncodingCommand = new RelayCommand(
                        _ => Encoding = HexViewerEncoding.UTF16BE);
                }
                return _selectUTF16BEEncodingCommand;
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
                    (_bufferSize < 2 * totalBytes && 2 * totalBytes < _stream.Length) ||
                    _offset < _bufferOffset || 
                    (_offset + totalBytes > _bufferOffset + _bufferSize && _bufferOffset + _bufferSize < _stream.Length)
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
                    switch (Encoding)
                    {
                        case HexViewerEncoding.ASCII:
                            for (int i = 0; i < _headerLength; i++)
                            {
                                index = lineOffset + i - _bufferOffset;
                                if (index >= _bufferSize)
                                    break;
                                char c = Convert.ToChar(_dataBuffer[index] & 0x7F);
                                dataStringBuilder.Append(Char.IsControl(c) ? '.' : c);
                            }
                            break;
                        case HexViewerEncoding.Latin1:
                            for (int i = 0; i < _headerLength; i++)
                            {
                                index = lineOffset + i - _bufferOffset;
                                if (index >= _bufferSize)
                                    break;
                                char c = Convert.ToChar(_dataBuffer[index]);
                                dataStringBuilder.Append(Char.IsControl(c) ? '.' : c);
                            }
                            break;
                        case HexViewerEncoding.UTF16BE:
                            for (int i = 0; i < _headerLength; i += 2)
                            {
                                index = lineOffset + i - _bufferOffset;
                                if (index >= _bufferSize)
                                    break;
                                char c = Convert.ToChar(_dataBuffer[index] * 0x100 + _dataBuffer[index + 1]);
                                dataStringBuilder.Append(Char.IsControl(c) ? '.' : c);
                            }
                            break;
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
        #endregion
    }
    public enum HexViewerEncoding
    {
        ASCII,
        Latin1,
        UTF16BE
    }
}

using Imas;
using Imas.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace ImasArchiveApp
{
    class UITextBoxModel : UITypedControlModel<TextBox>
    {
        #region Properties

        public byte TextAlpha
        {
            get => _control.textAlpha;
            set
            {
                _control.textAlpha = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public byte TextRed
        {
            get => _control.textRed;
            set
            {
                _control.textRed = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public byte TextGreen
        {
            get => _control.textGreen;
            set
            {
                _control.textGreen = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public byte TextBlue
        {
            get => _control.textBlue;
            set
            {
                _control.textBlue = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public uint TextAttributes
        {
            get => _control.textAttributes;
            set
            {
                _control.textAttributes = value;
				LoadImage();
                OnPropertyChanged();
                OnPropertyChanged(nameof(XAlignment));
                OnPropertyChanged(nameof(YAlignment));
                OnPropertyChanged(nameof(Multiline));
                OnPropertyChanged(nameof(WordWrap));
            }
        }
        public int XAlignment
        {
            get => (int)_control.XAlignment;
            set
            {
                _control.XAlignment = (HorizontalAlignment)value;
                LoadImage();
                OnPropertyChanged();
                OnPropertyChanged(nameof(TextAttributes));
            }
        }
        public int YAlignment
        {
            get => (int)_control.YAlignment / 4;
            set
            {
                _control.YAlignment = (VerticalAlignment)(value * 4);
                LoadImage();
                OnPropertyChanged();
                OnPropertyChanged(nameof(TextAttributes));
            }
        }
        public bool Multiline
        {
            get => _control.Multiline;
            set
            {
                _control.Multiline = value;
                LoadImage();
                OnPropertyChanged();
                OnPropertyChanged(nameof(TextAttributes));
            }
        }
        public bool WordWrap
        {
            get => _control.WordWrap;
            set
            {
                _control.WordWrap = value;
                LoadImage();
                OnPropertyChanged();
                OnPropertyChanged(nameof(TextAttributes));
            }
        }
        public string FontName
        {
            get => _control.FontName;
            set
            {
                _control.FontName = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public int CharLimit
        {
            get => _control.charLimit;
            set
            {
                _control.charLimit = value;
				LoadImage();
                OnPropertyChanged();
            }
        }
        public string Text
        {
            get => _control.Text;
            set
            {
                _control.Text = value;
				LoadImage();
                OnPropertyChanged();
                OnPropertyChanged(nameof(CharLimit));
            }
        }

        #endregion Properties
        public UITextBoxModel(UISubcomponentModel parent, TextBox control) : base(parent, control) { }
    }
}

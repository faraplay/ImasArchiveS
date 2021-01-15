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
                OnPropertyChanged();
            }
        }
        public byte TextRed
        {
            get => _control.textRed;
            set
            {
                _control.textRed = value;
                OnPropertyChanged();
            }
        }
        public byte TextGreen
        {
            get => _control.textGreen;
            set
            {
                _control.textGreen = value;
                OnPropertyChanged();
            }
        }
        public byte TextBlue
        {
            get => _control.textBlue;
            set
            {
                _control.textBlue = value;
                OnPropertyChanged();
            }
        }
        public int LineSpacing
        {
            get => _control.lineSpacing;
            set
            {
                _control.lineSpacing = value;
                OnPropertyChanged();
            }
        }
        public string FontName
        {
            get => _control.fontName;
            set
            {
                _control.fontName = value;
                OnPropertyChanged();
            }
        }
        public int CharLimit
        {
            get => _control.charLimit;
            set
            {
                _control.charLimit = value;
                OnPropertyChanged();
            }
        }
        public string Text
        {
            get => _control.text;
            set
            {
                _control.text = value;
                OnPropertyChanged();
            }
        }

        #endregion Properties
        public UITextBoxModel(UISubcomponentModel parent, TextBox control) : base(parent, control) { }
    }
}

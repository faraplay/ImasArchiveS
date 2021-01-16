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
        public int LineSpacing
        {
            get => _control.lineSpacing;
            set
            {
                _control.lineSpacing = value;
				LoadImage();
                OnPropertyChanged();
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
            }
        }

        #endregion Properties
        public UITextBoxModel(UISubcomponentModel parent, TextBox control) : base(parent, control) { }
    }
}

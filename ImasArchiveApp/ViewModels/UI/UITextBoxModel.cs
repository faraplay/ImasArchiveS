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
				LoadActiveImage();
                OnPropertyChanged();
            }
        }
        public byte TextRed
        {
            get => _control.textRed;
            set
            {
                _control.textRed = value;
				LoadActiveImage();
                OnPropertyChanged();
            }
        }
        public byte TextGreen
        {
            get => _control.textGreen;
            set
            {
                _control.textGreen = value;
				LoadActiveImage();
                OnPropertyChanged();
            }
        }
        public byte TextBlue
        {
            get => _control.textBlue;
            set
            {
                _control.textBlue = value;
				LoadActiveImage();
                OnPropertyChanged();
            }
        }
        public uint TextAttributes
        {
            get => _control.textAttributes;
            set
            {
                _control.textAttributes = value;
				LoadActiveImage();
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
                LoadActiveImage();
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
                LoadActiveImage();
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
                LoadActiveImage();
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
                LoadActiveImage();
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
				LoadActiveImage();
                OnPropertyChanged();
            }
        }
        public int CharLimit
        {
            get => _control.charLimit;
            set
            {
                _control.charLimit = value;
				LoadActiveImage();
                OnPropertyChanged();
            }
        }
        public string Text
        {
            get => _control.Text;
            set
            {
                _control.Text = value;
				LoadActiveImage();
                OnPropertyChanged();
                OnPropertyChanged(nameof(CharLimit));
            }
        }

        #endregion Properties

        public override bool? Visible
        {
            get => base.Visible;
            set
            {
                if (value.HasValue)
                    UIElement.myVisible = value.Value;
                base.Visible = value;
            }
        }
        public UITextBoxModel(UISubcomponentModel subcomponent, UIElementModel parent, TextBox control) : base(subcomponent, parent, control) { }
    }
}

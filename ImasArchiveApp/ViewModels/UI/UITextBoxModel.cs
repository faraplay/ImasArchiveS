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
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public byte TextRed
        {
            get => _control.textRed;
            set
            {
                _control.textRed = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public byte TextGreen
        {
            get => _control.textGreen;
            set
            {
                _control.textGreen = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public byte TextBlue
        {
            get => _control.textBlue;
            set
            {
                _control.textBlue = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public uint TextAttributes
        {
            get => _control.textAttributes;
            set
            {
                _control.textAttributes = value;
				LoadActiveImages();
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
                LoadActiveImages();
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
                LoadActiveImages();
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
                LoadActiveImages();
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
                LoadActiveImages();
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
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public int CharLimit
        {
            get => _control.charLimit;
            set
            {
                _control.charLimit = value;
				LoadActiveImages();
                OnPropertyChanged();
            }
        }
        public string Text
        {
            get => _control.Text;
            set
            {
                _control.Text = value;
				LoadActiveImages();
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

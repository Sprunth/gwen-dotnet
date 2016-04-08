using System;

namespace Gwen.Control
{
    /// <summary>
    /// Text box with masked text.
    /// </summary>
    /// <remarks>
    /// This class doesn't prevent programatic access to the text in any way.
    /// </remarks>
    public class TextBoxPassword : TextBox
    {
        private string m_Mask;

        /// <summary>
        /// Character used in place of actual characters for display.
        /// </summary>
        public char MaskCharacter { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextBoxPassword"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public TextBoxPassword(Base parent)
            : base(parent)
        {
            MaskCharacter = '*';
        }

        /// <summary>
        /// Handler for text changed event.
        /// </summary>
        protected override void OnTextChanged()
        {
            m_Mask = new String(MaskCharacter, Text.Length);
            TextOverride = m_Mask;
            base.OnTextChanged();
        }
    }
}
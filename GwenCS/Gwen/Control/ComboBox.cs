﻿using System;
using System.Drawing;
using System.Linq;
using Gwen.Control.EventArguments;
using Gwen.ControlInternal;

namespace Gwen.Control
{
    /// <summary>
    /// ComboBox control.
    /// </summary>
    public class ComboBox : Button
    {
        private readonly Menu m_Menu;
        private readonly Base m_Button;
        private MenuItem m_SelectedItem;

        /// <summary>
        /// Invoked when the selected item has changed.
        /// </summary>
        public event GwenEventHandler<ItemSelectedEventArgs> ItemSelected;

        /// <summary>
        /// Indicates whether the combo menu is open.
        /// </summary>
        public bool IsOpen => m_Menu != null && !m_Menu.IsHidden;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComboBox"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public ComboBox(Base parent)
            : base(parent)
        {
            SetSize(100, 20);
            m_Menu = new Menu(this);
            m_Menu.IsHidden = true;
            m_Menu.IconMarginDisabled = true;
            m_Menu.IsTabable = false;

            DownArrow arrow = new DownArrow(this);
            m_Button = arrow;

            Alignment = Pos.Left | Pos.CenterV;
            Text = String.Empty;
            Margin = new Margin(3, 0, 0, 0);

            IsTabable = true;
            KeyboardInputEnabled = true;
        }

        /// <summary>
        /// Selected item.
        /// </summary>
        /// <remarks>Not just String property, because items also have internal names.</remarks>
        public MenuItem SelectedItem
        {
            get { return m_SelectedItem; }
            set
            {
                if (value != null && value.Parent == m_Menu)
                {
                    m_SelectedItem = value;
                    OnItemSelected(m_SelectedItem, new ItemSelectedEventArgs(value));
                }
            }
        }

        internal override bool IsMenuComponent => true;

        /// <summary>
        /// Adds a new item.
        /// </summary>
        /// <param name="label">Item label (displayed).</param>
        /// <param name="name">Item name.</param>
        /// <returns>Newly created control.</returns>
        public virtual MenuItem AddItem(string label, string name = "", object UserData = null)
        {
            MenuItem item = m_Menu.AddItem(label, String.Empty);
            item.Name = name;
            item.Selected += OnItemSelected;
            item.UserData = UserData;

            if (m_SelectedItem == null)
                OnItemSelected(item, new ItemSelectedEventArgs(null));

            return item;
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.Base skin)
        {
            skin.DrawComboBox(this, IsDepressed, IsOpen);
        }

        public override void Disable()
        {
            base.Disable();
            GetCanvas().CloseMenus();
        }

        /// <summary>
        /// Internal Pressed implementation.
        /// </summary>
        protected override void OnClicked(int x, int y)
        {
            if (IsOpen)
            {
                GetCanvas().CloseMenus();
                return;
            }

            bool wasMenuHidden = m_Menu.IsHidden;

            GetCanvas().CloseMenus();

            if (wasMenuHidden)
            {
                Open();
            }

            base.OnClicked(x, y);
        }

        /// <summary>
        /// Removes all items.
        /// </summary>
        public virtual void DeleteAll()
        {
            m_Menu?.DeleteAll();
        }

        /// <summary>
        /// Internal handler for item selected event.
        /// </summary>
        /// <param name="control">Event source.</param>
        protected virtual void OnItemSelected(Base control, ItemSelectedEventArgs args)
        {
            if (!IsDisabled)
            {
                //Convert selected to a menu item
                MenuItem item = control as MenuItem;
                if (null == item) return;

                m_SelectedItem = item;
                Text = m_SelectedItem.Text;
                m_Menu.IsHidden = true;

                ItemSelected?.Invoke(this, args);

                Focus();
                Invalidate();
            }
        }

        /// <summary>
        /// Lays out the control's interior according to alignment, padding, dock etc.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Layout(Skin.Base skin)
        {
            m_Button.Position(Pos.Right | Pos.CenterV, 4, 0);
            base.Layout(skin);
        }

        /// <summary>
        /// Handler for losing keyboard focus.
        /// </summary>
        protected override void OnLostKeyboardFocus()
        {
            TextColor = Color.Black;
        }

        /// <summary>
        /// Handler for gaining keyboard focus.
        /// </summary>
        protected override void OnKeyboardFocus()
        {
            //Until we add the blue highlighting again
            TextColor = Color.Black;
        }

        /// <summary>
        /// Opens the combo.
        /// </summary>
        public virtual void Open()
        {
            if (!IsDisabled)
            {
                if (null == m_Menu) return;

                m_Menu.Parent = GetCanvas();
                m_Menu.IsHidden = false;
                m_Menu.BringToFront();

                Point p = LocalPosToCanvas(Point.Empty);

                m_Menu.SetBounds(new Rectangle(p.X, p.Y + Height, Width, m_Menu.Height));
            }
        }

        /// <summary>
        /// Closes the combo.
        /// </summary>
        public virtual void Close()
        {
            m_Menu?.Hide();
        }

        /// <summary>
        /// Handler for Down Arrow keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool OnKeyDown(bool down)
        {
            if (down)
            {
                var it = m_Menu.Children.FindIndex(x => x == m_SelectedItem);
                if (it + 1 < m_Menu.Children.Count)
                    OnItemSelected(this, new ItemSelectedEventArgs(m_Menu.Children[it + 1]));
            }
            return true;
        }

        /// <summary>
        /// Handler for Up Arrow keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool OnKeyUp(bool down)
        {
            if (down)
            {
                var it = m_Menu.Children.FindLastIndex(x => x == m_SelectedItem);
                if (it - 1 >= 0)
                    OnItemSelected(this, new ItemSelectedEventArgs(m_Menu.Children[it - 1]));
            }
            return true;
        }

        /// <summary>
        /// Renders the focus overlay.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void RenderFocus(Skin.Base skin)
        {
        }

        /// <summary>
        /// Selects the first menu item with the given text it finds. 
        /// If a menu item can not be found that matches input, nothing happens.
        /// </summary>
        /// <param name="text">The label to look for, this is what is shown to the user.</param>
        public void SelectByText(string text)
        {
            foreach (var item in m_Menu.Children.Cast<MenuItem>().Where(item => item.Text == text))
            {
                SelectedItem = item;
                return;
            }
        }

        /// <summary>
        /// Selects the first menu item with the given internal name it finds.
        /// If a menu item can not be found that matches input, nothing happens.
        /// </summary>
        /// <param name="name">The internal name to look for. To select by what is displayed to the user, use "SelectByText".</param>
        public void SelectByName(string name)
        {
            foreach (var item in m_Menu.Children.Cast<MenuItem>().Where(item => item.Name == name))
            {
                SelectedItem = item;
                return;
            }
        }

        /// <summary>
        /// Selects the first menu item with the given user data it finds.
        /// If a menu item can not be found that matches input, nothing happens.
        /// </summary>
        /// <param name="userdata">The UserData to look for. The equivalency check uses "param.Equals(item.UserData)".
        /// If null is passed in, it will look for null/unset UserData.</param>
        public void SelectByUserData(object userdata)
        {
            foreach (var item in m_Menu.Children.Cast<MenuItem>())
            {
                if (userdata == null)
                {
                    if (item.UserData == null)
                    {
                        SelectedItem = item;
                        return;
                    }
                }
                else if (userdata.Equals(item.UserData))
                {
                    SelectedItem = item;
                    return;
                }
            }
        }
    }
}
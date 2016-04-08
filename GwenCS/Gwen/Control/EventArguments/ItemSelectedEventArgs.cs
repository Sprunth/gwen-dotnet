﻿using System;

namespace Gwen.Control.EventArguments
{
    public class ItemSelectedEventArgs : EventArgs
    {
        public Base SelectedItem { get; private set; }

        internal ItemSelectedEventArgs(Base selecteditem)
        {
            this.SelectedItem = selecteditem;
        }
    }
}
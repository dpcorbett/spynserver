﻿using System;
using System.Windows.Input;


namespace WPFSpyn.ViewModel
{
    /// <summary>
    /// Represents an actionable item displayed by a View.
    /// </summary>
    public class CommandViewModel : ViewModelBase
    {

        #region Constructors

        /// <summary>
        /// Constructor taking the display name and command.
        /// </summary>
        /// <param name="displayName"></param>
        /// <param name="command"></param>
        public CommandViewModel(string displayName, ICommand command)
        {
            // Set display name and command.
            base.DisplayName = displayName;
            Command = command ?? throw new ArgumentNullException("command");
        }

        #endregion // Constructors


        #region Properties

        // Expose ICommand.
        public ICommand Command { get; private set; }

        #endregion // Properties

    }
}

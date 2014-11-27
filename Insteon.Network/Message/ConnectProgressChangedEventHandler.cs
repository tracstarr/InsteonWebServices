using System;

namespace Insteon.Network.Message
{
    /// <summary>
    /// Provides progress status reporting on the active connection operation.
    /// </summary>
    public class ConnectProgressChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the progress status reporting class.
        /// </summary>
        /// <param name="progressPercentage">The percentage of an asynchronous task that has been completed.</param>
        /// <param name="status">A display string indicating the current status of the operation.</param>
        public ConnectProgressChangedEventArgs(int progressPercentage, string status)
        {
            Cancel = false;
            ProgressPercentage = progressPercentage;
            Status = status;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the event should be canceled.
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets the asynchronous task progress percentage.
        /// </summary>
        public int ProgressPercentage { get; private set; }

        /// <summary>
        /// Gets a display string indicating the current status of the operation.
        /// </summary>
        public string Status { get; private set; }
    }

    /// <summary>
    /// Represents the method that handles a progress event.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="data">An object that contains the event data.</param>
    public delegate void ConnectProgressChangedEventHandler(object sender, ConnectProgressChangedEventArgs data);
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FaceWatcherEventArgs.cs" author="Jonathan Bauer (Based on the work of saviourofdp/affdexme-win)">
// Project-Code: Copyright (c) 2020 - Jonathan Bauer. All Rights Reserved
// Affdex SDK: Copyright (c) 2016 - Affectiva. All Rights Reserved
// </copyright>
// <summary>
// EventArgs concerning <see cref="FaceWatcher"/>-Events
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Affdex;
using System;
using System.Collections.Generic;

namespace InstantImprovement.SDKControl
{
    /// <summary>
    /// EventArgs concerning <see cref="FaceWatcher"/>-Events
    /// </summary>
    public class FaceWatcherEventArgs : EventArgs
    {
        /// <summary>
        /// Initialization of <see cref="FaceWatcherEventArgs"/> to pass on relevant properties
        /// </summary>
        /// <param name="faces">Detected Faces</param>
        /// <param name="frame">Captured Image-Frame</param>
        public FaceWatcherEventArgs(Dictionary<int, Face> faces, Frame frame)
        {
            Faces = faces;
            Frame = frame;
        }

        /// <summary>
        /// Dictionary of detected faces
        /// </summary>
        public Dictionary<int, Face> Faces { get; private set; }

        /// <summary>
        /// Captured Image-Frame
        /// </summary>
        public Frame Frame { get; private set; }
    }
}
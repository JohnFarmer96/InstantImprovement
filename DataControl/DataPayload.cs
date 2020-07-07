// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataPayload.cs" author="Jonathan Bauer (Based on the work of saviourofdp/affdexme-win)">
// Project-Code: Copyright (c) 2020 - Jonathan Bauer. All Rights Reserved
// Affdex SDK: Copyright (c) 2016 - Affectiva. All Rights Reserved
// </copyright>
// <summary>
// Data-Structure to store time-sensitive values (i.e. sensor-values)
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace InstantImprovement.DataControl
{
    /// <summary>
    /// Data-Structure to store time-sensitive values (i.e. sensor-values)
    /// </summary>
    public class DataPayload
    {
        /// <summary>
        /// Initialize <see cref="DataPayload"/> to store time-sensitive values
        /// </summary>
        /// <param name="value">Value to be stored</param>
        public DataPayload(float value, DateTime initTime)
        {
            Value = value;
            Timestamp = (DateTime.Now - initTime).TotalSeconds;
        }

        /// <summary>
        /// Relevant value
        /// </summary>
        public float Value { get; private set; }

        /// <summary>
        /// Timestamp belonging to certain <see cref="Value"/>
        /// </summary>
        public double Timestamp { get; private set; }
    }
}
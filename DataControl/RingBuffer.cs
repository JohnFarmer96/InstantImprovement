// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RingBuffer.cs" author="Jonathan Bauer (Based on the work of saviourofdp/affdexme-win)">
// Project-Code: Copyright (c) 2020 - Jonathan Bauer. All Rights Reserved
// Affdex SDK: Copyright (c) 2016 - Affectiva. All Rights Reserved
// </copyright>
// <summary>
// Data-Structure to store raw-values and ring-buffered-values
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using LiveCharts;
using System;
using System.Collections.Generic;

namespace InstantImprovement.DataControl
{
    /// <summary>
    /// Data-Structure to store raw-values <see cref="List{T}"/> and ring-buffered-values <see cref="ChartValues{T}"/>
    /// </summary>
    public class RingBuffer
    {
        /// <summary>
        /// Initialize <see cref="RingBuffer"/> to store raw-values <see cref="List{T}"/> and ring-buffered-values <see cref="ChartValues{T}"/>
        /// </summary>
        /// <param name="capacity">Max. number of values in Ring-Buffer</param>
        public RingBuffer(int capacity)
        {
            Values = new ChartValues<float>();
            RawData = new List<DataPayload>();
            Capacity = capacity;
            InitTime = DateTime.Now;
        }

        /// <summary>
        /// Max. number of values in Ring-Buffer
        /// </summary>
        public int Capacity { get; set; }

        /// <summary>
        /// Initialization Time to calculate time-span belonging to DataPayload
        /// </summary>
        public DateTime InitTime { get; private set; }

        /// <summary>
        /// Raw-Data List containing <see cref="DataPayload"/>
        /// </summary>
        public List<DataPayload> RawData { get; private set; }

        /// <summary>
        /// Ring-Buffered <see cref="ChartValues{T}"/>-List containing float-values
        /// </summary>
        public ChartValues<float> Values { get; private set; }
        /// <summary>
        /// Add new value to Ring-Buffer and Raw-Value-List
        /// </summary>
        /// <param name="newValue"></param>
        public void Add(float newValue)
        {
            if (Values.Count >= Capacity)
            {
                Values.RemoveAt(0);
            }

            Values.Add(newValue);
            RawData.Add(new DataPayload(newValue, InitTime));
        }

        /// <summary>
        /// Export Data to Excel-Worksheet
        /// </summary>
        /// <param name="worksheetName">Name of Worksheet</param>
        public void ExportData(string worksheetName)
        {
            XLSExporter.AddWorksheet(worksheetName);
            XLSExporter.WriteMetaToExcel(worksheetName, this);
        }
    }
}
﻿
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlTools.ServiceLayer.QueryExecution.Contracts;

namespace Microsoft.SqlTools.ServiceLayer.QueryExecution.DataStorage
{
    public class SaveAsCsvFileStreamWriter : SaveAsStreamWriter
    {

        #region Member Variables

        private bool headerWritten;

        #endregion

        public SaveAsCsvFileStreamWriter(IFileStreamWrapper fileWrapper, SaveResultsAsCsvRequestParams requestParams)
            : base(fileWrapper, requestParams)
        {
        }

        public override int WriteRow(IList<DbCellValue> row, IList<DbColumnWrapper> columns)
        {
            int bytesWritten = 0;

            // Write out the header if we haven't already and the user chose to have it
            if (saveParams.IncludeHeaders && !headerWritten)
            {
                // Build the string
                var selectedColumns = columns.Skip(columnStartIndex ?? 0).Take(columnCount ?? columns.Count)
                    .Select(c => EncodeCsvField(c.ColumnName) ?? string.Empty);
                string headerLine = string.Join(",", selectedColumns);

                // Encode it and write it out
                byte[] headerBytes = Encoding.Unicode.GetBytes(headerLine);
                bytesWritten += fileStream.WriteData(headerBytes, headerBytes.Length);

                headerWritten = true;
            }

            // Build the string for the row
            var selectedCells = row.Skip(columnStartIndex ?? 0)
                .Take(columnCount ?? columns.Count)
                .Select(c => EncodeCsvField(c.DisplayValue));
            string rowLine = string.Join(",", selectedCells);

            // Encode it and write it out
            byte[] rowBytes = Encoding.Unicode.GetBytes(rowLine);
            bytesWritten += fileStream.WriteData(rowBytes, rowBytes.Length);

            return bytesWritten;
        }

        /// <summary>
        /// Encodes a single field for inserting into a CSV record. The following rules are applied:
        /// <list type="bullet">
        /// <item><description>All double quotes (") are replaced with a pair of consecutive double quotes</description></item>
        /// </list>
        /// The entire field is also surrounded by a pair of double quotes if any of the following conditions are met:
        /// <list type="bullet">
        /// <item><description>The field begins or ends with a space</description></item>
        /// <item><description>The field begins or ends with a tab</description></item>
        /// <item><description>The field contains the ListSeparator string</description></item>
        /// <item><description>The field contains the '\n' character</description></item>
        /// <item><description>The field contains the '\r' character</description></item>
        /// <item><description>The field contains the '"' character</description></item>
        /// </list>
        /// </summary>
        /// <param name="field">The field to encode</param>
        /// <returns>The CSV encoded version of the original field</returns>
        internal static string EncodeCsvField(string field)
        {
            // Whether this field has special characters which require it to be embedded in quotes
            bool embedInQuotes = field.Contains(",\r\n\"")                          // Contains special characters
                                 || field.StartsWith(" ") || field.EndsWith(" ")    // Start/Ends with space
                                 || field.StartsWith("\t") || field.EndsWith("\t"); // Starts/Ends with tab

            //Replace all quotes in the original field with double quotes
            string ret = field.Replace("\"", "\"\"");

            if (embedInQuotes)
            {
                ret = $"\"{ret}\"";
            }

            return ret;
        }
        
    }
}
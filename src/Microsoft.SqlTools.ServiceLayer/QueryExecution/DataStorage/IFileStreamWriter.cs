﻿//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using System;

namespace Microsoft.SqlTools.ServiceLayer.QueryExecution.DataStorage
{
    /// <summary>
    /// Interface for a object that writes to a filesystem wrapper
    /// </summary>
    public interface IFileStreamWriter : IDisposable
    {
        int WriteRow(StorageDataReader dataReader);
        void FlushBuffer();
    }
}

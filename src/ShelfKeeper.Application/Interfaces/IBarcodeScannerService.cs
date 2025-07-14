// <copyright file="IBarcodeScannerService.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using ShelfKeeper.Application.Services.MediaItems.Models;

namespace ShelfKeeper.Application.Interfaces
{
    /// <summary>
    /// Defines the interface for a barcode scanning service.
    /// </summary>
    public interface IBarcodeScannerService
    {
        /// <summary>
        /// Scans a barcode asynchronously and returns a command to create a media item.
        /// </summary>
        /// <param name="barcode">The barcode string to scan.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a <see cref="CreateMediaItemCommand"/> populated with data from the barcode scan.</returns>
        Task<CreateMediaItemCommand> ScanBarcodeAsync(string barcode);
    }
}
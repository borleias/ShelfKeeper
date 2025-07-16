// <copyright file="FeatureType.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

namespace ShelfKeeper.Domain.Common
{
    /// <summary>
    /// Defines the different types of features that can be gated by subscription plans.
    /// </summary>
    public enum FeatureType
    {
        /// <summary>
        /// Represents the media item limit feature.
        /// </summary>
        MediaItemLimit = 0,

        /// <summary>
        /// Represents the shared lists feature.
        /// </summary>
        SharedLists = 1,

        /// <summary>
        /// Represents the advanced search feature.
        /// </summary>
        AdvancedSearch = 2,

        /// <summary>
        /// Represents the batch operations feature.
        /// </summary>
        BatchOperations = 3,

        /// <summary>
        /// Represents the CSV import/export feature.
        /// </summary>
        CsvImportExport = 4,
    }
}

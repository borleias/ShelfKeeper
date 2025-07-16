// <copyright file="FeatureGateAttribute.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using ShelfKeeper.Domain.Common;

namespace ShelfKeeper.WebApi.Filters
{
    /// <summary>
    /// An action filter attribute that checks if the authenticated user has access to a specific feature.
    /// </summary>
    public class FeatureGateAttribute : TypeFilterAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureGateAttribute"/> class.
        /// </summary>
        /// <param name="featureType">The type of feature to check access for.</param>
        public FeatureGateAttribute(FeatureType featureType) : base(typeof(FeatureGateActionFilter))
        {
            Arguments = new object[] { featureType };
        }
    }
}

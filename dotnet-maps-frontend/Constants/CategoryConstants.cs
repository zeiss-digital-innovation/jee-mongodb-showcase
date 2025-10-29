namespace DotNetMapsFrontend.Constants
{
    /// <summary>
    /// Central repository for POI category definitions and icon mappings.
    /// Compatible with JEE-Backend + Angular-Frontend reference implementation.
    /// </summary>
    public static class CategoryConstants
    {
        /// <summary>
        /// Default categories for fallback when backend does not provide a /categories endpoint.
        /// This list matches the JEE-Backend + Angular-Frontend reference implementation.
        /// All categories are lowercase for consistency.
        /// </summary>
        public static readonly List<string> DEFAULT_CATEGORIES = new()
        {
            "bank",
            "cash",
            "castle",
            "coffee",
            "company",
            "gasstation",
            "hotel",
            "landmark",
            "lodging",
            "museum",
            "parking",
            "pharmacy",
            "police",
            "post",
            "restaurant",
            "supermarket",
            "toilet"
        };

        /// <summary>
        /// Maps POI categories to Bootstrap Icons class names.
        /// Returns "bi-geo" as default fallback icon for unknown categories.
        /// </summary>
        /// <param name="category">Category name (case-insensitive)</param>
        /// <returns>Bootstrap icon class name (e.g., "bi-cup-hot")</returns>
        public static string GetCategoryIcon(string? category)
        {
            return category?.ToLower() switch
            {
                "bank" => "bi-bank",
                "cash" => "bi-credit-card",
                "castle" => "bi-building",
                "coffee" => "bi-cup-hot",
                "company" => "bi-building",
                "gasstation" => "bi-fuel-pump",
                "hotel" => "bi-house-door",
                "landmark" => "bi-geo-alt",
                "lodging" => "bi-house",
                "museum" => "bi-building-check",
                "parking" => "bi-car-front",
                "pharmacy" => "bi-capsule",
                "police" => "bi-shield-check",
                "post" => "bi-mailbox",
                "restaurant" => "bi-fork-knife",
                "supermarket" => "bi-shop",
                "toilet" => "bi-badge-wc",
                _ => "bi-geo"
            };
        }

        /// <summary>
        /// Returns the icon mapping as a dictionary for JavaScript serialization.
        /// Used to synchronize server-side and client-side icon rendering.
        /// </summary>
        public static Dictionary<string, string> GetIconMappingDictionary()
        {
            var mapping = new Dictionary<string, string>();
            foreach (var category in DEFAULT_CATEGORIES)
            {
                mapping[category] = GetCategoryIcon(category);
            }
            // Add fallback for unknown categories
            mapping["_default"] = "bi-geo";
            return mapping;
        }
    }
}

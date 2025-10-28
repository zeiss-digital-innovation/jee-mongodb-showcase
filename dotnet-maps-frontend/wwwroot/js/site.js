/**
 * Site-wide JavaScript functions for dotnet-maps-frontend
 * Provides text filtering functionality for POI lists and cards
 * Note: This is SEPARATE from category filtering (which is backend-based)
 */

// LocalStorage key for text filter
const TEXT_FILTER_STORAGE_KEY = 'poi_text_filter';

// Global function to apply text filter to POI cards, table rows, and map markers
function applyFilter(filterText) {
    const filter = filterText.toLowerCase().trim();
    
    // Filter Cards View (List page)
    $('#poiCardsContainer .col').each(function() {
        const $card = $(this);
        const category = $card.find('.card-title').text().toLowerCase();
        const details = $card.find('.card-text').text().toLowerCase();
        
        if (filter === '' || category.includes(filter) || details.includes(filter)) {
            $card.show();
        } else {
            $card.hide();
        }
    });
    
    // Filter Table View (List page)
    $('#poiTableBody tr').each(function() {
        const $row = $(this);
        const category = $row.find('.category-cell').text().toLowerCase();
        const details = $row.find('.details-cell').text().toLowerCase();
        
        if (filter === '' || category.includes(filter) || details.includes(filter)) {
            $row.show();
        } else {
            $row.hide();
        }
    });
    
    // Filter Map Markers (Map page)
    // Check if we're on the map page by checking for global variables
    if (typeof markersLayer !== 'undefined' && typeof pointsOfInterest !== 'undefined') {
        applyMapFilter(filter);
    }
    
    // Update visible count
    updateVisibleCount();
}

// Apply filter to map markers (Map page only)
function applyMapFilter(filter) {
    if (!markersLayer || !pointsOfInterest) {
        return;
    }
    
    // Clear all markers
    markersLayer.clearLayers();
    
    // Re-add only matching markers
    pointsOfInterest.forEach(function(poi) {
        const category = (poi.category || '').toLowerCase();
        const details = (poi.details || '').toLowerCase();
        
        // Show marker if filter is empty OR if category/details match
        if (filter === '' || category.includes(filter) || details.includes(filter)) {
            const coords = poi.location.coordinates;
            const lat = coords[1];
            const lng = coords[0];
            
            const marker = L.marker([lat, lng]);
            marker.bindPopup(getMarkerPopupFor(poi));
            markersLayer.addLayer(marker);
        }
    });
    
    console.log(`Map markers filtered: ${markersLayer.getLayers().length} visible`);
}

// Update count of visible POIs
function updateVisibleCount() {
    const visibleCards = $('#poiCardsContainer .col:visible').length;
    const visibleRows = $('#poiTableBody tr:visible').length;
    const count = Math.max(visibleCards, visibleRows);
    
    console.log(`Visible POIs after text filter: ${count}`);
}

// Setup filter input event listener with localStorage persistence
function setupPoiFilter() {
    const filterInput = $('#poiFilterInput');
    
    if (filterInput.length === 0) {
        return; // No filter input on this page
    }
    
    // Load saved filter value from localStorage
    const savedFilter = localStorage.getItem(TEXT_FILTER_STORAGE_KEY) || '';
    filterInput.val(savedFilter);
    
    // Apply initial filter if exists
    if (savedFilter) {
        console.log(`Restoring text filter: "${savedFilter}"`);
        applyFilter(savedFilter);
    }
    
    // Apply filter on input and save to localStorage
    filterInput.on('input', function() {
        const filterValue = $(this).val();
        localStorage.setItem(TEXT_FILTER_STORAGE_KEY, filterValue);
        applyFilter(filterValue);
    });
    
    // Clear filter button (if exists)
    $('#clearFilterBtn').on('click', function() {
        filterInput.val('');
        localStorage.removeItem(TEXT_FILTER_STORAGE_KEY);
        applyFilter('');
    });
    
    console.log('POI text filter initialized with localStorage persistence');
}

// Auto-initialize when DOM is ready
$(document).ready(function() {
    // Only setup if the filter input exists on the page
    if ($('#poiFilterInput').length > 0) {
        setupPoiFilter();
    }
});

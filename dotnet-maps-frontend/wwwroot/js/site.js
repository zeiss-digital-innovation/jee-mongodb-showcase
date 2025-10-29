/**
 * Site-wide JavaScript functions for dotnet-maps-frontend
 * Provides text filtering functionality for POI lists and cards
 * Note: This is SEPARATE from category filtering (which is backend-based)
 */

// LocalStorage keys for text filters
const NAME_FILTER_STORAGE_KEY = 'poi_name_filter';
const DETAILS_FILTER_STORAGE_KEY = 'poi_details_filter';

// Global function to apply both name and details filters
function applyFilters(nameFilter, detailsFilter) {
    const nameFilterLower = (nameFilter || '').toLowerCase().trim();
    const detailsFilterLower = (detailsFilter || '').toLowerCase().trim();
    
    // Filter Cards View (List page)
    $('#poiCardsContainer .col').each(function() {
        const $card = $(this);
        const name = $card.find('.card-title').text().toLowerCase();
        const details = $card.find('.card-text').text().toLowerCase();
        
        const nameMatch = nameFilterLower === '' || name.includes(nameFilterLower);
        const detailsMatch = detailsFilterLower === '' || details.includes(detailsFilterLower);
        
        if (nameMatch && detailsMatch) {
            $card.show();
        } else {
            $card.hide();
        }
    });
    
    // Filter Table View (List page)
    $('#poiTableBody tr').each(function() {
        const $row = $(this);
        const name = $row.find('td:first').text().toLowerCase(); // First column is Name
        const details = $row.find('.details-cell').text().toLowerCase();
        
        const nameMatch = nameFilterLower === '' || name.includes(nameFilterLower);
        const detailsMatch = detailsFilterLower === '' || details.includes(detailsFilterLower);
        
        if (nameMatch && detailsMatch) {
            $row.show();
        } else {
            $row.hide();
        }
    });
    
    // Filter Map Markers (Map page)
    if (typeof markersLayer !== 'undefined' && typeof pointsOfInterest !== 'undefined') {
        applyMapFilters(nameFilterLower, detailsFilterLower);
    }
    
    // Update visible count
    updateVisibleCount();
}

// Apply filters to map markers (Map page only)
function applyMapFilters(nameFilter, detailsFilter) {
    if (!markersLayer || !pointsOfInterest) {
        return;
    }
    
    // Clear all markers
    markersLayer.clearLayers();
    
    // Re-add only matching markers
    pointsOfInterest.forEach(function(poi) {
        const name = (poi.name || '').toLowerCase();
        const details = (poi.details || '').toLowerCase();
        
        const nameMatch = nameFilter === '' || name.includes(nameFilter);
        const detailsMatch = detailsFilter === '' || details.includes(detailsFilter);
        
        // Show marker only if both filters match
        if (nameMatch && detailsMatch) {
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

// Setup filter input event listeners with localStorage persistence
function setupPoiFilters() {
    const nameFilterInput = $('#poiNameFilterInput');
    const detailsFilterInput = $('#poiDetailsFilterInput');
    
    if (nameFilterInput.length === 0 && detailsFilterInput.length === 0) {
        return; // No filter inputs on this page
    }
    
    // Load saved filter values from localStorage
    const savedNameFilter = localStorage.getItem(NAME_FILTER_STORAGE_KEY) || '';
    const savedDetailsFilter = localStorage.getItem(DETAILS_FILTER_STORAGE_KEY) || '';
    
    nameFilterInput.val(savedNameFilter);
    detailsFilterInput.val(savedDetailsFilter);
    
    // Apply initial filters if they exist
    if (savedNameFilter || savedDetailsFilter) {
        console.log(`Restoring filters - Name: "${savedNameFilter}", Details: "${savedDetailsFilter}"`);
        applyFilters(savedNameFilter, savedDetailsFilter);
    }
    
    // Apply filters on input and save to localStorage
    function handleFilterChange() {
        const nameFilter = nameFilterInput.val();
        const detailsFilter = detailsFilterInput.val();
        
        localStorage.setItem(NAME_FILTER_STORAGE_KEY, nameFilter);
        localStorage.setItem(DETAILS_FILTER_STORAGE_KEY, detailsFilter);
        
        applyFilters(nameFilter, detailsFilter);
    }
    
    nameFilterInput.on('input', handleFilterChange);
    detailsFilterInput.on('input', handleFilterChange);
    
    console.log('POI text filters initialized with localStorage persistence');
}

// Auto-initialize when DOM is ready
$(document).ready(function() {
    // Only setup if the filter inputs exist on the page
    if ($('#poiNameFilterInput').length > 0 || $('#poiDetailsFilterInput').length > 0) {
        setupPoiFilters();
    }
});

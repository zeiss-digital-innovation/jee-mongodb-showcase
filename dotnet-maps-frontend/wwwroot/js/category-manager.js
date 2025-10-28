/**
 * Category Manager - Shared module for category filtering
 * Used across Map and List pages
 */

const CategoryManager = (function() {
    // Private variables
    let availableCategories = [];
    let selectedCategories = ['all'];
    const STORAGE_KEY = 'poi_selected_categories';
    
    /**
     * Initialize the category manager
     * Load categories from backend and restore selection from localStorage
     */
    async function init() {
        console.log('CategoryManager: Initializing...');
        
        // Load saved selection from localStorage
        const savedSelection = localStorage.getItem(STORAGE_KEY);
        if (savedSelection) {
            try {
                selectedCategories = JSON.parse(savedSelection);
                console.log('CategoryManager: Loaded saved selection:', selectedCategories);
            } catch (e) {
                console.warn('CategoryManager: Failed to parse saved selection:', e);
                selectedCategories = ['all'];
            }
        }
        
        // Fetch categories from backend
        await loadCategoriesFromBackend();
        
        // Setup event handlers
        setupEventHandlers();
        
        // Apply saved selection to UI
        applySelectionToUI();
        
        console.log('CategoryManager: Initialized successfully');
    }
    
    /**
     * Load categories from MongoDB backend
     */
    async function loadCategoriesFromBackend() {
        try {
            const response = await fetch('/api/categories');
            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }
            
            availableCategories = await response.json();
            console.log(`CategoryManager: Loaded ${availableCategories.length} categories from backend`);
            
            // Populate dropdown with categories
            populateCategoryDropdown();
            
        } catch (error) {
            console.error('CategoryManager: Failed to load categories:', error);
            
            // Show error in dropdown
            const menu = document.getElementById('categoryDropdownMenu');
            const loadingIndicator = document.getElementById('categoryLoadingIndicator');
            if (loadingIndicator) {
                loadingIndicator.innerHTML = '<span class="text-danger"><i class="bi bi-exclamation-triangle me-2"></i>Failed to load categories</span>';
            }
            
            // Fallback to empty array
            availableCategories = [];
        }
    }
    
    /**
     * Populate dropdown menu with category checkboxes
     */
    function populateCategoryDropdown() {
        const menu = document.getElementById('categoryDropdownMenu');
        const loadingIndicator = document.getElementById('categoryLoadingIndicator');
        
        if (!menu) {
            console.error('CategoryManager: Dropdown menu not found');
            return;
        }
        
        // Remove loading indicator
        if (loadingIndicator) {
            loadingIndicator.remove();
        }
        
        // Add category checkboxes after the divider
        availableCategories.forEach((category) => {
            const li = document.createElement('li');
            li.className = 'px-3 py-1';
            
            const checkboxId = `category-${category.replace(/\s+/g, '-')}`;
            
            li.innerHTML = `
                <div class="form-check">
                    <input class="form-check-input category-checkbox" type="checkbox" value="${category}" id="${checkboxId}">
                    <label class="form-check-label" for="${checkboxId}">
                        ${category}
                    </label>
                </div>
            `;
            
            menu.appendChild(li);
        });
        
        console.log(`CategoryManager: Added ${availableCategories.length} category checkboxes to dropdown`);
    }
    
    /**
     * Setup event handlers for category checkboxes
     */
    function setupEventHandlers() {
        // "All" checkbox handler
        const allCheckbox = document.querySelector('.category-checkbox-all');
        if (allCheckbox) {
            allCheckbox.addEventListener('change', function() {
                if (this.checked) {
                    // Deselect all individual categories
                    document.querySelectorAll('.category-checkbox').forEach(cb => {
                        cb.checked = false;
                    });
                    selectedCategories = ['all'];
                    updateDropdownLabel();
                    saveSelection();
                    notifySelectionChanged();
                }
            });
        }
        
        // Individual category checkboxes handler
        document.addEventListener('change', function(e) {
            if (e.target.classList.contains('category-checkbox')) {
                const allCheckbox = document.querySelector('.category-checkbox-all');
                
                // Deselect "All" when individual category is selected
                if (e.target.checked && allCheckbox) {
                    allCheckbox.checked = false;
                }
                
                // Update selected categories
                updateSelectedCategories();
                
                // If no categories selected, select "All"
                const hasSelection = document.querySelectorAll('.category-checkbox:checked').length > 0;
                if (!hasSelection && allCheckbox) {
                    allCheckbox.checked = true;
                    selectedCategories = ['all'];
                }
                
                updateDropdownLabel();
                saveSelection();
                notifySelectionChanged();
            }
        });
        
        // Prevent dropdown from closing when clicking checkboxes
        const dropdownMenu = document.getElementById('categoryDropdownMenu');
        if (dropdownMenu) {
            dropdownMenu.addEventListener('click', function(e) {
                // Only stop propagation for checkbox clicks
                if (e.target.classList.contains('form-check-input') || 
                    e.target.classList.contains('form-check-label')) {
                    e.stopPropagation();
                }
            });
        }
    }
    
    /**
     * Update selectedCategories array based on checked checkboxes
     */
    function updateSelectedCategories() {
        const checkedBoxes = document.querySelectorAll('.category-checkbox:checked');
        selectedCategories = Array.from(checkedBoxes).map(cb => cb.value);
        
        if (selectedCategories.length === 0) {
            selectedCategories = ['all'];
        }
        
        console.log('CategoryManager: Selection updated:', selectedCategories);
    }
    
    /**
     * Update dropdown label to show selected categories
     */
    function updateDropdownLabel() {
        const label = document.getElementById('categoryDropdownLabel');
        if (!label) return;
        
        const allCheckbox = document.querySelector('.category-checkbox-all');
        
        if (allCheckbox && allCheckbox.checked) {
            label.textContent = 'All Categories';
        } else if (selectedCategories.length === 0 || selectedCategories[0] === 'all') {
            label.textContent = 'All Categories';
        } else if (selectedCategories.length === 1) {
            label.textContent = selectedCategories[0];
        } else if (selectedCategories.length <= 3) {
            label.textContent = selectedCategories.join(', ');
        } else {
            label.textContent = `${selectedCategories.length} categories selected`;
        }
    }
    
    /**
     * Apply saved selection to UI checkboxes
     */
    function applySelectionToUI() {
        const allCheckbox = document.querySelector('.category-checkbox-all');
        
        if (selectedCategories.includes('all')) {
            if (allCheckbox) {
                allCheckbox.checked = true;
            }
            // Deselect all individual categories
            document.querySelectorAll('.category-checkbox').forEach(cb => {
                cb.checked = false;
            });
        } else {
            if (allCheckbox) {
                allCheckbox.checked = false;
            }
            // Select individual categories
            selectedCategories.forEach(category => {
                const checkbox = document.querySelector(`.category-checkbox[value="${category}"]`);
                if (checkbox) {
                    checkbox.checked = true;
                }
            });
        }
        
        updateDropdownLabel();
    }
    
    /**
     * Save selection to localStorage
     */
    function saveSelection() {
        localStorage.setItem(STORAGE_KEY, JSON.stringify(selectedCategories));
        console.log('CategoryManager: Selection saved to localStorage');
    }
    
    /**
     * Notify listeners that selection has changed
     */
    function notifySelectionChanged() {
        // Dispatch custom event
        const event = new CustomEvent('categorySelectionChanged', {
            detail: { categories: getSelectedCategories() }
        });
        document.dispatchEvent(event);
        
        console.log('CategoryManager: Selection changed event dispatched');
    }
    
    /**
     * Get currently selected categories
     * @returns {Array<string>} Selected categories (all available categories if 'all' is selected)
     */
    function getSelectedCategories() {
        const allCheckbox = document.querySelector('.category-checkbox-all');
        
        if (allCheckbox && allCheckbox.checked) {
            // Return all available categories
            return [...availableCategories];
        }
        
        if (selectedCategories.includes('all')) {
            return [...availableCategories];
        }
        
        return [...selectedCategories];
    }
    
    /**
     * Get all available categories
     * @returns {Array<string>} All categories loaded from backend
     */
    function getAllCategories() {
        return [...availableCategories];
    }
    
    /**
     * Check if "All" is selected
     * @returns {boolean} True if all categories are selected
     */
    function isAllSelected() {
        const allCheckbox = document.querySelector('.category-checkbox-all');
        return (allCheckbox && allCheckbox.checked) || selectedCategories.includes('all');
    }
    
    // Public API
    return {
        init,
        getSelectedCategories,
        getAllCategories,
        isAllSelected
    };
})();

// Export for use in other scripts
window.CategoryManager = CategoryManager;

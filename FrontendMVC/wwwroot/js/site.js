// Searchable Autocomplete Dropdown helper for School Management System

function makeSearchableDropdown(selectElement) {
    if (!selectElement || selectElement.style.display === 'none' || selectElement.dataset.searchableInit) return;

    selectElement.dataset.searchableInit = "true";

    // Create wrapper
    const wrapper = document.createElement('div');
    wrapper.className = 'autocomplete-wrapper';

    // Create trigger button
    const trigger = document.createElement('div');
    trigger.className = 'autocomplete-trigger';

    // Find current selected option or first option
    let selectedOption = selectElement.options[selectElement.selectedIndex];
    trigger.innerText = selectedOption ? selectedOption.text : '-- Chọn --';

    // Dropdown container
    const dropdown = document.createElement('div');
    dropdown.className = 'autocomplete-dropdown';

    // Search input
    const searchInput = document.createElement('input');
    searchInput.type = 'text';
    searchInput.className = 'autocomplete-search form-control';
    searchInput.placeholder = 'Tìm kiếm...';

    // Options list
    const optionsContainer = document.createElement('div');
    optionsContainer.className = 'autocomplete-options';

    // No results div
    const noResults = document.createElement('div');
    noResults.className = 'autocomplete-no-results';
    noResults.innerText = 'Không tìm thấy kết quả';
    noResults.style.display = 'none';

    dropdown.appendChild(searchInput);
    dropdown.appendChild(optionsContainer);
    dropdown.appendChild(noResults);

    // Rebuild options helper
    function rebuildOptions() {
        optionsContainer.innerHTML = '';
        const optionsList = Array.from(selectElement.options);

        optionsList.forEach((opt, index) => {
            const optionDiv = document.createElement('div');
            optionDiv.className = 'autocomplete-option';
            if (opt.value === selectElement.value) {
                optionDiv.classList.add('selected');
            }
            if (opt.disabled) {
                optionDiv.classList.add('disabled');
            }
            optionDiv.innerText = opt.text;
            optionDiv.dataset.value = opt.value;
            optionDiv.dataset.index = index;

            optionDiv.addEventListener('click', (e) => {
                if (opt.disabled) return;
                selectElement.value = opt.value;
                trigger.innerText = opt.text;

                // Remove selected class from others
                optionsContainer.querySelectorAll('.autocomplete-option').forEach(el => el.classList.remove('selected'));
                optionDiv.classList.add('selected');

                // Trigger change event
                const event = new Event('change', { bubbles: true });
                selectElement.dispatchEvent(event);

                closeDropdown();
            });
            optionsContainer.appendChild(optionDiv);
        });

        // Update trigger text
        let currOpt = selectElement.options[selectElement.selectedIndex];
        trigger.innerText = currOpt ? currOpt.text : '-- Chọn --';

        // Update trigger disabled state styles
        if (selectElement.disabled) {
            trigger.classList.add('disabled');
            trigger.style.backgroundColor = '#f1f5f9';
            trigger.style.cursor = 'not-allowed';
            trigger.style.color = '#94a3b8';
            trigger.style.borderColor = '#cbd5e1';
        } else {
            trigger.classList.remove('disabled');
            trigger.style.backgroundColor = '#ffffff';
            trigger.style.cursor = 'pointer';
            trigger.style.color = '';
            trigger.style.borderColor = '';
        }
    }

    rebuildOptions();

    // Toggle dropdown
    trigger.addEventListener('click', (e) => {
        if (selectElement.disabled) return;
        e.stopPropagation();
        const isOpen = dropdown.classList.contains('open');
        // Close all other dropdowns first
        document.querySelectorAll('.autocomplete-dropdown.open').forEach(el => {
            if (el !== dropdown) el.classList.remove('open');
        });

        if (isOpen) {
            closeDropdown();
        } else {
            openDropdown();
        }
    });

    function openDropdown() {
        dropdown.classList.add('open');
        searchInput.value = '';
        searchInput.focus();
        // Reset option display
        optionsContainer.querySelectorAll('.autocomplete-option').forEach(el => el.style.display = '');
        noResults.style.display = 'none';
    }

    function closeDropdown() {
        dropdown.classList.remove('open');
    }

    // Search filter
    searchInput.addEventListener('input', (e) => {
        const text = e.target.value.toLowerCase().trim();
        const options = optionsContainer.querySelectorAll('.autocomplete-option');
        let visibleCount = 0;

        options.forEach(optDiv => {
            const optText = optDiv.innerText.toLowerCase();
            if (optText.includes(text)) {
                optDiv.style.display = '';
                visibleCount++;
            } else {
                optDiv.style.display = 'none';
            }
        });

        if (visibleCount === 0) {
            noResults.style.display = '';
        } else {
            noResults.style.display = 'none';
        }
    });

    // Click outside
    document.addEventListener('click', (e) => {
        if (!wrapper.contains(e.target)) {
            closeDropdown();
        }
    });

    // Watch for dynamic changes in select options
    selectElement.refreshAutocomplete = rebuildOptions;

    // Insert in DOM
    selectElement.style.display = 'none';
    selectElement.parentNode.insertBefore(wrapper, selectElement.nextSibling);
    wrapper.appendChild(trigger);
    wrapper.appendChild(dropdown);
}

function initSearchableDropdowns() {
    document.querySelectorAll('select').forEach(select => {
        if (select.classList.contains('no-autocomplete') ||
            select.id === 'scheduleDay' ||
            select.id === 'schedulePeriod' ||
            select.name === 'Type' ||
            select.name === 'Gender' ||
            select.name === 'Role') {
            return;
        }
        makeSearchableDropdown(select);
    });
}

document.addEventListener('DOMContentLoaded', initSearchableDropdowns);

// ========================================================
// GLOBAL TOAST NOTIFICATION HELPER
// ========================================================
function showToast(type, message, title = '', duration = 4500) {
    if (!message) return;

    let container = document.getElementById('globalToastContainer');
    if (!container) {
        container = document.createElement('div');
        container.id = 'globalToastContainer';
        container.className = 'toast-container';
        document.body.appendChild(container);
    }

    const toast = document.createElement('div');
    toast.className = `toast-item toast-${type}`;

    let defaultTitle = '';
    let icon = 'ℹ️';
    if (type === 'success') {
        defaultTitle = title || 'Thành công';
        icon = '';
    } else if (type === 'error' || type === 'danger') {
        defaultTitle = title || 'Thông báo lỗi';
        icon = '❌';
    } else if (type === 'warning') {
        defaultTitle = title || 'Cảnh báo';
        icon = '';
    } else {
        defaultTitle = title || 'Thông báo';
        icon = 'ℹ️';
    }

    toast.innerHTML = `
        <div class="toast-icon">${icon}</div>
        <div class="toast-content">
            <div class="toast-title">${defaultTitle}</div>
            <div class="toast-message">${message}</div>
        </div>
        <button type="button" class="toast-close" title="Đóng">✕</button>
    `;

    container.appendChild(toast);

    // Trigger animation
    requestAnimationFrame(() => {
        toast.classList.add('show');
    });

    let isDismissed = false;
    function dismiss() {
        if (isDismissed) return;
        isDismissed = true;
        toast.classList.remove('show');
        toast.classList.add('hide');
        setTimeout(() => {
            toast.remove();
        }, 360);
    }

    const closeBtn = toast.querySelector('.toast-close');
    if (closeBtn) {
        closeBtn.addEventListener('click', dismiss);
    }

    if (duration > 0) {
        setTimeout(dismiss, duration);
    }
}

// Expose globally
window.showToast = showToast;
window.showMessage = showToast;


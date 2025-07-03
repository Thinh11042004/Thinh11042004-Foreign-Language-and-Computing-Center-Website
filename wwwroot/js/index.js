document.addEventListener("DOMContentLoaded", function () {
    console.log("User Management loaded");

    // Các elements cần thiết
    const searchInput = document.querySelector("input[name='searchTerm']");
    const roleSelect = document.querySelector("select[name='roleFilter']");
    const statusSelect = document.querySelector("select[name='statusFilter']");
    const filterForm = document.getElementById("filterForm");

    let typingTimer;
    const doneTypingInterval = 500;

    // Debug elements
    console.log("Elements found:", {
        searchInput: !!searchInput,
        roleSelect: !!roleSelect,
        statusSelect: !!statusSelect,
        filterForm: !!filterForm
    });

    // Function submit form filter
    function submitFilterForm() {
        console.log("submitFilterForm called");
        if (filterForm) {
            console.log("Submitting form...");
            filterForm.submit();
        } else {
            console.error("filterForm not found!");
        }
    }

    // Xử lý tìm kiếm với debounce
    if (searchInput) {
        searchInput.addEventListener("input", function () {
            console.log("Search input changed:", this.value);
            clearTimeout(typingTimer);
            typingTimer = setTimeout(() => {
                submitFilterForm();
            }, doneTypingInterval);
        });

        searchInput.addEventListener("keydown", function () {
            clearTimeout(typingTimer);
        });

        // Xử lý Enter key
        searchInput.addEventListener("keypress", function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                clearTimeout(typingTimer);
                submitFilterForm();
            }
        });
    }

    // Xử lý thay đổi role filter
    if (roleSelect) {
        roleSelect.addEventListener("change", function () {
            console.log("Role filter changed to:", this.value);
            submitFilterForm();
        });
        console.log("Role filter event listener added");
    } else {
        console.error("roleSelect not found!");
    }

    // Xử lý thay đổi status filter
    if (statusSelect) {
        statusSelect.addEventListener("change", function () {
            console.log("Status filter changed to:", this.value);
            submitFilterForm();
        });
        console.log("Status filter event listener added");
    } else {
        console.error("statusSelect not found!");
    }

    // Thêm nút reset filter
    function addResetButton() {
        const filterForm = document.getElementById('filterForm');
        if (filterForm) {
            const resetBtn = document.createElement('button');
            resetBtn.type = 'button';
            resetBtn.className = 'btn btn-outline-secondary btn-lg ms-2';
            resetBtn.innerHTML = '<i class="fas fa-undo"></i> Reset';
            resetBtn.onclick = function () {
                if (searchInput) searchInput.value = '';
                if (roleSelect) roleSelect.value = '';
                if (statusSelect) statusSelect.value = '';
                submitFilterForm();
            };

            const submitBtn = filterForm.querySelector('button[type="submit"]');
            if (submitBtn && submitBtn.parentNode) {
                submitBtn.parentNode.appendChild(resetBtn);
            }
        }
    }

    // Gọi addResetButton
    addResetButton();

    document.addEventListener('click', function (e) {
        const deleteBtn = e.target.closest('.delete-user-btn');
        if (!deleteBtn) return;

        const userId = deleteBtn.dataset.userId;
        const userName = deleteBtn.dataset.userName;

        console.log('Delete button clicked:', { userId, userName });

        deleteUser(userId, userName);
    });

    // Function cập nhật số lượng người dùng
    function updateUserCounts() {
        const visibleRows = document.querySelectorAll('table tbody tr:not([style*="display: none"])');
        const countBadge = document.querySelector('.badge.bg-primary');
        if (countBadge) {
            countBadge.textContent = `${visibleRows.length} người dùng`;
        }
    }

    // Function hiển thị thông báo
    

    // Test function
    window.testFilter = function () {
        console.log("=== TESTING FILTER ===");
        console.log("Role select element:", roleSelect);
        console.log("Role select value:", roleSelect?.value);
        console.log("Form element:", filterForm);

        if (roleSelect) {
            console.log("Manually triggering role change...");
            roleSelect.dispatchEvent(new Event('change'));
        }
    };

    console.log("User Management JavaScript initialized successfully");
});
window.deleteUser = function (userId, userName) {
    if (!userId || !userName) {
        alert('Thông tin người dùng không hợp lệ');
        return;
    }

    const confirmMessage = `Bạn có chắc chắn muốn xóa người dùng "${userName}"?\n\nHành động này không thể hoàn tác!`;

    if (confirm(confirmMessage)) {
        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        if (!token) {
            alert('Lỗi bảo mật: Không tìm thấy token xác thực');
            return;
        }

        // Hiển thị loading
        const deleteBtn = event.target.closest('button');
        const originalText = deleteBtn.innerHTML;
        deleteBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i>';
        deleteBtn.disabled = true;

        const formData = new FormData();
        formData.append('id', userId);

        fetch('/UserManagement/Delete', {
            method: 'POST',
            headers: {
                'RequestVerificationToken': token.value
            },
            body: formData
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }
                return response.json();
            })
            .then(data => {
                if (data.success) {
                    const row = deleteBtn.closest('tr');
                    if (row) {
                        row.remove();
                    }

                    location.reload();
                    showNotification('Xóa người dùng thành công!', 'success');
                } else {
                    throw new Error(data.message || 'Có lỗi xảy ra khi xóa người dùng');
                }
            })
            .catch(error => {
                console.error('Delete error:', error);
                alert('Lỗi: ' + error.message);

                deleteBtn.innerHTML = originalText;
                deleteBtn.disabled = false;
            });
    }
};
function showNotification(message, type = 'info') {
    // Tạo toast notification
    const toast = document.createElement('div');
    toast.className = `alert alert-${type} alert-dismissible fade show position-fixed`;
    toast.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
    toast.innerHTML = `
                    ${message}
                    <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
                `;

    document.body.appendChild(toast);

    // Tự động ẩn sau 3 giây
    setTimeout(() => {
        if (toast.parentNode) {
            toast.remove();
        }
    }, 3000);
}
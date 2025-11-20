/**
 * Staff Page Manager - Handles page-specific JavaScript initialization for SPA navigation
 */
window.StaffPageManager = (function () {
    let currentPageCleanup = null;
    let tableStatusSignalRConnection = null;
    let orderSignalRConnection = null;

    const pages = {
        "/Staff/StatusTable": {
            init: initStatusTablePage,
            cleanup: cleanupStatusTablePage,
        },
        "/Staff/Menu": {
            init: initMenuPage,
            cleanup: cleanupMenuPage,
        },
        "/Staff/CustomerRequests": {
            init: initCustomerRequestPage,
            cleanup: cleanupCustomerRequestPage,
        },
        "/Staff/Profile": {
            init: initProfilePage,
            cleanup: cleanupProfilePage,
        },
    };

    function init() {
        console.log("StaffPageManager initialized");
    }

    function reinitializePage(pathname) {
        console.log("Reinitializing page:", pathname);

        // Cleanup previous page
        if (currentPageCleanup) {
            console.log("Cleaning up previous page");
            currentPageCleanup();
            currentPageCleanup = null;
        }

        // Find matching page handler
        const pageHandler = pages[pathname];
        if (pageHandler && pageHandler.init) {
            console.log("Initializing page:", pathname);
            currentPageCleanup = pageHandler.init();
        } else {
            console.log("No specific handler for page:", pathname);
        }
    }

    // StatusTable page handlers
    function initStatusTablePage() {
        console.log("Initializing StatusTable page");

        function getTableStatusClass(status) {
            switch (status) {
                case 2:
                    return "table-status-occupied";
                case 1:
                    return "table-status-available";
                case 4:
                    return "table-status-cleaning";
                case 3:
                    return "table-status-reserved";
                default:
                    return "";
            }
        }

        // Initialize SignalR connection for table status if not exists
        if (
            !tableStatusSignalRConnection ||
            tableStatusSignalRConnection.state !== signalR.HubConnectionState.Connected
        ) {
            console.log("Creating new SignalR connection for StatusTable...");
            tableStatusSignalRConnection = new signalR.HubConnectionBuilder()
                .withUrl("https://localhost:7294/tableStatusHub", {
                    skipNegotiation: false,
                    withCredentials: true
                })
                .configureLogging(signalR.LogLevel.Debug)
                .withAutomaticReconnect()
                .build();

            async function startTableStatusSignalR() {
                try {
                    await tableStatusSignalRConnection.start();
                    console.log("SignalR Connected to table status hub successfully!");
                } catch (err) {
                    console.error("SignalR table status connection error:", err);
                    setTimeout(startTableStatusSignalR, 5000);
                }
            }

            tableStatusSignalRConnection.onclose(async () => {
                console.log("SignalR table status connection closed, attempting to reconnect...");
                await startTableStatusSignalR();
            });

            startTableStatusSignalR();
        }

        // Always re-register event handlers when page loads
        tableStatusSignalRConnection.off("ReceiveTableStatusUpdate");
        tableStatusSignalRConnection.off("ReceiveTableStatusList");

        // Real-time update handler for single table
        tableStatusSignalRConnection.on("ReceiveTableStatusUpdate", function (updatedTable) {
            console.log("Received SignalR update:", updatedTable);
            const tableCard = document.getElementById("table-" + updatedTable.id);

            if (tableCard) {
                console.log("Found table card, updating...");
                const statusDiv = tableCard.querySelector(".table-card-status");
                statusDiv.textContent = updatedTable.tableStatus.name;

                // Remove all status classes and add new one
                tableCard.className = "table-card";
                const newStatusClass = getTableStatusClass(
                    updatedTable.tableStatus.id
                );
                if (newStatusClass) {
                    tableCard.classList.add(newStatusClass);
                }
                console.log("Table card updated successfully");
            } else {
                console.warn(
                    "Table card not found for ID:",
                    "table-" + updatedTable.id
                );
            }
        });

        // Real-time update handler for table list (when order is confirmed)
        tableStatusSignalRConnection.on("ReceiveTableStatusList", function (tableList) {
            console.log("Received table status list update:", tableList);
            // Update all table cards based on the new list
            tableList.forEach(function(table) {
                const tableCard = document.getElementById("table-" + table.id);
                if (tableCard) {
                    const statusDiv = tableCard.querySelector(".table-card-status");
                    statusDiv.textContent = table.tableStatus.name;

                    // Remove all status classes and add new one
                    tableCard.className = "table-card";
                    const newStatusClass = getTableStatusClass(table.tableStatus.id);
                    if (newStatusClass) {
                        tableCard.classList.add(newStatusClass);
                    }
                }
            });
        });

        let activeTableId = null;

        function showModal(tableId, tableNumber) {
            const statusModal = document.getElementById("statusModal");
            const modalTableName = document.getElementById("modalTableName");
            if (statusModal && modalTableName) {
                activeTableId = tableId;
                modalTableName.textContent = `Select status for Table ${tableNumber}`;
                statusModal.style.display = "flex";
            }
        }

        function hideModal() {
            const statusModal = document.getElementById("statusModal");
            if (statusModal) {
                activeTableId = null;
                statusModal.style.display = "none";
            }
        }

        // Attach table click listeners
        const tableClickHandler = function () {
            const tableId = this.dataset.tableId;
            const tableNumber = this.querySelector(".table-card-number").textContent;
            console.log("Table clicked:", tableId, tableNumber);
            showModal(tableId, tableNumber);
        };

        document.querySelectorAll(".table-card").forEach((card) => {
            card.addEventListener("click", tableClickHandler);
        });

        // Modal event listeners
        const statusClickHandler = function (e) {
            if (e.target.classList.contains("status-option")) {
                const newStatusId = parseInt(e.target.dataset.statusId);
                console.log(
                    "Status option clicked:",
                    newStatusId,
                    "for table:",
                    activeTableId
                );

                if (activeTableId && tableStatusSignalRConnection) {
                    console.log("Sending UpdateTableStatus via SignalR...");
                    tableStatusSignalRConnection
                        .invoke("UpdateTableStatus", parseInt(activeTableId), newStatusId)
                        .then(() => {
                            console.log("UpdateTableStatus sent successfully");
                        })
                        .catch((err) => console.error("SignalR invoke error:", err));
                }
                hideModal();
            }
        };

        const statusOptionsContainer = document.querySelector(".status-options");
        if (statusOptionsContainer) {
            statusOptionsContainer.addEventListener("click", statusClickHandler);
        }

        const closeModalBtn = document.getElementById("closeModalBtn");
        if (closeModalBtn) {
            closeModalBtn.addEventListener("click", hideModal);
        }

        const statusModal = document.getElementById("statusModal");
        const modalClickHandler = function (e) {
            if (e.target === statusModal) {
                hideModal();
            }
        };
        if (statusModal) {
            statusModal.addEventListener("click", modalClickHandler);
        }

        // Return cleanup function
        return function () {
            console.log("Cleaning up StatusTable page");
            document.querySelectorAll(".table-card").forEach((card) => {
                card.removeEventListener("click", tableClickHandler);
            });
            if (statusOptionsContainer) {
                statusOptionsContainer.removeEventListener("click", statusClickHandler);
            }
            if (closeModalBtn) {
                closeModalBtn.removeEventListener("click", hideModal);
            }
            if (statusModal) {
                statusModal.removeEventListener("click", modalClickHandler);
            }
        };
    }

    function cleanupStatusTablePage() {
        console.log("Status table cleanup completed");
    }

    // Menu page handlers
    function initMenuPage() {
        console.log("Initializing Menu page");

        // =====================
        // 🔍 Search functionality
        // =====================
        const searchHandler = function (e) {
            const searchTerm = e.target.value.toLowerCase();
            const dishCards = document.querySelectorAll(".menu-dish-card-staff");
            const sections = document.querySelectorAll(".menu-section-staff");

            dishCards.forEach((card) => {
                const dishName = card.dataset.dishName;
                const matches = dishName.includes(searchTerm);
                card.style.display = matches ? "flex" : "none";
            });

            // Hide sections with no visible dishes
            sections.forEach((section) => {
                const visibleDishes = section.querySelectorAll(
                    '.menu-dish-card-staff[style="display: flex"], .menu-dish-card-staff:not([style*="display: none"])'
                );
                const hasVisibleDishes = Array.from(visibleDishes).some(
                    (dish) => !dish.style.display || dish.style.display === "flex"
                );
                section.style.display = hasVisibleDishes ? "block" : "none";
            });
        };

        const searchInput = document.getElementById("searchInput");
        if (searchInput) {
            searchInput.addEventListener("input", searchHandler);
        }

        // =====================
        // 🍽️ Availability toggle functionality
        // =====================
        const toggleHandlers = [];
        document.querySelectorAll(".availability-toggle").forEach((toggle) => {
            const toggleHandler = async function () {
                const dishId = toggle.dataset.dishId;
                const isCurrentlyActive = !toggle.classList.contains("off");

                try {
                    console.log("Updating dish availability:", {
                        dishId,
                        isActive: !isCurrentlyActive,
                    });

                    // GỌI API — sửa cho đúng ngữ cảnh backend của bạn
                    const apiUrl = "https://localhost:7294/api/Staff/dish-availability";

                    const response = await fetch(apiUrl, {
                        method: "PUT",
                        headers: {
                            "Content-Type": "application/json",
                            Accept: "application/json",
                        },
                        body: JSON.stringify({
                            dishId: parseInt(dishId),
                            isActive: !isCurrentlyActive,
                        }),
                    });

                    console.log("Response status:", response.status);

                    // Xử lý kết quả an toàn hơn
                    let result;
                    try {
                        result = await response.json();
                    } catch {
                        throw new Error("Invalid JSON response from server");
                    }

                    console.log("Response result:", result);

                    if (response.ok && result.success) {
                        // ✅ Cập nhật UI thành công
                        toggle.classList.toggle("off");
                        const icon = toggle.querySelector("i");
                        const label = toggle.querySelector("span");

                        if (toggle.classList.contains("off")) {
                            icon.className = "fa-solid fa-toggle-off";
                            label.textContent = "Out of Stock";
                        } else {
                            icon.className = "fa-solid fa-toggle-on";
                            label.textContent = "Available";
                        }
                    } else {
                        console.error("Failed to update dish availability:", result.message);
                        alert(
                            "Failed to update dish availability: " +
                            (result.message || "Please try again.")
                        );
                    }
                } catch (error) {
                    console.error("Error updating dish availability:", error);
                    alert("An error occurred. Please try again.");
                }
            };

            toggle.addEventListener("click", toggleHandler);
            toggleHandlers.push({ element: toggle, handler: toggleHandler });
        });

        // =====================
        // 🧹 Cleanup
        // =====================
        return function () {
            console.log("Cleaning up Menu page");
            if (searchInput) {
                searchInput.removeEventListener("input", searchHandler);
            }
            toggleHandlers.forEach(({ element, handler }) => {
                element.removeEventListener("click", handler);
            });
        };
    }


    function cleanupMenuPage() {
        console.log("Menu cleanup completed");
    }

    // Customer Request page handlers
    function initCustomerRequestPage() {
        console.log("Initializing CustomerRequest page");

        let currentOrderData = null;
        let hasChanges = false;

        // Always reload data from DOM for SPA navigation
        let ordersData = [];
        const dataScript = document.getElementById("customer-request-data");
        if (dataScript) {
            try {
                ordersData = JSON.parse(dataScript.textContent);
                window.customerRequestData = ordersData; // update global for modal functions
            } catch (e) {
                console.warn("Could not parse customer request data:", e);
            }
        }

        // --- SignalR real-time for new order ---
        if (!orderSignalRConnection || orderSignalRConnection.state !== signalR.HubConnectionState.Connected) {
            console.log("Creating new SignalR connection for Orders...");
            orderSignalRConnection = new signalR.HubConnectionBuilder()
                .withUrl("https://localhost:7294/signalrServer", {
                    skipNegotiation: false,
                    withCredentials: true
                })
                .configureLogging(signalR.LogLevel.Debug)
                .withAutomaticReconnect()
                .build();

            async function startOrderSignalR() {
                try {
                    await orderSignalRConnection.start();
                    console.log("SignalR Connected to order hub successfully!");
                } catch (err) {
                    console.error("SignalR order connection error:", err);
                    setTimeout(startOrderSignalR, 5000);
                }
            }

            orderSignalRConnection.onclose(async () => {
                console.log("SignalR order connection closed, attempting to reconnect...");
                await startOrderSignalR();
            });

            startOrderSignalR();
        }
        const signalRConnection = orderSignalRConnection;
        if (signalRConnection) {
            signalRConnection.off("ReceiveOrderList");
            signalRConnection.on("ReceiveOrderList", function (orders) {
                console.log("Received full order list via SignalR:", orders);
                ordersData = orders;
                window.customerRequestData = ordersData;
                renderOrderList();
            });
            signalRConnection.off("ReceiveNewOrder");
            signalRConnection.on("ReceiveNewOrder", function (order) {
                console.log("Received new order via SignalR:", order);
                ordersData.unshift(order);
                window.customerRequestData = ordersData;
                renderOrderList();
            });
        }

        // Render lại danh sách order (gọi lại khi có order mới)
        function renderOrderList() {
            const listDiv = document.querySelector(".request-list");
            if (!listDiv) return;
            let html = "";
            if (ordersData && ordersData.length > 0) {
                ordersData.forEach((order) => {
                    html += `<div class="request-card ${order.orderTime &&
                            new Date(order.orderTime).getTime() + 10 * 60 * 1000 > Date.now()
                            ? "attention"
                            : ""
                        }">
                    <div class="request-header">
                        <div class="request-info">
                            <span class="table-number">Table ${order.tableNumber
                        }</span>
                            <span class="customer-name">${order.customerName
                        }</span>
                            <span class="customer-phone">${order.customerPhone
                        }</span>
                            <span class="request-time">${formatTimeAgo(
                            order.orderTime
                        )}</span>
                        </div>
                        <div class="order-status">
                            <div class="status-badges">
                                <span class="status-badge status-${order.orderStatus
                            .toLowerCase()
                            .replace(/ /g, "-")}">${order.orderStatus}</span>
                                ${!order.isPaid ? '<span class="payment-badge payment-unpaid">UNPAID</span>' : ''}
                            </div>
                            <span class="total-amount">${order.totalAmount.toLocaleString(
                                "en-US",
                                { style: "currency", currency: "USD" }
                            )}</span>
                        </div>
                    </div>
                    <div class="request-content">
                        <div class="order-summary">
                            ${order.orderDetails &&
                            order.orderDetails.length > 0
                            ? `${order.orderDetails.length} items ordered`
                            : "No items in this order"
                        }
                        </div>
                    </div>
                    <div class="request-actions">
                        <button class="btn-action btn-view" onclick="showOrderDetails('${order.id
                        }')">
                            <i class="ti ti-eye"></i> View Details
                        </button>
                        ${order.orderDetails && order.orderDetails.length > 0
                            ? `<button class="btn-action btn-edit" onclick="editOrderDetails('${order.id}')"><i class="ti ti-edit"></i> Edit Items</button>`
                            : ""
                        }
                        ${order.orderStatusId === 4 && !order.isPaid
                            ? `<button class="btn-action btn-checkout" onclick="showCheckoutModal('${order.id}', ${Math.round(order.totalAmount)})" data-order-id="${order.id}" data-order-status="${order.orderStatusId}" data-is-paid="${order.isPaid}">
                                <i class="ti ti-cash"></i> Checkout
                               </button>`
                            : order.orderStatusId < 4
                                ? `<button class="btn-action btn-confirm" onclick="confirmOrder('${order.id}', ${order.orderStatusId})" data-order-id="${order.id}" data-current-status="${order.orderStatusId}">
                                    <i class="ti ti-arrow-right"></i> ${getNextStatusButtonText(order.orderStatusId)}
                                   </button>`
                                : ""
                        }
                    </div>
                </div>`;
                });
            } else {
                html = `<div class="no-orders">
                <i class="ti ti-clipboard-list"></i>
                <h3>No customer orders found</h3>
                <p>There are currently no active orders to display.</p>
            </div>`;
            }
            listDiv.innerHTML = html;
        }

        function getNextStatusButtonText(currentStatusId) {
            const buttonTexts = {
                1: "Start Preparing",   // Pending -> Preparing
                2: "Mark as Ready",     // Preparing -> Ready
                3: "Complete Order",    // Ready -> Completed
            };
            return buttonTexts[currentStatusId] || "Update Status";
        }

        function formatTimeAgo(orderTime) {
            if (!orderTime) return "";
            const orderDate = new Date(orderTime);
            const now = new Date();
            const diffMs = now - orderDate;
            const diffMin = Math.floor(diffMs / 60000);
            const diffHr = Math.floor(diffMin / 60);
            if (diffMin < 60) return `${diffMin} minutes ago`;
            if (diffHr < 24) return `${diffHr} hours ago`;
            return orderDate.toLocaleString();
        }

        window.showOrderDetails = function (orderId) {
            const order = ordersData.find((o) => o.id === orderId);
            if (!order) return;

            currentOrderData = order;
            hasChanges = false;

            document.getElementById(
                "modalTitle"
            ).textContent = `Order Details - Table ${order.tableNumber}`;

            let content = `
                <div class="order-info" style="margin-bottom: 20px; padding: 15px; background: #f8f9fa; border-radius: 6px;">
                    <h4 style="margin: 0 0 10px 0;">Customer Information</h4>
                    <p><strong>Name:</strong> ${order.customerName}</p>
                    <p><strong>Phone:</strong> ${order.customerPhone}</p>
                    <p><strong>Table:</strong> ${order.tableNumber}</p>
                    <p><strong>Status:</strong> ${order.orderStatus}</p>
                    <p><strong>Order Time:</strong> ${new Date(
                order.orderTime
            ).toLocaleString()}</p>
                    <p><strong>Total Amount:</strong> ${order.totalAmount.toLocaleString(
                "en-US",
                { style: "currency", currency: "USD" }
            )}</p>
                </div>
                <h4>Order Items:</h4>
            `;

            if (order.orderDetails && order.orderDetails.length > 0) {
                order.orderDetails.forEach((detail) => {
                    content += `
                        <div class="order-detail-item">
                            <div class="order-detail-info">
                                <div class="dish-name">${detail.dishName}</div>
                                <div class="dish-details">
                                    Quantity: ${detail.quantity} | 
                                    Price: ${detail.price.toLocaleString(
                        "en-US",
                        { style: "currency", currency: "USD" }
                    )} | 
                                    Subtotal: ${detail.subTotal.toLocaleString(
                        "en-US",
                        { style: "currency", currency: "USD" }
                    )}
                                </div>
                            </div>
                            <div class="order-detail-actions">
                                <span style="margin-right: 10px;">${detail.isActive ? "Active" : "Inactive"
                        }</span>
                            </div>
                        </div>
                    `;
                });
            } else {
                content += "<p>No items in this order.</p>";
            }

            document.getElementById("orderDetailsContent").innerHTML = content;
            document.getElementById("saveChangesBtn").style.display = "none";
            document.getElementById("orderDetailsModal").style.display = "block";
        };

        window.editOrderDetails = function (orderId) {
            const order = ordersData.find((o) => o.id === orderId);
            if (!order) return;

            currentOrderData = order;
            hasChanges = false;

            document.getElementById(
                "modalTitle"
            ).textContent = `Edit Order Items - Table ${order.tableNumber}`;

            let content = `
                <div class="order-info" style="margin-bottom: 20px; padding: 15px; background: #f8f9fa; border-radius: 6px;">
                    <h4 style="margin: 0 0 10px 0;">Customer Information</h4>
                    <p><strong>Name:</strong> ${order.customerName}</p>
                    <p><strong>Phone:</strong> ${order.customerPhone}</p>
                    <p><strong>Table:</strong> ${order.tableNumber}</p>
                    <p><strong>Status:</strong> ${order.orderStatus}</p>
                    <p><strong>Order Time:</strong> ${new Date(
                order.orderTime
            ).toLocaleString()}</p>
                </div>
                <h4>Order Items:</h4>
                <p style="color: #6c757d; margin-bottom: 15px;">Toggle items on/off to include/exclude them from the order</p>
            `;

            if (order.orderDetails && order.orderDetails.length > 0) {
                order.orderDetails.forEach((detail) => {
                    content += `
                        <div class="order-detail-item">
                            <div class="order-detail-info">
                                <div class="dish-name">${detail.dishName}</div>
                                <div class="dish-details">
                                    Quantity: ${detail.quantity} | 
                                    Price: ${detail.price.toLocaleString(
                        "en-US",
                        { style: "currency", currency: "USD" }
                    )} | 
                                    Subtotal: ${detail.subTotal.toLocaleString(
                        "en-US",
                        { style: "currency", currency: "USD" }
                    )}
                                </div>
                            </div>
                            <div class="order-detail-actions">
                                <label class="toggle-switch">
                                    <input type="checkbox" 
                                           ${detail.isActive ? "checked" : ""} 
                                           onchange="toggleOrderDetail('${detail.id
                        }', this.checked)"
                                           data-detail-id="${detail.id}">
                                    <span class="slider"></span>
                                </label>
                            </div>
                        </div>
                    `;
                });
            } else {
                content += "<p>No items in this order.</p>";
            }

            document.getElementById("orderDetailsContent").innerHTML = content;
            document.getElementById("saveChangesBtn").style.display = "inline-block";
            document.getElementById("orderDetailsModal").style.display = "block";
        };

        window.toggleOrderDetail = function (orderDetailId, isActive) {
            hasChanges = true;

            // Update the current order data
            const detail = currentOrderData.orderDetails.find(
                (d) => d.id === orderDetailId
            );
            if (detail) {
                detail.isActive = isActive;
            }
        };
        window.saveOrderDetailsChanges = async function () {
            if (!hasChanges) {
                closeModal();
                return;
            }

            const saveButton = document.getElementById("saveChangesBtn");
            saveButton.disabled = true;
            saveButton.textContent = "Saving...";

            try {
                const apiBaseUrl = "https://localhost:7294/api/Staff/order-detail-status";

                const promises = currentOrderData.orderDetails.map(async (detail) => {
                    const requestData = {
                        orderDetailId: detail.id,
                        isActive: detail.isActive
                    };

                    const response = await fetch(apiBaseUrl, {
                        method: "PUT",
                        headers: {
                            "Content-Type": "application/json",
                        },
                        body: JSON.stringify(requestData),
                    });

                    const text = await response.text();
                    let result = null;

                    try {
                        result = JSON.parse(text);
                    } catch {
                        console.warn("Non-JSON response:", text);
                    }

                    if (!response.ok || !result?.success) {
                        throw new Error(`Failed to update ${detail.dishName}`);
                    }

                    return result;
                });

                const results = await Promise.allSettled(promises);

                const failed = results.filter(r => r.status === "rejected");
                if (failed.length === 0) {
                    alert("✅ All changes saved successfully!");
                    location.reload();
                } else {
                    alert(`⚠️ Some updates failed: ${failed.length} item(s). Please try again.`);
                    console.error(failed);
                }

            } catch (error) {
                console.error("Error saving changes:", error);
                alert("❌ An error occurred while saving changes. Please try again.");
            } finally {
                saveButton.disabled = false;
                saveButton.textContent = "Save Changes";
            }
        };


        window.closeModal = function () {
            document.getElementById("orderDetailsModal").style.display = "none";
            currentOrderData = null;
            hasChanges = false;
        };

        // Change Order Status - Move to next status
        window.confirmOrder = async function (orderId, currentStatusId) {
            const statusTransitions = {
                1: { current: "Pending", next: "Preparing", action: "start preparing" },
                2: { current: "Preparing", next: "Ready", action: "mark as ready" },
                3: { current: "Ready", next: "Completed", action: "complete" }
            };

            const transition = statusTransitions[currentStatusId];
            if (!transition) {
                alert("Cannot change this order status.");
                return;
            }

            const confirmMessage = `Are you sure you want to ${transition.action} this order?\n\nStatus will change: ${transition.current} → ${transition.next}`;
            if (!confirm(confirmMessage)) {
                return;
            }

            const button = document.querySelector(`.btn-confirm[data-order-id="${orderId}"]`);
            if (button) {
                button.disabled = true;
                button.innerHTML = '<i class="ti ti-loader"></i> Processing...';
            }

            try {
                const apiBaseUrl = "https://localhost:7294/api/Staff/order-status";
                const newStatusId = currentStatusId + 1; // Move to next status

                const response = await fetch(apiBaseUrl, {
                    method: "PUT",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({
                        orderId: orderId,
                        newStatusId: newStatusId
                    }),
                });

                const result = await response.json();

                if (response.ok && result.success) {
                    console.log(`✅ Order status changed successfully to ${transition.next}`);
                    // Wait a bit for SignalR to broadcast, then reload manually as backup
                    setTimeout(() => {
                        // If SignalR hasn't updated yet (button still disabled), force reload
                        const checkButton = document.querySelector(`.btn-confirm[data-order-id="${orderId}"]`);
                        if (checkButton && checkButton.disabled) {
                            console.log("SignalR update delayed, reloading data manually...");
                            location.reload();
                        }
                    }, 2000);
                } else {
                    alert(`⚠️ Failed to update order status: ${result.message || "Please try again."}`);
                    if (button) {
                        button.disabled = false;
                        button.innerHTML = `<i class="ti ti-arrow-right"></i> ${getNextStatusButtonText(currentStatusId)}`;
                    }
                }
            } catch (error) {
                console.error("Error updating order status:", error);
                alert("❌ An error occurred. Please try again.");
                if (button) {
                    button.disabled = false;
                    button.innerHTML = `<i class="ti ti-arrow-right"></i> ${getNextStatusButtonText(currentStatusId)}`;
                }
            }
        };

        // Payment checkout functionality
        let currentPaymentData = null;
        let paymentCheckInterval = null;

        window.showCheckoutModal = async function(orderId, amount) {
            console.log("showCheckoutModal called with orderId:", orderId, "amount:", amount);

            try {
                console.log("Calling payment API...");

                // Call API to create payment link
                const response = await fetch("https://localhost:7294/api/Payment/create", {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify({
                        orderId: orderId,
                        description: `Payment for order`
                    })
                });

                console.log("Payment API response status:", response.status);
                const result = await response.json();
                console.log("Payment API result:", result);

                if (response.ok && result.success) {
                    currentPaymentData = result.data;

                    // Redirect to PayOS checkout page
                    console.log("Redirecting to PayOS checkout URL:", result.data.checkoutUrl);
                    window.open(result.data.checkoutUrl, '_blank');

                    // Start polling payment status in background
                    startPaymentStatusPolling(result.data.orderCode, orderId);
                } else {
                    console.error("Payment creation failed:", result);
                    alert(`❌ Failed to generate payment: ${result.message || "Please try again."}`);
                }
            } catch (error) {
                console.error("Error creating payment:", error);
                alert(`❌ An error occurred: ${error.message}`);
            }
        };

        function startPaymentStatusPolling(orderCode, orderId) {
            // Clear any existing interval
            if (paymentCheckInterval) {
                clearInterval(paymentCheckInterval);
            }

            // Check payment status every 3 seconds
            paymentCheckInterval = setInterval(async () => {
                try {
                    const response = await fetch(`https://localhost:7294/api/Payment/${orderCode}`);
                    const result = await response.json();

                    if (response.ok && result.success) {
                        const paymentStatus = result.data.status;

                        if (paymentStatus === "PAID" || paymentStatus === "COMPLETED") {
                            // Payment successful!
                            clearInterval(paymentCheckInterval);
                            await handlePaymentSuccess(orderId);
                        } else if (paymentStatus === "CANCELLED" || paymentStatus === "EXPIRED") {
                            clearInterval(paymentCheckInterval);
                            document.getElementById("paymentInfo").innerHTML = '<p style="color: #e74c3c; font-size: 1.2em;">❌ Payment cancelled or expired</p>';
                        }
                    }
                } catch (error) {
                    console.error("Error checking payment status:", error);
                }
            }, 3000); // Check every 3 seconds
        }

        async function handlePaymentSuccess(orderId) {
            // Clear the polling interval
            if (paymentCheckInterval) {
                clearInterval(paymentCheckInterval);
                paymentCheckInterval = null;
            }

            // Show success notification
            alert('✅ Payment completed successfully! The page will refresh to update the order list.');

            // Reload to fetch updated order list (now marked as PAID)
            location.reload();
        }

        window.closePaymentModal = function() {
            const modal = document.getElementById("paymentQRModal");
            if (modal) {
                modal.style.display = "none";
            }

            // Clear payment check interval
            if (paymentCheckInterval) {
                clearInterval(paymentCheckInterval);
                paymentCheckInterval = null;
            }

            currentPaymentData = null;
        };

        // Modal click outside handler
        const modalClickHandler = function (event) {
            const orderModal = document.getElementById("orderDetailsModal");

            if (event.target === orderModal) {
                window.closeModal();
            }
        };
        window.addEventListener("click", modalClickHandler);

        // Return cleanup function
        return function () {
            console.log("Cleaning up CustomerRequest page");
            window.removeEventListener("click", modalClickHandler);

            // Clear payment polling interval if active
            if (paymentCheckInterval) {
                clearInterval(paymentCheckInterval);
                paymentCheckInterval = null;
            }

            // Clean up global functions
            delete window.showOrderDetails;
            delete window.editOrderDetails;
            delete window.toggleOrderDetail;
            delete window.saveOrderDetailsChanges;
            delete window.closeModal;
            delete window.confirmOrder;
            delete window.showCheckoutModal;
            delete window.closePaymentModal;
        };
    }

    function cleanupCustomerRequestPage() {
        console.log("Customer request cleanup completed");
    }

    // Profile page handlers
    function initProfilePage() {
        console.log("Initializing Profile page");

        // Profile page specific initialization can go here
        return function () {
            console.log("Cleaning up Profile page");
        };
    }

    function cleanupProfilePage() {
        console.log("Profile cleanup completed");
    }

    // Public API
    return {
        init: init,
        reinitializePage: reinitializePage,
    };
})();

// Initialize the manager when DOM is ready
if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", window.StaffPageManager.init);
} else {
    window.StaffPageManager.init();
}

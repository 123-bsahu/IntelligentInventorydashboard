// Intelligent Inventory Dashboard - vanilla JS client.
// Chassis number is the vehicle identifier throughout (no VIN anywhere).

'use strict';

var INVENTORY_URL = '/api/vehicles/inventory';
var CREATE_URL = '/api/vehicles/create';
var REQUEST_UPDATE_URL = '/api/vehicles/request-update';
var SEARCH_URL = '/api/vehicles/search?query=';

var A_STOCK_AGED = 90; // days before "Price Reduction Recommended" appears server-side
var AGED_DAYS = 120;   // threshold for "Auction / Dealer Transfer Recommended"

// ---------- helpers ----------

async function fetchJson(url, options) {
    var response = await fetch(url, options);
    var data = null;
    var text = await response.text();
    if (text) {
        try { data = JSON.parse(text); } catch (e) { data = null; }
    }
    return { ok: response.ok, status: response.status, data: data };
}

function postJson(url, body) {
    return fetchJson(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(body)
    });
}

function setMessage(element, text, isError) {
    element.textContent = text;
    element.classList.toggle('is-error', Boolean(isError));
}

function formatPrice(value) {
    var number = Number(value);
    return isNaN(number) ? '' : number.toFixed(2);
}

// ---------- vehicle submission form ----------

function initVehicleForm() {
    var form = document.getElementById('vehicle-form');
    var message = document.getElementById('form-message');

    form.addEventListener('submit', async function (event) {
        event.preventDefault();
        setMessage(message, 'Submitting...', false);

        var payload = {
            chassisNumber: form.chassisNumber.value.trim(),
            make: form.make.value.trim(),
            model: form.model.value.trim(),
            year: Number(form.year.value),
            price: Number(form.price.value),
            arrivalDate: form.arrivalDate.value
        };

        try {
            var result = await postJson(CREATE_URL, payload);

            if (result.ok) {
                setMessage(message, 'Vehicle ' + payload.chassisNumber + ' submitted successfully (id ' + result.data.id + ').', false);
                form.reset();
                await loadInventory();
            } else if (result.status === 409) {
                setMessage(message, (result.data && result.data.message) || 'Duplicate chassis number.', true);
            } else if (result.status === 400 && result.data && result.data.errors) {
                var details = [];
                Object.keys(result.data.errors).forEach(function (field) {
                    details.push(field + ': ' + result.data.errors[field].join(' '));
                });
                setMessage(message, 'Validation failed. ' + details.join(' | '), true);
            } else {
                setMessage(message, 'Submission failed (HTTP ' + result.status + ').', true);
            }
        } catch (err) {
            setMessage(message, 'Network error: ' + err.message, true);
        }
    });
}

function initFormCollapse() {
    var button = document.getElementById('toggle-form-btn');
    var container = document.getElementById('form-container');

    if (!button || !container) return;

    button.addEventListener('click', function () {
        var isCollapsed = container.hidden;

        container.hidden = !isCollapsed;
        button.textContent = isCollapsed ? 'Collapse' : 'Expand';
        button.setAttribute('aria-expanded', String(isCollapsed));
    });
}

// ---------- stock list ----------

async function loadInventory() {
    var tbody = document.getElementById('inventory-body');
    var tableMessage = document.getElementById('table-message');

    try {
        var result = await fetchJson(INVENTORY_URL);

        if (!result.ok) {
            tbody.textContent = '';
            setMessage(tableMessage, 'Failed to load inventory (HTTP ' + result.status + ').', true);
            return;
        }

        var vehicles = Array.isArray(result.data) ? result.data : [];
        tbody.textContent = ''; // clears rows; safe against leftover content

        if (vehicles.length === 0) {
            var emptyRow = document.createElement('tr');
            var emptyCell = document.createElement('td');
            emptyCell.colSpan = 10;
            emptyCell.textContent = 'No vehicles in stock.';
            emptyRow.appendChild(emptyCell);
            tbody.appendChild(emptyRow);
            setMessage(tableMessage, '', false);
            return;
        }

        vehicles.forEach(function (vehicle) {
            tbody.appendChild(buildRow(vehicle));
        });
        setMessage(tableMessage, vehicles.length + ' vehicle(s) listed.', false);
    } catch (err) {
        tbody.textContent = '';
        setMessage(tableMessage, 'Network error: ' + err.message, true);
    }
}

function buildRow(vehicle) {
    var tr = document.createElement('tr');
    var chassis = vehicle.chassisNumber === null || vehicle.chassisNumber === undefined ? '' : String(vehicle.chassisNumber);

    [
        chassis,
        textOr(vehicle.make),
        textOr(vehicle.model),
        vehicle.year,
        formatPrice(vehicle.price),
        vehicle.arrivalDate,
        vehicle.daysInStock,
        textOr(vehicle.status),
        textOr(vehicle.recommendation)
    ].forEach(function (value) {
        var td = document.createElement('td');
        td.textContent = value;
        tr.appendChild(td);
    });

    var actionCell = document.createElement('td');

    // Only show the Update Price button if days in stock are greater than 90
    if (Number(vehicle.daysInStock) > 90) {
        var actionButton = document.createElement('button');
        actionButton.type = 'button';
        actionButton.className = 'price-update';
        actionButton.textContent = 'Update Price';
        actionButton.setAttribute('aria-label', 'Request price update for ' + chassis);
        actionButton.addEventListener('click', function () { requestPriceUpdate(vehicle); });
        actionCell.appendChild(actionButton);
    } else {
        actionCell.textContent = '-';
    }

    tr.appendChild(actionCell);

    return tr;
}

function textOr(value) {
    return value === null || value === undefined ? '' : String(value);
}

// Send a maker price update request; refresh the stock list afterwards.
async function requestPriceUpdate(vehicle) {
    var tableMessage = document.getElementById('table-message');
    var chassis = textOr(vehicle.chassisNumber);

    var input = window.prompt('New price for ' + chassis + ' (current: ' + formatPrice(vehicle.price) + '):', '');
    if (input === null) return;
    if (input.trim() === '' || isNaN(Number(input))) {
        setMessage(tableMessage, 'Invalid price. No request sent.', true);
        return;
    }

    var proposal = Number(input);

    if (proposal <= 0) {
        setMessage(tableMessage, 'Price must be a positive value.', true);
        return;
    }

    var reason = window.prompt('Reason for changing the price of ' + chassis + ':', daysSummary(vehicle)) || '';
    if (reason.trim() === '') {
        setMessage(tableMessage, 'A reason is required. No request sent.', true);
        return;
    }

    try {
        var result = await postJson(REQUEST_UPDATE_URL, {
            vehicleId: vehicle.id,
            proposedPrice: proposal,
            reason: reason.trim()
        });

        if (result.ok) {
            setMessage(tableMessage, 'Price update request ' + result.data.requestId + ' submitted for ' + chassis + ' - pending senior manager approval.', false);
            await loadInventory(); // shows the vehicle flipping to PendingUpdate
        } else if (result.status === 404) {
            setMessage(tableMessage, (result.data && result.data.message) || 'Vehicle not found.', true);
            await loadInventory();
        } else {
            setMessage(tableMessage, 'Request failed (HTTP ' + result.status + ').', true);
        }
    } catch (err) {
        setMessage(tableMessage, 'Network error: ' + err.message, true);
    }
}

function daysSummary(vehicle) {
    var days = vehicle.daysInStock;
    if (days > AGED_DAYS) return days + ' days in stock - ' + String(vehicle.recommendation || '').toLowerCase();
    if (days > A_STOCK_AGED) return days + ' days in stock - aged stock needs a price review';
    return days + ' days in stock';
}

// ---------- inventory assistant agent (vehicle search) ----------

function initSearchAgent() {
    var output = document.getElementById('chat-output');
    var input = document.getElementById('chat-input');
    var send = document.getElementById('chat-send');
    if (!output || !input || !send) return;

    send.addEventListener('click', function () { runSearch(); });
    input.addEventListener('keydown', function (event) {
        if (event.key === 'Enter') {
            event.preventDefault();
            runSearch();
        }
    });
}

async function runSearch() {
    var output = document.getElementById('chat-output');
    var input = document.getElementById('chat-input');

    var text = input.value.trim();
    if (text === '') return;

    appendChat(output, 'user', '> ' + text);
    input.value = '';

    try {
        var result = await fetchJson(SEARCH_URL + encodeURIComponent(text));

        if (!result.ok) {
            appendChat(output, 'agent error', 'Search failed (HTTP ' + result.status + ').');
            return;
        }

        var vehicles = Array.isArray(result.data) ? result.data : [];

        if (vehicles.length === 0) {
            appendChat(output, 'agent empty', 'No vehicles match "' + text + '". Try part of a chassis number, make, or model.');
            return;
        }

        appendChat(output, 'agent', 'Found ' + vehicles.length + ' vehicle(s) matching "' + text + '":');
        vehicles.forEach(function (vehicle) {
            appendChat(output, 'agent result', describeVehicle(vehicle));
        });
    } catch (err) {
        appendChat(output, 'agent error', 'Network error: ' + err.message);
    }
}

function describeVehicle(vehicle) {
    return [
        String(vehicle.chassisNumber || ''),
        textOr(vehicle.make) + ' ' + textOr(vehicle.model),
        'Year ' + textOr(vehicle.year),
        '$' + formatPrice(vehicle.price),
        textOr(vehicle.daysInStock) + ' days in stock',
        textOr(vehicle.status),
        textOr(vehicle.recommendation)
    ].join(' - ');
}

function appendChat(output, kind, text) {
    var entry = document.createElement('p');
    entry.className = 'chat-entry ' + kind;
    entry.textContent = text;
    output.appendChild(entry);
    output.scrollTop = output.scrollHeight;
}

// ---------- boot ----------

document.addEventListener('DOMContentLoaded', function () {
    initVehicleForm();
    initFormCollapse();
    initSearchAgent();

    var refreshButton = document.createElement('button');
    refreshButton.type = 'button';
    refreshButton.textContent = 'Refresh';
    refreshButton.className = 'price-update';
    refreshButton.addEventListener('click', loadInventory);

    var heading = document.getElementById('stock-heading');
    if (heading) {
        heading.appendChild(document.createTextNode(' '));
        heading.appendChild(refreshButton);
    }

    loadInventory();
});

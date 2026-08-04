(function () {
  'use strict';

  function initializeSidebar() {
    var toggle = document.getElementById('nlSidebarToggle');
    var sidebar = document.getElementById('nlSidebar');
    if (!toggle || !sidebar) { return; }

    var backdrop;
    function close() {
      sidebar.classList.remove('show');
      toggle.setAttribute('aria-expanded', 'false');
      if (backdrop) { backdrop.remove(); backdrop = null; }
    }

    toggle.addEventListener('click', function () {
      var open = sidebar.classList.toggle('show');
      toggle.setAttribute('aria-expanded', open ? 'true' : 'false');
      if (open) {
        backdrop = document.createElement('div');
        backdrop.className = 'nl-backdrop';
        backdrop.addEventListener('click', close);
        document.body.appendChild(backdrop);
      } else {
        close();
      }
    });
    document.addEventListener('keydown', function (event) {
      if (event.key === 'Escape') { close(); }
    });
  }

  function initializeCreateRequestForm() {
    var form = document.querySelector('[data-create-request-form]');
    if (!form) { return; }

    var mode = document.getElementById('inputMode');
    var start = document.getElementById('startDate');
    var end = document.getElementById('endDate');
    var days = document.getElementById('workingDays');
    var reason = document.getElementById('Reason');
    var counter = document.getElementById('reasonCount');
    var errors = document.getElementById('createErrors');
    var summary = document.querySelector('[data-balance-summary]');
    var projected = document.querySelector('[data-projected-balance]');
    var projectedCard = document.querySelector('[data-projected-balance-card]');
    var projectedHint = document.querySelector('[data-projected-balance-hint]');
    var availableDays = summary ? Number(summary.getAttribute('data-available-days')) : 0;

    if (errors && errors.textContent.trim().length > 0) { errors.classList.remove('d-none'); }

    function parseDate(input) {
      return input.value ? new Date(input.value + 'T00:00:00Z') : null;
    }

    function formatDate(date) {
      return date ? date.toISOString().slice(0, 10) : '';
    }

    function isWorkingDay(date) {
      return date.getUTCDay() !== 0 && date.getUTCDay() !== 6;
    }

    function countWorkingDays(from, to) {
      if (!from || !to || to < from) { return '' }
      var total = 0;
      var cursor = new Date(from.getTime());
      while (cursor <= to) {
        if (isWorkingDay(cursor)) { total += 1; }
        cursor.setUTCDate(cursor.getUTCDate() + 1);
      }
      return total;
    }

    function deriveEndDate(from, requestedDays) {
      if (!from || !isWorkingDay(from) || !Number.isInteger(requestedDays) || requestedDays < 1) { return null; }
      var cursor = new Date(from.getTime());
      var counted = 0;
      while (counted < requestedDays) {
        if (isWorkingDay(cursor)) { counted += 1; }
        if (counted < requestedDays) { cursor.setUTCDate(cursor.getUTCDate() + 1); }
      }
      return cursor;
    }

    function isDateRange() {
      return (mode.value || '').toLowerCase().indexOf('startplusdays') === -1;
    }

    function updateProjectedBalance() {
      if (!projected) { return; }
      var requestedDays = Number(days.value) || 0;
      var remaining = availableDays - requestedDays;
      projected.textContent = remaining + (Math.abs(remaining) === 1 ? ' día' : ' días');
      projected.classList.toggle('text-danger', remaining < 0);
      if (projectedCard) { projectedCard.classList.toggle('nl-stat-danger', remaining < 0); }
      if (projectedHint) {
        projectedHint.textContent = remaining < 0
          ? 'Saldo insuficiente para esta solicitud'
          : 'Si la solicitud es aprobada';
      }
    }

    function applyMode() {
      var dateRange = isDateRange();
      days.disabled = dateRange;
      days.readOnly = dateRange;
      end.disabled = !dateRange;
      end.readOnly = !dateRange;
      days.setAttribute('aria-disabled', dateRange ? 'true' : 'false');
      end.setAttribute('aria-disabled', dateRange ? 'false' : 'true');

      form.querySelectorAll('[data-mode-hint]').forEach(function (hint) {
        hint.hidden = (hint.getAttribute('data-mode-hint') === 'dateRange') !== dateRange;
      });

      if (dateRange) {
        days.value = countWorkingDays(parseDate(start), parseDate(end));
      } else {
        end.value = formatDate(deriveEndDate(parseDate(start), Number(days.value)));
      }
      updateProjectedBalance();
    }

    ['change', 'input'].forEach(function (eventName) {
      mode.addEventListener(eventName, applyMode);
      start.addEventListener(eventName, applyMode);
      end.addEventListener(eventName, applyMode);
      days.addEventListener(eventName, applyMode);
    });
    applyMode();

    if (reason && counter) {
      var updateCount = function () { counter.textContent = reason.value.length; };
      reason.addEventListener('input', updateCount);
      updateCount();
    }

    form.addEventListener('submit', function () {
      var dateRange = isDateRange();
      days.disabled = dateRange;
      end.disabled = !dateRange;
      var button = document.getElementById('submitButton');
      var spinner = document.getElementById('submitSpinner');
      if (button && !button.disabled) {
        spinner.classList.remove('d-none');
        document.getElementById('submitLabel').textContent = 'Creando...';
        setTimeout(function () { button.disabled = true; }, 0);
      }
    });
  }

  initializeSidebar();
  initializeCreateRequestForm();
}());

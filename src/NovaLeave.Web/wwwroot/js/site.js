(() => {
  const demoAccountSelect = document.querySelector("[data-demo-account-select]");
  const emailInput = document.querySelector("[data-login-email]");

  if (!demoAccountSelect || !emailInput) {
    return;
  }

  demoAccountSelect.addEventListener("change", () => {
    if (demoAccountSelect.value) {
      emailInput.value = demoAccountSelect.value;
      emailInput.dispatchEvent(new Event("input", { bubbles: true }));
    }
  });
})();

(() => {
  const form = document.querySelector("[data-request-form]");
  if (!form) return;

  const modeInputs = [...document.querySelectorAll('input[name="InputMode"]')];
  const start = form.querySelector("[data-request-start]");
  const end = form.querySelector("[data-request-end]");
  const days = form.querySelector("[data-request-days]");
  const rangeField = form.querySelector("[data-date-range-field]");
  const daysField = form.querySelector("[data-working-days-field]");
  const calculatedDays = form.querySelector("[data-calculated-days]");
  const calculatedEnd = form.querySelector("[data-calculated-end]");
  const projection = form.querySelector("[data-calculated-projection]");
  const available = Number.parseInt(form.dataset.availableDays ?? "", 10);

  const parseDate = (value) => value ? new Date(`${value}T00:00:00Z`) : null;
  const formatDate = (date) => date
    ? new Intl.DateTimeFormat("es-GT", { day: "2-digit", month: "2-digit", year: "numeric", timeZone: "UTC" }).format(date)
    : "—";
  const isWorkingDay = (date) => ![0, 6].includes(date.getUTCDay());

  const countWorkingDays = (from, to) => {
    if (!from || !to || to < from) return null;
    let total = 0;
    const cursor = new Date(from);
    while (cursor <= to) {
      if (isWorkingDay(cursor)) total += 1;
      cursor.setUTCDate(cursor.getUTCDate() + 1);
    }
    return total;
  };

  const addWorkingDays = (from, amount) => {
    if (!from || !Number.isInteger(amount) || amount < 1) return null;
    const cursor = new Date(from);
    let remaining = amount;
    while (remaining > 0) {
      if (isWorkingDay(cursor)) remaining -= 1;
      if (remaining > 0) cursor.setUTCDate(cursor.getUTCDate() + 1);
    }
    return cursor;
  };

  const update = () => {
    const dateRange = document.querySelector('input[name="InputMode"]:checked')?.value.toLowerCase().includes("daterange") ?? true;
    rangeField.hidden = !dateRange;
    daysField.hidden = dateRange;
    end.disabled = !dateRange;
    days.disabled = dateRange;

    const startDate = parseDate(start.value);
    const total = dateRange
      ? countWorkingDays(startDate, parseDate(end.value))
      : Number.parseInt(days.value, 10) || null;
    const finalDate = dateRange ? parseDate(end.value) : addWorkingDays(startDate, total);

    calculatedDays.textContent = total ? `${total} días` : "—";
    calculatedEnd.textContent = formatDate(finalDate);
    if (projection) {
      projection.textContent = total && Number.isInteger(available) ? `${available - total} días` : "—";
      projection.classList.toggle("text-danger", Boolean(total && Number.isInteger(available) && available - total < 0));
    }
  };

  [...modeInputs, start, end, days].forEach((control) => {
    control.addEventListener("change", update);
    control.addEventListener("input", update);
  });
  update();
})();

document.querySelectorAll("[data-character-source]").forEach((source) => {
  const counter = source.closest("form")?.querySelector("[data-character-count]");
  if (!counter) return;
  const update = () => { counter.textContent = `${source.value.length} / ${source.maxLength}`; };
  source.addEventListener("input", update);
  update();
});

document.querySelectorAll("[data-submit-once]").forEach((form) => {
  form.addEventListener("submit", () => {
    const button = form.querySelector("[data-submit-button]");
    if (!button) return;
    button.disabled = true;
    button.setAttribute("aria-busy", "true");
    button.textContent = "Procesando…";
  });
});

const validationPopover = document.querySelector("[data-validation-popover]");
if (validationPopover) {
  requestAnimationFrame(() => validationPopover.focus({ preventScroll: true }));
}

(() => {
  if (window.matchMedia("(prefers-reduced-motion: reduce)").matches) return;

  const targets = document.querySelectorAll(
    ".nl-page-header, .nl-summary-card, .nl-module-card, main > .nl-content-container > .nl-card, main > .nl-content-container > section.nl-card"
  );

  targets.forEach((target, index) => {
    target.classList.add("nl-reveal");
    target.style.setProperty("--nl-reveal-delay", `${Math.min(index * 35, 175)}ms`);
  });

  requestAnimationFrame(() => {
    targets.forEach((target) => target.classList.add("nl-reveal-visible"));
  });
})();

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

(() => {
  const modalElement = document.getElementById("nlConfirmationModal");
  const confirmButton = modalElement?.querySelector("[data-confirm-submit]");
  if (!modalElement || !confirmButton || typeof bootstrap === "undefined") return;

  const title = modalElement.querySelector("#nlConfirmationTitle");
  const message = modalElement.querySelector("#nlConfirmationMessage");
  const modal = bootstrap.Modal.getOrCreateInstance(modalElement);
  let pendingForm = null;

  document.querySelectorAll("form[data-confirm]").forEach((form) => {
    form.addEventListener("submit", (event) => {
      if (form.dataset.confirmed === "true") return;
      event.preventDefault();
      pendingForm = form;
      title.textContent = form.dataset.confirmTitle ?? "Confirmar operación";
      message.textContent = form.dataset.confirmMessage ?? "Revise la operación antes de continuar.";
      confirmButton.textContent = form.dataset.confirmLabel ?? "Confirmar";
      confirmButton.className = form.dataset.confirmVariant === "success"
        ? "btn btn-success"
        : form.dataset.confirmVariant === "danger"
          ? "btn nl-button-danger-confirm"
          : "btn btn-primary";
      modal.show();
    });
  });

  confirmButton.addEventListener("click", () => {
    if (!pendingForm) return;
    const form = pendingForm;
    pendingForm = null;
    form.dataset.confirmed = "true";
    modal.hide();
    form.requestSubmit();
  });

  modalElement.addEventListener("hidden.bs.modal", () => {
    pendingForm = null;
  });
})();

document.querySelectorAll("[data-submit-once]").forEach((form) => {
  form.addEventListener("submit", () => {
    if (form.hasAttribute("data-confirm") && form.dataset.confirmed !== "true") return;
    const button = form.querySelector("[data-submit-button]");
    if (!button) return;
    const originalLabel = button.textContent.trim();
    const loadingLabel = originalLabel.startsWith("Aprobar")
      ? "Aprobando…"
      : originalLabel.startsWith("Rechazar")
        ? "Rechazando…"
        : originalLabel.startsWith("Guardar")
          ? "Guardando…"
          : originalLabel.startsWith("Crear")
            ? "Creando…"
            : "Procesando…";

    button.style.minWidth = `${Math.ceil(button.getBoundingClientRect().width)}px`;
    button.disabled = true;
    button.setAttribute("aria-busy", "true");
    button.replaceChildren();
    const spinner = document.createElement("span");
    spinner.className = "nl-button-spinner";
    spinner.setAttribute("aria-hidden", "true");
    button.append(spinner, document.createTextNode(loadingLabel));
  });
});

const validationPopover = document.querySelector("[data-validation-popover]");
if (validationPopover) {
  requestAnimationFrame(() => validationPopover.focus({ preventScroll: true }));
}

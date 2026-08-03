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

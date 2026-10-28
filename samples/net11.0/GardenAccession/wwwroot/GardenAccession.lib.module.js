// snippet:client
export function afterWebStarted(blazor) {
  blazor.formValidation.addValidator('garden.accession', context => {
    const value = context.value;
    if (!value) return { success: true };
    const letters = Number(context.params.letters);
    const digits = Number(context.params.digits);
    const separator = context.params.separator;
    const head = value.slice(0, letters);
    const tail = value.slice(letters + separator.length);
    return { success: value.length === letters + separator.length + digits
      && head.length === letters && !/[^A-Z]/.test(head) && value.slice(letters, letters + separator.length) === separator
      && tail.length === digits && !/[^0-9]/.test(tail) };
  });
}
// endsnippet:client

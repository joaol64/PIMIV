// eventos.html — "Ver detalhes" mockado (substitua por navegação real depois).

document.querySelectorAll(".eventos-detalhe-btn").forEach((btn) => {
  btn.addEventListener("click", () => {
    const nome = btn.getAttribute("data-nome") || "Evento";
    window.alert(
      `Detalhes (exemplo) do evento:\n\n"${nome}"\n\nEm uma próxima etapa, você pode abrir outra página ou carregar a programação da API.`
    );
  });
});

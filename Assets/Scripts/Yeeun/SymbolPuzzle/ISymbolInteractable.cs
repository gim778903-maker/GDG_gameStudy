namespace Yeeun.SymbolPuzzle
{
    /// <summary>
    /// Something the player can use with the interact key while looking at it
    /// (symbol pieces, sockets, rotating pillars).
    /// </summary>
    public interface ISymbolInteractable
    {
        /// <summary>Prompt text to show, or null when it cannot be used right now.</summary>
        string GetPrompt(SymbolInteractor interactor);

        void Interact(SymbolInteractor interactor);
    }
}

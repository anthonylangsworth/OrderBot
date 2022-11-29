using OrderBot.ToDo;

namespace OrderBot.Test.ToDo;
internal class FakeValidator : INameValidator
{
    public Task<bool> IsKnownMinorFaction(string minorFactionName) => Task.FromResult(true);
    public Task<bool> IsKnownStarSystem(string starSystemName) => Task.FromResult(true);
}

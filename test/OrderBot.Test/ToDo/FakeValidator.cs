using OrderBot.ToDo;

namespace OrderBot.Test.ToDo;
internal class FakeValidator : Validator
{
    public override Task<bool> IsKnownMinorFactionAsync(string minorFactionName) => Task.FromResult(true);
    public override Task<bool> IsKnownStarSystemAsync(string starSystemName) => Task.FromResult(true);
}

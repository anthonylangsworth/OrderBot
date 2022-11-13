using Discord;
using Microsoft.EntityFrameworkCore;
using OrderBot.Core;
using OrderBot.Discord;
using OrderBot.EntityFramework;
using System.Transactions;

namespace OrderBot.ToDo;

/// <summary>
/// Separate the core functionalitiy for automated testing, potential reuse and command aliasing.
/// </summary>
/// <remarks>
/// For implementors:
/// <list type="number">
/// <li>
/// Code as you would normally in C#, e.g. throw exceptions for errors. It is the wrapping code's 
/// responsibility to respond to the caller, audit and log details.
/// </li>
/// <li>
/// The class is stateless. Do not save or require state from one call to the next.
/// </li>
/// <li>
/// Pass in a <see cref="OrderBotDbContext"/> to allow transactions across calls to multiple 
/// exposed methods or easier testing. This should be the first argument for consistency.
/// </li>
/// <li>
/// Use <see cref="TransactionScope"/> to create transactions around adds, updates or deletes 
/// to ensure consistency.
/// </li>
/// <li>
/// Since the code uses <see cref="TransactionScope"/> for transactions, avoid async/await.
/// While this decision means a small decrease in responsiveness
/// </li>
/// </list>
/// </remarks>
public class ToDoListApi
{
    /// <summary>
    /// Create a new <see cref="ToDoListApi"/>.
    /// </summary>
    /// <param name="generator"></param>
    /// <param name="formatter"></param>
    public ToDoListApi(ToDoListGenerator generator, ToDoListFormatter formatter)
    {
        Generator = generator;
        Formatter = formatter;
    }

    internal ToDoListGenerator Generator { get; }
    internal ToDoListFormatter Formatter { get; }

    /// <summary>
    /// Get the list of suggestions.
    /// </summary>
    /// <param name="dbContext">
    /// The <see cref="OrderBotDbContext"/> to use.
    /// </param>
    /// <param name="guild">
    /// The <see cref="IGuild"/> to get the list for.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Either there is no <see cref="DiscordGuild"/> for <paramref name="guild"/>
    /// in the database, that guild supports no minor factions.
    /// </exception>
    /// <exception cref="UnknownGoalException">
    /// A goal in a star system for a minor faction is not known.
    /// </exception>
    public string GetTodoList(OrderBotDbContext dbContext, IGuild guild)
    {
        // TODO: Consider using the passed in dbContext for consistency
        return Formatter.Format(Generator.Generate(guild.Id));
    }

    /// <summary>
    /// Set the supported minor faction.
    /// </summary>
    /// <param name="dbContext">
    /// The <see cref="OrderBotDbContext"/> to use.
    /// </param>
    /// <param name="guild">
    /// The <see cref="IGuild"/> to get the supported minor faction.
    /// </param>
    /// <param name="minorFactionName">
    /// The minor faction to support.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="minorFactionName"/> is not a known or valid minor faction.
    /// </exception>
    public void SetSupportedMinorFaction(OrderBotDbContext dbContext, IGuild guild, string minorFactionName)
    {
        MinorFaction minorFaction;
        using TransactionScope transactionScope = new();

        // TODO: Use a different validation mechanism, e.g. web service call. Otherwise,
        // we have a "chicken and egg" problem with minor faction creation.
        try
        {
            minorFaction = dbContext.MinorFactions.First(mf => mf.Name == minorFactionName);
        }
        catch (InvalidOperationException)
        {
            throw new ArgumentException($"Minor faction {minorFactionName} is not known");
        }

        DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(dbContext, guild,
            dbContext.DiscordGuilds.Include(e => e.SupportedMinorFactions));
        if (!discordGuild.SupportedMinorFactions.Contains(minorFaction))
        {
            discordGuild.SupportedMinorFactions.Add(minorFaction);
        }

        dbContext.SaveChanges();
        transactionScope.Complete();
    }

    /// <summary>
    /// Add goals.
    /// </summary>
    /// <param name="dbContext">
    /// The <see cref="OrderBotDbContext"/> to use.
    /// </param>
    /// <param name="guild">
    /// Add goals for this <see cref="IGuild"/>.
    /// </param>
    /// <param name="goals">
    /// The goal(s) to add.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Either the minor faction, system name or goal is invalid.
    /// </exception>
    public void AddGoals(OrderBotDbContext dbContext, IGuild guild,
        IEnumerable<(string minorFactionName, string starSystemName, string goalName)> goals)
    {
        using TransactionScope transactionScope = new();

        DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(dbContext, guild);

        foreach ((string minorFactionName, string starSystemName, string goalName) in goals)
        {
            MinorFaction? minorFaction = dbContext.MinorFactions.FirstOrDefault(mf => mf.Name == minorFactionName);
            if (minorFaction == null)
            {
                throw new ArgumentException($"*{minorFactionName}* is not a known minor faction");
            }

            StarSystem? starSystem = dbContext.StarSystems.FirstOrDefault(ss => ss.Name == starSystemName);
            if (starSystem == null)
            {
                throw new ArgumentException($"{starSystemName} is not a known star system");
            }

            if (!ToDo.Goals.Map.TryGetValue(goalName, out Goal? goal))
            {
                throw new ArgumentException($"{goalName} is not a known goal");
            }

            Presence? starSystemMinorFaction =
                dbContext.Presences.Include(ssmf => ssmf.StarSystem)
                                   .Include(ssmf => ssmf.MinorFaction)
                                   .FirstOrDefault(ssmf => ssmf.StarSystem.Name == starSystemName
                                                        && ssmf.MinorFaction.Name == minorFactionName);
            if (starSystemMinorFaction == null)
            {
                starSystemMinorFaction = new Presence() { MinorFaction = minorFaction, StarSystem = starSystem };
                dbContext.Presences.Add(starSystemMinorFaction);
            }

            DiscordGuildPresenceGoal? discordGuildStarSystemMinorFactionGoal =
                dbContext.DiscordGuildPresenceGoals
                         .Include(dgssmfg => dgssmfg.Presence)
                         .Include(dgssmfg => dgssmfg.Presence.StarSystem)
                         .Include(dgssmfg => dgssmfg.Presence.MinorFaction)
                         .FirstOrDefault(
                             dgssmfg => dgssmfg.DiscordGuild == discordGuild
                                     && dgssmfg.Presence.MinorFaction == minorFaction
                                     && dgssmfg.Presence.StarSystem == starSystem);
            if (discordGuildStarSystemMinorFactionGoal == null)
            {
                discordGuildStarSystemMinorFactionGoal = new DiscordGuildPresenceGoal()
                { DiscordGuild = discordGuild, Presence = starSystemMinorFaction };
                dbContext.DiscordGuildPresenceGoals.Add(discordGuildStarSystemMinorFactionGoal);
            }
            discordGuildStarSystemMinorFactionGoal.Goal = goalName;
            dbContext.SaveChanges();
        }
        transactionScope.Complete();
    }

    /// <summary>
    /// List goals.
    /// </summary>
    /// <param name="dbContext">
    /// The <see cref="OrderBotDbContext"/> to use.
    /// </param>
    /// <param name="guild">
    /// List goals for this <see cref="IGuild"/>.
    /// </param>
    /// <returns>
    /// The goals.
    /// </returns>
    public IEnumerable<DiscordGuildPresenceGoal> ListGoals(OrderBotDbContext dbContext, IGuild guild)
    {
        DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(dbContext, guild);
        return dbContext.DiscordGuildPresenceGoals
                        .Include(dgssmfg => dgssmfg.Presence)
                        .Include(dgssmfg => dgssmfg.Presence.StarSystem)
                        .Include(dgssmfg => dgssmfg.Presence.MinorFaction);
    }

    /// <summary>
    /// Remove a goal.
    /// </summary>
    /// <param name="dbContext">
    /// The <see cref="OrderBotDbContext"/> to use.
    /// </param>
    /// <param name="guild">
    /// Remove goals for this <see cref="IGuild"/>.
    /// The goals.
    /// </returns>
    public void RemoveGoals(OrderBotDbContext dbContext, IGuild guild, string minorFactionName,
        string starSystemName)
    {
        using TransactionScope transactionScope = new();
        DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(dbContext, guild);

        MinorFaction? minorFaction = dbContext.MinorFactions.FirstOrDefault(mf => mf.Name == minorFactionName);
        if (minorFaction == null)
        {
            throw new ArgumentException($"*{minorFactionName}* is not a known minor faction");
        }

        StarSystem? starSystem = dbContext.StarSystems.FirstOrDefault(ss => ss.Name == starSystemName);
        if (starSystem == null)
        {
            throw new ArgumentException($"{starSystemName} is not a known star system");
        }

        DiscordGuildPresenceGoal? discordGuildStarSystemMinorFactionGoal =
            dbContext.DiscordGuildPresenceGoals
                        .Include(dgssmfg => dgssmfg.Presence)
                        .Include(dgssmfg => dgssmfg.Presence.StarSystem)
                        .Include(dgssmfg => dgssmfg.Presence.MinorFaction)
                        .FirstOrDefault(
                            dgssmfg => dgssmfg.DiscordGuild == discordGuild
                                    && dgssmfg.Presence.MinorFaction == minorFaction
                                    && dgssmfg.Presence.StarSystem == starSystem);
        if (discordGuildStarSystemMinorFactionGoal != null)
        {
            dbContext.DiscordGuildPresenceGoals.Remove(discordGuildStarSystemMinorFactionGoal);
        }
        dbContext.SaveChanges();
        transactionScope.Complete();
    }
}

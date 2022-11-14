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
/// The class is stateless. Do not save or require state from one call to the next. Passing in 
/// the <see cref="OrderBotDbContext"/> in the constructor forces the caller to instantiate a
/// new instance each interaction, anyway.
/// </li>
/// <li>
/// The caller should always wrap calls in a <see cref="TransactionScope"> to ensure 
/// consistency. The calls cannot do it themselves because exceptions may be thrown as
/// part of normal behaviour, in turn throwing a <see cref="TransactionAbortedException"/>.
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
    /// <param name="dbContext">
    /// The <see cref="OrderBotDbContext"/> to use.
    /// </param>
    /// <param name="guild">
    /// The <see cref="IGuild"/> to act on or for.
    /// </param>
    public ToDoListApi(OrderBotDbContext dbContext, IGuild guild)
    {
        DbContext = dbContext;
        Guild = guild;
    }

    internal OrderBotDbContext DbContext { get; }
    internal IGuild Guild { get; }

    /// <summary>
    /// Get the list of suggestions.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Either there is no <see cref="DiscordGuild"/> for <paramref name="guild"/>
    /// in the database, that guild supports no minor factions.
    /// </exception>
    /// <exception cref="UnknownGoalException">
    /// A goal in a star system for a minor faction is not known.
    /// </exception>
    public string GetTodoList()
    {
        return new ToDoListFormatter().Format(new ToDoListGenerator(DbContext).Generate(Guild.Id));
    }

    /// <summary>
    /// Set the supported minor faction.
    /// </summary>
    /// <param name="minorFactionName">
    /// The minor faction to support.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="minorFactionName"/> is not a known or valid minor faction.
    /// </exception>
    public void SetSupportedMinorFaction(string minorFactionName)
    {
        // TODO: Use a different validation mechanism, e.g. web service call. Otherwise,
        // we have a "chicken and egg" problem with minor faction creation.
        MinorFaction minorFaction;
        try
        {
            minorFaction = DbContext.MinorFactions.First(mf => mf.Name == minorFactionName);
        }
        catch (InvalidOperationException)
        {
            throw new ArgumentException($"Minor faction {minorFactionName} is not known");
        }

        DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(DbContext, Guild,
            DbContext.DiscordGuilds.Include(e => e.SupportedMinorFactions));
        if (!discordGuild.SupportedMinorFactions.Contains(minorFaction))
        {
            discordGuild.SupportedMinorFactions.Add(minorFaction);
        }

        DbContext.SaveChanges();
    }

    /// <summary>
    /// Get the supported minor faction.
    /// </summary>
    /// <returns>
    /// The supported minor faction or <see cref="null"/> if there is none.
    /// </returns>
    public MinorFaction? GetSupportedMinorFaction()
    {
        DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(DbContext, Guild,
            DbContext.DiscordGuilds.Include(e => e.SupportedMinorFactions));
        return discordGuild.SupportedMinorFactions.Any()
            ? discordGuild.SupportedMinorFactions.FirstOrDefault()
            : null;
    }

    /// <summary>
    /// Clear the supported minor faction.
    /// </summary>
    public void ClearSupportedMinorFaction()
    {
        DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(DbContext, Guild,
            DbContext.DiscordGuilds.Include(e => e.SupportedMinorFactions));
        discordGuild.SupportedMinorFactions.Clear();
        DbContext.SaveChanges();
    }

    /// <summary>
    /// Add goals.
    /// </summary>
    /// <param name="goals">
    /// The goal(s) to add.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Either the minor faction, system name or goal is invalid.
    /// </exception>
    public void AddGoals(
        IEnumerable<(string minorFactionName, string starSystemName, string goalName)> goals)
    {
        DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(DbContext, Guild);

        foreach ((string minorFactionName, string starSystemName, string goalName) in goals)
        {
            MinorFaction? minorFaction = DbContext.MinorFactions.FirstOrDefault(mf => mf.Name == minorFactionName);
            if (minorFaction == null)
            {
                throw new ArgumentException($"*{minorFactionName}* is not a known minor faction");
            }

            StarSystem? starSystem = DbContext.StarSystems.FirstOrDefault(ss => ss.Name == starSystemName);
            if (starSystem == null)
            {
                throw new ArgumentException($"{starSystemName} is not a known star system");
            }

            if (!ToDo.Goals.Map.TryGetValue(goalName, out Goal? goal))
            {
                throw new ArgumentException($"{goalName} is not a known goal");
            }

            Presence? starSystemMinorFaction =
                DbContext.Presences.Include(ssmf => ssmf.StarSystem)
                                   .Include(ssmf => ssmf.MinorFaction)
                                   .FirstOrDefault(ssmf => ssmf.StarSystem.Name == starSystemName
                                                        && ssmf.MinorFaction.Name == minorFactionName);
            if (starSystemMinorFaction == null)
            {
                starSystemMinorFaction = new Presence() { MinorFaction = minorFaction, StarSystem = starSystem };
                DbContext.Presences.Add(starSystemMinorFaction);
            }

            DiscordGuildPresenceGoal? discordGuildStarSystemMinorFactionGoal =
                DbContext.DiscordGuildPresenceGoals
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
                DbContext.DiscordGuildPresenceGoals.Add(discordGuildStarSystemMinorFactionGoal);
            }
            discordGuildStarSystemMinorFactionGoal.Goal = goalName;
            DbContext.SaveChanges();
        }
    }

    /// <summary>
    /// List goals.
    /// </summary>
    /// <returns>
    /// The goals.
    /// </returns>
    public IEnumerable<DiscordGuildPresenceGoal> ListGoals()
    {
        DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(DbContext, Guild);
        return DbContext.DiscordGuildPresenceGoals
                        .Include(dgssmfg => dgssmfg.Presence)
                        .Include(dgssmfg => dgssmfg.Presence.StarSystem)
                        .Include(dgssmfg => dgssmfg.Presence.MinorFaction);
    }

    /// <summary>
    /// Remove a goal.
    /// </summary>
    /// <param name="minorFactionName">
    /// </param>
    /// <param name="starSystemName">
    /// </param>
    /// <exception cref="ArgumentException">
    /// Both <paramref name="minorFactionName"/> and <paramref name="starSystemName"/>
    /// must be valid.
    /// </exception>
    public void RemoveGoals(string minorFactionName,
        string starSystemName)
    {
        DiscordGuild discordGuild = DiscordHelper.GetOrAddGuild(DbContext, Guild);

        MinorFaction? minorFaction = DbContext.MinorFactions.FirstOrDefault(mf => mf.Name == minorFactionName);
        if (minorFaction == null)
        {
            throw new ArgumentException($"*{minorFactionName}* is not a known minor faction");
        }

        StarSystem? starSystem = DbContext.StarSystems.FirstOrDefault(ss => ss.Name == starSystemName);
        if (starSystem == null)
        {
            throw new ArgumentException($"{starSystemName} is not a known star system");
        }

        DiscordGuildPresenceGoal? discordGuildStarSystemMinorFactionGoal =
            DbContext.DiscordGuildPresenceGoals
                        .Include(dgssmfg => dgssmfg.Presence)
                        .Include(dgssmfg => dgssmfg.Presence.StarSystem)
                        .Include(dgssmfg => dgssmfg.Presence.MinorFaction)
                        .FirstOrDefault(
                            dgssmfg => dgssmfg.DiscordGuild == discordGuild
                                    && dgssmfg.Presence.MinorFaction == minorFaction
                                    && dgssmfg.Presence.StarSystem == starSystem);
        if (discordGuildStarSystemMinorFactionGoal != null)
        {
            DbContext.DiscordGuildPresenceGoals.Remove(discordGuildStarSystemMinorFactionGoal);
        }
        DbContext.SaveChanges();
    }
}

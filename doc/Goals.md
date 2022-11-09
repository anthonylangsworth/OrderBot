# Goals

## List

This bot generates the following suggestions for each goal:

|Goal     |Conflict|Influence|Security|State|Auto-Remove|
|---------|--------|---------|--------|-----|-----------|
|Control (Default) |Fight for the minor faction in any conflict.|If there are no conflicts, keep influence between 55% and 65%.|Keep security at Medium or higher.|(TODO)|Never|
|Maintain |Fight for the minor faction except for conflicts against the controlling minor faction, where you fight for the controlling minor faction.|If there are no conflicts, have at least 10% influence and up to 3% below the controlling minor faction.|N/A|(TODO)|Never|
|Expand   |Fight for the minor faction in any conflict.|If there are no conflicts, keep influence above 75% force an expansion.|N/A|(TODO)|(TODO)|
|Retreat  |Fight against the minor faction in all conflicts.|If there are no conflicts, always work against the minor faction to force a retreat.|N/A|(TODO)|(TODO)|
|Ignore   |(None)|(None)|N/A|(None)|Never|

Note:
1. The bot does not check whether a minor faction targeted with a `Retreat` goal is non-native. Using a `Retreat` goal with a non-native minor faction will continually generate suggestions to work against that minor faction.
2. The bot does not check whether a minor faction is already in the Expansion state. Using a `Expand` goal in this case will generate suggestions to keep the influence above 75%.
3. `Retreat` and `Expand` goals remain until removed. Auto-remove may be added in the future.

## Principles

Goals are designed with the following principles:

1. Succint, descriptive goal names: Goals should have short, descriptive names and preferably a single word verb. Their effect should be clear to those familiar with the background simulation in Elite Dangerous.
2. Few, useful goals over many, niche goals: Goals should be broadly applicable. Niche goals should be handled manually.

# Goals

## List

Based on the specified goal, this bot generates the following suggestions:

|Goal     |Conflict|Influence|Security|State|
|---------|--------|---------|--------|------|
|Control (Default) |Fight for the minor faction in any conflict.|If there are no conflicts, keep influence between 55% and 65%.|Keep security at Medium or higher.|(TODO)|
|Maintain |Fight for the minor faction except for conflicts against the controlling minor faction, where you fight for the controlling minor faction.|If there are no conflicts, have at least 10% influence and up to 3% below the controlling minor faction.|N/A|(TODO)|
|Expand   |Fight for the minor faction in any conflict.|If there are no conflicts, maximize influence to force an expansion.|N/A|(TODO)|
|Retreat  |Fight against the minor faction in all conflicts.|If there are no conflicts, always work against the minor faction to force a retreat.|N/A|(TODO)|
|Ignore   |(None)|(None)|N/A|(None)|

## Principles

Goals are designed with the following principles:

1. Succint, descriptive goal names: Goals should have short, descriptive names and preferably a single word verb. Their effect should be clear to those familiar with the background simulation in Elite Dangerous.
2. Few, useful goals over many, niche goals: Goals should be broadly applicable. Niche goals should be handled manually.

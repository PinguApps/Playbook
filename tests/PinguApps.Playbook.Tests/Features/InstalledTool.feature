Feature: Installed offline guidance
  Coding agents retrieve original guidance from the distributed artifact without a checkout.

  Scenario: Install and retrieve complete property guidance over client-owned stdio
    Given the actual package is installed from an isolated local feed and cache
    Then bare invocation help and version identify the installed command
    When a client initializes the installed stdio server outside the checkout
    Then exactly the two read-only tools advertise output schemas
    And discovery and detail preserve complete structured and text results
    And the served examples match the canonical compiled sample sources
    When the client closes its input
    Then the server exits cleanly with diagnostics on stderr

  Scenario: Conditional routes and known recipes without a compatible edition remain explicit
    Given the release contract fixture package is installed from an isolated local feed and cache
    When a client initializes the installed stdio server outside the checkout
    Then a known incompatible recipe fails and a conditional older-target route stays explicit
    When the client closes its input
    Then the server exits cleanly with diagnostics on stderr

  Scenario: Combined discovery retains only applicable shared and neutral guidance
    Given the release contract fixture package is installed from an isolated local feed and cache
    When a client initializes the installed stdio server outside the checkout
    Then combined filters respect shared areas demonstrated languages and neutral configuration
    When the client closes its input
    Then the server exits cleanly with diagnostics on stderr

  Scenario: Discovery pages traverse stable identities without duplicates or omissions
    Given the release contract fixture package is installed from an isolated local feed and cache
    When a client initializes the installed stdio server outside the checkout
    Then discovery pages preserve ordinal order totals and explicit boundaries
    When the client closes its input
    Then the server exits cleanly with diagnostics on stderr

  Scenario: Invalid discovery filters are distinct from successful empty results
    Given the actual package is installed from an isolated local feed and cache
    When a client initializes the installed stdio server outside the checkout
    Then invalid filters and ranges explain corrections while zero matches succeed
    When the client closes its input
    Then the server exits cleanly with diagnostics on stderr

  Scenario: Explicit targets never retrieve an incompatible property edition
    Given the actual package is installed from an isolated local feed and cache
    When a client initializes the installed stdio server outside the checkout
    Then an explicit undeclared target fails instead of returning newer guidance
    When the client closes its input
    Then the server exits cleanly with diagnostics on stderr

  Scenario: Release aliases retain editions and disclose assumptions and coverage
    Given the actual package is installed from an isolated local feed and cache
    When a client initializes the installed stdio server outside the checkout
    Then release aliases select compatible retained editions with complete requirements
    And omitted targets disclose the newest installed release and coverage limits
    And invalid requests return actionable machine-readable tool failures
    When the client closes its input
    Then the server exits cleanly with diagnostics on stderr

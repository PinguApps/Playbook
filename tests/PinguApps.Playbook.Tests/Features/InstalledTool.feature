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

  Scenario: Explicit targets never retrieve an incompatible property edition
    Given the actual package is installed from an isolated local feed and cache
    When a client initializes the installed stdio server outside the checkout
    Then an explicit undeclared target fails instead of returning newer guidance
    When the client closes its input
    Then the server exits cleanly with diagnostics on stderr

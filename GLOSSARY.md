# Playbook

Playbook gives coding agents practical guidance for adopting newer .NET capabilities across the .NET ecosystem. Its initial comprehensive release coverage is .NET 11.

## Language

**Playbook**:
The product that helps coding agents choose and apply newer .NET approaches.
_Avoid_: Compass, PracticalDotNet.BlazorGuide

**Goal**:
The outcome a developer wants to achieve, such as preserving a grid's view after a refresh.

**Recipe**:
A unit of guidance for a developer goal, comparing an older approach with an approach enabled by a specified .NET release. A recipe may involve several framework features.
_Avoid_: Feature, when referring to the unit of guidance

**Older approach**:
The code or workaround used to achieve a goal before the newer capability was available.
_Avoid_: Bad code, obsolete code

**Newer approach**:
The approach to a goal made available by a specified .NET release.
_Avoid_: Latest approach

**Applicability**:
The circumstances in which guidance is relevant, including its .NET release, application framework, language, and platform. Blazor recipes may also depend on rendering context.

**Target release**:
The .NET release that the consuming application's code must support. This is separate from the installed Playbook package's version.

# Yardarm.Client

This project is a "fake" project to hold C# files which will be added to SDK projects. The compiled
output of this project is never referenced or distributed, instead the C# files are stored as
resources in Yardarm.dll.

Placing these C# files in a separate project allows us to validate the files by building the project,
ensuring that the C# code compiles. We can also run unit tests against the classes to ensure
correct behavior.

All members in this project **must** be `internal` members or be in `$rootnamespace$`. This avoids naming conflicts
if a consumer is using more than one generated SDK in their project.

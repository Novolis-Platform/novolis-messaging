# Release

Versions follow `build/version.json` and Novolis shared MSBuild imports.

Publish both **Novolis.Messaging.Channels** and **Novolis.Messaging** to GitHub Packages after a green `dotnet build` on `Novolis.Messaging.slnx`. Each package ships XML documentation and its README under `src/<PackageId>/README.md`.

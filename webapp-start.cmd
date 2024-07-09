start "PreProcessor" cmd /k "cd PreProcessor\PreProcessor && dotnet run"
start "Processor" cmd /k "cd Processor\Processor && dotnet run"
start "PostProcessor" cmd /k "cd PostProcessor\PostProcessor && dotnet run"
start "HeartbeatApp" cmd /k "cd HeartbeatApp\HeartbeatApp && dotnet run"
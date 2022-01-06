rm -rf bin
rm -rf obj
dotnet restore
dotnet msbuild
cp ~/cks/server.key .

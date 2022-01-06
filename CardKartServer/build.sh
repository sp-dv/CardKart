rm -rf bin
rm -rf obj
dotnet restore
notnet msbuild
cp ~/cks/server.key .
